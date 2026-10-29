using System;

namespace AstroIdle.Progression
{
    /// <summary>Pure calculation of credits earned while the app was closed.</summary>
    public static class OfflineEarnings
    {
        /// <returns>floor(min(secondsAway, maxHours) * creditsPerSecond * efficiency), or 0.</returns>
        public static double Calculate(double secondsAway, double creditsPerSecond, float efficiency, float maxHours)
        {
            if (secondsAway <= 0d || creditsPerSecond <= 0d || efficiency <= 0f) return 0d;
            double capped = Math.Min(secondsAway, maxHours * 3600d);
            return Math.Floor(capped * creditsPerSecond * efficiency);
        }
    }
}
