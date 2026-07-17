using ReflecBeatSimulator.UI;
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
        [SerializeField] private Text _storedOffsetText;
        [SerializeField] private GameObject _doneGroup;
        [SerializeField] private PanelFadeSlide _donePanelFade;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private Button _nudgeUpButton;
        [SerializeField] private Button _nudgeDownButton;

        private CalibrationController _controller;

        private const double NudgeStepSec = 0.005;

        private void Start()
        {
            _controller = FindObjectOfType<CalibrationController>();

            if (_donePanelFade == null && _doneGroup != null)
                _donePanelFade = _doneGroup.GetComponent<PanelFadeSlide>();

            if (_retryButton != null)
                _retryButton.onClick.AddListener(OnRetry);

            if (_doneButton != null)
                _doneButton.onClick.AddListener(OnDone);

            if (_nudgeUpButton != null)
                _nudgeUpButton.onClick.AddListener(() => _controller?.NudgeOffset(NudgeStepSec));

            if (_nudgeDownButton != null)
                _nudgeDownButton.onClick.AddListener(() => _controller?.NudgeOffset(-NudgeStepSec));
        }

        private void OnDestroy()
        {
            if (_retryButton != null)
                _retryButton.onClick.RemoveAllListeners();

            if (_doneButton != null)
                _doneButton.onClick.RemoveAllListeners();

            if (_nudgeUpButton != null)
                _nudgeUpButton.onClick.RemoveAllListeners();

            if (_nudgeDownButton != null)
                _nudgeDownButton.onClick.RemoveAllListeners();
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

        public void SetStoredOffset(double? offsetSec)
        {
            if (_storedOffsetText != null)
                _storedOffsetText.text = offsetSec.HasValue
                    ? $"Stored offset: {offsetSec.Value * 1000.0:F1} ms"
                    : "Stored offset: --";
        }

        public void SetDoneVisible(bool visible)
        {
            if (_donePanelFade != null)
            {
                if (visible)
                    _donePanelFade.Show();
                else
                    _donePanelFade.Hide();
                return;
            }

            if (_doneGroup != null)
                _doneGroup.SetActive(visible);
        }

        private void OnRetry() => _controller?.Retry();

        private void OnDone() => _controller?.Done();
    }
}
