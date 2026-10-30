using System.Collections.Generic;
using AstroIdle.Core;
using AstroIdle.UI.Bindings;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.UI.Tabs
{
    /// <summary>
    /// Balancing/testing tools. Only shown in the editor and development builds (Debug.isDebugBuild),
    /// never in release builds. Debug credit grants do NOT count toward "credits earned" milestones.
    /// </summary>
    public class DevTab : IPanelTab
    {
        private static readonly float[] Speeds = { 1f, 2f, 4f, 8f };

        private readonly GameContext context;

        public DevTab(GameContext context)
        {
            this.context = context;
        }

        public string Title => "DEV";

        public IReadOnlyList<ISubTab> SubTabs => System.Array.Empty<ISubTab>();

        public int DefaultSubTabIndex => 0;

        public IEnumerable<IRowBinding> BuildRows(int subTabIndex)
        {
            yield return new ActionRowBinding(
                () => "Grant credits",
                () => "Adds 1,000 credits (not counted as earned).",
                () => "+1K",
                () => context.Wallet.Add(1000d, false));

            yield return new ActionRowBinding(
                () => "Grant lots of credits",
                () => "Adds 100,000 credits (not counted as earned).",
                () => "+100K",
                () => context.Wallet.Add(100000d, false));

            yield return new ActionRowBinding(
                () => "Skip wave",
                () => $"Clears the field and jumps to wave {context.Waves.CurrentWave + 1}.",
                () => "Skip",
                () => context.Waves.DebugSkipWave());

            yield return new ActionRowBinding(
                () => "Game speed",
                () => "Cycles x1 / x2 / x4 / x8 to fast-forward balance testing.",
                () => "x" + Time.timeScale.ToString("0"),
                CycleSpeed);

            bool confirmPending = false;
            yield return new ActionRowBinding(
                () => "Reset ALL progress",
                () => confirmPending ? "Tap again to permanently delete the save." : $"Deletes the save file: {NumberFormatter.Format(context.Wallet.Credits)} credits, wave {context.Save.highestWave}.",
                () => confirmPending ? "CONFIRM" : "Reset",
                () =>
                {
                    if (confirmPending) context.ResetProgress?.Invoke();
                    else confirmPending = true;
                });
        }

        private static void CycleSpeed()
        {
            int index = System.Array.IndexOf(Speeds, Time.timeScale);
            Time.timeScale = Speeds[(index + 1) % Speeds.Length];
        }
    }
}
