using System;
using System.IO;
using UnityEngine;

namespace AstroIdle.Persistence
{
    /// <summary>
    /// Loads/saves <see cref="SaveData"/> as JSON in Application.persistentDataPath.
    /// Writes go to a temp file first and are then swapped in, so a crash mid-write can't corrupt the save.
    /// </summary>
    public class SaveService
    {
        public const string FileName = "astroidle_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>Loads the save, or returns a fresh one if none exists / it can't be read.</summary>
        public SaveData Load(out bool isNewGame)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null)
                    {
                        Migrate(data);
                        isNewGame = false;
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Could not read save, starting a new game. {e}");
                BackUpCorruptSave();
            }

            isNewGame = true;
            return new SaveData { lastSaveUtcTicks = DateTime.UtcNow.Ticks };
        }

        public void Save(SaveData data)
        {
            try
            {
                data.version = SaveData.CurrentVersion;
                data.lastSaveUtcTicks = DateTime.UtcNow.Ticks;

                string tempPath = SavePath + ".tmp";
                File.WriteAllText(tempPath, JsonUtility.ToJson(data, Debug.isDebugBuild));
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tempPath, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Save failed. {e}");
            }
        }

        /// <summary>Deletes the save file (used by "Reset progress" and the editor menu).</summary>
        public static void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Could not delete save. {e}");
            }
        }

        /// <summary>Upgrades older save versions in place. Add a step here whenever CurrentVersion is bumped.</summary>
        private static void Migrate(SaveData data)
        {
            // Example for the future:
            // if (data.version < 2) { ...convert fields...; data.version = 2; }
            if (data.currentWave < 1) data.currentWave = 1;
            if (data.highestWave < data.currentWave) data.highestWave = data.currentWave;
            data.version = SaveData.CurrentVersion;
        }

        private static void BackUpCorruptSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Copy(SavePath, SavePath + ".corrupt", true);
            }
            catch (Exception)
            {
                // Best effort only.
            }
        }
    }
}
