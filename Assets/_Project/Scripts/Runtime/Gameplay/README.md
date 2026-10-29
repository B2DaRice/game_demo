# Gameplay (`AstroIdle.Gameplay.*`)

Layer 6: the **real-time simulation**, everything that moves, shoots, collides or spawns each frame. Gameplay
reads Content and Progression, reports outward through [`GameEvents`](../Events/README.md), and never references
UI, Core or Bootstrap.

`GameplayContext.cs` bundles the dependencies entities need (content, arena, registry, spawners, effects,
upgrades, weapon inventory). It's built once by `GameBootstrap` and passed to entities on `Initialize`/`Activate`.

## Sub-areas

### Arena (`Gameplay.Arena`)
| File | Purpose |
|------|---------|
| `ArenaCamera.cs` | Configures the orthographic camera: viewport above the UI panel, constant 10-unit world width on any phone. Returns the HUD height in world units. |
| `PlayArea.cs` | World bounds from the camera. `ShipBounds` (excludes HUD strip and padding), spawn points just off-screen, Asteroids-style `Wrap`, clamping. |
| `Starfield.cs` | Generated static star background sized to the arena. |

### Combat (`Gameplay.Combat`)
| File | Purpose |
|------|---------|
| `CombatRegistry.cs` | List of live enemies plus the player. Spatial queries: `FindNearest`, `FindFirstOverlap` (swept along the projectile's path this frame, so fast shots can't tunnel), `FindInRadius`. Circle collisions, no Physics2D. |
| `Projectile.cs` | Pooled projectile for either team. Player shots: pierce, splash, homing. Enemy shots: straight at the ship. |
| `ProjectileSpawner.cs` | Pool plus `FirePlayer`/`FireEnemy`/`DespawnAll`. |
| `ProjectileSpec.cs` | Resolved per-shot parameters (post-upgrade). |
| `ProjectileTeam.cs` | `Player` / `Enemy`. |

### Enemies (`Gameplay.Enemies`)
| File | Purpose |
|------|---------|
| `Enemy.cs` | Pooled enemy: runtime stats (scaled per wave), damage, hit flash, contact damage, explode (reward and split) or silent despawn. |
| `EnemySpawner.cs` | Pool plus `Spawn(def, pos, scaling)` / `DespawnAll`. Raises `EnemySpawned`. |
| `EnemyScaling.cs` | Per-wave multipliers (health, damage, reward, speed). |
| `IEnemyBehaviour.cs` | Stateless AI strategy interface (`OnActivated`, `Tick`). Per-enemy state lives on `Enemy`. |
| `EnemyBehaviourFactory.cs` | Maps `EnemyBehaviourType` to its behaviour instance. |
| `DriftBehaviour.cs` | Fly through the center, wrap at edges. |
| `ChaseBehaviour.cs` | Accelerate toward the ship. |
| `ShooterBehaviour.cs` | Keep range, orbit, fire volleys. Clamped on-screen. |

### Player (`Gameplay.Player`)
| File | Purpose |
|------|---------|
| `PlayerShip.cs` | Hull/regen, autopilot movement, facing the nearest enemy, owns the `WeaponSystem`, damage/death/respawn with invulnerability. Refreshes stats on `UpgradePurchased`/`WeaponPurchased`. |
| `ShipAutopilot.cs` | Steering-force pilot (avoid enemies, center pull, wall push, wander). Tuning constants at the top. |

### Weapons (`Gameplay.Weapons`)
| File | Purpose |
|------|---------|
| `WeaponSystem.cs` | All owned weapons. `SyncWithInventory()` mounts newly bought ones. Ticks them. |
| `WeaponRuntime.cs` | One mounted weapon: cached final stats, cooldown, nearest-target selection, target leading, spread volleys. |

### Waves (`Gameplay.Waves`)
| File | Purpose |
|------|---------|
| `WaveDirector.cs` | The wave state machine (Spawning → Intermission → next). Auto-advance/farming, boss spawn, failure handling (deferred a frame), progress for the HUD, debug skip. |
| `WaveMath.cs` | Pure formulas for count, spawn interval, boss waves, scaling. **Keep in sync with docs/GAME_DESIGN.md.** |
| `SpawnPicker.cs` | Weighted random pick from the spawn table, filtered by wave. |

### Effects (`Gameplay.Effects`)
| File | Purpose |
|------|---------|
| `EffectsSystem.cs` | Ring buffer of 192 sprite particles: explosions, hit sparks, blast rings. Bounded cost. |

## Key flows

- **Firing:** `PlayerShip.Update` → `WeaponSystem.Tick` → `WeaponRuntime.Tick` → `CombatRegistry.FindNearest` →
  `ProjectileSpawner.FirePlayer`.
- **Hit and kill:** `Projectile.CheckEnemyHits` → `Enemy.TakeDamage` → `Enemy.Explode(true)` →
  `GameEvents.EnemyKilled` plus split children → `Enemy.Despawn` → `GameEvents.EnemyRemoved`.
- **Ship death:** `PlayerShip.TakeDamage` → `GameEvents.PlayerDestroyed` → `WaveDirector` sets `pendingFail` → next
  frame `HandleFail` clears everything, sets back the wave, turns farming on, and calls `PlayerShip.Respawn`.

## Rules specific to Gameplay

- Spawn and despawn **only** through the spawners. `Despawn()` methods are idempotent.
- Enemy behaviours are **stateless singletons**: put per-enemy state on `Enemy`.
- Remove yourself *before* damaging the player (damage can trigger a full clear).
- Use `Time.deltaTime` so the DEV speed-up works.

## How to...

**Add an enemy AI behaviour**
1. Append a value to `Content/EnemyBehaviourType.cs` (e.g. `Kamikaze = 3`).
2. Create `KamikazeBehaviour : IEnemyBehaviour` here. Move with `enemy.SetPosition`, set `enemy.Facing`.
3. Add a case to `EnemyBehaviourFactory.Get`.
4. If it needs new tunables, add fields to `EnemyDefinition` (with tooltips).
5. Set `behaviour: 3` on an enemy asset, then document it in the design doc's enemy table.

**Add a projectile motion** (e.g. boomerang): append to `ProjectileMotion`, handle it in `Projectile.Update`, and add
any parameters to `WeaponDefinition` and `ProjectileSpec`.

**Tune ship movement**: edit the constants at the top of `ShipAutopilot.cs`.
