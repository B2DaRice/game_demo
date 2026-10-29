# Content guide: `Resources/Content/`

All game content and balance numbers live here as ScriptableObject assets. At startup `ContentDatabase` loads
**every asset in each sub-folder**, so adding content means creating an asset in the right folder. No code, no
registration lists, no scene changes.

> Files under any `Resources/` folder ship inside the app, this README included (a few KB). Keep large non-content
> files out of here.

| Folder | Asset type (script) | Count | Assets |
|--------|---------------------|-------|--------|
| `Ship/` | `ShipDefinition` | exactly 1 | `Ship_Sentinel` |
| `Weapons/` | `WeaponDefinition` | 1+ (≥1 owned at start) | `Weapon_PulseBlaster`, `Weapon_ScatterCannon`, `Weapon_HomingMissiles` |
| `Enemies/` | `EnemyDefinition` | 1+ | `Enemy_Asteroid`, `Enemy_AsteroidShard`, `Enemy_Drone`, `Enemy_Gunship`, `Enemy_Mothership` |
| `Milestones/` | `MilestoneDefinition` | any | `Milestone_01` … `Milestone_10` |
| `Config/` | `WaveConfig`, `EconomyConfig` | exactly 1 each | `WaveConfig`, `EconomyConfig` |

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
3. Set acquisition: usually `Owned At Start` off and a `Purchase Cost`.
4. Tune base stats and projectile settings (`Motion` Homing needs a `Homing Turn Rate`).
5. Edit its **Upgrades** list (see [upgrade tracks](#how-to-add-an-upgrade-track)).
6. Make it obtainable: create a milestone whose `Unlock Weapon Id` is your weapon's id (or tick `Owned At Start`).
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
2. Unique `Id`, name, description (shown as the goal text, and as the unlock hint for weapons it unlocks).
3. `Condition` + `Target Value`, then rewards: `Reward Credits` and/or `Unlock Weapon Id`.
4. `Sort Order` controls the order on the GOALS tab.

## How to add an upgrade track
On the ship or a weapon asset, add an element to **Upgrades**:

| Field | Meaning |
|-------|---------|
| `Id` | Unique within this ship/weapon, permanent (save key is `{ownerId}.{trackId}`). |
| `Display Name` / `Description` | Shown on the row. The current → next value line is added automatically. |
| `Stat` | Which stat it changes (table below). Weapon tracks should target weapon stats. Ship tracks should target ship stats. |
| `Modifier` | `Flat` adds `Value Per Level` × level to the base. `Percent` adds it to the multiplier (0.1 = +10%/level). |
| `Base Cost` / `Cost Growth` | Next level costs `ceil(BaseCost × CostGrowth^level)`. Growth must be ≥ 1 (typically 1.15–1.5; use 2–4 for capped "count" upgrades). |
| `Max Level` | 0 = unlimited. |

## How to tune waves and the economy
`Config/WaveConfig`: enemy counts, spawn timing, per-wave growth of HP/damage/reward/speed, boss cadence, failure
setback, spawn table. `Config/EconomyConfig`: starting credits, offline efficiency/cap/minimum, income smoothing
window, autosave interval. See [GAME_DESIGN §7 Tuning guide](../../../../docs/GAME_DESIGN.md#7-tuning-guide).

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

**Booleans:** 0 = false, 1 = true.

## Things that break content (avoid)
- Changing an `Id` after release orphans players' saved progress for it.
- Deleting or regenerating the `.cs.meta` of a Content script: every asset of that type loses its script.
- Reordering enums in code: every asset's numbers silently mean something else.
- Two assets with the same id: validation reports it, and only the first is used.
