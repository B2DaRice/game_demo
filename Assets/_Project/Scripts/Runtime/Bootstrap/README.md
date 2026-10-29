# Bootstrap (`AstroIdle.Bootstrap`)

Layer 9 (top): the **composition root**. `GameBootstrap` is the only component placed in the scene. It builds the
entire game in code and runs the app-level loop.

| File | Purpose |
|------|---------|
| `GameBootstrap.cs` | Startup, app loop (income tick, milestone polling, autosave, screen-change relayout), app pause/quit saving, offline earnings, progress reset. |

## Startup order (`Awake` → `Start`)

1. **App settings:** `targetFrameRate = 60`, screen never sleeps, `timeScale = 1`.
2. **Content:** `ContentDatabase.LoadFromResources()` then `Validate()` (errors logged). If the ship, wave config
   or economy config is missing, startup stops with an error.
3. **Services** (`CreateServices`): load the save → `CurrencyWallet` → `WeaponInventory.EnsureStartingWeapons()` →
   starting credits for a new game → `UpgradeService` → `MilestoneService` → `RewardService` → `GameContext`.
4. **World** (`BuildWorld`): camera (`ArenaCamera.Configure`) → `PlayArea` → spawners and effects →
   `GameplayContext` → starfield → `PlayerShip` (registered in `CombatRegistry` *before* `Initialize`) →
   `WaveDirector`.
5. **UI:** `UIRoot.Build(context)`.
6. **`Start`:** `WaveDirector.Begin()` (resumes the saved wave), then offline earnings check.

Order matters: services before world (the ship needs upgrade stats), world before UI (the HUD reads the player and
wave director), and waves begin only after everything exists.

## App loop (`Update`)
- `RewardService.Tick` (credits/sec smoothing, game time).
- `MilestoneService.Evaluate` every 0.25s (real time).
- Autosave every `EconomyConfig.autosaveIntervalSeconds` (real time).
- If the screen size or safe area changed: reconfigure camera, play area, starfield and UI layout.

## Lifecycle hooks
| Hook | Action |
|------|--------|
| `OnApplicationPause(true)` | Save (Android backgrounding). |
| `OnApplicationPause(false)` | Offline earnings check (app resumed after a while). |
| `OnApplicationQuit` | Save. |
| `OnDestroy` | Dispose `RewardService` (event unsubscribe). |

## Offline earnings
Away time = now − `SaveData.lastSaveUtcTicks`. If it's ≥ `minOfflineSeconds`, compute via `OfflineEarnings`, mark the
time consumed, and show the popup. Credits are granted on **Collect**. Clock rollback is ignored.

## Reset progress (DEV tab)
Disables saving, deletes the save file, resets time scale, reloads the active scene. Every object unsubscribes from
`GameEvents` in `OnDestroy`, so the reloaded scene starts clean.

## How to...
**Add a new service:** construct it in `CreateServices` (respecting dependency order), add a property to
[`GameContext`](../Core/README.md), and dispose it in `OnDestroy` if it subscribes to events.
