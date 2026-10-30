# Content guide: `Resources/Content/`

All game content and balance numbers live here as ScriptableObject assets. At startup `ContentDatabase` loads
**every asset in each sub-folder**, so adding content means creating an asset in the right folder. No code, no
registration lists, no scene changes.

> Files under any `Resources/` folder ship inside the app, this README included (a few KB). Keep large non-content
> files out of here.

| Folder | Asset type (script) | Count | Assets |
|--------|---------------------|-------|--------|
| `Ship/` | `ShipDefinition` | exactly 1 | `Ship_Sentinel` |
| `Weapons/` | `WeaponDefinition` | 1+ (≥1 can be the starting weapon) | `Weapon_PulseBlaster`, `Weapon_ScatterCannon`, `Weapon_HomingMissiles` |
| `Enemies/` | `EnemyDefinition` | 1+ | `Enemy_Asteroid`, `Enemy_AsteroidShard`, `Enemy_Drone`, `Enemy_Gunship`, `Enemy_Mothership` |
| `Milestones/` | `MilestoneDefinition` | any | `Milestone_01` … `Milestone_10` |
| `Config/` | `WaveConfig`, `EconomyConfig`, `AutoBuyConfig` | exactly 1 each | `WaveConfig`, `EconomyConfig`, `AutoBuyConfig` |

The current values and the reasoning behind them are in [docs/GAME_DESIGN.md](../../../../docs/GAME_DESIGN.md).
Field-by-field meaning is in the tooltips (hover a field in the Inspector) and the class docs in
[Scripts/Runtime/Content](../../Scripts/Runtime/Content/README.md).

---

## Editing values

- **In Unity (recommended):** select an asset in the Project window and edit it in the Inspector. Edits made
  **during Play mode are lost when you stop**, so edit while stopped.
- **As text (AI agents / quick diffs):** assets are YAML. Edit the values, keeping indentation and field names.
  Enums are stored as **numbers** (tables below). Strings with `:` `#` or leading symbols must be quoted
  (`'like this'`). Never touch the `m_Script` line.
- After any change: **Astro Idle → Validate Content**, then playtest.

## How to add a weapon
1. In `Weapons/`, select an existing weapon and **duplicate** it (Ctrl+D). Rename it to `Weapon_YourName`.
2. Set a **new unique `Id`** (lower_snake_case, permanent), display name, description and `Sort Order`.
3. Set acquisition:
   - `Available From Start` on: it can be unlocked for the shared price (`EconomyConfig.weaponUnlockCost`) at any
     time. Off: a milestone must make it available first (set that milestone's `Unlock Weapon Id`).
   - `Can Be Starting Weapon` on: it may be picked as a new game's random free weapon.
4. Tune base stats and projectile settings (`Motion` Homing needs a `Homing Turn Rate`). Keep base DPS
   (damage × fire rate × projectiles) close to the other weapons, so a random starter is never a bad roll.
5. Edit its **Upgrades** list (see [upgrade tracks](#how-to-add-an-upgrade-track)). Point each track at a shared
   cost curve (`weapon_damage`, `weapon_fire_rate`, `weapon_utility`, `weapon_special`) so it costs the same as
   the equivalent upgrade on every other weapon.
6. It appears automatically as a new entry in the Weapons tab's left-hand list (order = `Sort Order`).
   Auto mode also starts considering it as soon as it's available.

**Wave-gated weapon (the plan for weapons after wave 10, one per 10 waves):** untick `Available From Start` and
`Can Be Starting Weapon`, then create a milestone with `Condition` ReachWave, `Target Value` 10 (20, 30…) and
`Unlock Weapon Id` set to the weapon's id. Until then the weapon's tab shows LOCKED with the milestone text as the
hint. Once the milestone completes, it costs the normal escalating unlock price.
7. Validate, playtest, and add it to the weapon tables in `docs/GAME_DESIGN.md`.

## How to add an enemy
1. Duplicate an enemy in `Enemies/` with the closest behaviour and rename it `Enemy_YourName`.
2. Set a new unique `Id`, shape, color and radius. Wave-1 stats: health, contact damage, speed, reward.
3. Choose `Behaviour` (Drift / Chase / Shooter). Shooter uses the *Shooter Settings* block.
4. Optional: `Split Into Enemy Id` and `Split Count`.
5. **Make it spawn:** open `Config/WaveConfig` → *Spawn Table* → add an entry with your id, `Min Wave`, optional
   `Max Wave` (0 = forever) and `Weight`. For a boss, set `Is Boss` and point `Boss Enemy Id` at it.
6. Validate, playtest, update the enemy table in the design doc.

## How to add a milestone
1. Duplicate a milestone and name it `Milestone_NN_Name` (NN = position).
2. Unique `Id`, name, description (shown as the goal text, and as the hint on any weapon it makes available).
3. `Condition` + `Target Value`, then rewards: `Reward Credits` and/or `Unlock Weapon Id` (makes a weapon that
   isn't `Available From Start` unlockable).
4. `Sort Order` controls the order on the GOALS tab.

## How to add an upgrade track
On the ship or a weapon asset, add an element to **Upgrades**:

| Field | Meaning |
|-------|---------|
| `Id` | Unique within this ship/weapon, permanent (save key is `{ownerId}.{trackId}`). |
| `Display Name` / `Description` | Shown on the row. The current → next value line is added automatically. |
| `Stat` | Which stat it changes (table below). Weapon tracks should target weapon stats. Ship tracks should target ship stats. |
| `Modifier` | `Flat` adds `Value Per Level` × level to the base. `Percent` adds it to the multiplier (0.1 = +10%/level). |
| `Cost Curve Id` | **Weapons: always set this** to a shared curve in `EconomyConfig` (see below) so equivalent upgrades cost the same on every weapon. When set, `Base Cost`/`Cost Growth` are ignored. |
| `Base Cost` / `Cost Growth` | Used only when there's no `Cost Curve Id` (ship upgrades). Next level costs `ceil(BaseCost × CostGrowth^level)`. Growth must be ≥ 1 (typically 1.15–1.5; use 2–4 for capped "count" upgrades). |
| `Max Level` | 0 = unlimited. |

## How to tune waves and the economy
- `Config/WaveConfig`: enemy counts, spawn timing, per-wave growth of HP/damage/reward, enemy speed (starting
  multiplier, growth, cap), boss cadence, failure setback, spawn table.
- `Config/EconomyConfig`:
  - **Income:** `Kill Reward Multiplier`, one dial for all kill income (0.9 = 10% less).
  - **Weapon unlocks:** `Weapon Unlock Cost` (price of the 2nd weapon; the same for whichever weapon you pick)
    and `Weapon Unlock Cost Growth` (each further weapon costs this much more: 1.5 → 60, 90, 135…).
  - **Upgrade cost curves:** named `{id, baseCost, costGrowth}` entries shared by weapon upgrade tracks.
    Re-pricing a curve re-prices that upgrade on every weapon.
  - Starting credits, offline efficiency/cap/minimum, income smoothing window, autosave interval.

- `Config/AutoBuyConfig`: auto mode shopping interval, purchase cap per pass, and one profile per strategy
  (Attack / Balanced / Defense). Each profile has a weight per category: weapon upgrade, weapon unlock, ship
  offense (global damage/fire rate), ship defense (hull/regen/thrusters), ship economy (salvage). Auto mode buys
  the lowest cost ÷ weight and saves up when that isn't affordable, so a higher weight means bought sooner and
  0 means never auto-bought. The `description` is shown in the auto mode dialog.

See [GAME_DESIGN §7 Tuning guide](../../../../docs/GAME_DESIGN.md#7-tuning-guide).

---

## Enum reference (numbers used in the YAML)

**StatId**
| Value | Stat | Owner |
|-------|------|-------|
| 0 | MaxHull | Ship |
| 1 | HullRegen (per sec) | Ship |
| 2 | MoveSpeed | Ship |
| 3 | CreditMultiplier | Ship |
| 4 | GlobalDamageMultiplier | Ship |
| 5 | GlobalFireRateMultiplier | Ship |
| 100 | Damage | Weapon |
| 101 | FireRate (volleys/sec) | Weapon |
| 102 | ProjectileCount | Weapon |
| 103 | Pierce | Weapon |
| 104 | SplashRadius | Weapon |
| 105 | ProjectileSpeed | Weapon |
| 106 | Range | Weapon |

**ModifierKind:** 0 Flat · 1 Percent

**ShapeType:** 0 Ship · 1 Circle · 2 Diamond · 3 Square · 4 Hexagon · 5 Rock · 6 Saucer · 7 Dot · 8 Bolt · 9 Missile · 10 Spark

**EnemyBehaviourType:** 0 Drift · 1 Chase · 2 Shooter

**ProjectileMotion:** 0 Straight · 1 Homing

**MilestoneCondition:** 0 ReachWave · 1 TotalKills · 2 TotalCreditsEarned

**AutoBuyStrategy:** 0 Balanced · 1 Attack · 2 Defense

**Booleans:** 0 = false, 1 = true.

## Things that break content (avoid)
- Changing an `Id` after release orphans players' saved progress for it.
- Deleting or regenerating the `.cs.meta` of a Content script: every asset of that type loses its script.
- Reordering enums in code: every asset's numbers silently mean something else.
- Two assets with the same id: validation reports it, and only the first is used.
