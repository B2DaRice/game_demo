using UnityEngine;

namespace AstroIdle.Content
{
    /// <summary>
    /// Global economy and idle knobs (offline earnings, autosave). Exactly one must exist in
    /// Resources/Content/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Astro Idle/Economy Config", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("New Game")]
        [SerializeField] private double startingCredits = 0d;

        [Header("Offline Earnings")]
        [Tooltip("Fraction of the recent live credits/sec earned while the app is closed.")]
        [SerializeField] private float offlineEfficiency = 0.25f;
        [SerializeField] private float maxOfflineHours = 8f;
        [Tooltip("Absences shorter than this grant nothing (avoids popups on quick app switches).")]
        [SerializeField] private float minOfflineSeconds = 120f;

        [Header("Tracking")]
        [Tooltip("Smoothing window for the live credits/sec estimate.")]
        [SerializeField] private float creditRateWindowSeconds = 60f;
        [SerializeField] private float autosaveIntervalSeconds = 15f;

        public double StartingCredits => startingCredits;
        public float OfflineEfficiency => offlineEfficiency;
        public float MaxOfflineHours => maxOfflineHours;
        public float MinOfflineSeconds => minOfflineSeconds;
        public float CreditRateWindowSeconds => creditRateWindowSeconds;
        public float AutosaveIntervalSeconds => autosaveIntervalSeconds;
    }
}
