using System.IO;
using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Persists user-facing gameplay settings to Application.persistentDataPath as JSON.
    /// </summary>
    public static class SettingsStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

        /// <summary>Serializable gameplay settings container.</summary>
        [System.Serializable]
        public class GameSettings
        {
            public double approachTimeSec = Constants.DefaultApproachTime;
            public bool hideRivalVisuals = false;
            public bool autoplay = false;
            public bool syncCheck = false;
        }

        /// <summary>Loads stored settings, or sensible defaults if none exist.</summary>
        public static GameSettings Load()
        {
            if (!File.Exists(FilePath))
                return Default();

            string json = File.ReadAllText(FilePath);
            var settings = JsonUtility.FromJson<GameSettings>(json);
            return settings ?? Default();
        }

        /// <summary>Saves the given settings to disk.</summary>
        public static void Save(GameSettings settings)
        {
            if (settings == null)
                return;

            File.WriteAllText(FilePath, JsonUtility.ToJson(settings));
        }

        /// <summary>Returns a fresh default settings instance.</summary>
        public static GameSettings Default()
        {
            return new GameSettings();
        }
    }
}
