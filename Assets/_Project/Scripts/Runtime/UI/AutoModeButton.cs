using AstroIdle.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Small toggle-style button on the right edge of the play field. Shows whether auto mode is on and which
    /// strategy is active; tapping it opens the <see cref="AutoModeDialog"/>.
    /// </summary>
    public class AutoModeButton : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;

        private AutoBuyService autoBuy;
        private Button button;
        private Text label;
        private float refreshTimer;

        public RectTransform Rect => (RectTransform)transform;

        public static AutoModeButton Create(Transform parent, AutoBuyService autoBuy, AutoModeDialog dialog)
        {
            Button button = UiFactory.CreateButton("AutoModeButton", parent, "", UiTheme.FontSmall, UiTheme.ButtonSecondary, dialog.Open, out Text label);
            var view = button.gameObject.AddComponent<AutoModeButton>();
            view.autoBuy = autoBuy;
            view.button = button;
            view.label = label;
            view.Refresh();
            return view;
        }

        private void Update()
        {
            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            bool enabled = autoBuy.Enabled;
            label.text = enabled ? "AUTO\n" + autoBuy.Strategy.ToString().ToUpperInvariant() : "AUTO\nOFF";
            button.targetGraphic.color = enabled ? UiTheme.AutoModeOn : UiTheme.ButtonSecondary;
        }
    }
}
