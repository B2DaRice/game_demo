using System;
using AstroIdle.Content;
using UnityEngine;

namespace AstroIdle.Events
{
    /// <summary>
    /// Global static event hub. Systems announce what happened here instead of referencing each other,
    /// keeping gameplay, progression and UI decoupled (e.g. an Enemy never knows the wallet exists).
    ///
    /// Rules (see docs/CODING_STANDARDS.md):
    ///  - Only raise events through the Raise* methods.
    ///  - Every subscriber MUST unsubscribe in OnDestroy / Dispose.
    ///  - Handlers must not assume ordering relative to other handlers.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>An enemy was destroyed by the player (grants rewards). Not raised for collisions/clears.</summary>
        public static event Action<EnemyKilledInfo> EnemyKilled;

        /// <summary>Any enemy entered play (wave spawn or split child).</summary>
        public static event Action EnemySpawned;

        /// <summary>Any enemy left play for any reason (killed, rammed the player, cleared).</summary>
        public static event Action EnemyRemoved;

        public static event Action<int> WaveStarted;
        public static event Action<int> WaveCompleted;

        /// <summary>The ship was destroyed; argument is the wave the player falls back to.</summary>
        public static event Action<int> WaveFailed;

        public static event Action PlayerDestroyed;
        public static event Action<bool> AutoAdvanceChanged;
        public static event Action<double> CreditsChanged;

        /// <summary>Argument is the upgrade save key "{ownerId}.{trackId}".</summary>
        public static event Action<string> UpgradePurchased;

        public static event Action<WeaponDefinition> WeaponUnlocked;
        public static event Action<WeaponDefinition> WeaponPurchased;
        public static event Action<MilestoneDefinition> MilestoneCompleted;

        /// <summary>Generic short message for the toast UI.</summary>
        public static event Action<string> ToastRequested;

        public static void RaiseEnemyKilled(EnemyKilledInfo info) => EnemyKilled?.Invoke(info);
        public static void RaiseEnemySpawned() => EnemySpawned?.Invoke();
        public static void RaiseEnemyRemoved() => EnemyRemoved?.Invoke();
        public static void RaiseWaveStarted(int wave) => WaveStarted?.Invoke(wave);
        public static void RaiseWaveCompleted(int wave) => WaveCompleted?.Invoke(wave);
        public static void RaiseWaveFailed(int retreatWave) => WaveFailed?.Invoke(retreatWave);
        public static void RaisePlayerDestroyed() => PlayerDestroyed?.Invoke();
        public static void RaiseAutoAdvanceChanged(bool enabled) => AutoAdvanceChanged?.Invoke(enabled);
        public static void RaiseCreditsChanged(double credits) => CreditsChanged?.Invoke(credits);
        public static void RaiseUpgradePurchased(string upgradeKey) => UpgradePurchased?.Invoke(upgradeKey);
        public static void RaiseWeaponUnlocked(WeaponDefinition weapon) => WeaponUnlocked?.Invoke(weapon);
        public static void RaiseWeaponPurchased(WeaponDefinition weapon) => WeaponPurchased?.Invoke(weapon);
        public static void RaiseMilestoneCompleted(MilestoneDefinition milestone) => MilestoneCompleted?.Invoke(milestone);
        public static void RaiseToast(string message) => ToastRequested?.Invoke(message);

        /// <summary>
        /// Removes every subscriber. Runs automatically before each play session so stale handlers from a
        /// previous session can't leak when "Enter Play Mode Options" disables domain reload.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearAll()
        {
            EnemyKilled = null;
            EnemySpawned = null;
            EnemyRemoved = null;
            WaveStarted = null;
            WaveCompleted = null;
            WaveFailed = null;
            PlayerDestroyed = null;
            AutoAdvanceChanged = null;
            CreditsChanged = null;
            UpgradePurchased = null;
            WeaponUnlocked = null;
            WeaponPurchased = null;
            MilestoneCompleted = null;
            ToastRequested = null;
        }
    }
}
