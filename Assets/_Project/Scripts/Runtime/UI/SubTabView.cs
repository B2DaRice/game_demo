using System;
using AstroIdle.UI.Tabs;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// One entry in the bottom panel's left-hand sub-tab list: accent stripe, title and subtitle.
    /// Content comes from an <see cref="ISubTab"/>; call Refresh() to re-read it.
    /// </summary>
    public class SubTabView : MonoBehaviour
    {
        private const float Height = 130f;
        private const float StripeWidth = 12f;

        private Image background;
        private Image stripe;
        private Text title;
        private Text subtitle;
        private ISubTab subTab;
        private bool selected;

        public static SubTabView Create(Transform parent, ISubTab subTab, Action onClick)
        {
            Button button = UiFactory.CreateButton("SubTab", parent, "", UiTheme.FontBody, UiTheme.SubTabBackground, () => onClick(), out Text unusedLabel);
            Destroy(unusedLabel.gameObject); // this button lays out its own two-line text

            var layout = button.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = Height;
            layout.preferredHeight = Height;

            var view = button.gameObject.AddComponent<SubTabView>();
            view.subTab = subTab;
            view.background = (Image)button.targetGraphic;

            view.stripe = UiFactory.CreateImage("Accent", button.transform, subTab.Accent);
            UiFactory.SetAnchors(view.stripe.rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(StripeWidth, 0f));

            view.title = UiFactory.CreateText("Title", button.transform, "", UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.LowerLeft);
            UiFactory.SetAnchors(view.title.rectTransform, new Vector2(0f, 0.48f), Vector2.one, new Vector2(StripeWidth + 14f, 0f), new Vector2(-8f, -8f));

            view.subtitle = UiFactory.CreateText("Subtitle", button.transform, "", UiTheme.FontSmall - 2, UiTheme.TextSecondary, TextAnchor.UpperLeft);
            UiFactory.SetAnchors(view.subtitle.rectTransform, Vector2.zero, new Vector2(1f, 0.48f), new Vector2(StripeWidth + 14f, 8f), new Vector2(-8f, 0f));

            view.Refresh();
            return view;
        }

        public void SetSelected(bool isSelected)
        {
            selected = isSelected;
            Refresh();
        }

        public void Refresh()
        {
            title.text = subTab.Title;
            subtitle.text = subTab.Subtitle;

            bool dimmed = subTab.Dimmed;
            background.color = selected ? UiTheme.SubTabSelected : dimmed ? UiTheme.SubTabDimmed : UiTheme.SubTabBackground;
            title.color = dimmed && !selected ? UiTheme.TextSecondary : UiTheme.TextPrimary;

            Color accent = subTab.Accent;
            accent.a = dimmed ? 0.4f : 1f;
            stripe.color = accent;
        }
    }
}
