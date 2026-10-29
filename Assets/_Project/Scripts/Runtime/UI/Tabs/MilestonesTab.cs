using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.UI.Bindings;

namespace AstroIdle.UI.Tabs
{
    /// <summary>Milestones in progress first (in sort order), then completed ones.</summary>
    public class MilestonesTab : IPanelTab
    {
        private readonly GameContext context;

        public MilestonesTab(GameContext context)
        {
            this.context = context;
        }

        public string Title => "GOALS";

        public IEnumerable<IRowBinding> BuildRows()
        {
            foreach (MilestoneDefinition milestone in context.Content.Milestones)
            {
                if (!context.Milestones.IsCompleted(milestone)) yield return new MilestoneRowBinding(context, milestone);
            }

            foreach (MilestoneDefinition milestone in context.Content.Milestones)
            {
                if (context.Milestones.IsCompleted(milestone)) yield return new MilestoneRowBinding(context, milestone);
            }
        }
    }
}
