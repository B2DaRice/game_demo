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
  kills → credits ──► spend on upgrades / weapons ──► stronger ship
        │                                                    ▲
        ▼                                                    │
  milestones reached ──► new weapons unlocked for purchase ──┘

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
- New weapons are **unlocked by milestones**, then **bought** with credits.

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
- Rewards: credits and/or **unlocking a weapon** for purchase.
- Shown on the **GOALS** tab with live progress. Completing one shows a toast.
- Milestones are the player-facing "level up" system for this pass. Wave number is the main measure of progress.

### 3.9 Offline earnings
- On return after ≥ 2 minutes away: `credits/sec × time away × 25%`, capped at 8 hours, collected via a popup.
- Uses the smoothed live credits/sec, so farming a good wave before closing the app pays off.

## 4. Formulas

Code: `Gameplay/Waves/WaveMath.cs`, `Progression/ProgressionMath.cs`, `Progression/OfflineEarnings.cs`.
Knobs: `Resources/Content/Config/WaveConfig.asset`, `EconomyConfig.asset`.

| Quantity | Formula | Baseline knobs |
|----------|---------|----------------|
| Enemies in wave *w* | `min(base + perWave·(w−1), max)` (+1 boss on boss waves) | 8, +2, max 80 |
| Spawn interval | `max(min, base · mult^(w−1))` | 1.1s, ×0.96, min 0.3s |
| Enemy health | `baseHP · 1.15^(w−1)` | `healthGrowthPerWave` |
| Enemy damage | `baseDmg · 1.07^(w−1)` | `damageGrowthPerWave` |
| Enemy reward | `baseReward · 1.13^(w−1)` | `rewardGrowthPerWave` |
| Enemy speed | `baseSpeed · min(1 + 0.015·(w−1), 1.6)` | `speedGrowthPerWave`, `maxSpeedMultiplier` |
| Upgrade cost (next level) | `ceil(baseCost · growth^level)` | per track |
| Stat value | `(base + Σflat) · (1 + Σpercent)` | per track |
| Weapon damage / fire rate | weapon stat × ship global multiplier | *Overcharge*, *Fire Control* |
| Kill reward paid | `enemy reward × ship credit multiplier` | *Salvage Drones* |
| Offline earnings | `floor(min(away, 8h) · credits/sec · 0.25)` | `EconomyConfig` |

**Why these shapes?** Enemy HP grows faster (1.15) than rewards (1.13), so income alone can't keep up and the
player must keep upgrading. Upgrade costs grow geometrically while most upgrade effects grow linearly, which makes
each extra level a bit less efficient (diminishing returns). That eventually creates the walls that make
progression feel earned. These are the standard idle-game curves.

### Scaling at a glance

| Wave | Enemies | Spawn interval | HP × | Dmg × | Reward × | Asteroid HP | Boss HP |
|------|---------|----------------|------|-------|----------|-------------|---------|
| 1 | 8 | 1.10s | 1.00 | 1.00 | 1.00 | 20 | n/a |
| 5 | 16 | 0.93s | 1.75 | 1.31 | 1.63 | 35 | n/a |
| 10 | 26 + boss | 0.76s | 3.52 | 1.84 | 3.00 | 70 | 2,111 |
| 15 | 36 | 0.62s | 7.08 | 2.58 | 5.53 | 142 | n/a |
| 20 | 46 + boss | 0.51s | 14.2 | 3.62 | 10.2 | 285 | 8,538 |

## 5. Baseline content & numbers

### Ship: *Sentinel*
Hull 100 · Regen 1/s · Speed 3 · Collision radius 0.35 · 2s invulnerability after respawn.

| Upgrade | Effect / level | Base cost | Growth | Max |
|---------|----------------|-----------|--------|-----|
| Reinforced Hull | Max hull +15% | 20 | 1.22 | n/a |
| Nano Repair | Regen +0.5/s | 30 | 1.25 | n/a |
| Thrusters | Move speed +5% | 40 | 1.30 | 20 |
| Salvage Drones | Credits +10% | 75 | 1.35 | n/a |
| Fire Control | All fire rate +4% | 100 | 1.40 | 25 |
| Overcharge | All damage +5% | 150 | 1.45 | n/a |

### Weapons
| Weapon | Unlock | Cost | Dmg | Rate/s | Proj. | Spread | Speed | Range | Special |
|--------|--------|------|-----|--------|-------|--------|-------|-------|---------|
| Pulse Blaster | start | n/a | 10 | 2.0 | 1 | n/a | 14 | 7 | Leading shots |
| Scatter Cannon | reach wave 5 | 300 | 6 | 0.9 | 5 | 40° | 11 | 5 | Short range shotgun |
| Seeker Missiles | reach wave 10 | 1,500 | 30 | 0.5 | 2 | 30° | 7 | 9 | Homing, 1.2 splash |

| Weapon upgrades | Effect / level | Base cost | Growth | Max |
|-----------------|----------------|-----------|--------|-----|
| Blaster: Bolt Damage / Capacitors / Phase Bolts | +20% dmg / +8% rate / +1 pierce | 10 / 25 / 500 | 1.2 / 1.28 / 4 | n/a / 30 / 3 |
| Scatter: Heavy Shot / Auto Loader / Wide Choke | +20% dmg / +8% rate / +1 pellet | 60 / 120 / 400 | 1.22 / 1.3 / 2.2 | n/a / 30 / 6 |
| Missiles: Warheads / Rapid Racks / Shaped Charge / Extra Tubes | +25% dmg / +8% rate / +10% blast / +1 missile | 400 / 600 / 800 / 3000 | 1.25 / 1.32 / 1.5 / 3 | n/a / 25 / 10 / 3 |

### Enemies (wave-1 values)
| Enemy | Behaviour | First wave | Spawn weight | HP | Contact dmg | Speed | Reward | Notes |
|-------|-----------|-----------|--------------|----|-------------|-------|--------|-------|
| Asteroid | Drift | 1 | 10 | 20 | 10 | 1.1 | 3 | Splits into 2 shards |
| Asteroid Shard | Drift | (split only) | n/a | 7 | 5 | 1.8 | 1 | n/a |
| Drone | Chase | 3 | 6 | 12 | 8 | 2.0 | 4 | Rams the ship |
| Gunship | Shooter | 6 | 3 | 45 | 12 | 1.4 | 12 | 6 dmg shot every 2.2s at 4.5 range |
| Mothership | Shooter (boss) | every 10th | n/a | 600 | 25 | 0.8 | 250 | 5-shot 60° volley every 1s |

### Milestones
| Goal | Condition | Reward |
|------|-----------|--------|
| First Contact | 10 kills | 25 |
| Holding the Line | reach wave 3 | 50 |
| Scatter License | reach wave 5 | unlock Scatter Cannon |
| Rock Breaker | 150 kills | 300 |
| Deep Patrol | reach wave 8 | 500 |
| Seeker License | reach wave 10 | unlock Seeker Missiles |
| Mothership Down | reach wave 11 | 2,000 |
| Scrap Baron | earn 10,000 total | 1,000 |
| Ace Pilot | 1,000 kills | 5,000 |
| Sector Veteran | reach wave 15 | 7,500 |

## 6. Expected pacing (first ~5 minutes)

Rough estimate assuming no deaths (average root-enemy value ≈ 5 credits early, ≈ 5.8 once gunships appear):

| Wave | Approx. credits from wave | Cumulative | What the player is doing |
|------|---------------------------|------------|--------------------------|
| 1 | ~40 | 40 | First Bolt Damage / Hull levels |
| 3 | ~70 | ~165 | Drones appear and start ramming; hull/regen matter |
| 5 | ~120 | ~380 | Scatter Cannon unlocks (300) |
| 8 | ~300 | ~1,100 | Gunships shooting back; probably the first deaths and farming |
| 10 | ~450 + 750 boss | ~2,700 | Mothership boss wall; Seeker Missiles unlock (1,500) |

A wave lasts about 15s early and about 25s at wave 9. The boss adds about 20s. That's about 4–5 minutes to reach
wave 10 without deaths, longer with them, which matches the target for this pass. **Playtest questions to answer:**
Is the first minute too slow? Is the wave-8/10 wall too hard or too soft? Does the ship die to things the player
can't understand?

## 7. Tuning guide

| If playtesting shows... | Try adjusting |
|-------------------------|---------------|
| Early game too slow | Lower first-level costs (`baseCost`) or raise Asteroid `creditReward`. |
| Players hit walls too early | Lower `healthGrowthPerWave` (1.15 → 1.13) or raise `rewardGrowthPerWave`. |
| Upgrades feel pointless | Raise `valuePerLevel` on that track or lower its `costGrowth`. |
| Too easy to max everything | Raise `costGrowth` (1.2 → 1.25+). |
| Ship dies too often / unfairly | Raise base hull/regen, lower `damageGrowthPerWave`, or tune `ShipAutopilot` weights. |
| Waves feel too long | Lower `enemiesPerWave` or `baseSpawnInterval`. |
| Screen too cluttered | Lower `maxConcurrentEnemies`. |
| Offline earnings too generous / stingy | `offlineEfficiency`, `maxOfflineHours`. |

Workflow: edit numbers in the Inspector (while **not** in Play mode), press Play, and use the DEV tab speed-up
(x4/x8) and credit grants to reach the part of the game you're tuning. Then update the tables above.

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
3. **Prestige system** (Star Cores + permanent upgrade tree). Needs a second currency in `SaveData` and
   `CurrencyWallet`.
4. **Sectors:** every 25 waves change background color, spawn table and boss.
5. **Audio:** shot, explosion, purchase and milestone sounds plus music, with a settings toggle.
6. **Juice:** floating "+credits" numbers, screen shake on big hits, boss health bar.
7. **More enemy AIs:** splitter swarms, shielded tanks, kamikaze chargers. Add a behaviour plus enum value.
8. **More weapons:** laser beam (continuous), orbiting drones, chain lightning.
9. **Daily rewards / ad-based x2 offline earnings** (if monetization is ever wanted).
10. **Cloud save** (Google Play Games) once the save format stabilizes.
