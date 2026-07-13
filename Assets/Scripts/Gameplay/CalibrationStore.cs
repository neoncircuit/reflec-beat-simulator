using System.IO;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Persists the calibrated input offset to Application.persistentDataPath as JSON.
    /// </summary>
    public static class CalibrationStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "calibration.json");

        [System.Serializable]
        private class CalibrationData
        {
            public double inputOffsetSec;
        }

        /// <summary>Saves the measured input offset.</summary>
        public static void Save(double inputOffsetSec)
        {
            var data = new CalibrationData { inputOffsetSec = inputOffsetSec };
            File.WriteAllText(FilePath, JsonUtility.ToJson(data));
        }

        /// <summary>Loads the stored input offset, or 0 if no calibration exists.</summary>
        public static double Load()
        {
            if (!File.Exists(FilePath))
                return 0.0;

            string json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<CalibrationData>(json);
            return data?.inputOffsetSec ?? 0.0;
        }
    }
}
