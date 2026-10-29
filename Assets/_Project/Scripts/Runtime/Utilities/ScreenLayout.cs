using UnityEngine;

namespace AstroIdle.Utilities
{
    /// <summary>
    /// Single source of truth for how the portrait screen is split between the play field and the UI.
    /// All values are normalized screen coordinates (0 = bottom, 1 = top) and respect the device safe area
    /// (notches / gesture bars). The camera, play area and UI all read from here so they always line up.
    ///
    ///   1.0 ┌───────────────┐
    ///       │  HUD (overlay)│  HudBottom .. SafeTop
    ///       │               │
    ///       │  play field   │  camera viewport: PanelTop .. 1.0
    ///       ├───────────────┤  PanelTop
    ///       │  upgrade panel│  SafeBottom .. PanelTop
    ///   0.0 └───────────────┘
    /// </summary>
    public static class ScreenLayout
    {
        /// <summary>Fraction of the safe-area height used by the bottom upgrade panel.</summary>
        public const float BottomPanelFraction = 0.40f;

        /// <summary>Fraction of the safe-area height used by the top HUD (drawn over the play field).</summary>
        public const float HudFraction = 0.13f;

        public static float SafeBottom => Screen.height > 0 ? Screen.safeArea.yMin / Screen.height : 0f;

        public static float SafeTop => Screen.height > 0 ? Screen.safeArea.yMax / Screen.height : 1f;

        public static float SafeHeight => SafeTop - SafeBottom;

        public static float PanelTop => SafeBottom + SafeHeight * BottomPanelFraction;

        public static float HudBottom => SafeTop - SafeHeight * HudFraction;
    }
}
