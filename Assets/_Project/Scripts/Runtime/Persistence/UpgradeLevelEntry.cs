using System;

namespace AstroIdle.Persistence
{
    /// <summary>Key/level pair (JsonUtility cannot serialize dictionaries).</summary>
    [Serializable]
    public class UpgradeLevelEntry
    {
        public string key;
        public int level;
    }
}
