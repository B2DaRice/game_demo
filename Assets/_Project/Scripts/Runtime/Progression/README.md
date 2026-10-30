# Progression (`AstroIdle.Progression`)

Layer 5: the **economy and meta-progression rules**. Everything here is plain C# with no MonoBehaviours and no
references to Gameplay or UI, so the rules are easy to read and test.

| File | Purpose |
|------|---------|
| `CurrencyWallet.cs` | The only place credits change. `Add(amount, countAsEarned)`, `CanAfford`, `TrySpend`. Tracks lifetime earned credits. |
| `UpgradeService.cs` | Upgrade levels, costs (resolving shared `CostCurve`s from `EconomyConfig`), purchases and **final stat evaluation** for ship and weapons (including global damage/fire-rate multipliers). Supports "preview +1 level" for the UI and total levels per weapon. |
| `WeaponInventory.cs` | Weapon lifecycle: available (from start, or via milestone `MakeAvailable`) → `TryUnlock` (shared price from `GetUnlockCost`) → owned. `EnsureStartingWeapons(roll)` runs every load: marks start-available weapons and, on a new game, grants one **random** starting weapon (saved as `startingWeaponId`). |
| `MilestoneService.cs` | Polled `Evaluate()`. Completes milestones whose condition ≥ target, grants credits and unlocks. |
| `AutoBuyService.cs` | Auto mode. When enabled (saved), every `AutoBuyConfig.tickIntervalSeconds` it gathers every purchasable candidate (ship tracks, owned weapons' tracks, the next weapon unlock), asks the planner for the top pick, and buys it through the normal `UpgradeService`/`WeaponInventory` calls. Stops and saves up when the top pick isn't affordable. |
| `AutoBuyPlanner.cs` | Pure auto mode rules (tested): `PickBest` = lowest cost ÷ profile weight; `CategoryFor(stat, isShip)` maps tracks to categories. |
| `AutoBuyCandidate.cs` | `{ category, cost }` value passed to the planner. |
| `RewardService.cs` | Listens for `EnemyKilled`: counts the kill, pays `reward × killRewardMultiplier × credit multiplier`, maintains the smoothed credits/sec used by the HUD and offline earnings. `IDisposable`. |
| `ProgressionMath.cs` | Pure formulas: `UpgradeCost = ceil(base · growth^level)`, `WeaponUnlockCost = ceil(base · growth^(owned−1))`, `PickIndex` (random roll → index), `EvaluateStat = (base + flat) · (1 + percent)`. |
| `OfflineEarnings.cs` | Pure formula: `floor(min(away, maxHours) · credits/sec · efficiency)`. |

The formulas and their baseline numbers are explained in [docs/GAME_DESIGN.md §4](../../../../../docs/GAME_DESIGN.md#4-formulas).

## Interactions

```
Enemy killed ──GameEvents.EnemyKilled──► RewardService ──► CurrencyWallet.Add
UI unlock button ──► WeaponInventory.TryUnlock ──► CurrencyWallet.TrySpend ──► GameEvents.WeaponPurchased ──► PlayerShip mounts it
UI buy button ──► UpgradeService.TryPurchase ──► CurrencyWallet.TrySpend ──► GameEvents.UpgradePurchased ──► PlayerShip refreshes stats
GameBootstrap (every 0.5s) ──► AutoBuyService (if ON) ──► AutoBuyPlanner.PickBest ──► UpgradeService.TryPurchase / WeaponInventory.TryUnlock
GameBootstrap (4x/sec) ──► MilestoneService.Evaluate ──► Wallet.Add / WeaponInventory.MakeAvailable ──► GameEvents.MilestoneCompleted
```

All services read and write the shared `SaveData` instance ([Persistence](../Persistence/README.md)).

## How to...

**Add a new currency** (e.g. prestige "Star Cores")
1. Add fields to `SaveData`.
2. Create a wallet for it (or generalize `CurrencyWallet` to keyed currencies). Raise a changed event.
3. Construct it in `GameBootstrap.CreateServices` and expose it on `GameContext`.
4. Add UI and update [docs/GAME_DESIGN.md](../../../../../docs/GAME_DESIGN.md).

**Add a milestone condition**: append to `MilestoneCondition`, return its current value in
`MilestoneService.GetCurrentValue`, and make sure that value is persisted in `SaveData`.

**Change how stats stack**: edit `ProgressionMath.EvaluateStat` and `UpgradeService.EvaluateStat`, update tests
and the design doc.
