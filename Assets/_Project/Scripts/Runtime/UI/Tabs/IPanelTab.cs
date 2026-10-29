using System.Collections.Generic;
using AstroIdle.UI.Bindings;

namespace AstroIdle.UI.Tabs
{
    /// <summary>
    /// One tab of the bottom panel. BuildRows() is called whenever the tab is (re)built — on selection and
    /// when content structure changes (weapon bought/unlocked, milestone completed).
    /// To add a tab: implement this and register it in BottomPanelView.CreateTabs().
    /// </summary>
    public interface IPanelTab
    {
        string Title { get; }

        IEnumerable<IRowBinding> BuildRows();
    }
}
