using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Arena
{
    /// <summary>
    /// Configures the world camera so it renders only the play field (above the bottom UI panel) and shows a
    /// consistent world width on every phone aspect ratio.
    /// </summary>
    public static class ArenaCamera
    {
        /// <summary>World units visible horizontally on a portrait phone.</summary>
        public const float DesiredWorldWidth = 10f;

        /// <summary>Lower bound for half the visible height (protects landscape editor windows).</summary>
        public const float MinHalfHeight = 5f;

        public static readonly Color BackgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f);

        /// <returns>Height in world units covered by the HUD at the top of the play field.</returns>
        public static float Configure(Camera camera)
        {
            float panelTop = Mathf.Clamp(ScreenLayout.PanelTop, 0f, 0.9f);
            float viewFraction = 1f - panelTop;

            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            camera.rect = new Rect(0f, panelTop, 1f, viewFraction);
            camera.ResetAspect();
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;

            float aspect = Screen.height > 0 ? Screen.width / (Screen.height * viewFraction) : 0.75f;
            float halfHeight = Mathf.Max(DesiredWorldWidth / (2f * Mathf.Max(aspect, 0.01f)), MinHalfHeight);
            camera.orthographicSize = halfHeight;

            float hudScreenFraction = 1f - ScreenLayout.HudBottom;
            return hudScreenFraction / viewFraction * 2f * halfHeight;
        }
    }
}
