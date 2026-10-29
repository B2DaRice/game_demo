using System.Globalization;
using AstroIdle.Content;
using AstroIdle.Utilities;

namespace AstroIdle.UI
{
    /// <summary>Human-readable display of stat values, chosen per stat (multipliers, rates, counts, ...).</summary>
    public static class StatFormatter
    {
        public static string Format(StatId stat, float value)
        {
            switch (stat)
            {
                case StatId.CreditMultiplier:
                case StatId.GlobalDamageMultiplier:
                case StatId.GlobalFireRateMultiplier:
                    return "x" + value.ToString("0.00", CultureInfo.InvariantCulture);
                case StatId.HullRegen:
                    return value.ToString("0.0", CultureInfo.InvariantCulture) + "/s";
                case StatId.FireRate:
                    return value.ToString("0.00", CultureInfo.InvariantCulture) + "/s";
                case StatId.ProjectileCount:
                case StatId.Pierce:
                    return ((int)(value + 0.5f)).ToString(CultureInfo.InvariantCulture);
                case StatId.MoveSpeed:
                case StatId.SplashRadius:
                case StatId.Range:
                case StatId.ProjectileSpeed:
                    return value.ToString("0.0", CultureInfo.InvariantCulture);
                case StatId.MaxHull:
                case StatId.Damage:
                default:
                    return value < 100f
                        ? value.ToString("0.#", CultureInfo.InvariantCulture)
                        : NumberFormatter.Format(value);
            }
        }
    }
}
