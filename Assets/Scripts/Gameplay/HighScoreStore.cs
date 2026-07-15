using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Persists the best result for each chart to Application.persistentDataPath as JSON.
    /// </summary>
    public static class HighScoreStore
    {
        private static string FilePath => Path.Combine(Application.persistentDataPath, "highscores.json");

        /// <summary>A single chart's best recorded result.</summary>
        [System.Serializable]
        public class HighScoreEntry
        {
            public string chartPath;
            public string grade;
            public double achievementRate;
            public int maxCombo;
            public int justCount;
            public int greatCount;
            public int goodCount;
            public int missCount;
            public string date;
        }

        [System.Serializable]
        private class HighScoreData
        {
            public List<HighScoreEntry> entries = new List<HighScoreEntry>();
        }

        /// <summary>Loads the best stored entry for the given chart path, or null.</summary>
        public static HighScoreEntry LoadBest(string chartPath)
        {
            if (string.IsNullOrEmpty(chartPath))
                return null;

            string key = NormalizePath(chartPath);
            HighScoreData data = LoadData();
            return data.entries.FirstOrDefault(e => NormalizePath(e.chartPath) == key);
        }

        /// <summary>
        /// Saves the score as the new best for the chart if it beats the existing entry
        /// by achievement rate or, on a tie, by max combo.
        /// </summary>
        public static void SaveIfBetter(string chartPath, Score score)
        {
            if (string.IsNullOrEmpty(chartPath) || score == null)
                return;

            string key = NormalizePath(chartPath);
            HighScoreData data = LoadData();
            HighScoreEntry existing = data.entries.FirstOrDefault(e => NormalizePath(e.chartPath) == key);

            if (existing != null && !IsBetterThan(score, existing))
                return;

            var entry = new HighScoreEntry
            {
                chartPath = key,
                grade = score.Grade,
                achievementRate = score.AchievementRate,
                maxCombo = score.MaxCombo,
                justCount = score.JustCount,
                greatCount = score.GreatCount,
                goodCount = score.GoodCount,
                missCount = score.MissCount,
                date = DateTime.UtcNow.ToString("O")
            };

            if (existing != null)
                data.entries.Remove(existing);

            data.entries.Add(entry);
            SaveData(data);
        }

        private static bool IsBetterThan(Score score, HighScoreEntry existing)
        {
            if (score.AchievementRate > existing.achievementRate)
                return true;

            if (Math.Abs(score.AchievementRate - existing.achievementRate) < 1e-9 && score.MaxCombo > existing.maxCombo)
                return true;

            return false;
        }

        private static HighScoreData LoadData()
        {
            if (!File.Exists(FilePath))
                return new HighScoreData();

            string json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<HighScoreData>(json);
            return data ?? new HighScoreData();
        }

        private static void SaveData(HighScoreData data)
        {
            if (data == null)
                return;

            File.WriteAllText(FilePath, JsonUtility.ToJson(data));
        }

        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/') ?? string.Empty;
        }
    }
}
