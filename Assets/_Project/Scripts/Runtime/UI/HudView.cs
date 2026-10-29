using AstroIdle.Core;
using AstroIdle.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// Top HUD over the play field: credits and income, wave / best wave, hull bar, wave progress bar and the
    /// Auto-advance / Advance toggle. Polls game state ~10x per second (cheap and decoupled).
    /// </summary>
    public class HudView : MonoBehaviour
    {
        private const float RefreshInterval = 0.1f;

        private GameContext context;
        private Text creditsText;
        private Text rateText;
        private Text waveText;
        private Text bestText;
        private ProgressBar hullBar;
        private ProgressBar waveBar;
        private Button advanceButton;
        private Text advanceLabel;
        private float refreshTimer;

        public RectTransform Rect => (RectTransform)transform;

        public static HudView Create(Transform parent, GameContext context)
        {
            Image background = UiFactory.CreateImage("Hud", parent, UiTheme.HudBackground);
            var view = background.gameObject.AddComponent<HudView>();
            view.Build(context);
            return view;
        }

        private void Build(GameContext gameContext)
        {
            context = gameContext;
            Transform root = transform;

            // Band A (top): credits / wave
            creditsText = UiFactory.CreateText("Credits", root, "0", UiTheme.FontLarge, UiTheme.Credits, TextAnchor.MiddleLeft);
            UiFactory.SetAnchors(creditsText.rectTransform, new Vector2(0f, 0.55f), new Vector2(0.6f, 1f), new Vector2(28f, 0f), new Vector2(0f, -6f));

            waveText = UiFactory.CreateText("Wave", root, "WAVE 1", UiTheme.FontLarge, UiTheme.TextPrimary, TextAnchor.MiddleRight);
            UiFactory.SetAnchors(waveText.rectTransform, new Vector2(0.4f, 0.55f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(-28f, -6f));

            // Band B (middle): income / best
            rateText = UiFactory.CreateText("Rate", root, "", UiTheme.FontSmall, UiTheme.TextSecondary, TextAnchor.MiddleLeft);
            UiFactory.SetAnchors(rateText.rectTransform, new Vector2(0f, 0.34f), new Vector2(0.6f, 0.55f), new Vector2(28f, 0f), Vector2.zero);

            bestText = UiFactory.CreateText("Best", root, "", UiTheme.FontSmall, UiTheme.TextSecondary, TextAnchor.MiddleRight);
            UiFactory.SetAnchors(bestText.rectTransform, new Vector2(0.4f, 0.34f), new Vector2(1f, 0.55f), Vector2.zero, new Vector2(-28f, 0f));

            // Band C (bottom): hull bar, wave progress, advance toggle
            hullBar = ProgressBar.Create("HullBar", root, UiTheme.HullBar, true);
            UiFactory.SetAnchors(hullBar.Root, new Vector2(0f, 0.06f), new Vector2(0.5f, 0.3f), new Vector2(28f, 0f), new Vector2(-8f, 0f));

            waveBar = ProgressBar.Create("WaveBar", root, UiTheme.WaveBar, true);
            UiFactory.SetAnchors(waveBar.Root, new Vector2(0.5f, 0.06f), new Vector2(0.72f, 0.3f), new Vector2(8f, 0f), new Vector2(-8f, 0f));

            advanceButton = UiFactory.CreateButton("Advance", root, "", UiTheme.FontSmall, UiTheme.ButtonSecondary, OnAdvanceClicked, out advanceLabel);
            UiFactory.SetAnchors((RectTransform)advanceButton.transform, new Vector2(0.72f, 0.03f), new Vector2(1f, 0.33f), new Vector2(8f, 0f), new Vector2(-28f, 0f));

            Refresh();
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
            creditsText.text = NumberFormatter.Format(context.Wallet.Credits) + " <size=34>CR</size>";
            rateText.text = "+" + NumberFormatter.FormatRate(context.Rewards.CreditsPerSecond) + " CR/sec";

            int wave = context.Waves.CurrentWave;
            waveText.text = context.Waves.IsBossWave ? $"<color=#FF7A7A>BOSS</color> {wave}" : $"WAVE {wave}";
            bestText.text = "Best wave " + context.Save.highestWave;

            float maxHull = Mathf.Max(1f, context.Player.MaxHull);
            hullBar.SetValue(context.Player.Hull / maxHull);
            hullBar.Label.text = $"HULL {NumberFormatter.Format(context.Player.Hull)}";

            waveBar.SetValue(context.Waves.Progress);
            waveBar.Label.text = "WAVE";

            bool auto = context.Save.autoAdvance;
            advanceLabel.text = auto ? "AUTO\nON" : "ADVANCE\n>>";
            advanceButton.targetGraphic.color = auto ? UiTheme.ButtonSecondary : UiTheme.ButtonWarning;
        }

        private void OnAdvanceClicked()
        {
            context.Waves.SetAutoAdvance(!context.Save.autoAdvance);
            Refresh();
        }
    }
}
