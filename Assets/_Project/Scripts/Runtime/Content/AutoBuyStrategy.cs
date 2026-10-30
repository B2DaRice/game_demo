namespace AstroIdle.Content
{
    /// <summary>
    /// Auto mode spending priority chosen by the player. Saved as an int in save files and in AutoBuyConfig
    /// profiles: NEVER reorder or renumber, only append.
    /// </summary>
    public enum AutoBuyStrategy
    {
        Balanced = 0,
        Attack = 1,
        Defense = 2,
    }
}
