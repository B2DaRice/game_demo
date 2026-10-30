# Core (`AstroIdle.Core`)

Layer 7: shared top-level wiring that sits above Gameplay and below UI.

| File | Purpose |
|------|---------|
| `GameContext.cs` | One object holding every service and top-level system (content, save, wallet, upgrades, weapons, milestones, rewards, auto buy, gameplay context, player, wave director), plus the `SaveNow` and `ResetProgress` actions. Built by [`GameBootstrap`](../Bootstrap/README.md) and passed to the [UI](../UI/README.md). |

## Why it exists

The UI needs to read from and command many systems. Rather than letting UI search for them (hidden coupling), the
composition root builds this explicit container once and hands it down. It's not a global: nothing can fetch it
statically.

## Rules

- `GameContext` is a container. **No logic in it.**
- Gameplay code must not use it. Gameplay uses the narrower
  [`GameplayContext`](../Gameplay/README.md).
- When you add a new service, add a property here and assign it in `GameBootstrap.CreateServices`/`BuildWorld`.
