namespace AstroIdle.Content
{
    /// <summary>
    /// Selects the AI used by an enemy (see Gameplay/Enemies/EnemyBehaviourFactory).
    /// Serialized as ints: NEVER reorder, only append.
    /// </summary>
    public enum EnemyBehaviourType
    {
        /// <summary>Flies straight through the arena and wraps around the edges (classic asteroid).</summary>
        Drift = 0,

        /// <summary>Steers directly toward the player and rams it.</summary>
        Chase = 1,

        /// <summary>Holds a preferred distance, circles the player and fires projectiles.</summary>
        Shooter = 2,
    }
}
