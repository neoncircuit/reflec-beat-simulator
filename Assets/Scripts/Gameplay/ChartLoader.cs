using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Loads a chart folder: parses the JSON file and loads its companion audio.</summary>
    public static class ChartLoader
    {
        /// <summary>Loads the first <see cref="*.rbchart.json"/> in <paramref name="chartDirectory"/> and its audio.</summary>
        public static async Task<(Chart chart, AudioClip audio)> LoadAsync(string chartDirectory, CancellationToken cancellationToken = default)
        {
            string chartPath = FindChartFile(chartDirectory);
            string json = await File.ReadAllTextAsync(chartPath, cancellationToken);
            Chart chart = ChartParser.Parse(json);

            string audioPath = ResolveAudioPath(chartDirectory, chart.Meta.AudioFile);
            AudioClip audio = await AudioLoader.LoadAsync(audioPath, cancellationToken);

            return (chart, audio);
        }

        private static string ResolveAudioPath(string chartDirectory, string audioFile)
        {
            string audioPath = Path.GetFullPath(Path.Combine(chartDirectory, audioFile));
            string root = Path.GetFullPath(chartDirectory);
            char separator = Path.DirectorySeparatorChar;

            if (!audioPath.StartsWith(root + separator, StringComparison.Ordinal) &&
                !audioPath.Equals(root, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Audio file path must be inside the chart directory: '{audioFile}'");
            }

            return audioPath;
        }

        private static string FindChartFile(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.rbchart.json"))
                return file;

            throw new FileNotFoundException($"No *.rbchart.json found in '{directory}'");
        }
    }
}
