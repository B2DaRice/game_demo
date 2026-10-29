using AstroIdle.Core;
using AstroIdle.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Builds and lays out the entire UI from code: canvas, event system, HUD, bottom panel, toasts and popups.
    /// Layout regions come from ScreenLayout so the UI always matches the camera's play field.
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        private RectTransform panelBackdrop;
        private BottomPanelView panel;
        private HudView hud;
        private ToastView toast;

        public OfflineEarningsPopup OfflinePopup { get; private set; }

        public void Build(GameContext context)
        {
            EnsureEventSystem();

            RectTransform canvasRect = UiFactory.CreateRect("Canvas", transform);
            var canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // portrait: scale by width
            canvasRect.gameObject.AddComponent<GraphicRaycaster>();

            // Order = draw order: later siblings draw on top.
            panelBackdrop = UiFactory.CreateImage("PanelBackdrop", canvasRect, UiTheme.PanelBackground, true).rectTransform;
            panel = BottomPanelView.Create(canvasRect, context);
            hud = HudView.Create(canvasRect, context);
            toast = ToastView.Create(canvasRect);
            OfflinePopup = OfflineEarningsPopup.Create(canvasRect);

            ApplyLayout();
        }

        /// <summary>Re-applies anchors from ScreenLayout (call after resolution / safe-area changes).</summary>
        public void ApplyLayout()
        {
            float panelTop = ScreenLayout.PanelTop;
            float hudBottom = ScreenLayout.HudBottom;

            UiFactory.SetRegion(panelBackdrop, 0f, 0f, 1f, panelTop);
            UiFactory.SetRegion(panel.Rect, 0f, ScreenLayout.SafeBottom, 1f, panelTop);
            UiFactory.SetRegion(hud.Rect, 0f, hudBottom, 1f, ScreenLayout.SafeTop);
            UiFactory.SetRegion(toast.Rect, 0.05f, hudBottom - 0.065f, 0.95f, hudBottom - 0.01f);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
