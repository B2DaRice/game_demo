# Scripts

All C# for the game, split into two assemblies:

| Assembly | Folder | Platforms | Purpose |
|----------|--------|-----------|---------|
| `AstroIdle.Runtime` | `Runtime/` | All | The game itself. |
| `AstroIdle.Editor` | `Editor/` | Editor only | Project setup and developer menu tools. [README](Editor/README.md) |

Tests are a third assembly under [`../Tests`](../Tests/README.md).

## Runtime layers

`Runtime/` is organized into layers. **Dependencies only point downward in this table** (a layer may use the ones
listed above it). The rules are in [CODING_STANDARDS.md §2](../../../docs/CODING_STANDARDS.md#2-layering--dependencies).

| # | Folder | Namespace | What lives here | Docs |
|---|--------|-----------|-----------------|------|
| 1 | `Utilities/` | `AstroIdle.Utilities` | Generic helpers: number formatting, procedural sprites, screen layout, vector math. | [README](Runtime/Utilities/README.md) |
| 2 | `Content/` | `AstroIdle.Content` | ScriptableObject definitions (ship, weapon, enemy, milestone, configs) and `ContentDatabase`. | [README](Runtime/Content/README.md) |
| 3 | `Events/` | `AstroIdle.Events` | `GameEvents` static event hub. | [README](Runtime/Events/README.md) |
| 4 | `Persistence/` | `AstroIdle.Persistence` | `SaveData` and `SaveService` (JSON save file). | [README](Runtime/Persistence/README.md) |
| 5 | `Progression/` | `AstroIdle.Progression` | Wallet, upgrades, weapon inventory, milestones, rewards, offline earnings. | [README](Runtime/Progression/README.md) |
| 6 | `Gameplay/` | `AstroIdle.Gameplay.*` | Real-time simulation: arena, combat, enemies, player ship, weapons, waves, effects. | [README](Runtime/Gameplay/README.md) |
| 7 | `Core/` | `AstroIdle.Core` | `GameContext`: references to every top-level system, handed to the UI. | [README](Runtime/Core/README.md) |
| 8 | `UI/` | `AstroIdle.UI` | Code-built uGUI: HUD, upgrade panel and tabs, toasts, popups. | [README](Runtime/UI/README.md) |
| 9 | `Bootstrap/` | `AstroIdle.Bootstrap` | `GameBootstrap`, the composition root that builds and runs everything. | [README](Runtime/Bootstrap/README.md) |

## Where do I put new code?

| You're adding... | Put it in |
|------------------|-----------|
| A formula or rule about credits, costs, unlocks | `Progression/` (plain C#, add tests) |
| Something that moves, shoots or collides each frame | `Gameplay/<area>/` |
| A new kind of content data | `Content/` (a new Definition/Config) plus a folder in `Resources/Content/` |
| Something the player sees or taps | `UI/` |
| A generic helper with no game knowledge | `Utilities/` |
| Editor menu items or automation | `Editor/` |

## Assembly references

- `AstroIdle.Runtime` references `UnityEngine.UI` (uGUI) and `Unity.InputSystem` (the UI input module only).
- `AstroIdle.Editor` references `AstroIdle.Runtime`.
