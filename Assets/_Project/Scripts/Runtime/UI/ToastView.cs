using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Queued, auto-fading notification banner just below the HUD. Listens to the events that deserve a
    /// message (milestones, unlocks, wave failure) plus the generic GameEvents.ToastRequested.
    /// </summary>
    public class ToastView : MonoBehaviour
    {
        private const float DisplaySeconds = 2.4f;
        private const float FadeSeconds = 0.3f;

        private readonly Queue<string> queue = new Queue<string>();
        private CanvasGroup group;
        private Text text;
        private float timer;

        public RectTransform Rect => (RectTransform)transform;

        public static ToastView Create(Transform parent)
        {
            Image background = UiFactory.CreateImage("Toast", parent, new Color(0.05f, 0.08f, 0.15f, 0.92f));
            var view = background.gameObject.AddComponent<ToastView>();
            view.group = background.gameObject.AddComponent<CanvasGroup>();
            view.group.alpha = 0f;
            view.group.blocksRaycasts = false;
            view.group.interactable = false;

            view.text = UiFactory.CreateText("Text", background.transform, "", UiTheme.FontBody, UiTheme.TextPrimary, TextAnchor.MiddleCenter);
            UiFactory.Stretch(view.text.rectTransform, 12f);

            GameEvents.ToastRequested += view.Enqueue;
            GameEvents.MilestoneCompleted += view.OnMilestoneCompleted;
            GameEvents.WeaponUnlocked += view.OnWeaponUnlocked;
            GameEvents.WaveFailed += view.OnWaveFailed;
            return view;
        }

        public void Enqueue(string message)
        {
            queue.Enqueue(message);
        }

        private void OnDestroy()
        {
            GameEvents.ToastRequested -= Enqueue;
            GameEvents.MilestoneCompleted -= OnMilestoneCompleted;
            GameEvents.WeaponUnlocked -= OnWeaponUnlocked;
            GameEvents.WaveFailed -= OnWaveFailed;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (timer > 0f)
            {
                timer -= dt;
                group.alpha = Mathf.Clamp01(Mathf.Min(timer, DisplaySeconds - timer) / FadeSeconds);
                return;
            }

            group.alpha = 0f;
            if (queue.Count == 0) return;
            text.text = queue.Dequeue();
            timer = DisplaySeconds;
        }

        private void OnMilestoneCompleted(MilestoneDefinition milestone)
        {
            string reward = milestone.RewardCredits > 0d ? $"  +{NumberFormatter.Format(milestone.RewardCredits)} CR" : string.Empty;
            Enqueue($"<color=#7CFF9A>Goal complete:</color> {milestone.DisplayName}{reward}");
        }

        private void OnWeaponUnlocked(WeaponDefinition weapon)
        {
            Enqueue($"<color=#FFD95A>New weapon available:</color> {weapon.DisplayName}");
        }

        private void OnWaveFailed(int retreatWave)
        {
            Enqueue($"<color=#FF7A7A>Hull breached!</color> Falling back to wave {retreatWave}. Upgrade, then tap ADVANCE.");
        }
    }
}
