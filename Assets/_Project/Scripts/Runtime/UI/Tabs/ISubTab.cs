using UnityEngine;

namespace AstroIdle.UI.Tabs
{
    /// <summary>
    /// One entry in a tab's left-hand sub-tab list. Properties are re-read on every refresh, so return live
    /// values (e.g. "LOCKED" until a weapon is unlocked, then its total upgrade level).
    /// </summary>
    public interface ISubTab
    {
        string Title { get; }
        string Subtitle { get; }

        /// <summary>Color of the accent stripe on the left edge of the entry.</summary>
        Color Accent { get; }

        /// <summary>True to draw the entry muted (e.g. a weapon not unlocked yet).</summary>
        bool Dimmed { get; }
    }
}
