# UI (`AstroIdle.UI`)

Layer 8: everything the player sees and taps. The UI is **built entirely from code** (uGUI, legacy `Text` with
Unity's built-in font) so there are no prefabs or scene objects to keep in sync. It **reads** state (polling at 5–10
Hz) and **commands** services. It never contains game rules.

## Files

| File | Purpose |
|------|---------|
| `UIRoot.cs` | Creates the EventSystem (picks the input module for the active input backend), the Canvas (scale with width, 1080x1920 reference), and all views. `ApplyLayout()` positions everything from `ScreenLayout`. |
| `UiFactory.cs` | Builders: rects, images, text, buttons (consistent tints), vertical scroll lists. **Always build UI through this.** |
| `UiTheme.cs` | All colors, font sizes and the font. **Change the look here only.** |
| `HudView.cs` | Top overlay: credits, credits/sec, wave/boss, best wave, hull bar, wave progress bar, AUTO/ADVANCE toggle. |
| `BottomPanelView.cs` | Tab bar plus scrolling row list. Rebuilds rows on tab change or structure change (weapon unlocked/bought, milestone done). Refreshes rows 5×/s. |
| `RowView.cs` | Generic row (title, two-line detail, optional button) driven by an `IRowBinding`. |
| `ProgressBar.cs` | Simple fill bar with an outlined label. |
| `StatFormatter.cs` | How each `StatId` is displayed (`x1.25`, `2.40/s`, `150`, …). |
| `ToastView.cs` | Queued fading banner for milestones, unlocks, wave failure, and `GameEvents.ToastRequested`. |
| `OfflineEarningsPopup.cs` | Modal "welcome back" popup with a Collect button. |

### `Bindings/`: what a row shows and does
| File | Row |
|------|-----|
| `IRowBinding.cs` | Interface: `Title`, `Detail`, `HasButton`, `ButtonLabel`, `ButtonEnabled`, `Style`, `OnClick()`. Re-read on every refresh. |
| `RowStyle.cs` | Background style: Normal / Header / Completed / Locked. |
| `UpgradeRowBinding.cs` | Upgrade track: level, current → next value, cost button. |
| `WeaponRowBinding.cs` | Weapon header: locked (with unlock hint) / available (buy) / equipped (summary stats). |
| `MilestoneRowBinding.cs` | Goal progress and rewards. |
| `ActionRowBinding.cs` | Delegate-driven row for info and dev tools. |

### `Tabs/`: bottom panel tabs
| File | Tab |
|------|-----|
| `IPanelTab.cs` | Interface: `Title` plus `BuildRows()`. |
| `ShipTab.cs` | SHIP: ship summary plus ship upgrades. |
| `WeaponsTab.cs` | WEAPONS: each weapon, with upgrades listed under owned ones. |
| `MilestonesTab.cs` | GOALS: in-progress first, then completed. |
| `DevTab.cs` | DEV (editor and development builds only): grant credits, skip wave, game speed, reset progress. |

## Layout

Regions come from [`ScreenLayout`](../Utilities/README.md) so the UI always matches the camera's play field:
HUD = top 13% of the safe area, panel = bottom 40%, toast just under the HUD, popup full-screen.
Draw order is creation order in `UIRoot.Build` (later = on top).

## How to...

**Add a tab**
1. Create `Tabs/MyTab.cs` implementing `IPanelTab`. Yield `IRowBinding`s from `BuildRows()`.
2. Register it in `BottomPanelView.CreateTabs()`.
3. If its rows depend on new events, subscribe in `BottomPanelView.Build` and set `rebuildRequested` (and
   unsubscribe in `OnDestroy`).

**Add a new kind of row**: implement `IRowBinding` in `Bindings/`. `RowView` doesn't change.

**Add a HUD element**: create it in `HudView.Build` with `UiFactory`, position it with anchors inside one of the
three bands, and update it in `Refresh()`.

**Restyle**: edit `UiTheme`. For sharper text later, swap `UiFactory.CreateText` to TextMeshPro (one place).
