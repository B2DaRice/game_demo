using AstroIdle.Content;
using AstroIdle.Gameplay.Arena;
using AstroIdle.Gameplay.Combat;
using AstroIdle.Gameplay.Effects;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Gameplay.Player;
using AstroIdle.Progression;

namespace AstroIdle.Gameplay
{
    /// <summary>
    /// The dependencies gameplay entities need, handed to them on Initialize/Activate.
    /// Built once by GameBootstrap. Entities never search the scene (no Find* calls); they use this.
    /// </summary>
    public class GameplayContext
    {
        public GameplayContext(
            ContentDatabase content,
            PlayArea arena,
            CombatRegistry registry,
            EnemySpawner enemies,
            ProjectileSpawner projectiles,
            EffectsSystem effects,
            UpgradeService upgrades,
            WeaponInventory weapons)
        {
            Content = content;
            Arena = arena;
            Registry = registry;
            Enemies = enemies;
            Projectiles = projectiles;
            Effects = effects;
            Upgrades = upgrades;
            Weapons = weapons;
        }

        public ContentDatabase Content { get; }
        public PlayArea Arena { get; }
        public CombatRegistry Registry { get; }
        public EnemySpawner Enemies { get; }
        public ProjectileSpawner Projectiles { get; }
        public EffectsSystem Effects { get; }
        public UpgradeService Upgrades { get; }
        public WeaponInventory Weapons { get; }

        public PlayerShip Player => Registry.Player;
    }
}
