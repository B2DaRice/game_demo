# Events (`AstroIdle.Events`)

Layer 3: the global **event hub** that lets systems announce what happened without knowing who is listening.

| File | Purpose |
|------|---------|
| `GameEvents.cs` | Static C# events plus `Raise…` methods. `ClearAll()` runs automatically before each play session. |
| `EnemyKilledInfo.cs` | Payload for `EnemyKilled`: definition, position, wave-scaled reward (before the credit multiplier). |

## Event catalogue

| Event | Raised by | Typical listeners |
|-------|-----------|-------------------|
| `EnemyKilled(info)` | `Enemy.Explode(true)` (player kills only) | `RewardService` (credits, kill count) |
| `EnemySpawned()` | `EnemySpawner.Spawn` | `WaveDirector` (progress) |
| `EnemyRemoved()` | `Enemy.Despawn` (any reason) | `WaveDirector` (progress) |
| `WaveStarted(wave)` / `WaveCompleted(wave)` | `WaveDirector` | (free for UI/audio) |
| `WaveFailed(retreatWave)` | `WaveDirector.HandleFail` | `ToastView` |
| `PlayerDestroyed()` | `PlayerShip.TakeDamage` | `WaveDirector` (flags a fail for next frame) |
| `AutoAdvanceChanged(on)` | `WaveDirector.SetAutoAdvance` | (free) |
| `CreditsChanged(total)` | `CurrencyWallet` | (free; UI polls instead) |
| `UpgradePurchased(key)` | `UpgradeService.TryPurchase` | `PlayerShip` (refresh stats) |
| `WeaponUnlocked(def)` | `WeaponInventory.Unlock` | `ToastView`, `BottomPanelView` |
| `WeaponPurchased(def)` | `WeaponInventory` | `PlayerShip` (mount weapon), `BottomPanelView` |
| `MilestoneCompleted(def)` | `MilestoneService` | `ToastView`, `BottomPanelView` |
| `ToastRequested(text)` | anyone | `ToastView` |

## Rules

- Raise only through the `Raise…` methods.
- **Every subscriber unsubscribes** in `OnDestroy` (MonoBehaviours) or `Dispose` (plain classes).
- Don't rely on handler order. If ordering matters, use a direct call or polling (see `MilestoneService`).
- Don't raise per-frame events.

## How to add an event

1. Add `public static event Action<T> Name;` and `public static void RaiseName(T arg) => Name?.Invoke(arg);`.
2. Add `Name = null;` to `ClearAll()`. **Easy to forget.**
3. If the payload has several values, add a `readonly struct NameInfo` file here.
4. Add a row to the catalogue above.
