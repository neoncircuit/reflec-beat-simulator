using System;
using System.Collections.Generic;
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

        private readonly List<GameObject> _buttons = new List<GameObject>();

        /// <summary>Invoked with the chart index when a chart button is pressed.</summary>
        public event Action<int> OnChartSelected;

        /// <summary>Whether the autoplay toggle is on.</summary>
        public bool Autoplay => _autoplayToggle != null && _autoplayToggle.isOn;

        /// <summary>Whether the sync-check toggle is on.</summary>
        public bool SyncCheck => _syncCheckToggle != null && _syncCheckToggle.isOn;

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
