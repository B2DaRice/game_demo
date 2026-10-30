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
| `BottomPanelView.cs` | Tab bar on top; below it an optional **left-hand sub-tab list** (for tabs that have sub-tabs, e.g. one per weapon) and the scrolling row list. Remembers the selected sub-tab per tab. Rebuilds rows on tab/sub-tab change or structure change (weapon unlocked, milestone done). Refreshes rows and sub-tabs 5×/s. |
| `RowView.cs` | Generic row (title, wrapping detail, optional button) driven by an `IRowBinding`. Grows to fit its text, and uses the full width when there's no button. |
| `SubTabView.cs` | One sidebar entry (accent stripe, title, subtitle; selected/dimmed states) driven by an `ISubTab`. |
| `ProgressBar.cs` | Simple fill bar with an outlined label. |
| `StatFormatter.cs` | How each `StatId` is displayed (`x1.25`, `2.40/s`, `150`, …). |
| `ToastView.cs` | Queued fading banner for milestones, weapons becoming available or coming online, wave failure, and `GameEvents.ToastRequested`. |
| `OfflineEarningsPopup.cs` | Modal "welcome back" popup with a Collect button. |
| `AutoModeButton.cs` | Small button at the bottom-right of the play field: shows `AUTO OFF` or `AUTO <STRATEGY>` (green when on). Opens the dialog. |
| `AutoModeDialog.cs` | Pop-out dialog: ON/OFF switch, Attack / Balanced / Defense buttons, and the selected profile's description. Tap outside or CLOSE to dismiss. Calls `AutoBuyService.SetEnabled/SetStrategy`. |

### `Bindings/`: what a row shows and does
| File | Row |
|------|-----|
| `IRowBinding.cs` | Interface: `Title`, `Detail`, `HasButton`, `ButtonLabel`, `ButtonEnabled`, `Style`, `OnClick()`. Re-read on every refresh. |
| `RowStyle.cs` | Background style: Normal / Header / Completed / Locked. |
| `UpgradeRowBinding.cs` | Upgrade track: level, current → next value, cost button. |
| `WeaponRowBinding.cs` | Weapon header for the selected weapon: description and stats (base stats when not owned), **UNLOCK** button with the shared price, or a milestone hint if the weapon is gated. |
| `MilestoneRowBinding.cs` | Goal progress and rewards. |
| `ActionRowBinding.cs` | Delegate-driven row for info and dev tools. |

### `Tabs/`: bottom panel tabs
| File | Tab |
|------|-----|
| `IPanelTab.cs` | Interface: `Title`, `SubTabs` (empty = no sidebar), `DefaultSubTabIndex`, `BuildRows(subTabIndex)`. |
| `ISubTab.cs` | Interface for a sidebar entry: `Title`, `Subtitle`, `Accent`, `Dimmed` (re-read on every refresh). |
| `ShipTab.cs` | SHIP: ship summary plus ship upgrades. |
| `WeaponsTab.cs` | WEAPONS: one sub-tab per weapon, opening on the save's random starting weapon. Shows the weapon header, then its upgrades once owned. |
| `WeaponSubTab.cs` | Weapon sidebar entry: name, weapon-colored stripe, and `Lv N` (total upgrades), `Unlock <price>`, or `LOCKED`. Dimmed until owned. |
| `MilestonesTab.cs` | GOALS: in-progress first, then completed. |
| `DevTab.cs` | DEV (editor and development builds only): grant credits, skip wave, game speed, reset progress. |

## Layout

Regions come from [`ScreenLayout`](../Utilities/README.md) so the UI always matches the camera's play field:
HUD = top 13% of the safe area, panel = bottom 40%, toast just under the HUD, popup full-screen.
Auto mode button: bottom-right corner of the play field, just above the panel (clear of the HUD and toasts).
Inside the panel: tab bar (110px) on top; for tabs with sub-tabs, the sidebar takes the left 30% and the rows the rest.
Draw order is creation order in `UIRoot.Build` (later = on top).

## How to...

**Add a tab**
1. Create `Tabs/MyTab.cs` implementing `IPanelTab`. Yield `IRowBinding`s from `BuildRows(subTabIndex)`.
   For a plain tab, return `System.Array.Empty<ISubTab>()` from `SubTabs` and ignore the index.
2. Register it in `BottomPanelView.CreateTabs()`.
3. If its rows depend on new events, subscribe in `BottomPanelView.Build` and set `rebuildRequested` (and
   unsubscribe in `OnDestroy`).

**Give a tab a left-hand sub-tab list** (like WEAPONS): return one `ISubTab` per entry from `SubTabs` (build the
list once in the constructor), choose the initial entry in `DefaultSubTabIndex`, and build rows for the selected
index in `BuildRows`. `BottomPanelView` handles the sidebar, selection memory and layout.

**Add a new kind of row**: implement `IRowBinding` in `Bindings/`. `RowView` doesn't change.

**Add a HUD element**: create it in `HudView.Build` with `UiFactory`, position it with anchors inside one of the
three bands, and update it in `Refresh()`.

**Restyle**: edit `UiTheme`. For sharper text later, swap `UiFactory.CreateText` to TextMeshPro (one place).
