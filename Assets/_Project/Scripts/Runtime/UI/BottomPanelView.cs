using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Events;
using AstroIdle.UI.Bindings;
using AstroIdle.UI.Tabs;
using UnityEngine;
using UnityEngine.UI;

namespace AstroIdle.UI
{
    /// <summary>
    /// The bottom upgrade panel: a row of tab buttons on top; below it an optional vertical sub-tab list on the
    /// left (for tabs that have sub-tabs, e.g. one entry per weapon) and a scrolling list of rows on the right.
    /// Rows are rebuilt when the tab/sub-tab changes or the content structure changes, and refreshed (costs,
    /// affordability, levels) a few times per second. The selected sub-tab is remembered per tab.
    /// </summary>
    public class BottomPanelView : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;
        private const float TabBarHeight = 110f;
        private const float Padding = 16f;
        private const float SidebarWidthFraction = 0.3f;

        private readonly List<IPanelTab> tabs = new List<IPanelTab>();
        private readonly List<Button> tabButtons = new List<Button>();
        private readonly List<RowView> rows = new List<RowView>();
        private readonly List<SubTabView> subTabViews = new List<SubTabView>();
        private readonly Dictionary<int, int> selectedSubTabs = new Dictionary<int, int>();

        private GameContext context;
        private RectTransform sidebar;
        private RectTransform sidebarContent;
        private ScrollRect scroll;
        private RectTransform listContent;
        private int activeTab;
        private bool rebuildRequested;
        private float refreshTimer;

        public RectTransform Rect => (RectTransform)transform;

        public static BottomPanelView Create(Transform parent, GameContext context)
        {
            RectTransform root = UiFactory.CreateRect("BottomPanel", parent);
            var view = root.gameObject.AddComponent<BottomPanelView>();
            view.Build(context);
            return view;
        }

        private void Build(GameContext gameContext)
        {
            context = gameContext;
            CreateTabs();

            RectTransform tabBar = UiFactory.CreateRect("TabBar", transform);
            UiFactory.SetAnchors(tabBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(Padding, -(TabBarHeight + Padding)), new Vector2(-Padding, -Padding));
            var tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 12f;
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;

            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i;
                Button button = UiFactory.CreateButton("Tab_" + tabs[i].Title, tabBar, tabs[i].Title, UiTheme.FontTitle, UiTheme.ButtonSecondary, () => SelectTab(index), out _);
                tabButtons.Add(button);
            }

            ScrollRect sidebarScroll = UiFactory.CreateVerticalScroll("SubTabs", transform, out sidebarContent);
            sidebar = (RectTransform)sidebarScroll.transform;
            UiFactory.SetAnchors(sidebar, Vector2.zero, new Vector2(SidebarWidthFraction, 1f), new Vector2(Padding, Padding), new Vector2(0f, -(TabBarHeight + Padding * 2f)));

            scroll = UiFactory.CreateVerticalScroll("List", transform, out listContent);

            GameEvents.WeaponUnlocked += OnStructureChanged;
            GameEvents.WeaponPurchased += OnStructureChanged;
            GameEvents.MilestoneCompleted += OnMilestoneCompleted;

            SelectTab(0);
        }

        private void CreateTabs()
        {
            tabs.Add(new ShipTab(context));
            tabs.Add(new WeaponsTab(context));
            tabs.Add(new MilestonesTab(context));
            if (Debug.isDebugBuild) tabs.Add(new DevTab(context));
        }

        private void OnDestroy()
        {
            GameEvents.WeaponUnlocked -= OnStructureChanged;
            GameEvents.WeaponPurchased -= OnStructureChanged;
            GameEvents.MilestoneCompleted -= OnMilestoneCompleted;
        }

        private void Update()
        {
            if (rebuildRequested)
            {
                rebuildRequested = false;
                Rebuild(true);
            }

            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = RefreshInterval;
            RefreshAll();
        }

        private void SelectTab(int index)
        {
            activeTab = index;
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].targetGraphic.color = i == activeTab ? UiTheme.ButtonPrimary : UiTheme.ButtonSecondary;
            }

            if (!selectedSubTabs.ContainsKey(activeTab)) selectedSubTabs[activeTab] = tabs[activeTab].DefaultSubTabIndex;
            BuildSidebar();
            Rebuild(false);
        }

        private void SelectSubTab(int index)
        {
            selectedSubTabs[activeTab] = index;
            for (int i = 0; i < subTabViews.Count; i++) subTabViews[i].SetSelected(i == index);
            Rebuild(false);
        }

        /// <summary>Recreates the sub-tab list for the active tab and sizes the row list beside it.</summary>
        private void BuildSidebar()
        {
            foreach (SubTabView view in subTabViews)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            subTabViews.Clear();

            IReadOnlyList<ISubTab> subTabs = tabs[activeTab].SubTabs;
            bool hasSidebar = subTabs.Count > 0;
            sidebar.gameObject.SetActive(hasSidebar);

            float listLeft = hasSidebar ? SidebarWidthFraction : 0f;
            float leftPadding = hasSidebar ? Padding * 0.75f : Padding;
            UiFactory.SetAnchors((RectTransform)scroll.transform, new Vector2(listLeft, 0f), Vector2.one, new Vector2(leftPadding, Padding), new Vector2(-Padding, -(TabBarHeight + Padding * 2f)));

            int selected = selectedSubTabs[activeTab];
            for (int i = 0; i < subTabs.Count; i++)
            {
                int index = i;
                SubTabView view = SubTabView.Create(sidebarContent, subTabs[i], () => SelectSubTab(index));
                view.SetSelected(i == selected);
                subTabViews.Add(view);
            }
        }

        private void Rebuild(bool keepScrollPosition)
        {
            float scrollPosition = scroll.verticalNormalizedPosition;

            foreach (RowView row in rows)
            {
                // Deactivate first: Destroy is deferred to end of frame and the layout would still count it.
                row.gameObject.SetActive(false);
                Destroy(row.gameObject);
            }
            rows.Clear();

            foreach (IRowBinding binding in tabs[activeTab].BuildRows(selectedSubTabs[activeTab]))
            {
                RowView row = RowView.Create(listContent);
                row.Bind(binding);
                rows.Add(row);
            }

            // Rows size their height from wrapped text, which needs their final width: lay out, re-measure, lay out.
            Canvas.ForceUpdateCanvases();
            RefreshAll();
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = keepScrollPosition ? scrollPosition : 1f;
        }

        private void RefreshAll()
        {
            for (int i = 0; i < rows.Count; i++) rows[i].Refresh();
            for (int i = 0; i < subTabViews.Count; i++) subTabViews[i].Refresh();
        }

        private void OnStructureChanged(WeaponDefinition weapon)
        {
            rebuildRequested = true;
        }

        private void OnMilestoneCompleted(MilestoneDefinition milestone)
        {
            rebuildRequested = true;
        }
    }
}
