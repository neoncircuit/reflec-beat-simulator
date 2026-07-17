using System;
using System.Collections.Generic;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Renders the chart-select list and option toggles.</summary>
    public sealed class ChartSelectUI : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _chartButtonPrefab;
        [SerializeField] private Toggle _autoplayToggle;
        [SerializeField] private Toggle _syncCheckToggle;
        [SerializeField] private Toggle _hideRivalToggle;
        [SerializeField] private Button _calibrateButton;
        [SerializeField] private Text _approachTimeLabel;
        [SerializeField] private Button _approachTimePrevButton;
        [SerializeField] private Button _approachTimeNextButton;

        private readonly List<GameObject> _buttons = new List<GameObject>();
        private int _approachTimeIndex;

        /// <summary>Invoked with the chart index when a chart button is pressed.</summary>
        public event Action<int> OnChartSelected;

        /// <summary>Invoked when the Calibrate button is pressed.</summary>
        public event Action OnCalibrate;

        /// <summary>Whether the autoplay toggle is on.</summary>
        public bool Autoplay => _autoplayToggle != null && _autoplayToggle.isOn;

        /// <summary>Whether the sync-check toggle is on.</summary>
        public bool SyncCheck => _syncCheckToggle != null && _syncCheckToggle.isOn;

        /// <summary>Whether rival note visuals should be hidden.</summary>
        public bool HideRivalVisuals => _hideRivalToggle != null && _hideRivalToggle.isOn;

        /// <summary>The selected approach time in seconds.</summary>
        public double ApproachTime
        {
            get
            {
                if (_approachTimeIndex < 0 || _approachTimeIndex >= Constants.ApproachTimeOptions.Count)
                    return Constants.DefaultApproachTime;

                return Constants.ApproachTimeOptions[_approachTimeIndex];
            }
        }

        private void Start()
        {
            ApplySettings(SettingsStore.Load());
            UpdateApproachTimeLabel();

            if (_calibrateButton != null)
                _calibrateButton.onClick.AddListener(() => OnCalibrate?.Invoke());

            if (_approachTimePrevButton != null)
                _approachTimePrevButton.onClick.AddListener(() => CycleApproachTime(-1));

            if (_approachTimeNextButton != null)
                _approachTimeNextButton.onClick.AddListener(() => CycleApproachTime(1));
        }

        private void OnDestroy()
        {
            if (_calibrateButton != null)
                _calibrateButton.onClick.RemoveAllListeners();

            if (_approachTimePrevButton != null)
                _approachTimePrevButton.onClick.RemoveAllListeners();

            if (_approachTimeNextButton != null)
                _approachTimeNextButton.onClick.RemoveAllListeners();
        }

        /// <summary>Applies stored settings to the UI controls.</summary>
        public void ApplySettings(SettingsStore.GameSettings settings)
        {
            if (settings == null)
                return;

            if (_autoplayToggle != null)
                _autoplayToggle.isOn = settings.Autoplay;

            if (_syncCheckToggle != null)
                _syncCheckToggle.isOn = settings.SyncCheck;

            if (_hideRivalToggle != null)
                _hideRivalToggle.isOn = settings.HideRivalVisuals;

            SetApproachTimeIndexFromValue(settings.ApproachTimeSec);
        }

        private void CycleApproachTime(int delta)
        {
            int count = Constants.ApproachTimeOptions.Count;
            if (count == 0)
                return;

            _approachTimeIndex = (_approachTimeIndex + delta + count) % count;
            UpdateApproachTimeLabel();
        }

        private void SetApproachTimeIndexFromValue(double approachTimeSec)
        {
            _approachTimeIndex = 0;
            for (int i = 0; i < Constants.ApproachTimeOptions.Count; i++)
            {
                if (Math.Abs(Constants.ApproachTimeOptions[i] - approachTimeSec) < 1e-6)
                {
                    _approachTimeIndex = i;
                    break;
                }
            }

            UpdateApproachTimeLabel();
        }

        private void UpdateApproachTimeLabel()
        {
            if (_approachTimeLabel != null)
                _approachTimeLabel.text = $"Approach: {ApproachTime:0.00}s";
        }

        public void SetTitle(string title)
        {
            if (_titleText != null)
                _titleText.text = title;
        }

        public void ClearButtons()
        {
            foreach (GameObject button in _buttons)
            {
                if (button == null)
                    continue;

                var buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                    buttonComponent.onClick.RemoveAllListeners();

                Destroy(button);
            }

            _buttons.Clear();
        }

        public void AddButton(string title, string artist, string difficulty, string best, int index)
        {
            if (_content == null)
            {
                Debug.LogError("ChartSelectUI: _content is not assigned; cannot add button.");
                return;
            }

            if (_chartButtonPrefab == null)
            {
                Debug.LogError("ChartSelectUI: _chartButtonPrefab is not assigned; cannot add button.");
                return;
            }

            GameObject button = Instantiate(_chartButtonPrefab, _content, false);
            button.name = $"ChartButton_{index}";

            SetButtonText(button, "TitleText", title);
            SetButtonText(button, "ArtistText", artist);
            SetButtonText(button, "DifficultyText", difficulty);
            SetButtonText(button, "BestText", best);

            var buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int capturedIndex = index;
                buttonComponent.onClick.AddListener(() => OnChartSelected?.Invoke(capturedIndex));
            }

            _buttons.Add(button);
            Debug.Log($"ChartSelectUI: added button '{title}' at index {index}.");
        }

        private static void SetButtonText(GameObject button, string childName, string text)
        {
            Transform child = button.transform.Find(childName);
            if (child == null)
                return;

            var label = child.GetComponent<Text>();
            if (label != null)
                label.text = text ?? string.Empty;
        }

        /// <summary>Enables or disables every chart button.</summary>
        public void SetInteractable(bool interactable)
        {
            foreach (GameObject button in _buttons)
            {
                if (button == null)
                    continue;

                var buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                    buttonComponent.interactable = interactable;
            }
        }
    }
}
