# Progression (`AstroIdle.Progression`)

Layer 5: the **economy and meta-progression rules**. Everything here is plain C# with no MonoBehaviours and no
references to Gameplay or UI, so the rules are easy to read and test.

| File | Purpose |
|------|---------|
| `CurrencyWallet.cs` | The only place credits change. `Add(amount, countAsEarned)`, `CanAfford`, `TrySpend`. Tracks lifetime earned credits. |
| `UpgradeService.cs` | Upgrade levels, costs, purchases and **final stat evaluation** for ship and weapons (including global damage/fire-rate multipliers). Supports "preview +1 level" for the UI. |
| `WeaponInventory.cs` | Weapon lifecycle: locked → `Unlock` (by milestone) → `TryPurchase` → owned. `EnsureStartingWeapons()` runs every load. |
| `MilestoneService.cs` | Polled `Evaluate()`. Completes milestones whose condition ≥ target, grants credits and unlocks. |
| `RewardService.cs` | Listens for `EnemyKilled`: counts the kill, pays `reward × credit multiplier`, maintains the smoothed credits/sec used by the HUD and offline earnings. `IDisposable`. |
| `ProgressionMath.cs` | Pure formulas: `UpgradeCost = ceil(base · growth^level)`, `EvaluateStat = (base + flat) · (1 + percent)`. |
| `OfflineEarnings.cs` | Pure formula: `floor(min(away, maxHours) · credits/sec · efficiency)`. |

The formulas and their baseline numbers are explained in [docs/GAME_DESIGN.md §4](../../../../../docs/GAME_DESIGN.md#4-formulas).

## Interactions

```
Enemy killed ──GameEvents.EnemyKilled──► RewardService ──► CurrencyWallet.Add
UI buy button ──► UpgradeService.TryPurchase ──► CurrencyWallet.TrySpend ──► GameEvents.UpgradePurchased ──► PlayerShip refreshes stats
GameBootstrap (4x/sec) ──► MilestoneService.Evaluate ──► Wallet.Add / WeaponInventory.Unlock ──► GameEvents.MilestoneCompleted
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
