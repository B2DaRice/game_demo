using System;
using System.Globalization;

namespace AstroIdle.Utilities
{
    /// <summary>
    /// Formats large idle-game numbers as short strings: 999, 1.23K, 45.6M, 789B, ... then scientific (1.23e36).
    /// Values are truncated (not rounded) so the display never shows more than the player actually has.
    /// </summary>
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes =
        {
            "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc"
        };

        /// <summary>Whole-number display used for credits, costs and counts.</summary>
        public static string Format(double value)
        {
            return Format(value, 0);
        }

        /// <summary>Display for rates (e.g. credits/sec) where small values need a decimal place.</summary>
        public static string FormatRate(double value)
        {
            return Format(value, Math.Abs(value) < 100d ? 1 : 0);
        }

        /// <param name="value">Number to format.</param>
        /// <param name="decimalsBelowThousand">Decimal places to show when |value| &lt; 1000.</param>
        public static string Format(double value, int decimalsBelowThousand)
        {
            if (double.IsNaN(value)) return "NaN";
            if (double.IsInfinity(value)) return value > 0 ? "inf" : "-inf";

            bool negative = value < 0d;
            double abs = Math.Abs(value);
            string result;

            if (abs < 1000d)
            {
                double truncated = Truncate(abs, decimalsBelowThousand);
                string format = decimalsBelowThousand > 0 ? "0." + new string('#', decimalsBelowThousand) : "0";
                result = truncated.ToString(format, CultureInfo.InvariantCulture);
            }
            else
            {
                int tier = (int)Math.Floor(Math.Log10(abs) / 3d);
                double scaled = abs / Math.Pow(1000d, tier);

                // Guard against floating point error at exact boundaries (e.g. Log10(1e6) = 5.9999...).
                if (scaled >= 1000d)
                {
                    tier++;
                    scaled /= 1000d;
                }
                else if (scaled < 1d)
                {
                    tier--;
                    scaled *= 1000d;
                }

                if (tier >= Suffixes.Length)
                {
                    result = abs.ToString("0.00e0", CultureInfo.InvariantCulture);
                }
                else
                {
                    int decimals = scaled >= 100d ? 0 : scaled >= 10d ? 1 : 2;
                    string format = decimals == 0 ? "0" : "0." + new string('0', decimals);
                    result = Truncate(scaled, decimals).ToString(format, CultureInfo.InvariantCulture) + Suffixes[tier];
                }
            }

            return negative ? "-" + result : result;
        }

        /// <summary>Formats a duration like "2h 05m" or "45s".</summary>
        public static string FormatDuration(double seconds)
        {
            if (seconds < 0d) seconds = 0d;
            var span = TimeSpan.FromSeconds(seconds);
            if (span.TotalHours >= 1d) return $"{(int)span.TotalHours}h {span.Minutes:00}m";
            if (span.TotalMinutes >= 1d) return $"{span.Minutes}m {span.Seconds:00}s";
            return $"{span.Seconds}s";
        }

        private static double Truncate(double value, int decimals)
        {
            double factor = Math.Pow(10d, decimals);
            // Small epsilon prevents 1.23 being shown as 1.22 due to binary floating point.
            return Math.Floor(value * factor + 1e-9) / factor;
        }
    }
}
