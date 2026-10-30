using System;
using AstroIdle.Content;
using AstroIdle.Core;
using AstroIdle.Gameplay;
using AstroIdle.Gameplay.Arena;
using AstroIdle.Gameplay.Combat;
using AstroIdle.Gameplay.Effects;
using AstroIdle.Gameplay.Enemies;
using AstroIdle.Gameplay.Player;
using AstroIdle.Gameplay.Waves;
using AstroIdle.Persistence;
using AstroIdle.Progression;
using AstroIdle.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AstroIdle.Bootstrap
{
    /// <summary>
    /// Composition root and the ONLY component placed in the scene. On Awake it loads content and the save,
    /// constructs every service, builds the world and UI from code, then runs the app-level loop
    /// (autosave, milestone checks, offline earnings, screen-size changes).
    /// Startup order matters and is documented in Assets/_Project/Scripts/Runtime/Bootstrap/README.md.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private const float MilestoneCheckInterval = 0.25f;

        [Tooltip("Optional. Falls back to Camera.main, or creates a camera if none exists.")]
        [SerializeField] private Camera worldCamera;

        private GameContext context;
        private UIRoot ui;
        private Starfield starfield;
        private float autosaveTimer;
        private float milestoneTimer;
        private int lastScreenWidth;
        private int lastScreenHeight;
        private Rect lastSafeArea;
        private bool savingDisabled;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Time.timeScale = 1f;

            ContentDatabase content = ContentDatabase.LoadFromResources();
            foreach (string error in content.Validate()) Debug.LogError("[Content] " + error);
            if (content.Ship == null || content.Waves == null || content.Economy == null)
            {
                Debug.LogError("[GameBootstrap] Required content is missing; the game cannot start. Run 'Astro Idle > Validate Content'.");
                enabled = false;
                return;
            }

            CreateServices(content);
            BuildWorld(content);

            ui = new GameObject("UI").AddComponent<UIRoot>();
            ui.Build(context);
            CacheScreenState();
        }

        private void Start()
        {
            if (context == null) return;
            context.Waves.Begin();
            TryGrantOfflineEarnings();
        }

        private void Update()
        {
            if (context == null) return;

            context.Rewards.Tick(Time.deltaTime);
            context.AutoBuy.Tick(Time.unscaledDeltaTime);

            milestoneTimer -= Time.unscaledDeltaTime;
            if (milestoneTimer <= 0f)
            {
                milestoneTimer = MilestoneCheckInterval;
                context.Milestones.Evaluate();
            }

            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= context.Content.Economy.AutosaveIntervalSeconds)
            {
                autosaveTimer = 0f;
                SaveGame();
            }

            if (ScreenChanged()) HandleScreenChanged();
        }

        private void OnApplicationPause(bool paused)
        {
            if (context == null) return;
            if (paused) SaveGame();
            else TryGrantOfflineEarnings();
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnDestroy()
        {
            context?.Rewards?.Dispose();
        }

        private void CreateServices(ContentDatabase content)
        {
            var saveService = new SaveService();
            SaveData save = saveService.Load(out bool isNewGame);

            var wallet = new CurrencyWallet(save);
            var weapons = new WeaponInventory(content, save, wallet);
            weapons.EnsureStartingWeapons(UnityEngine.Random.value);
            if (isNewGame) save.credits = content.Economy.StartingCredits;

            var upgrades = new UpgradeService(content, save, wallet);

            context = new GameContext
            {
                Content = content,
                Save = save,
                SaveService = saveService,
                Wallet = wallet,
                Weapons = weapons,
                Upgrades = upgrades,
                Milestones = new MilestoneService(content, save, wallet, weapons),
                Rewards = new RewardService(save, wallet, upgrades, content.Economy),
                AutoBuy = new AutoBuyService(content, save, wallet, upgrades, weapons),
                SaveNow = SaveGame,
                ResetProgress = ResetProgress,
            };
        }

        private void BuildWorld(ContentDatabase content)
        {
            Camera camera = worldCamera != null ? worldCamera : Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                camera = cameraObject.AddComponent<Camera>();
            }
            worldCamera = camera;

            var arena = new PlayArea();
            arena.Recalculate(camera, ArenaCamera.Configure(camera));

            Transform worldRoot = new GameObject("World").transform;
            var enemies = new EnemySpawner(CreateChild(worldRoot, "Enemies"));
            var projectiles = new ProjectileSpawner(CreateChild(worldRoot, "Projectiles"));
            var effects = CreateChild(worldRoot, "Effects").gameObject.AddComponent<EffectsSystem>();
            effects.Initialize();

            var gameplay = new GameplayContext(content, arena, new CombatRegistry(), enemies, projectiles, effects, context.Upgrades, context.Weapons);
            enemies.Bind(gameplay);
            projectiles.Bind(gameplay);

            starfield = CreateChild(worldRoot, "Starfield").gameObject.AddComponent<Starfield>();
            starfield.Build(arena.Bounds);

            var player = CreateChild(worldRoot, "PlayerShip").gameObject.AddComponent<PlayerShip>();
            gameplay.Registry.Player = player;
            player.Initialize(content.Ship, gameplay);

            var director = CreateChild(worldRoot, "WaveDirector").gameObject.AddComponent<WaveDirector>();
            director.Initialize(gameplay, context.Save);

            context.Gameplay = gameplay;
            context.Player = player;
            context.Waves = director;
        }

        private void TryGrantOfflineEarnings()
        {
            if (ui == null || ui.OfflinePopup.IsOpen) return;

            EconomyConfig economy = context.Content.Economy;
            long nowTicks = DateTime.UtcNow.Ticks;
            long lastTicks = context.Save.lastSaveUtcTicks;
            if (lastTicks <= 0 || lastTicks > nowTicks) return; // never saved, or the clock moved backwards

            double secondsAway = TimeSpan.FromTicks(nowTicks - lastTicks).TotalSeconds;
            if (secondsAway < economy.MinOfflineSeconds) return;

            double amount = OfflineEarnings.Calculate(secondsAway, context.Save.creditsPerSecond, economy.OfflineEfficiency, economy.MaxOfflineHours);
            context.Save.lastSaveUtcTicks = nowTicks; // consume the absence so it can't be granted twice
            if (amount < 1d) return;

            double cappedSeconds = Math.Min(secondsAway, economy.MaxOfflineHours * 3600d);
            ui.OfflinePopup.Show(amount, cappedSeconds, () =>
            {
                context.Wallet.Add(amount);
                SaveGame();
            });
        }

        private void SaveGame()
        {
            if (savingDisabled || context == null) return;
            context.SaveService.Save(context.Save);
        }

        private void ResetProgress()
        {
            savingDisabled = true;
            SaveService.DeleteSaveFile();
            Time.timeScale = 1f;

            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            if (sceneIndex < 0)
            {
                Debug.LogError("[GameBootstrap] Active scene is not in Build Settings; run 'Astro Idle > Setup > Run Project Setup'.");
                return;
            }
            SceneManager.LoadScene(sceneIndex);
        }

        private bool ScreenChanged()
        {
            return Screen.width != lastScreenWidth || Screen.height != lastScreenHeight || Screen.safeArea != lastSafeArea;
        }

        private void HandleScreenChanged()
        {
            PlayArea arena = context.Gameplay.Arena;
            arena.Recalculate(worldCamera, ArenaCamera.Configure(worldCamera));
            starfield.Build(arena.Bounds);
            ui.ApplyLayout();
            CacheScreenState();
        }

        private void CacheScreenState()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastSafeArea = Screen.safeArea;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
    }
}
