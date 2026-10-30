# Game Design: Astro Idle

This is the **living design doc**. When a rule, formula or baseline number changes in code or content, update it here
in the same change. All numbers are a **first-pass baseline** meant to be tuned through playtesting.

- [1. Pitch](#1-pitch)
- [2. Core loop](#2-core-loop)
- [3. Systems](#3-systems)
- [4. Formulas](#4-formulas)
- [5. Baseline content & numbers](#5-baseline-content--numbers)
- [6. Expected pacing (first ~5 minutes)](#6-expected-pacing-first-5-minutes)
- [7. Tuning guide](#7-tuning-guide)
- [8. What is the "end game" of an idle game?](#8-what-is-the-end-game-of-an-idle-game)
- [9. Roadmap / backlog](#9-roadmap--backlog)

---

## 1. Pitch

*Asteroids* meets idle tower defense. Your ship pilots itself, dodging and auto-firing at waves of space debris and
hostile drones. You don't aim. You decide **what to upgrade** and **when to push forward**. Kills pay credits, credits
buy power, and power pushes you to harder waves that pay more. The game keeps earning (at reduced efficiency) while
the phone is in your pocket.

## 2. Core loop

```
        ┌──────────── fight wave (automatic) ◄──────────────┐
        │                                                    │
        ▼                                                    │
  kills → credits ──► spend: unlock more weapons (broad) ──► stronger ship
        │             or upgrade a few weapons (deep)            ▲
        ▼                                                        │
  milestones reached ──► bonus credits ──────────────────────────┘

  ship destroyed ──► fall back one wave + "farming" mode ──► upgrade ──► tap ADVANCE
```

**Session loop:** open the app, collect offline earnings, spend, watch a few waves, spend again, close.
**Wall loop:** progress stalls at a hard wave → farm the previous wave → upgrade → beat the wall. This is the
"number goes up" rhythm the genre lives on.

## 3. Systems

### 3.1 Play field & ship autopilot
- Portrait screen. The top 60% of the safe area is the play field and the bottom 40% is the upgrade panel. The HUD
  overlays the top of the play field.
- The ship steers itself using weighted forces: **avoid** nearby enemies (bosses count double), **pull** toward
  the center, **push** off walls, plus slow random **wander**. It faces the nearest enemy.
- Tuning lives in constants at the top of `ShipAutopilot.cs`.

### 3.2 Weapons
- Every **owned** weapon fires automatically and independently at the **nearest enemy within its range**.
- Straight projectiles lead their target. Homing projectiles steer toward their target (or re-acquire one).
- Stats per weapon: damage, fire rate (volleys/sec), projectiles per volley, spread angle, projectile speed, range,
  pierce, splash radius.
- **Random starter:** a new game starts with one weapon picked at random (from weapons marked *Can Be Starting
  Weapon*), so runs don't all open the same way. The Weapons tab opens on that weapon.
- **Broad vs. deep:** every other weapon can be **unlocked from the start**. The price depends only on how many
  weapons you own, not on which weapon: the 2nd weapon costs **60** (about the first four damage upgrades on your
  starter: 10 + 13 + 16 + 20), the 3rd costs **90**, the 4th **135** (×1.5 each). The core choice is: put credits
  into one weapon's upgrades, or unlock more weapons and upgrade each a little. Target: a player can own all three
  current weapons around **wave 7–8**, depending on how they spend.
- **Same price per level everywhere:** equivalent upgrades cost the same on every weapon. All weapon tracks use
  shared **cost curves** from `EconomyConfig` (damage, fire rate, utility, special), so one edit re-prices every
  weapon at once.
- **Planned: one new weapon every 10 waves after wave 10.** Each will be held back behind a "Reach wave 10/20/30…"
  milestone (untick *Available From Start* and point that milestone's *Unlock Weapon Id* at it). When the milestone
  completes, the weapon shows up for the same escalating unlock price. No such weapons exist yet. See
  [§9](#9-roadmap--backlog) and the [content guide](../Assets/_Project/Resources/Content/README.md#how-to-add-a-weapon).

### 3.3 Enemies
| Behaviour | Description | Used by |
|-----------|-------------|---------|
| **Drift** | Flies straight through the arena and wraps around the screen edges (Asteroids). | Asteroid, Shard |
| **Chase** | Accelerates toward the ship and rams it. | Drone |
| **Shooter** | Holds a preferred range, circles the ship and fires volleys. | Gunship, Mothership (boss) |

- **Split:** an enemy can split into others when the *player* kills it (Asteroid → 2 Shards).
- **Ramming:** a regular enemy that touches the ship deals contact damage and is destroyed **without** paying
  credits. Bosses survive contact and hit again every 1s.

### 3.4 Waves
- Each wave spawns a set number of enemies from the edges, chosen from a **weighted spawn table** filtered by wave.
- A wave is **cleared** when everything has spawned and nothing is alive. Then a short intermission.
- Every **10th wave** adds a **boss** as the last spawn.
- Enemy health, damage, reward and speed scale up each wave ([formulas](#4-formulas)).

### 3.5 Failure & farming
- Hull reaches 0 → the field is cleared, the player **falls back 1 wave**, and **Auto-advance turns OFF**.
- With Auto-advance OFF the current wave **repeats** ("farming") so the player can earn and upgrade.
- Tapping **ADVANCE** in the HUD turns Auto-advance back on, and the next clear moves forward.
- There is no game over. Failure just costs time, which is standard for idle games.

### 3.6 Economy
- **Credits** are the only currency (for now). Earned from kills (× the ship's *credit multiplier*), milestone
  rewards and offline earnings.
- The HUD shows a smoothed **credits/sec** (exponential moving average over ~60s of play).

### 3.7 Upgrades
- Each ship and weapon owns **upgrade tracks** (e.g. *Bolt Damage*, *Reinforced Hull*).
- Each level adds a **Flat** amount or a **Percent** bonus to one stat. Cost grows geometrically per level.
- Ship tracks include **global** multipliers (*Overcharge* = all weapon damage, *Fire Control* = all fire rate).
- Some tracks have a max level (typically "count" upgrades like pierce or extra projectiles).

### 3.8 Milestones (progression tracking)
- One-time goals: *reach wave N*, *destroy N enemies*, *earn N credits in total*.
- Rewards: credits and/or making a gated weapon available to unlock (no weapons are gated right now).
- Shown on the **GOALS** tab with live progress. Completing one shows a toast.
- Milestones are the player-facing "level up" system for this pass. Wave number is the main measure of progress.

### 3.9 Offline earnings
- On return after ≥ 2 minutes away: `credits/sec × time away × 25%`, capped at 8 hours, collected via a popup.
- Uses the smoothed live credits/sec, so farming a good wave before closing the app pays off.

### 3.10 Auto mode
- A small **AUTO** button sits at the bottom-right of the play field. Tapping it opens a dialog with an **ON/OFF**
  switch and a priority: **Attack**, **Balanced** or **Defense**. Settings are saved.
- When ON, auto mode shops every 0.5s with the same buy calls the UI uses. The player can still buy manually at
  any time.
- **Rule:** every purchasable thing is a candidate in one of five categories (weapon upgrade, weapon unlock, ship
  offense, ship defense, ship economy). Auto mode picks the candidate with the lowest **cost ÷ category weight**.
  If that one isn't affordable yet, it **saves up** for it instead of buying something less important. Each
  purchase raises that track's next price, so spending spreads across categories roughly in proportion to their
  weights.
- Ship tracks map to categories by stat: hull/regen/thrusters → defense, Salvage Drones → economy,
  Overcharge/Fire Control → offense.

| Strategy | Weapon upgrades | Weapon unlocks | Ship offense | Ship defense | Ship economy | ≈ share spent on the ship* |
|----------|-----------------|----------------|--------------|--------------|--------------|-----------------------------|
| Attack | 1.0 | 1.6 | 0.8 | 0.3 | 0.5 | ~25% |
| Balanced (default) | 1.0 | 1.4 | 0.6 | 0.6 | 0.6 | ~33% |
| Defense | 0.7 | 1.0 | 0.4 | 1.0 | 0.5 | ~45% |

\*From the economy simulation over 15 minutes. Weights live in `Resources/Content/Config/AutoBuyConfig.asset`.

## 4. Formulas

Code: `Gameplay/Waves/WaveMath.cs`, `Progression/ProgressionMath.cs`, `Progression/OfflineEarnings.cs`.
Knobs: `Resources/Content/Config/WaveConfig.asset`, `EconomyConfig.asset`.

| Quantity | Formula | Baseline knobs |
|----------|---------|----------------|
| Enemies in wave *w* | `min(base + perWave·(w−1), max)` (+1 boss on boss waves) | 6, +2, max 80 |
| Spawn interval | `max(min, base · mult^(w−1))` | 1.4s, ×0.96, min 0.35s |
| Enemy health | `baseHP · 1.17^(w−1)` | `healthGrowthPerWave` |
| Enemy damage | `baseDmg · 1.07^(w−1)` | `damageGrowthPerWave` |
| Enemy reward | `baseReward · 1.135^(w−1)` | `rewardGrowthPerWave` |
| Kill reward paid | `enemy reward × 0.9 × ship credit multiplier` | `EconomyConfig.killRewardMultiplier`, *Salvage Drones* |
| Enemy speed | `baseSpeed · min(0.7 + 0.02·(w−1), 1.6)` | `startingSpeedMultiplier`, `speedGrowthPerWave`, `maxSpeedMultiplier` |
| Weapon unlock price | `ceil(60 · 1.5^(weaponsOwned−1))`: 60, 90, 135, 203… | `weaponUnlockCost`, `weaponUnlockCostGrowth` |
| Upgrade cost (next level) | `ceil(baseCost · growth^level)`, from the track's shared cost curve if it has one | `upgradeCostCurves` / per track |
| Stat value | `(base + Σflat) · (1 + Σpercent)` | per track |
| Weapon damage / fire rate | weapon stat × ship global multiplier | *Overcharge*, *Fire Control* |
| Offline earnings | `floor(min(away, 8h) · credits/sec · 0.25)` | `EconomyConfig` |

**Why these shapes?** Enemy HP grows faster (1.17) than rewards (1.135), so income alone can't keep up and the
player must keep upgrading. Upgrade costs grow geometrically while most upgrade effects grow linearly, which makes
each extra level a bit less efficient (diminishing returns). That eventually creates the walls that make
progression feel earned. These are the standard idle-game curves.

**Why HP went up for the multi-weapon model:** owning 2–3 weapons early roughly doubles or triples early damage
output compared with the old one-weapon start. Base enemy HP was raised 1.5× and HP growth from 1.15 to 1.17 to
absorb that.

**Second tuning pass (income vs. spending):** credits were piling up faster than they could be spent. Kill income
dropped about 10% (`killRewardMultiplier` 0.9, reward growth 1.14 → 1.135), upgrade cost growth went up (weapon
damage 1.2 → 1.25, fire rate 1.25 → 1.3, ship tracks +0.03), and weapon unlocks now escalate (60 × 1.5 per
weapon owned) instead of a flat 15.

### Scaling at a glance

| Wave | Enemies | Spawn interval | HP × | Dmg × | Reward × | Speed × | Asteroid HP | Boss HP |
|------|---------|----------------|------|-------|----------|---------|-------------|---------|
| 1 | 6 | 1.40s | 1.00 | 1.00 | 1.00 | 0.70 | 30 | n/a |
| 5 | 14 | 1.19s | 1.87 | 1.31 | 1.66 | 0.78 | 56 | n/a |
| 10 | 24 + boss | 0.97s | 4.11 | 1.84 | 3.13 | 0.88 | 123 | 3,698 |
| 15 | 34 | 0.79s | 9.01 | 2.58 | 5.89 | 0.98 | 270 | n/a |
| 20 | 44 + boss | 0.64s | 19.7 | 3.62 | 11.1 | 1.08 | 592 | 17,774 |

## 5. Baseline content & numbers

### Ship: *Sentinel*
Hull 100 · Regen 1/s · Speed 3 · Collision radius 0.35 · 2s invulnerability after respawn.

| Upgrade | Effect / level | Base cost | Growth | Max |
|---------|----------------|-----------|--------|-----|
| Reinforced Hull | Max hull +15% | 20 | 1.25 | n/a |
| Nano Repair | Regen +0.5/s | 30 | 1.28 | n/a |
| Thrusters | Move speed +5% | 40 | 1.33 | 20 |
| Salvage Drones | Credits +10% | 75 | 1.38 | n/a |
| Fire Control | All fire rate +4% | 100 | 1.43 | 25 |
| Overcharge | All damage +5% | 150 | 1.48 | n/a |

### Weapons
Every weapon can be the random starter. The others unlock for **60, then 90** (×1.5 per weapon owned). Base damage output is
roughly matched (about 20 DPS each before accuracy and splash), so no starter is a bad roll.

| Weapon | Dmg | Rate/s | Proj. | Spread | Speed | Range | Base DPS | Style |
|--------|-----|--------|-------|--------|-------|-------|----------|-------|
| Pulse Blaster | 10 | 2.0 | 1 | n/a | 14 | 7 | 20 | Long range, leads targets, reliable |
| Scatter Cannon | 5 | 0.9 | 5 | 40° | 11 | 5 | 22.5 (if all pellets hit) | Short range shotgun, great vs swarms |
| Seeker Missiles | 20 | 0.5 | 2 | 30° | 7 | 9 | 20 + 1.2 splash | Homing, area damage, slow |

### Weapon upgrade cost curves (shared by every weapon, `EconomyConfig`)
| Curve | Used by | Base cost | Growth | Costs for levels 1–5 |
|-------|---------|-----------|--------|----------------------|
| `weapon_damage` | Damage tracks (+20%/level, all weapons) | 10 | 1.25 | 10, 13, 16, 20, 25 |
| `weapon_fire_rate` | Fire-rate tracks (+8%/level, max 30) | 20 | 1.30 | 20, 26, 34, 44, 58 |
| `weapon_utility` | Blast radius (+10%/level, max 10) | 60 | 1.45 | 60, 87, 127, 183, 266 |
| `weapon_special` | "+1 count" tracks: pierce (max 3), pellets (max 4), missiles (max 3) | 250 | 3.00 | 250, 750, 2,250 |

| Weapon | Tracks |
|--------|--------|
| Pulse Blaster | Bolt Damage, Capacitors (fire rate), Phase Bolts (+1 pierce) |
| Scatter Cannon | Heavy Shot, Auto Loader (fire rate), Wide Choke (+1 pellet) |
| Seeker Missiles | Warheads, Rapid Racks (fire rate), Shaped Charge (blast radius), Extra Tubes (+1 missile) |

### Enemies (wave-1 values)
| Enemy | Behaviour | First wave | Spawn weight | HP | Contact dmg | Speed | Reward | Notes |
|-------|-----------|-----------|--------------|----|-------------|-------|--------|-------|
| Asteroid | Drift | 1 | 10 | 30 | 10 | 1.1 | 3 | Splits into 2 shards |
| Asteroid Shard | Drift | (split only) | n/a | 10 | 5 | 1.8 | 1 | n/a |
| Drone | Chase | 3 | 6 | 18 | 8 | 2.0 | 4 | Rams the ship |
| Gunship | Shooter | 6 | 3 | 65 | 12 | 1.4 | 12 | 6 dmg shot every 2.2s at 4.5 range |
| Mothership | Shooter (boss) | every 10th | n/a | 900 | 25 | 0.8 | 250 | 5-shot 60° volley every 1s |

Speeds shown are base values. Wave 1 runs at 0.7× of them (see the scaling table).

### Milestones
| Goal | Condition | Reward |
|------|-----------|--------|
| First Contact | 10 kills | 25 |
| Holding the Line | reach wave 3 | 50 |
| Picking Up Speed | reach wave 5 | 150 |
| Rock Breaker | 150 kills | 300 |
| Deep Patrol | reach wave 8 | 500 |
| Mothership Sighted | reach wave 10 | 400 |
| Mothership Down | reach wave 11 | 2,000 |
| Scrap Baron | earn 10,000 total | 1,000 |
| Ace Pilot | 1,000 kills | 5,000 |
| Sector Veteran | reach wave 15 | 7,500 |

## 6. Expected pacing (first ~5–10 minutes)

These estimates come from a simple economy simulation: no deaths, with either a hands-on player who buys the best
damage per credit or auto mode's rule. Treat them as a starting point for playtesting, not a promise.

| Milestone | Hands-on player | Auto mode (Attack / Balanced / Defense) |
|-----------|-----------------|------------------------------------------|
| 2nd weapon (60) | wave ~4 | wave ~5–6 / ~6 / ~6 (earlier with a Scatter starter) |
| 3rd weapon (90) | wave ~6–9 | wave ~8–9 / ~8–10 / ~9–10 |
| Wave reached at 3 min | ~8 | ~7 / ~7 / ~5 |
| Wave reached at 5 min | ~10 (first boss) | ~10 / ~9 / ~7 |

Defense looks slow here because the simulation doesn't model dying. In real play, the extra hull and regen should
mean fewer deaths and less farming. **Playtest questions:** Does the 60-credit 2nd weapon feel like a meaningful
"save up" moment? Is the 3rd weapon landing around wave 7–8? Does credit income still outpace spending? Does any
auto mode strategy feel clearly wrong?

## 7. Tuning guide

All of these are asset fields: no code changes needed.

| If playtesting shows... | Try adjusting |
|-------------------------|---------------|
| Credits still pile up / run dry | `EconomyConfig.killRewardMultiplier` (0.9 → 0.8 / 1.0): one dial for all kill income. |
| Opening still too hectic | `WaveConfig`: lower `baseEnemyCount`, raise `baseSpawnInterval`, lower `startingSpeedMultiplier` (0.7 → 0.6). |
| Opening too slow / boring | Reverse the above, or raise `killRewardMultiplier`. |
| Game speeds up too fast later | Lower `speedGrowthPerWave` or `maxSpeedMultiplier`. Raise `spawnIntervalMultiplierPerWave` toward 1. |
| 2nd weapon comes too early / late | `EconomyConfig.weaponUnlockCost` (60). |
| 3rd+ weapons come too early / late | `EconomyConfig.weaponUnlockCostGrowth` (1.5). |
| Upgrades come too fast / slow | Cost curve `costGrowth` in `EconomyConfig` (weapons) or per ship track. |
| Nobody bothers upgrading deep | Lower `weapon_damage` / `weapon_fire_rate` curve growth, or raise `valuePerLevel` on those tracks. |
| One weapon is clearly best | Adjust that weapon's base stats (keep base DPS roughly equal across weapons). |
| Auto mode neglects / overspends on something | `AutoBuyConfig` profile weights (higher = bought sooner). Weight 0 = never auto-buy that category. |
| Players hit walls too early | Lower `healthGrowthPerWave` (1.17 → 1.15) or raise `rewardGrowthPerWave`. |
| Ship dies too often / unfairly | Raise base hull/regen, lower `damageGrowthPerWave`, or tune `ShipAutopilot` weights. |
| Waves feel too long | Lower `enemiesPerWave` or `baseSpawnInterval`. |
| Screen too cluttered | Lower `maxConcurrentEnemies`. |
| Offline earnings too generous / stingy | `offlineEfficiency`, `maxOfflineHours`. |

Workflow: edit numbers in the Inspector (while **not** in Play mode), press Play, and use the DEV tab speed-up
(x4/x8) and credit grants to reach the part of the game you're tuning. Then update the tables above. Use DEV →
*Reset ALL progress* to try a fresh run (and a new random starting weapon). Auto mode at x8 speed is a quick way to
see how a full strategy plays out.

## 8. What is the "end game" of an idle game?

Most idle games don't end. They keep players engaged with layered **reset loops**:

1. **Prestige / rebirth:** at some point the player can voluntarily reset waves, credits and upgrades in exchange
   for a permanent meta-currency (e.g. *Star Cores*), based on how far they got. Cores buy permanent multipliers.
   The second run goes much faster and further. This is the backbone of almost every successful idle game and is
   the natural next step for Astro Idle.
2. **Content gates:** new sectors (biomes), enemy families and weapons at higher waves give long-term goals.
3. **Soft caps:** after a very high wave, growth deliberately slows so players turn to prestige.
4. **Collections / achievements:** completion goals that don't block progress.

A reasonable "complete" state for this game: a finite set of sectors, each ending in a unique boss, with prestige
tiers. A player who has beaten the last sector at max prestige has "finished", but can still push numbers.

## 9. Roadmap / backlog

Ordered by suggested priority after the first playtest:

1. **Balance pass** based on playtest feedback (this doc's tables).
2. **Buy multipliers** (x1 / x10 / Max) on upgrade buttons, a standard idle quality-of-life feature.
3. **New weapon every 10 waves (after wave 10):** add a `WeaponDefinition` with *Available From Start* off, plus a
   "Reach wave 10/20/30…" milestone whose *Unlock Weapon Id* points at it. No code is needed. The unlock price
   keeps escalating (60 × 1.5^(owned−1)), so check that `weaponUnlockCostGrowth` still fits once there are 5+ weapons.
4. **Prestige system** (Star Cores + permanent upgrade tree). Needs a second currency in `SaveData` and
   `CurrencyWallet`.
5. **Sectors:** every 25 waves change background color, spawn table and boss.
6. **Audio:** shot, explosion, purchase and milestone sounds plus music, with a settings toggle.
7. **Juice:** floating "+credits" numbers, screen shake on big hits, boss health bar.
8. **More enemy AIs:** splitter swarms, shielded tanks, kamikaze chargers. Add a behaviour plus enum value.
9. **More weapon types:** laser beam (continuous), orbiting drones, chain lightning.
10. **Daily rewards / ad-based x2 offline earnings** (if monetization is ever wanted).
11. **Cloud save** (Google Play Games) once the save format stabilizes.
