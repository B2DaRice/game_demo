using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>Simple horizontal bar (background + fill) with an optional centered label.</summary>
    public class ProgressBar
    {
        private readonly RectTransform fill;

        private ProgressBar(RectTransform root, RectTransform fill, Text label)
        {
            Root = root;
            this.fill = fill;
            Label = label;
        }

        public RectTransform Root { get; }
        public Text Label { get; }

        public static ProgressBar Create(string name, Transform parent, Color fillColor, bool withLabel)
        {
            Image background = UiFactory.CreateImage(name, parent, UiTheme.BarBackground);
            Image fillImage = UiFactory.CreateImage("Fill", background.transform, fillColor);
            UiFactory.SetAnchors(fillImage.rectTransform, Vector2.zero, new Vector2(0f, 1f));

            Text label = null;
            if (withLabel)
            {
                label = UiFactory.CreateText("Label", background.transform, "", UiTheme.FontSmall, UiTheme.TextPrimary, TextAnchor.MiddleCenter);
                UiFactory.Stretch(label.rectTransform);
                var outline = label.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            }

            return new ProgressBar(background.rectTransform, fillImage.rectTransform, label);
        }

        public void SetValue(float normalized)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        }
    }
}
