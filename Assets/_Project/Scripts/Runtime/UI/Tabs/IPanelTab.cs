using System.Collections.Generic;
using AstroIdle.UI.Bindings;

namespace AstroIdle.UI.Tabs
{
    /// <summary>
    /// One tab of the bottom panel. A tab may expose sub-tabs, shown as a vertical list on the left of the panel
    /// (e.g. one per weapon); tabs without sub-tabs return an empty list and get the full panel width.
    /// BuildRows() is called whenever the tab is (re)built — on selection, sub-tab selection, and when content
    /// structure changes (weapon unlocked, milestone completed).
    /// To add a tab: implement this and register it in BottomPanelView.CreateTabs().
    /// </summary>
    public interface IPanelTab
    {
        string Title { get; }

        /// <summary>Sub-tabs for the left-hand list. Empty = no sub-tab list.</summary>
        IReadOnlyList<ISubTab> SubTabs { get; }

        /// <summary>Sub-tab selected the first time this tab is opened.</summary>
        int DefaultSubTabIndex { get; }

        /// <param name="subTabIndex">Selected sub-tab (ignored by tabs without sub-tabs).</param>
        IEnumerable<IRowBinding> BuildRows(int subTabIndex);
    }
}
