using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Helpers for building uGUI hierarchies from code. All UI is constructed through these so styling and
    /// setup stay consistent (fonts, raycast targets, layers, button tints).
    /// </summary>
    public static class UiFactory
    {
        private const int UiLayer = 5;

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        /// <summary>Anchors the rect to a normalized region of its parent with uniform padding (in pixels).</summary>
        public static void SetRegion(RectTransform rect, float xMin, float yMin, float xMax, float yMax, float padding = 0f)
        {
            SetAnchors(rect, new Vector2(xMin, yMin), new Vector2(xMax, yMax), new Vector2(padding, padding), new Vector2(-padding, -padding));
        }

        public static void Stretch(RectTransform rect, float padding = 0f)
        {
            SetRegion(rect, 0f, 0f, 1f, 1f, padding);
        }

        public static Image CreateImage(string name, Transform parent, Color color, bool raycastTarget = false)
        {
            RectTransform rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = UiTheme.Font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = true;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, int fontSize, Color color, UnityAction onClick, out Text labelText)
        {
            Image background = CreateImage(name, parent, color, true);
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            button.colors = colors;

            if (onClick != null) button.onClick.AddListener(onClick);

            labelText = CreateText("Label", background.transform, label, fontSize, UiTheme.TextPrimary, TextAnchor.MiddleCenter);
            Stretch(labelText.rectTransform, 6f);
            return button;
        }

        /// <summary>Vertical scroll list. Add rows as children of <paramref name="content"/>.</summary>
        public static ScrollRect CreateVerticalScroll(string name, Transform parent, out RectTransform content)
        {
            Image background = CreateImage(name, parent, new Color(0f, 0f, 0f, 0.15f), true);
            var scroll = background.gameObject.AddComponent<ScrollRect>();

            Image viewportImage = CreateImage("Viewport", background.transform, new Color(0f, 0f, 0f, 0f), true);
            RectTransform viewport = viewportImage.rectTransform;
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;
            return scroll;
        }
    }
}
