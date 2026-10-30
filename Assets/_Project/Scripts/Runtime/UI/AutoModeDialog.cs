using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Pop-out dialog for auto mode: an ON/OFF switch, a strategy picker (Attack / Balanced / Defense) and the
    /// selected profile's description. Tapping the dimmed background or CLOSE dismisses it.
    /// </summary>
    public class AutoModeDialog : MonoBehaviour
    {
        // Display order of the strategy buttons (left to right).
        private static readonly AutoBuyStrategy[] StrategyOrder = { AutoBuyStrategy.Attack, AutoBuyStrategy.Balanced, AutoBuyStrategy.Defense };

        private readonly List<Button> strategyButtons = new List<Button>();
        private AutoBuyService autoBuy;
        private AutoBuyConfig config;
        private Button switchButton;
        private Text switchLabel;
        private Text description;

        public bool IsOpen => gameObject.activeSelf;

        public static AutoModeDialog Create(Transform parent, AutoBuyService autoBuy, AutoBuyConfig config)
        {
            // Full-screen dimmer: blocks input behind the dialog and closes it when tapped.
            Button dim = UiFactory.CreateButton("AutoModeDialog", parent, "", UiTheme.FontBody, UiTheme.PopupDim, null, out Text unusedLabel);
            Destroy(unusedLabel.gameObject);
            UiFactory.Stretch((RectTransform)dim.transform);
            ColorBlock dimColors = dim.colors;
            dimColors.highlightedColor = Color.white;
            dimColors.pressedColor = Color.white;
            dim.colors = dimColors;

            var view = dim.gameObject.AddComponent<AutoModeDialog>();
            view.autoBuy = autoBuy;
            view.config = config;
            dim.onClick.AddListener(view.Close);

            Image box = UiFactory.CreateImage("Box", dim.transform, UiTheme.RowHeaderBackground, true);
            UiFactory.SetRegion(box.rectTransform, 0.06f, 0.33f, 0.94f, 0.72f);
            Transform root = box.transform;

            Text title = UiFactory.CreateText("Title", root, "AUTO MODE", UiTheme.FontTitle, UiTheme.Credits, TextAnchor.MiddleLeft);
            UiFactory.SetRegion(title.rectTransform, 0f, 0.84f, 0.65f, 1f, 24f);

            view.switchButton = UiFactory.CreateButton("Switch", root, "", UiTheme.FontBody, UiTheme.ButtonSecondary, view.ToggleEnabled, out view.switchLabel);
            UiFactory.SetRegion((RectTransform)view.switchButton.transform, 0.62f, 0.85f, 0.97f, 0.98f);

            Text hint = UiFactory.CreateText("Hint", root, "Spends your credits automatically. Priority:", UiTheme.FontSmall, UiTheme.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetRegion(hint.rectTransform, 0f, 0.72f, 1f, 0.84f, 24f);

            for (int i = 0; i < StrategyOrder.Length; i++)
            {
                AutoBuyStrategy strategy = StrategyOrder[i];
                AutoBuyProfile profile = config != null ? config.GetProfile(strategy) : null;
                string label = profile != null ? profile.DisplayName.ToUpperInvariant() : strategy.ToString().ToUpperInvariant();
                Button button = UiFactory.CreateButton("Strategy_" + strategy, root, label, UiTheme.FontBody, UiTheme.ButtonSecondary, () => view.SelectStrategy(strategy), out _);
                float xMin = 0.03f + i * (0.94f / StrategyOrder.Length);
                UiFactory.SetRegion((RectTransform)button.transform, xMin, 0.53f, xMin + 0.94f / StrategyOrder.Length, 0.7f, 6f);
                view.strategyButtons.Add(button);
            }

            view.description = UiFactory.CreateText("Description", root, "", UiTheme.FontSmall, UiTheme.TextPrimary, TextAnchor.UpperLeft);
            UiFactory.SetRegion(view.description.rectTransform, 0f, 0.2f, 1f, 0.51f, 24f);

            Button close = UiFactory.CreateButton("Close", root, "CLOSE", UiTheme.FontBody, UiTheme.ButtonPrimary, view.Close, out _);
            UiFactory.SetRegion((RectTransform)close.transform, 0.3f, 0.04f, 0.7f, 0.18f);

            dim.gameObject.SetActive(false);
            return view;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void ToggleEnabled()
        {
            autoBuy.SetEnabled(!autoBuy.Enabled);
            Refresh();
        }

        private void SelectStrategy(AutoBuyStrategy strategy)
        {
            autoBuy.SetStrategy(strategy);
            Refresh();
        }

        private void Refresh()
        {
            bool enabled = autoBuy.Enabled;
            switchLabel.text = enabled ? "ON" : "OFF";
            switchButton.targetGraphic.color = enabled ? UiTheme.AutoModeOn : UiTheme.ButtonSecondary;

            for (int i = 0; i < strategyButtons.Count; i++)
            {
                strategyButtons[i].targetGraphic.color = StrategyOrder[i] == autoBuy.Strategy ? UiTheme.ButtonPrimary : UiTheme.ButtonSecondary;
            }

            AutoBuyProfile profile = autoBuy.ActiveProfile;
            description.text = profile != null ? profile.Description : "Auto mode is not configured (missing AutoBuyConfig).";
        }
    }
}
