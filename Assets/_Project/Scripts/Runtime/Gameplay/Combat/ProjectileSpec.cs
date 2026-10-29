using AstroIdle.Content;
using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Combat
{
    /// <summary>Fully resolved (post-upgrade) projectile parameters for one shot.</summary>
    public struct ProjectileSpec
    {
        public float Damage;
        public float Speed;
        public float Lifetime;
        public int Pierce;
        public float SplashRadius;
        public ProjectileMotion Motion;
        public float TurnRate;
        public ShapeType Shape;
        public float Size;
        public Color Color;
    }
}
