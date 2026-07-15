using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReflecBeatSimulator.UI
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
                string label = FormatLabel(_chartDirectories[i]);
                _ui?.AddButton(label, i);
            }

            if (_chartDirectories.Count == 0)
                _ui?.AddButton("No charts found", -1);

            _ui?.SetInteractable(true);
        }

        private static string FormatLabel(string directory)
        {
            string chartPath = FindChartFile(directory);
            if (chartPath == null)
                return Path.GetFileName(directory);

            try
            {
                string json = File.ReadAllText(chartPath);
                Chart chart = ChartParser.Parse(json);
                return $"{chart.Meta.Title} - {chart.Meta.Artist} [{chart.Meta.Difficulty} {chart.Meta.Level}]";
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ChartSelectController: Failed to read '{chartPath}': {ex.Message}");
                return Path.GetFileName(directory);
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
            _loadCts = new CancellationTokenSource();

            _isLoading = true;
            if (_ui != null)
                _ui.SetInteractable(false);

            string directory = _chartDirectories[index];
            ChartSession.Clear();

            try
            {
                var (chart, audio) = await ChartLoader.LoadAsync(directory, _loadCts.Token);
                ChartSession.Chart = chart;
                ChartSession.AudioClip = audio;
                ChartSession.Autoplay = _ui != null && _ui.Autoplay;
                ChartSession.SyncCheck = _ui != null && _ui.SyncCheck;
                ChartSession.HideRivalVisuals = _ui != null && _ui.HideRivalVisuals;
                ChartSession.ApproachTime = _ui != null ? _ui.ApproachTime : Constants.DefaultApproachTime;
                SceneManager.LoadScene("GameplayScene");
            }
            catch (System.OperationCanceledException)
            {
                _isLoading = false;
                _ui?.SetInteractable(true);
                // load was canceled (e.g., scene change); do nothing
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load chart from '{directory}': {ex.Message}");
                _isLoading = false;
                if (_ui != null)
                    _ui.SetInteractable(true);
            }
            finally
            {
                _loadCts?.Dispose();
                _loadCts = null;
            }
        }

        private void OnCalibrate()
        {
            ChartSession.Clear();
            SceneManager.LoadScene("CalibrationScene");
        }
    }
}
