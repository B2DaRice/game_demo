using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// A one-time progression goal. When its condition reaches the target value it completes, grants its
    /// rewards (credits and/or unlocking a weapon for purchase) and is shown as done on the Milestones tab.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Milestone Definition", fileName = "Milestone_New")]
    public class MilestoneDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique id. Used in save data, so do not rename after release.")]
        [SerializeField] private string id = "new_milestone";
        [SerializeField] private string displayName = "New Milestone";
        [SerializeField] private string description = "";
        [SerializeField] private int sortOrder = 0;

        [Header("Condition")]
        [SerializeField] private MilestoneCondition condition = MilestoneCondition.ReachWave;
        [SerializeField] private double targetValue = 1d;

        [Header("Rewards")]
        [SerializeField] private double rewardCredits = 0d;
        [Tooltip("Optional weapon id made available for purchase.")]
        [SerializeField] private string unlockWeaponId = "";

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public int SortOrder => sortOrder;
        public MilestoneCondition Condition => condition;
        public double TargetValue => targetValue;
        public double RewardCredits => rewardCredits;
        public string UnlockWeaponId => unlockWeaponId;
    }
}
