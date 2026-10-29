namespace AstroIdle.Content
{
    /// <summary>
    /// Every stat an <see cref="UpgradeTrack"/> can modify.
    /// Serialized as ints in .asset files: NEVER reorder or renumber existing entries, only append.
    /// Ship stats live in 0-99, weapon stats in 100-199.
    /// </summary>
    public enum StatId
    {
        // ---- Ship stats ----
        MaxHull = 0,
        HullRegen = 1,
        MoveSpeed = 2,
        CreditMultiplier = 3,
        GlobalDamageMultiplier = 4,
        GlobalFireRateMultiplier = 5,

        // ---- Weapon stats ----
        Damage = 100,
        FireRate = 101,
        ProjectileCount = 102,
        Pierce = 103,
        SplashRadius = 104,
        ProjectileSpeed = 105,
        Range = 106,
    }
}
