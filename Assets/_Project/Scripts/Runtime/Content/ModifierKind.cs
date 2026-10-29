namespace AstroIdle.Content
{
    /// <summary>
    /// How an upgrade's value combines with a base stat. Final = (base + sum(Flat)) * (1 + sum(Percent)).
    /// Serialized as ints: NEVER reorder, only append.
    /// </summary>
    public enum ModifierKind
    {
        /// <summary>Adds valuePerLevel per level to the base value (e.g. +0.5 hull regen/sec).</summary>
        Flat = 0,

        /// <summary>Adds valuePerLevel per level to the multiplier (0.15 = +15% per level).</summary>
        Percent = 1,
    }
}
