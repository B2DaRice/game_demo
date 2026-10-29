using System.Collections.Generic;
using System.IO;
using AstroIdle.Bootstrap;
using AstroIdle.Gameplay.Arena;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AstroIdle.Editor
{
    /// <summary>
    /// One-time project setup that would otherwise be manual clicking in the editor:
    ///  1. Creates Assets/_Project/Scenes/Main.unity (camera + GameBootstrap) if it doesn't exist.
    ///  2. Makes sure that scene is first in the build scene list.
    ///  3. Applies Android/player settings (package id, portrait, IL2CPP + ARM64) if they're still defaults.
    /// Runs automatically once per editor session after scripts compile; re-run from
    /// "Astro Idle > Setup > Run Project Setup" (which also force-applies the player settings).
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBootstrapper
    {
        public const string MainScenePath = "Assets/_Project/Scenes/Main.unity";

        private const string CompanyName = "B2DaRice";
        private const string ProductName = "Astro Idle";
        private const string AndroidPackageId = "com.b2darice.astroidle";
        private const string BundleVersion = "0.1.0";
        private const string SessionKey = "AstroIdle.ProjectBootstrapper.Ran";

        static ProjectBootstrapper()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            EditorApplication.delayCall += () => RunSetup(false);
        }

        [MenuItem("Astro Idle/Setup/Run Project Setup", priority = 0)]
        public static void RunSetupFromMenu()
        {
            RunSetup(true);
        }

        private static void RunSetup(bool forcePlayerSettings)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            bool createdScene = EnsureMainScene();
            EnsureSceneInBuildSettings();
            if (forcePlayerSettings || HasDefaultPlayerSettings()) ApplyPlayerSettings();

            if (createdScene) Debug.Log($"[Astro Idle] Created {MainScenePath} and added it to the build. Press Play to run the game.");
        }

        private static bool EnsureMainScene()
        {
            if (File.Exists(MainScenePath)) return false;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            Directory.CreateDirectory(Path.GetDirectoryName(MainScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ArenaCamera.BackgroundColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();

            var gameObject = new GameObject("Game");
            var bootstrap = gameObject.AddComponent<GameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("worldCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MainScenePath);
            AssetDatabase.Refresh();
            return true;
        }

        private static void EnsureSceneInBuildSettings()
        {
            if (!File.Exists(MainScenePath)) return;

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int index = scenes.FindIndex(s => s.path == MainScenePath);
            if (index == 0 && scenes[0].enabled) return;

            if (index >= 0) scenes.RemoveAt(index);
            scenes.Insert(0, new EditorBuildSettingsScene(MainScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static bool HasDefaultPlayerSettings()
        {
            string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            return string.IsNullOrEmpty(id) || id.StartsWith("com.DefaultCompany") || PlayerSettings.companyName == "DefaultCompany";
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = BundleVersion;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidPackageId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();
            Debug.Log($"[Astro Idle] Applied player settings: {ProductName} ({AndroidPackageId}), portrait, IL2CPP/ARM64.");
        }
    }
}
