using AstroIdle.Content;
using UnityEngine;

namespace AstroIdle.Events
{
    /// <summary>Payload for <see cref="GameEvents.EnemyKilled"/>.</summary>
    public readonly struct EnemyKilledInfo
    {
        public EnemyKilledInfo(EnemyDefinition definition, Vector2 position, double creditReward)
        {
            Definition = definition;
            Position = position;
            CreditReward = creditReward;
        }

        public EnemyDefinition Definition { get; }
        public Vector2 Position { get; }

        /// <summary>Wave-scaled reward BEFORE the player's credit multiplier is applied.</summary>
        public double CreditReward { get; }
    }
}
