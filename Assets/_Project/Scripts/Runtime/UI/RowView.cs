using AstroIdle.UI.Bindings;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Generic list row: title, wrapping detail text, and an optional button on the right.
    /// All content comes from an <see cref="IRowBinding"/>; call Refresh() to re-read it. The row grows to fit its
    /// detail text, and the text uses the full width when there is no button.
    /// </summary>
    public class RowView : MonoBehaviour
    {
        private const float MinRowHeight = 180f;
        private const float TitleBandHeight = 64f;
        private const float Padding = 24f;
        private const float TextWidthWithButton = 0.68f;

        private Image background;
        private LayoutElement layout;
        private Text title;
        private Text detail;
        private Button button;
        private Text buttonLabel;
        private IRowBinding binding;

        public static RowView Create(Transform parent)
        {
            Image background = UiFactory.CreateImage("Row", parent, UiTheme.RowBackground);
            var view = background.gameObject.AddComponent<RowView>();
            view.background = background;
            view.layout = background.gameObject.AddComponent<LayoutElement>();
            view.layout.minHeight = MinRowHeight;
            view.layout.preferredHeight = MinRowHeight;

            view.title = UiFactory.CreateText("Title", background.transform, "", UiTheme.FontTitle, UiTheme.TextPrimary, TextAnchor.LowerLeft);
            view.detail = UiFactory.CreateText("Detail", background.transform, "", UiTheme.FontSmall, UiTheme.TextSecondary, TextAnchor.UpperLeft);

            view.button = UiFactory.CreateButton("Button", background.transform, "", UiTheme.FontBody, UiTheme.ButtonPrimary, view.HandleClick, out view.buttonLabel);
            UiFactory.SetAnchors((RectTransform)view.button.transform, new Vector2(0.7f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -60f), new Vector2(-20f, 60f));

            view.LayoutText(true);
            return view;
        }

        public void Bind(IRowBinding rowBinding)
        {
            binding = rowBinding;
            Refresh();
        }

        public void Refresh()
        {
            if (binding == null) return;

            title.text = binding.Title;
            detail.text = binding.Detail;
            background.color = ColorFor(binding.Style);

            bool hasButton = binding.HasButton;
            if (button.gameObject.activeSelf != hasButton)
            {
                button.gameObject.SetActive(hasButton);
                LayoutText(hasButton);
            }

            if (hasButton)
            {
                buttonLabel.text = binding.ButtonLabel;
                button.interactable = binding.ButtonEnabled;
            }

            FitHeight();
        }

        private void LayoutText(bool leaveRoomForButton)
        {
            float right = leaveRoomForButton ? TextWidthWithButton : 1f;
            float rightPadding = leaveRoomForButton ? 0f : -Padding;
            UiFactory.SetAnchors(title.rectTransform, new Vector2(0f, 1f), new Vector2(right, 1f), new Vector2(Padding, -TitleBandHeight), new Vector2(rightPadding, -10f));
            UiFactory.SetAnchors(detail.rectTransform, new Vector2(0f, 0f), new Vector2(right, 1f), new Vector2(Padding, Padding * 0.5f), new Vector2(rightPadding, -TitleBandHeight));
        }

        /// <summary>
        /// Grows the row to fit wrapped detail text. Text wrapping depends on the row's width, which is only
        /// known after a layout pass, so BottomPanelView refreshes rows again right after building them.
        /// </summary>
        private void FitHeight()
        {
            float wanted = Mathf.Max(MinRowHeight, TitleBandHeight + detail.preferredHeight + Padding);
            if (Mathf.Abs(layout.preferredHeight - wanted) < 1f) return;
            layout.minHeight = wanted;
            layout.preferredHeight = wanted;
        }

        private void HandleClick()
        {
            binding?.OnClick();
            Refresh();
        }

        private static Color ColorFor(RowStyle style)
        {
            switch (style)
            {
                case RowStyle.Header: return UiTheme.RowHeaderBackground;
                case RowStyle.Completed: return UiTheme.RowCompletedBackground;
                case RowStyle.Locked: return UiTheme.RowLockedBackground;
                default: return UiTheme.RowBackground;
            }
        }
    }
}
