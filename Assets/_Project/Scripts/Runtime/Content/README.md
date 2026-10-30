# Content (`AstroIdle.Content`)

Layer 2: the **shape of the game's data**. ScriptableObject classes whose assets (in
[`Resources/Content/`](../../../Resources/Content/README.md)) hold every balance number. Classes here are pure data
plus tiny helpers. **No game logic.**

| File | Kind | Purpose |
|------|------|---------|
| `ShipDefinition.cs` | ScriptableObject | Player ship visuals, base stats and the ship-wide upgrade tracks. Exactly one asset. |
| `WeaponDefinition.cs` | ScriptableObject | One weapon: acquisition (available from start / can be the random starter), base stats, projectile settings, upgrade tracks. |
| `EnemyDefinition.cs` | ScriptableObject | One enemy type: visuals, wave-1 stats, AI behaviour, shooter settings, split-on-death. |
| `MilestoneDefinition.cs` | ScriptableObject | One goal: condition + target, rewards (credits and/or making a gated weapon available). |
| `WaveConfig.cs` | ScriptableObject | Wave pacing, per-wave scaling, bosses, failure setback, spawn table. Exactly one asset. |
| `AutoBuyConfig.cs` | ScriptableObject | Auto mode: shopping interval, purchase cap, one `AutoBuyProfile` per strategy. Exactly one asset. |
| `EconomyConfig.cs` | ScriptableObject | Starting credits, kill reward multiplier, weapon unlock price (+ growth per owned weapon), shared upgrade cost curves, offline earnings, income smoothing, autosave interval. Exactly one asset. |
| `UpgradeTrack.cs` | Serializable class | A levelable upgrade embedded in a ship/weapon: stat, Flat/Percent per level, cost (own base/growth or a shared `costCurveId`), max level. |
| `CostCurve.cs` | Serializable class | Named shared cost curve (`id`, base cost, growth) defined in `EconomyConfig` and referenced by upgrade tracks. |
| `SpawnEntry.cs` | Serializable class | Spawn table row: enemy id, wave window, weight. |
| `AutoBuyProfile.cs` | Serializable class | One auto mode strategy: display name, description, weight per `AutoBuyCategory`. |
| `AutoBuyStrategy.cs` | Enum | `Balanced`, `Attack`, `Defense` (saved as int, append only). |
| `AutoBuyCategory.cs` | Enum | Auto mode buckets: weapon upgrade, weapon unlock, ship offense / defense / economy. |
| `StatId.cs` | Enum | Every upgradable stat (ship 0–99, weapon 100–199). |
| `ModifierKind.cs` | Enum | `Flat` or `Percent`. |
| `EnemyBehaviourType.cs` | Enum | `Drift`, `Chase`, `Shooter` (maps to AI classes in Gameplay/Enemies). |
| `ProjectileMotion.cs` | Enum | `Straight`, `Homing`. |
| `MilestoneCondition.cs` | Enum | `ReachWave`, `TotalKills`, `TotalCreditsEarned`. |
| `ContentDatabase.cs` | Class | Loads all assets from `Resources/Content/<Type>/`, sorts them, gives id lookups, and **validates** cross-references (spawn table, splits, milestone weapons, cost curve ids). |

## Important rules

- **All enums are serialized as integers** in `.asset` files. Append new values with explicit numbers. Never
  reorder or renumber.
- **`.cs.meta` GUIDs of the seven ScriptableObject scripts (Ship/Weapon/Enemy/Milestone definitions, Wave/Economy/
  AutoBuy configs) are referenced by the asset files.** Never delete or
  regenerate those `.meta` files, or every asset loses its script.
- Fields are `[SerializeField] private` with read-only properties, so runtime code can't mutate content.
- Cross-references between content use **string ids**, checked by `ContentDatabase.Validate()`.

## How the rest of the game uses this

- `GameBootstrap` calls `ContentDatabase.LoadFromResources()` once at startup and shares it.
- `UpgradeService` (Progression) combines `GetBaseStat()` with upgrade tracks to produce final stats.
- `WaveDirector` and `WaveMath` (Gameplay/Waves) read `WaveConfig`. Enemies read `EnemyDefinition`.
- The UI reads display names, descriptions and costs.

## How to...

**Add a new upgradable stat**
1. Append to `StatId` (ship stats < 100, weapon stats 100–199).
2. Add a field for its base value to `ShipDefinition` or `WeaponDefinition` and return it from `GetBaseStat`.
3. Read the final value via `UpgradeService.GetShipStat` / `GetWeaponStat` where gameplay uses it (for weapons,
   `WeaponRuntime.RefreshStats`; for the ship, `PlayerShip.RefreshStats`).
4. Add display formatting in `UI/StatFormatter.cs`.
5. Now any upgrade track can target it.

**Add a new content type** (e.g. `PowerUpDefinition`)
1. Create the class here with `[CreateAssetMenu(menuName = "Astro Idle/…")]`.
2. Add a path constant and a loader to `ContentDatabase`, plus validation rules.
3. Create the folder `Resources/Content/<Type>/` and assets via *Create → Astro Idle → …*.
4. Document it in this README and the [content guide](../../../Resources/Content/README.md).
