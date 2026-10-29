using System.Collections.Generic;
using AstroIdle.Content;
using AstroIdle.Persistence;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AstroIdle.Editor
{
    /// <summary>Developer menu items under "Astro Idle" in the Unity menu bar.</summary>
    public static class AstroIdleMenu
    {
        [MenuItem("Astro Idle/Open Main Scene", priority = 20)]
        public static void OpenMainScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ProjectBootstrapper.MainScenePath);
            }
        }

        [MenuItem("Astro Idle/Validate Content", priority = 40)]
        public static void ValidateContent()
        {
            ContentDatabase content = ContentDatabase.LoadFromResources();
            List<string> errors = content.Validate();
            string summary = $"{content.Weapons.Count} weapons, {content.Enemies.Count} enemies, {content.Milestones.Count} milestones";

            if (errors.Count == 0)
            {
                Debug.Log($"[Astro Idle] Content OK ({summary}).");
                return;
            }

            foreach (string error in errors) Debug.LogError("[Content] " + error);
            Debug.LogError($"[Astro Idle] Content has {errors.Count} problem(s) ({summary}).");
        }

        [MenuItem("Astro Idle/Save Data/Delete Save", priority = 60)]
        public static void DeleteSave()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Astro Idle] Stop Play mode first (or use the DEV tab's Reset button).");
                return;
            }

            if (EditorUtility.DisplayDialog("Delete save?", "Permanently delete the local save file at:\n" + SaveService.SavePath, "Delete", "Cancel"))
            {
                SaveService.DeleteSaveFile();
                Debug.Log("[Astro Idle] Save deleted.");
            }
        }

        [MenuItem("Astro Idle/Save Data/Open Save Folder", priority = 61)]
        public static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
