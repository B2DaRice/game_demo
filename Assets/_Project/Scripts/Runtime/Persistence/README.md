# Persistence (`AstroIdle.Persistence`)

Layer 4: what is saved and how.

| File | Purpose |
|------|---------|
| `SaveData.cs` | The complete persistent state as a plain `[Serializable]` class: credits, totals, current/highest wave, auto-advance, upgrade levels, unlocked/owned weapons, completed milestones, last save time, smoothed credits/sec. Helper methods for upgrade levels. |
| `UpgradeLevelEntry.cs` | `{ key, level }` pair (JsonUtility can't serialize dictionaries). Key format: `"{ownerId}.{trackId}"`, e.g. `pulse_blaster.damage`. |
| `SaveService.cs` | Loads/saves JSON at `Application.persistentDataPath/astroidle_save.json`. Atomic writes (temp file then swap), corrupt-file backup, `Migrate()` for version upgrades, `DeleteSaveFile()`. |

## When saves happen

Owned by `GameBootstrap`: every `EconomyConfig.autosaveIntervalSeconds` (15s), when the app is paused or
backgrounded (`OnApplicationPause(true)`, the reliable hook on Android), on quit, and after collecting offline
earnings.

## Where the file is

| Platform | Location |
|----------|----------|
| Windows editor | `%USERPROFILE%\AppData\LocalLow\B2DaRice\Astro Idle\astroidle_save.json` (use **Astro Idle → Save Data → Open Save Folder**) |
| Android | App-private storage (removed on uninstall) |

Development builds write pretty-printed JSON, which is handy for inspecting saves.

## How to...

**Add a new saved value**
1. Add a public field with a safe default to `SaveData` (old saves will load that default).
2. Have exactly one service own reading and writing it.
3. No version bump needed for pure additions.

**Rename, remove or restructure saved data**
1. Bump `SaveData.CurrentVersion`.
2. Add a step in `SaveService.Migrate()`: `if (data.version < N) { …convert…; }`.
3. Add a test in `SaveDataTests` that loads old-format JSON.
