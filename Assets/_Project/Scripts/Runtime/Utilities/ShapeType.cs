namespace AstroIdle.Utilities
{
    /// <summary>
    /// Procedural vector shapes available to content (ships, enemies, projectiles, effects).
    /// Serialized as ints in .asset files: NEVER reorder or renumber, only append.
    /// </summary>
    public enum ShapeType
    {
        Ship = 0,
        Circle = 1,
        Diamond = 2,
        Square = 3,
        Hexagon = 4,
        Rock = 5,
        Saucer = 6,
        Dot = 7,
        Bolt = 8,
        Missile = 9,
        Spark = 10,
    }
}
