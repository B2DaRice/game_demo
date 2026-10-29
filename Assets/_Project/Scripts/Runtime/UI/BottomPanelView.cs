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
    /// The bottom upgrade panel: a row of tab buttons above a scrolling list of rows.
    /// Rows are rebuilt when the tab changes or the content structure changes, and refreshed (costs,
    /// affordability, levels) a few times per second.
    /// </summary>
    public class BottomPanelView : MonoBehaviour
    {
        private const float RefreshInterval = 0.2f;
        private const float TabBarHeight = 110f;
        private const float Padding = 16f;

        private readonly List<IPanelTab> tabs = new List<IPanelTab>();
        private readonly List<Button> tabButtons = new List<Button>();
        private readonly List<RowView> rows = new List<RowView>();

        private GameContext context;
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

            scroll = UiFactory.CreateVerticalScroll("List", transform, out listContent);
            UiFactory.SetAnchors((RectTransform)scroll.transform, Vector2.zero, Vector2.one, new Vector2(Padding, Padding), new Vector2(-Padding, -(TabBarHeight + Padding * 2f)));

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
            for (int i = 0; i < rows.Count; i++) rows[i].Refresh();
        }

        private void SelectTab(int index)
        {
            activeTab = index;
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].targetGraphic.color = i == activeTab ? UiTheme.ButtonPrimary : UiTheme.ButtonSecondary;
            }
            Rebuild(false);
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

            foreach (IRowBinding binding in tabs[activeTab].BuildRows())
            {
                RowView row = RowView.Create(listContent);
                row.Bind(binding);
                rows.Add(row);
            }

            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = keepScrollPosition ? scrollPosition : 1f;
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
