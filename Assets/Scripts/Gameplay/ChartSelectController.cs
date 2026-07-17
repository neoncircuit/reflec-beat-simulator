using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Populates the chart-select screen and loads the chosen chart into the gameplay scene.</summary>
    public sealed class ChartSelectController : MonoBehaviour
    {
        [SerializeField] private ChartSelectUI _ui;

        private readonly List<string> _chartDirectories = new List<string>();
        private bool _isLoading;
        private CancellationTokenSource _loadCts;

        private void Start()
        {
            if (_ui != null)
            {
                _ui.OnChartSelected += OnChartSelected;
                _ui.OnCalibrate += OnCalibrate;
            }

            if (ContentBootstrapper.IsComplete)
            {
                RefreshList();
            }
            else
            {
                _ui?.SetTitle("Loading charts...");
                ContentBootstrapper.OnBootstrapComplete += OnBootstrapComplete;
                ContentBootstrapper.OnBootstrapFailed += OnBootstrapFailed;
            }
        }

        private void OnBootstrapComplete()
        {
            ContentBootstrapper.OnBootstrapComplete -= OnBootstrapComplete;
            ContentBootstrapper.OnBootstrapFailed -= OnBootstrapFailed;
            RefreshList();
        }

        private void OnBootstrapFailed()
        {
            ContentBootstrapper.OnBootstrapComplete -= OnBootstrapComplete;
            ContentBootstrapper.OnBootstrapFailed -= OnBootstrapFailed;
            _ui?.SetTitle("Failed to load charts");
        }

        private void OnDestroy()
        {
            if (_ui != null)
            {
                _ui.OnChartSelected -= OnChartSelected;
                _ui.OnCalibrate -= OnCalibrate;
            }

            ContentBootstrapper.OnBootstrapComplete -= OnBootstrapComplete;
            ContentBootstrapper.OnBootstrapFailed -= OnBootstrapFailed;

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
        }

        private void RefreshList()
        {
            _chartDirectories.Clear();
            _chartDirectories.AddRange(ChartRepository.DiscoverChartDirectories());

            Debug.Log($"ChartSelectController: found {_chartDirectories.Count} chart directories.");
            for (int i = 0; i < _chartDirectories.Count; i++)
                Debug.Log($"  [{i}] {_chartDirectories[i]}");

            _ui?.SetTitle("Select Chart");
            _ui?.ClearButtons();

            for (int i = 0; i < _chartDirectories.Count; i++)
            {
                (string title, string artist, string difficulty, string best) = FormatLabelParts(_chartDirectories[i]);
                _ui?.AddButton(title, artist, difficulty, best, i);
            }

            if (_chartDirectories.Count == 0)
                _ui?.AddButton("No charts found", string.Empty, string.Empty, string.Empty, -1);

            _ui?.SetInteractable(true);
        }

        private static (string title, string artist, string difficulty, string best) FormatLabelParts(string directory)
        {
            string chartPath = FindChartFile(directory);
            if (chartPath == null)
                return (Path.GetFileName(directory), string.Empty, string.Empty, string.Empty);

            try
            {
                string json = File.ReadAllText(chartPath);
                Chart chart = ChartParser.Parse(json);
                string title = chart.Meta.Title;
                string artist = chart.Meta.Artist;
                string difficulty = $"[{chart.Meta.Difficulty} {chart.Meta.Level}]";

                var best = HighScoreStore.LoadBest(directory);
                string bestText = best != null
                    ? $"Best: {best.grade} {best.achievementRate:F2}%"
                    : string.Empty;

                return (title, artist, difficulty, bestText);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ChartSelectController: Failed to read '{chartPath}': {ex.Message}");
                return (Path.GetFileName(directory), string.Empty, string.Empty, string.Empty);
            }
        }

        private static string FindChartFile(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.rbchart.json"))
                return file;

            return null;
        }

        private async void OnChartSelected(int index)
        {
            if (_isLoading || index < 0 || index >= _chartDirectories.Count)
                return;

            _loadCts?.Cancel();
            _loadCts?.Dispose();

            var cts = new CancellationTokenSource();
            _loadCts = cts;

            _isLoading = true;
            if (_ui != null)
                _ui.SetInteractable(false);

            string directory = _chartDirectories[index];
            ChartSession.Clear();

            try
            {
                var (chart, audio) = await ChartLoader.LoadAsync(directory, cts.Token);

                // If another selection started while we awaited, abandon this one.
                if (_loadCts != cts)
                {
                    cts.Dispose();
                    return;
                }

                ChartSession.Chart = chart;
                ChartSession.AudioClip = audio;
                ChartSession.ChartPath = directory;
                ChartSession.Autoplay = _ui != null && _ui.Autoplay;
                ChartSession.SyncCheck = _ui != null && _ui.SyncCheck;
                ChartSession.HideRivalVisuals = _ui != null && _ui.HideRivalVisuals;
                ChartSession.ApproachTime = _ui != null ? _ui.ApproachTime : Constants.DefaultApproachTime;

                SettingsStore.Save(new SettingsStore.GameSettings
                {
                    ApproachTimeSec = ChartSession.ApproachTime,
                    HideRivalVisuals = ChartSession.HideRivalVisuals,
                    Autoplay = ChartSession.Autoplay,
                    SyncCheck = ChartSession.SyncCheck
                });

                TransitionManager.LoadScene("GameplayScene");
            }
            catch (System.OperationCanceledException)
            {
                if (_loadCts == cts)
                {
                    _isLoading = false;
                    _ui?.SetInteractable(true);
                }
                // load was canceled (e.g., scene change or newer selection); do nothing
            }
            catch (System.Exception ex)
            {
                if (_loadCts == cts)
                {
                    Debug.LogError($"Failed to load chart from '{directory}': {ex.Message}");
                    _isLoading = false;
                    _ui?.SetInteractable(true);
                }
            }
            finally
            {
                if (_loadCts == cts)
                {
                    cts.Dispose();
                    _loadCts = null;
                }
            }
        }

        private void OnCalibrate()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
            _isLoading = false;
            _ui?.SetInteractable(true);

            ChartSession.Clear();
            TransitionManager.LoadScene("CalibrationScene");
        }
    }
}
