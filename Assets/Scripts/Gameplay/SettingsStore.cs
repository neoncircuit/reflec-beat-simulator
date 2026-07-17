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
            [SerializeField] private double approachTimeSec = Constants.DefaultApproachTime;
            [SerializeField] private bool hideRivalVisuals = false;
            [SerializeField] private bool autoplay = false;
            [SerializeField] private bool syncCheck = false;

            public double ApproachTimeSec
            {
                get => approachTimeSec;
                set => approachTimeSec = value;
            }

            public bool HideRivalVisuals
            {
                get => hideRivalVisuals;
                set => hideRivalVisuals = value;
            }

            public bool Autoplay
            {
                get => autoplay;
                set => autoplay = value;
            }

            public bool SyncCheck
            {
                get => syncCheck;
                set => syncCheck = value;
            }
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
