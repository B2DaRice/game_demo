using UnityEngine;

namespace AstroIdle.UI
{
    /// <summary>
    /// Central colors, font sizes and font for all UI. Change the look of the game here, never inline.
    /// Font sizes assume the 1080x1920 reference resolution used by the CanvasScaler.
    /// </summary>
    public static class UiTheme
    {
        public const int FontSmall = 30;
        public const int FontBody = 34;
        public const int FontTitle = 40;
        public const int FontLarge = 60;

        public static readonly Color PanelBackground = new Color(0.06f, 0.07f, 0.11f, 1f);
        public static readonly Color HudBackground = new Color(0f, 0f, 0f, 0.35f);
        public static readonly Color RowBackground = new Color(0.11f, 0.13f, 0.19f, 1f);
        public static readonly Color RowHeaderBackground = new Color(0.14f, 0.18f, 0.28f, 1f);
        public static readonly Color RowCompletedBackground = new Color(0.10f, 0.20f, 0.14f, 1f);
        public static readonly Color RowLockedBackground = new Color(0.09f, 0.09f, 0.11f, 1f);

        public static readonly Color SubTabBackground = new Color(0.10f, 0.12f, 0.17f, 1f);
        public static readonly Color SubTabSelected = new Color(0.17f, 0.30f, 0.52f, 1f);
        public static readonly Color SubTabDimmed = new Color(0.07f, 0.08f, 0.11f, 1f);

        public static readonly Color ButtonPrimary = new Color(0.20f, 0.55f, 0.95f, 1f);
        public static readonly Color ButtonSecondary = new Color(0.22f, 0.25f, 0.33f, 1f);
        public static readonly Color ButtonWarning = new Color(0.95f, 0.55f, 0.20f, 1f);
        public static readonly Color AutoModeOn = new Color(0.18f, 0.62f, 0.38f, 1f);

        public static readonly Color TextPrimary = new Color(0.93f, 0.95f, 1f, 1f);
        public static readonly Color TextSecondary = new Color(0.65f, 0.70f, 0.80f, 1f);
        public static readonly Color Credits = new Color(1f, 0.85f, 0.35f, 1f);
        public static readonly Color Positive = new Color(0.45f, 1f, 0.6f, 1f);
        public static readonly Color Warning = new Color(1f, 0.45f, 0.4f, 1f);

        public static readonly Color BarBackground = new Color(1f, 1f, 1f, 0.12f);
        public static readonly Color HullBar = new Color(0.4f, 1f, 0.6f, 1f);
        public static readonly Color WaveBar = new Color(0.4f, 0.8f, 1f, 1f);

        public static readonly Color PopupDim = new Color(0f, 0f, 0f, 0.7f);

        private static Font font;

        /// <summary>Unity's built-in runtime font (no TextMeshPro setup needed).</summary>
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }
    }
}
