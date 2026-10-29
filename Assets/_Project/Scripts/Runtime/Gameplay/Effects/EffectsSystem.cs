using AstroIdle.Utilities;
using UnityEngine;

namespace AstroIdle.Gameplay.Effects
{
    /// <summary>
    /// Lightweight particle effects (explosions, hit sparks, blast rings) built from pooled sprites.
    /// A fixed ring buffer is used: when it's full the oldest particle is recycled, so cost is bounded.
    /// Uses SpriteRenderers instead of ParticleSystem so no material/shader assets are required.
    /// </summary>
    public class EffectsSystem : MonoBehaviour
    {
        private const int MaxParticles = 192;
        private const int SortingOrder = 20;

        private struct Particle
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float MaxLife;
            public float StartScale;
            public float EndScale;
            public Color Color;
            public bool Active;
        }

        private Particle[] particles;
        private int nextIndex;

        public void Initialize()
        {
            particles = new Particle[MaxParticles];
            for (int i = 0; i < MaxParticles; i++)
            {
                var go = new GameObject("Particle");
                go.transform.SetParent(transform, false);
                var spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = SortingOrder;
                spriteRenderer.enabled = false;
                particles[i] = new Particle { Transform = go.transform, Renderer = spriteRenderer };
            }
        }

        /// <summary>Debris burst plus an expanding ring, scaled by the exploding object's radius.</summary>
        public void SpawnExplosion(Vector2 position, Color color, float radius)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(6f + radius * 12f), 6, 28);
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = Random.insideUnitCircle.normalized;
                float speed = Random.Range(1.5f, 4f) * (0.6f + radius);
                Emit(ShapeType.Spark, position, direction * speed, Random.Range(0.3f, 0.7f), 0.08f + radius * 0.08f, 0f, color);
            }
            Emit(ShapeType.Circle, position, Vector2.zero, 0.35f, radius * 0.6f, radius * 1.8f, color);
        }

        public void SpawnHitSpark(Vector2 position, Color color)
        {
            for (int i = 0; i < 2; i++)
            {
                Emit(ShapeType.Spark, position, Random.insideUnitCircle.normalized * 3f, 0.15f, 0.06f, 0f, color);
            }
        }

        public void SpawnBlast(Vector2 position, float radius, Color color)
        {
            Emit(ShapeType.Circle, position, Vector2.zero, 0.3f, 0.2f, radius, color);
        }

        private void Emit(ShapeType shape, Vector2 position, Vector2 velocity, float life, float startScale, float endScale, Color color)
        {
            if (particles == null) return;

            ref Particle p = ref particles[nextIndex];
            nextIndex = (nextIndex + 1) % MaxParticles;

            p.Position = position;
            p.Velocity = velocity;
            p.Life = life;
            p.MaxLife = life;
            p.StartScale = startScale;
            p.EndScale = endScale;
            p.Color = color;
            p.Active = true;

            p.Renderer.sprite = ShapeSpriteFactory.Get(shape);
            p.Renderer.color = color;
            p.Renderer.enabled = true;
            Quaternion rotation = velocity.sqrMagnitude > 0.0001f ? VectorUtils.FacingRotation(velocity) : Quaternion.identity;
            p.Transform.SetPositionAndRotation(position, rotation);
            p.Transform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            if (particles == null) return;

            float dt = Time.deltaTime;
            float damping = Mathf.Clamp01(1f - 2f * dt);
            for (int i = 0; i < particles.Length; i++)
            {
                ref Particle p = ref particles[i];
                if (!p.Active) continue;

                p.Life -= dt;
                if (p.Life <= 0f)
                {
                    p.Active = false;
                    p.Renderer.enabled = false;
                    continue;
                }

                float t = 1f - p.Life / p.MaxLife;
                p.Position += p.Velocity * dt;
                p.Velocity *= damping;
                p.Transform.position = p.Position;
                p.Transform.localScale = Vector3.one * Mathf.Lerp(p.StartScale, p.EndScale, t);

                Color color = p.Color;
                color.a = 1f - t;
                p.Renderer.color = color;
            }
        }
    }
}
