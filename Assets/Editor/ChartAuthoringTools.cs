#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.EditorTools
{
    /// <summary>
    /// Static helpers for creating chart folders and starter files from the Unity editor.
    /// </summary>
    public static class ChartAuthoringTools
    {
        /// <summary>
        /// Creates a chart folder under <see cref="Application.persistentDataPath"/>/charts/<paramref name="chartName"/>,
        /// copies <paramref name="sourceAudioPath"/> into it, and writes a starter <c>*.rbchart.json</c> file.
        /// </summary>
        /// <returns>The full path to the new chart directory, or <c>null</c> if creation failed.</returns>
        public static string CreateChartFolder(string chartName, string sourceAudioPath)
        {
            if (string.IsNullOrWhiteSpace(chartName))
            {
                Debug.LogError("ChartAuthoringTools: Chart name must not be empty.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(sourceAudioPath) || !File.Exists(sourceAudioPath))
            {
                Debug.LogError($"ChartAuthoringTools: Audio file not found: '{sourceAudioPath}'");
                return null;
            }

            string safeName = SanitizeDirectoryName(chartName);
            if (string.IsNullOrWhiteSpace(safeName))
            {
                Debug.LogError("ChartAuthoringTools: Chart name contains only invalid characters.");
                return null;
            }

            string chartsRoot = Path.Combine(Application.persistentDataPath, "charts");
            string chartDirectory = Path.Combine(chartsRoot, safeName);

            try
            {
                Directory.CreateDirectory(chartDirectory);

                string audioFileName = Path.GetFileName(sourceAudioPath);
                string destinationAudioPath = Path.Combine(chartDirectory, audioFileName);
                File.Copy(sourceAudioPath, destinationAudioPath, overwrite: true);

                string chartPath = Path.Combine(chartDirectory, $"{safeName}.rbchart.json");
                Chart chart = BuildStarterChart(chartName, audioFileName);
                ChartWriter.Write(chart, chartPath);

                return chartDirectory;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ChartAuthoringTools: Failed to create chart folder '{chartDirectory}': {ex.Message}");
                return null;
            }
        }

        /// <summary>Returns the full paths of every chart directory that contains a <c>*.rbchart.json</c> file.</summary>
        public static List<string> DiscoverChartDirectories()
        {
            string chartsRoot = Path.Combine(Application.persistentDataPath, "charts");
            if (!Directory.Exists(chartsRoot))
                return new List<string>();

            return Directory.GetDirectories(chartsRoot)
                .Where(directory => Directory.GetFiles(directory, "*.rbchart.json").Length > 0)
                .OrderBy(directory => directory)
                .ToList();
        }

        /// <summary>Reads a chart folder's JSON file and returns the parsed chart.</summary>
        public static Chart LoadChart(string chartDirectory)
        {
            string chartPath = FindChartFile(chartDirectory);
            if (chartPath == null)
            {
                Debug.LogError($"ChartAuthoringTools: No *.rbchart.json found in '{chartDirectory}'");
                return null;
            }

            try
            {
                string json = File.ReadAllText(chartPath);
                return ChartParser.Parse(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ChartAuthoringTools: Failed to parse chart in '{chartDirectory}': {ex.Message}");
                return null;
            }
        }

        private static string FindChartFile(string directory)
        {
            return Directory.GetFiles(directory, "*.rbchart.json").FirstOrDefault();
        }

        private static Chart BuildStarterChart(string title, string audioFileName)
        {
            return new Chart
            {
                Meta = new ChartMeta
                {
                    Title = title,
                    Artist = "Unknown",
                    Charter = Environment.UserName,
                    AudioFile = audioFileName,
                    Difficulty = "EASY",
                    Level = 1
                },
                Timing = new ChartTiming
                {
                    Ppq = 480,
                    AudioOffsetMs = 0.0,
                    BpmChanges = new List<BpmChange>
                    {
                        new BpmChange { Tick = 0, Bpm = 120.0 }
                    }
                },
                Notes = new List<Note>()
            };
        }

        private static string SanitizeDirectoryName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string sanitized = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
            return sanitized.Trim(' ', '.', '_');
        }
    }
}
#endif
