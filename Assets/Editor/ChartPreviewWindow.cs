#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ReflecBeatSimulator.EditorTools
{
    /// <summary>
    /// Editor window for authoring charts: create chart folders, list existing charts,
    /// and preview a time range of a chart in the gameplay scene.
    /// </summary>
    public class ChartPreviewWindow : EditorWindow
    {
        private const string GameplayScenePath = "Assets/Scenes/GameplayScene.unity";
        private const string NewChartNameKey = "ReflecBeatSimulator.NewChartName";
        private const string NewAudioPathKey = "ReflecBeatSimulator.NewAudioPath";

        private List<string> _chartDirectories = new List<string>();
        private Vector2 _scrollPosition;
        private int _selectedIndex = -1;

        private float _startTime;
        private float _duration = 10f;
        private bool _autoplay = true;

        private string _newChartName = "";
        private string _newAudioPath = "";
        private string _statusMessage = "";

        private static EnterPlayModeOptions s_savedEnterPlayModeOptions;
        private static bool s_savedEnterPlayModeOptionsEnabled;
        private static bool s_restoringPlayModeOptions;

        [MenuItem("Reflec Beat/Chart Preview")]
        public static void ShowWindow()
        {
            GetWindow<ChartPreviewWindow>("Chart Preview");
        }

        private void OnEnable()
        {
            _newChartName = SessionState.GetString(NewChartNameKey, "");
            _newAudioPath = SessionState.GetString(NewAudioPathKey, "");
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Refresh();
        }

        private void OnDisable()
        {
            SessionState.SetString(NewChartNameKey, _newChartName);
            SessionState.SetString(NewAudioPathKey, _newAudioPath);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnGUI()
        {
            GUILayout.Label("Chart Preview", EditorStyles.boldLabel);

            DrawChartList();
            DrawPreviewControls();
            DrawNewChartSection();
            DrawStatus();
        }

        private void DrawChartList()
        {
            GUILayout.Label("Charts", EditorStyles.boldLabel);

            if (_chartDirectories.Count == 0)
            {
                EditorGUILayout.HelpBox("No charts found. Create one below or add chart folders to your persistentDataPath/charts/ directory.", MessageType.Info);
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            for (int i = 0; i < _chartDirectories.Count; i++)
            {
                string label = FormatLabel(_chartDirectories[i]);
                bool selected = i == _selectedIndex;
                bool newSelected = GUILayout.Toggle(selected, label, EditorStyles.radioButton);
                if (newSelected && !selected)
                    _selectedIndex = i;
            }
            GUILayout.EndScrollView();
        }

        private void DrawPreviewControls()
        {
            GUILayout.Label("Preview Range", EditorStyles.boldLabel);
            _startTime = EditorGUILayout.FloatField("Start Time (sec)", _startTime);
            _duration = EditorGUILayout.FloatField("Duration (sec)", _duration);
            _autoplay = EditorGUILayout.Toggle("Autoplay", _autoplay);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
                Refresh();

            GUI.enabled = _selectedIndex >= 0 && _duration > 0f;
            if (GUILayout.Button("Preview Section"))
                _ = PreviewSelectedAsync();
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNewChartSection()
        {
            GUILayout.Label("New Chart", EditorStyles.boldLabel);
            _newChartName = EditorGUILayout.TextField("Chart Name", _newChartName);

            EditorGUILayout.BeginHorizontal();
            _newAudioPath = EditorGUILayout.TextField("Audio File", _newAudioPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
            {
                string path = EditorUtility.OpenFilePanel("Select Audio", "", "wav,mp3,ogg");
                if (!string.IsNullOrEmpty(path))
                    _newAudioPath = path;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Chart"))
            {
                string directory = ChartAuthoringTools.CreateChartFolder(_newChartName, _newAudioPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _statusMessage = $"Created chart at {directory}";
                    _newChartName = "";
                    _newAudioPath = "";
                    Refresh();
                }
            }
        }

        private void DrawStatus()
        {
            if (string.IsNullOrEmpty(_statusMessage))
                return;

            EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
        }

        private void Refresh()
        {
            _chartDirectories = ChartAuthoringTools.DiscoverChartDirectories();
            if (_selectedIndex >= _chartDirectories.Count)
                _selectedIndex = _chartDirectories.Count - 1;
        }

        private static string FormatLabel(string directory)
        {
            Chart chart = ChartAuthoringTools.LoadChart(directory);
            if (chart != null)
                return $"{chart.Meta.Title} - {chart.Meta.Artist} [{chart.Meta.Difficulty} {chart.Meta.Level}]";

            return Path.GetFileName(directory);
        }

        private async Task PreviewSelectedAsync()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _chartDirectories.Count)
                return;

            string directory = _chartDirectories[_selectedIndex];
            double startSec = Math.Max(0.0, _startTime);
            double endSec = startSec + Math.Max(0.1, _duration);

            try
            {
                using var cts = new CancellationTokenSource();
                var (chart, audio) = await ChartLoader.LoadAsync(directory, cts.Token);

                if (audio == null)
                {
                    _statusMessage = "Failed to load audio clip.";
                    return;
                }

                ChartSession.SetPreview(chart, audio, startSec, endSec);
                ChartSession.Autoplay = _autoplay;

                SaveEnterPlayModeOptions();
                OpenGameplayScene();
                EnterPlayModeWithDomainReloadDisabled();

                _statusMessage = "Entering preview...";
            }
            catch (Exception ex)
            {
                _statusMessage = $"Preview failed: {ex.Message}";
                Debug.LogError($"ChartPreviewWindow: Preview failed for '{directory}': {ex}");
            }
        }

        private static void OpenGameplayScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(GameplayScenePath);
        }

        private static void SaveEnterPlayModeOptions()
        {
            s_savedEnterPlayModeOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            s_savedEnterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            s_restoringPlayModeOptions = true;
        }

        private static void EnterPlayModeWithDomainReloadDisabled()
        {
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!s_restoringPlayModeOptions)
                return;

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorSettings.enterPlayModeOptionsEnabled = s_savedEnterPlayModeOptionsEnabled;
                EditorSettings.enterPlayModeOptions = s_savedEnterPlayModeOptions;
                s_restoringPlayModeOptions = false;
            }
        }
    }
}
#endif
