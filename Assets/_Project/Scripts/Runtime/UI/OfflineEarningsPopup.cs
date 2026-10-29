using System;
using AstroIdle.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>Modal "welcome back" popup that grants offline earnings when the player taps Collect.</summary>
    public class OfflineEarningsPopup : MonoBehaviour
    {
        private Text body;
        private Action onCollect;

        public bool IsOpen => gameObject.activeSelf;

        public static OfflineEarningsPopup Create(Transform parent)
        {
            // Full-screen dimmer that also blocks input to everything behind it.
            Image dim = UiFactory.CreateImage("OfflinePopup", parent, UiTheme.PopupDim, true);
            UiFactory.Stretch(dim.rectTransform);
            var view = dim.gameObject.AddComponent<OfflineEarningsPopup>();

            Image box = UiFactory.CreateImage("Box", dim.transform, UiTheme.RowHeaderBackground, true);
            UiFactory.SetRegion(box.rectTransform, 0.08f, 0.36f, 0.92f, 0.64f);

            Text title = UiFactory.CreateText("Title", box.transform, "WELCOME BACK, PILOT", UiTheme.FontTitle, UiTheme.Credits, TextAnchor.MiddleCenter);
            UiFactory.SetRegion(title.rectTransform, 0f, 0.75f, 1f, 1f, 16f);

            view.body = UiFactory.CreateText("Body", box.transform, "", UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.MiddleCenter);
            UiFactory.SetRegion(view.body.rectTransform, 0f, 0.35f, 1f, 0.75f, 24f);

            Button collect = UiFactory.CreateButton("Collect", box.transform, "COLLECT", UiTheme.FontTitle, UiTheme.ButtonPrimary, view.Collect, out _);
            UiFactory.SetRegion((RectTransform)collect.transform, 0.25f, 0.06f, 0.75f, 0.3f);

            dim.gameObject.SetActive(false);
            return view;
        }

        public void Show(double amount, double secondsAway, Action collectCallback)
        {
            onCollect = collectCallback;
            body.text = $"Your salvage drones worked for {NumberFormatter.FormatDuration(secondsAway)} while you were away.\n" +
                        $"<color=#FFD95A><size=52>+{NumberFormatter.Format(amount)} CR</size></color>";
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void Collect()
        {
            gameObject.SetActive(false);
            Action callback = onCollect;
            onCollect = null;
            callback?.Invoke();
        }
    }
}
