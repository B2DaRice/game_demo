# Agent & contributor instructions

This file is read automatically by Claude Code, and [AGENTS.md](AGENTS.md) points other AI tools here.
Human developers should follow it too.

## Before you change anything

1. Read [docs/CODING_STANDARDS.md](docs/CODING_STANDARDS.md). It is the project's single source of truth for folder
   structure, naming, layering and patterns. **Do not invent new conventions.** If something isn't covered,
   follow the closest existing example in the codebase, then add the rule to the standards doc.
2. Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for how the systems fit together.
3. Read the README in the folder you're about to touch. Every code folder has one.

## Hard rules

- **Layering:** respect the dependency direction in the standards doc
  (Utilities → Content → Events → Persistence → Progression → Gameplay → Core → UI → Bootstrap).
  A lower layer must never reference a higher one.
- **Data over code:** balance numbers and content live in ScriptableObject assets under
  `Assets/_Project/Resources/Content/`, not in C#. Follow the
  [content guide](Assets/_Project/Resources/Content/README.md) to add a weapon, enemy, upgrade or milestone.
- **Serialized enums are append-only:** `StatId`, `ModifierKind`, `ShapeType`, `EnemyBehaviourType`,
  `ProjectileMotion`, `MilestoneCondition`, `AutoBuyStrategy` and `AutoBuyCategory` are stored as integers in `.asset`
  or save files. Never reorder or renumber them.
- **Ids are permanent:** content ids and upgrade track ids are written into players' save files. Don't rename them.
- **Save compatibility:** only add fields to `SaveData`. Anything else requires a version bump plus a migration in
  `SaveService.Migrate`.
- **Events:** every `GameEvents` subscription must be removed in `OnDestroy`/`Dispose`.
- **No scene wiring:** the scene contains only a camera and `GameBootstrap`. Everything else is built in code.
  Don't add `Find*`/`FindObjectOfType` calls; pass dependencies explicitly.
- **Script GUIDs:** the `.meta` files of ScriptableObject scripts in `Scripts/Runtime/Content/` have hand-picked GUIDs
  that the `.asset` files reference. Never delete or regenerate those `.meta` files.

## Definition of done for a change

- [ ] Code follows [docs/CODING_STANDARDS.md](docs/CODING_STANDARDS.md).
- [ ] Unity compiles with no errors or new warnings.
- [ ] `Astro Idle > Validate Content` reports no problems (when content changed).
- [ ] Edit-mode tests pass (Window → General → Test Runner → EditMode → Run All). New pure logic gets tests.
- [ ] The README of every folder you changed is updated, plus `docs/GAME_DESIGN.md` if balance or rules changed.
- [ ] Unity-generated `.meta` files for new assets/folders are committed alongside them.

## Workflow notes

- The owner builds and runs the game themselves. Don't trigger Unity builds unless asked.
- Use the in-game **DEV** tab (editor and development builds only) for credits, wave skips, speed-up and resets
  while testing balance.
