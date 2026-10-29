using AstroIdle.UI.Bindings;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Generic list row: title, two-line detail, and an optional button on the right.
    /// All content comes from an <see cref="IRowBinding"/>; call Refresh() to re-read it.
    /// </summary>
    public class RowView : MonoBehaviour
    {
        private const float RowHeight = 180f;

        private Image background;
        private Text title;
        private Text detail;
        private Button button;
        private Text buttonLabel;
        private IRowBinding binding;

        public static RowView Create(Transform parent)
        {
            Image background = UiFactory.CreateImage("Row", parent, UiTheme.RowBackground);
            var layout = background.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = RowHeight;
            layout.preferredHeight = RowHeight;

            var view = background.gameObject.AddComponent<RowView>();
            view.background = background;

            view.title = UiFactory.CreateText("Title", background.transform, "", UiTheme.FontTitle, UiTheme.TextPrimary, TextAnchor.LowerLeft);
            UiFactory.SetAnchors(view.title.rectTransform, new Vector2(0f, 0.58f), new Vector2(0.7f, 1f), new Vector2(24f, 0f), new Vector2(0f, -12f));

            view.detail = UiFactory.CreateText("Detail", background.transform, "", UiTheme.FontSmall, UiTheme.TextSecondary, TextAnchor.UpperLeft);
            UiFactory.SetAnchors(view.detail.rectTransform, new Vector2(0f, 0f), new Vector2(0.7f, 0.58f), new Vector2(24f, 8f), new Vector2(0f, -4f));

            view.button = UiFactory.CreateButton("Button", background.transform, "", UiTheme.FontTitle, UiTheme.ButtonPrimary, view.HandleClick, out view.buttonLabel);
            UiFactory.SetAnchors((RectTransform)view.button.transform, new Vector2(0.72f, 0.18f), new Vector2(1f, 0.82f), new Vector2(0f, 0f), new Vector2(-20f, 0f));

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
            if (button.gameObject.activeSelf != hasButton) button.gameObject.SetActive(hasButton);
            if (!hasButton) return;

            buttonLabel.text = binding.ButtonLabel;
            button.interactable = binding.ButtonEnabled;
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
