using AstroIdle.Content;
using AstroIdle.Events;
using AstroIdle.Persistence;
using UnityEngine;

namespace AstroIdle.Gameplay.Waves
{
    /// <summary>
    /// Runs the wave loop: Spawning -> (all spawned and none alive) -> Intermission -> next wave.
    ///  - Auto advance ON: each cleared wave moves to the next.
    ///  - Auto advance OFF ("farming"): the same wave repeats until the player taps Advance.
    ///  - Ship destroyed: everything is cleared, the player falls back FailWaveSetback waves and auto advance
    ///    turns off, so the player farms an easier wave until they've upgraded enough.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Spawning,
            Intermission,
        }

        private GameplayContext context;
        private SaveData save;
        private WaveConfig config;

        private State state = State.Idle;
        private int plannedRoots;
        private int rootsSpawned;
        private int spawnedTotal;
        private int resolved;
        private float spawnTimer;
        private float intermissionTimer;
        private int nextWave;
        private bool lastWaveCleared;
        private bool pendingFail;

        public int CurrentWave => save.currentWave;
        public bool IsBossWave => WaveMath.IsBossWave(config, save.currentWave);

        /// <summary>0..1 share of this wave's enemies (including split children) that are gone.</summary>
        public float Progress
        {
            get
            {
                if (state == State.Intermission) return lastWaveCleared ? 1f : 0f;
                int total = plannedRoots + Mathf.Max(0, spawnedTotal - rootsSpawned);
                return total <= 0 ? 0f : Mathf.Clamp01((float)resolved / total);
            }
        }

        public void Initialize(GameplayContext gameplay, SaveData saveData)
        {
            context = gameplay;
            save = saveData;
            config = gameplay.Content.Waves;

            GameEvents.EnemySpawned += OnEnemySpawned;
            GameEvents.EnemyRemoved += OnEnemyRemoved;
            GameEvents.PlayerDestroyed += OnPlayerDestroyed;
        }

        /// <summary>Starts the saved current wave. Call once after everything is initialized.</summary>
        public void Begin()
        {
            StartWave(Mathf.Max(1, save.currentWave));
        }

        public void SetAutoAdvance(bool enabled)
        {
            if (save.autoAdvance == enabled) return;
            save.autoAdvance = enabled;

            // If we're between waves after a clear, honour the change immediately.
            if (state == State.Intermission && lastWaveCleared) nextWave = enabled ? save.currentWave + 1 : save.currentWave;
            GameEvents.RaiseAutoAdvanceChanged(enabled);
        }

        /// <summary>Debug: clears the field and jumps to the next wave.</summary>
        public void DebugSkipWave()
        {
            context.Enemies.DespawnAll();
            context.Projectiles.DespawnAll();
            lastWaveCleared = true;
            nextWave = save.currentWave + 1;
            state = State.Intermission;
            intermissionTimer = 0.1f;
        }

        private void OnDestroy()
        {
            GameEvents.EnemySpawned -= OnEnemySpawned;
            GameEvents.EnemyRemoved -= OnEnemyRemoved;
            GameEvents.PlayerDestroyed -= OnPlayerDestroyed;
        }

        private void Update()
        {
            if (context == null || state == State.Idle) return;

            // Failure is handled here, a frame later, rather than inside the damage call stack.
            if (pendingFail)
            {
                HandleFail();
                return;
            }

            float dt = Time.deltaTime;
            if (state == State.Spawning) UpdateSpawning(dt);
            else if (state == State.Intermission) UpdateIntermission(dt);
        }

        private void StartWave(int wave)
        {
            save.currentWave = wave;
            if (wave > save.highestWave) save.highestWave = wave;

            plannedRoots = WaveMath.EnemyCount(config, wave) + (WaveMath.IsBossWave(config, wave) ? 1 : 0);
            rootsSpawned = 0;
            spawnedTotal = 0;
            resolved = 0;
            spawnTimer = 0.5f;
            lastWaveCleared = false;
            state = State.Spawning;

            GameEvents.RaiseWaveStarted(wave);
            if (WaveMath.IsBossWave(config, wave)) GameEvents.RaiseToast($"Wave {wave}: BOSS INCOMING");
        }

        private void UpdateSpawning(float dt)
        {
            if (rootsSpawned < plannedRoots)
            {
                spawnTimer -= dt;
                if (spawnTimer <= 0f && context.Registry.EnemyCount < config.MaxConcurrentEnemies)
                {
                    SpawnNext();
                    spawnTimer = WaveMath.SpawnInterval(config, save.currentWave);
                }
                return;
            }

            if (context.Registry.EnemyCount == 0) CompleteWave();
        }

        private void SpawnNext()
        {
            int wave = save.currentWave;
            bool spawnBoss = WaveMath.IsBossWave(config, wave) && rootsSpawned == plannedRoots - 1;
            string enemyId = spawnBoss ? config.BossEnemyId : SpawnPicker.Pick(config.SpawnTable, wave, Random.value);
            EnemyDefinition definition = context.Content.GetEnemy(enemyId);

            rootsSpawned++;
            if (definition == null)
            {
                Debug.LogWarning($"[WaveDirector] No spawnable enemy for wave {wave} (id '{enemyId}').");
                resolved++; // count it as done so the wave can still finish
                return;
            }

            context.Enemies.Spawn(definition, context.Arena.RandomSpawnPoint(), WaveMath.Scaling(config, wave));
        }

        private void CompleteWave()
        {
            int wave = save.currentWave;
            lastWaveCleared = true;
            nextWave = save.autoAdvance ? wave + 1 : wave;
            state = State.Intermission;
            intermissionTimer = config.IntermissionSeconds;
            GameEvents.RaiseWaveCompleted(wave);
        }

        private void UpdateIntermission(float dt)
        {
            intermissionTimer -= dt;
            if (intermissionTimer <= 0f) StartWave(nextWave);
        }

        private void HandleFail()
        {
            pendingFail = false;
            context.Enemies.DespawnAll();
            context.Projectiles.DespawnAll();

            int retreatWave = Mathf.Max(1, save.currentWave - config.FailWaveSetback);
            SetAutoAdvance(false);

            lastWaveCleared = false;
            nextWave = retreatWave;
            state = State.Intermission;
            intermissionTimer = config.IntermissionSeconds + 1f;

            context.Player.Respawn();
            GameEvents.RaiseWaveFailed(retreatWave);
        }

        private void OnEnemySpawned()
        {
            if (state == State.Spawning) spawnedTotal++;
        }

        private void OnEnemyRemoved()
        {
            if (state == State.Spawning) resolved++;
        }

        private void OnPlayerDestroyed()
        {
            pendingFail = true;
        }
    }
}
