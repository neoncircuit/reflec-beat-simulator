using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Discovers chart folders under <see cref="Application.persistentDataPath"/>/charts.</summary>
    public static class ChartRepository
    {
        public static string ChartsRoot => Path.Combine(Application.persistentDataPath, "charts");

        /// <summary>Returns the full directory paths of every chart that contains a <see cref="*.rbchart.json"/>.</summary>
        public static IEnumerable<string> DiscoverChartDirectories()
        {
            if (!Directory.Exists(ChartsRoot))
                yield break;

            foreach (string directory in Directory.GetDirectories(ChartsRoot))
            {
                if (HasChartFile(directory))
                    yield return directory;
            }
        }

        private static bool HasChartFile(string directory)
        {
            return Directory.GetFiles(directory, "*.rbchart.json").Length > 0;
        }
    }
}
