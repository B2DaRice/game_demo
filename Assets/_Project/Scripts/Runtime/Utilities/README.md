# Utilities (`AstroIdle.Utilities`)

Layer 1: generic helpers with **no knowledge of game rules**. Anything here may be used by every other layer, and
nothing here may reference another project namespace.

| File | Purpose |
|------|---------|
| `NumberFormatter.cs` | Formats big numbers idle-style: `999`, `1.23K`, `45.6M` … `Dc`, then scientific (`1.23e36`). Truncates instead of rounding so the display never overstates. Also `FormatRate` (one decimal under 100) and `FormatDuration` (`2h 05m`). Tested in `NumberFormatterTests`. |
| `ShapeType.cs` | Enum of procedural shapes (Ship, Rock, Diamond, Saucer, Bolt, ...). **Serialized in assets: append only.** |
| `ShapeSpriteFactory.cs` | Generates and caches glowing vector sprites for each `ShapeType` at runtime (128px, radius 1 at scale 1). Hollow shapes get an outline, faint fill and glow. Projectile/particle shapes are solid. Replaces art assets for now. |
| `ScreenLayout.cs` | Single source of truth for the portrait split: bottom panel (40% of safe area), play field above it, HUD (13%) overlaid at the top. Camera, play area and UI all read these values. |
| `VectorUtils.cs` | 2D helpers: rotate a vector, facing rotation for up-pointing sprites, evenly spread angles for multi-shot volleys. |

## How to...

**Add a new shape**
1. Append a value to `ShapeType` with the next number (never reorder).
2. Add its polygon (points in −1..1 space, pointing up) to `ShapeSpriteFactory.GetPolygon`.
3. If it should be solid (like projectiles), add it to `IsSolid`.
4. Use it from any Definition's `shape` / `projectileShape` field.

**Change the screen split** (e.g. bigger upgrade panel): edit `BottomPanelFraction` / `HudFraction` in
`ScreenLayout`. The camera, play area and UI adapt automatically.
