using System;
using System.Collections.Generic;
using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Renders the chart-select list and autoplay toggle.</summary>
    public sealed class ChartSelectUI : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _chartButtonPrefab;
        [SerializeField] private Toggle _autoplayToggle;
        [SerializeField] private Toggle _syncCheckToggle;
        [SerializeField] private Toggle _hideRivalToggle;
        [SerializeField] private Button _calibrateButton;
        [SerializeField] private Dropdown _approachTimeDropdown;

        private readonly List<GameObject> _buttons = new List<GameObject>();

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
                if (_approachTimeDropdown == null)
                    return Constants.DefaultApproachTime;

                int index = _approachTimeDropdown.value;
                if (index < 0 || index >= Constants.ApproachTimeOptions.Count)
                    return Constants.DefaultApproachTime;

                return Constants.ApproachTimeOptions[index];
            }
        }

        private void Start()
        {
            InitializeApproachTimeDropdown();
            ApplySettings(SettingsStore.Load());

            if (_calibrateButton != null)
                _calibrateButton.onClick.AddListener(() => OnCalibrate?.Invoke());
        }

        /// <summary>Applies stored settings to the UI controls.</summary>
        public void ApplySettings(SettingsStore.GameSettings settings)
        {
            if (settings == null)
                return;

            if (_autoplayToggle != null)
                _autoplayToggle.isOn = settings.autoplay;

            if (_syncCheckToggle != null)
                _syncCheckToggle.isOn = settings.syncCheck;

            if (_hideRivalToggle != null)
                _hideRivalToggle.isOn = settings.hideRivalVisuals;

            SetApproachTimeDropdown(settings.approachTimeSec);
        }

        private void SetApproachTimeDropdown(double approachTimeSec)
        {
            if (_approachTimeDropdown == null)
                return;

            for (int i = 0; i < Constants.ApproachTimeOptions.Count; i++)
            {
                if (Math.Abs(Constants.ApproachTimeOptions[i] - approachTimeSec) < 1e-6)
                {
                    _approachTimeDropdown.value = i;
                    return;
                }
            }
        }

        private void InitializeApproachTimeDropdown()
        {
            if (_approachTimeDropdown == null)
                return;

            _approachTimeDropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            foreach (double time in Constants.ApproachTimeOptions)
                options.Add(new Dropdown.OptionData($"{time:0.00}s"));

            _approachTimeDropdown.AddOptions(options);

            int defaultIndex = 0;
            for (int i = 0; i < Constants.ApproachTimeOptions.Count; i++)
            {
                if (Math.Abs(Constants.ApproachTimeOptions[i] - Constants.DefaultApproachTime) < 1e-6)
                {
                    defaultIndex = i;
                    break;
                }
            }

            _approachTimeDropdown.value = defaultIndex;
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

        public void AddButton(string label, int index)
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

            var text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;

            var buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int capturedIndex = index;
                buttonComponent.onClick.AddListener(() => OnChartSelected?.Invoke(capturedIndex));
            }

            _buttons.Add(button);
            Debug.Log($"ChartSelectUI: added button '{label}' at index {index}.");
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
