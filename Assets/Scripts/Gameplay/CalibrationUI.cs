using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>HUD for the calibration scene. Wired to CalibrationController in the scene.</summary>
    public sealed class CalibrationUI : MonoBehaviour
    {
        [SerializeField] private Text _instructionText;
        [SerializeField] private Text _lastDeltaText;
        [SerializeField] private Text _averageText;
        [SerializeField] private GameObject _doneGroup;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _doneButton;

        private CalibrationController _controller;

        private void Start()
        {
            _controller = FindObjectOfType<CalibrationController>();

            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetry);

            if (_doneButton != null)
                _doneButton.onClick.AddListener(OnDone);
        }

        public void SetInstruction(string text)
        {
            if (_instructionText != null)
                _instructionText.text = text;
        }

        public void SetLastDelta(double? delta)
        {
            if (_lastDeltaText != null)
                _lastDeltaText.text = delta.HasValue
                    ? $"Last: {delta.Value * 1000.0:F1} ms"
                    : "Last: --";
        }

        public void SetAverage(double? average)
        {
            if (_averageText != null)
                _averageText.text = average.HasValue
                    ? $"Average: {average.Value * 1000.0:F1} ms"
                    : "Average: --";
        }

        public void SetDoneVisible(bool visible)
        {
            if (_doneGroup != null)
                _doneGroup.SetActive(visible);
        }

        private void OnRetry() => _controller?.Retry();

        private void OnDone() => _controller?.Done();
    }
}
