# Assets/_Project

Every first-party file in the game lives under this folder. The leading underscore sorts it to the top of the
Project window and keeps it separate from any third-party packages or asset-store imports added later.

| Folder | Contents | Docs |
|--------|----------|------|
| `Scenes/` | `Main.unity`, the only scene. It contains just a camera and `GameBootstrap`. **Generated automatically** by the editor bootstrapper the first time the project opens. | [Bootstrap README](Scripts/Runtime/Bootstrap/README.md) |
| `Scripts/` | All C# code: runtime game code and editor tooling. | [Scripts README](Scripts/README.md) |
| `Resources/Content/` | ScriptableObject assets that define the ship, weapons, enemies, waves, economy and milestones. **This is where balancing happens.** | [Content README](Resources/Content/README.md) |
| `Tests/` | Edit-mode unit tests. | [Tests README](Tests/README.md) |

Future folders (create them when needed, following [the standards](../../docs/CODING_STANDARDS.md#1-folder-structure)):
`Art/`, `Audio/`, `Prefabs/`.

## How the pieces connect

1. Unity loads `Scenes/Main.unity`, and its `GameBootstrap` component wakes up.
2. The bootstrap loads every asset in `Resources/Content/`, loads the save file, and builds the whole game
   (world and UI) in code from `Scripts/`.
3. Designers tune the game by editing the assets in `Resources/Content/`. No code changes needed.

See [docs/ARCHITECTURE.md](../../docs/ARCHITECTURE.md) for the full picture.
