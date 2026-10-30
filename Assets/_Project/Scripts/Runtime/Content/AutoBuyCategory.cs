namespace AstroIdle.Content
{
    /// <summary>
    /// Buckets that auto mode weighs against each other. Every purchasable thing falls into exactly one
    /// (see AutoBuyPlanner.CategoryFor). Append only.
    /// </summary>
    public enum AutoBuyCategory
    {
        /// <summary>Any weapon upgrade track (damage, fire rate, pierce, ...).</summary>
        WeaponUpgrade = 0,

        /// <summary>Unlocking another weapon.</summary>
        WeaponUnlock = 1,

        /// <summary>Ship upgrades that boost all weapons (global damage / fire rate).</summary>
        ShipOffense = 2,

        /// <summary>Ship survivability: max hull, regen, move speed.</summary>
        ShipDefense = 3,

        /// <summary>Ship income: credit multiplier.</summary>
        ShipEconomy = 4,
    }
}
