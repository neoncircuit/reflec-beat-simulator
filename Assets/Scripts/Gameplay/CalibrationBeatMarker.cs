using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Visual pulse on every calibration beat.</summary>
    public sealed class CalibrationBeatMarker : MonoBehaviour
    {
        [SerializeField] private float _pulseScale = 1.3f;
        [SerializeField] private float _decay = 5.0f;

        private Vector3 _baseScale;
        private float _currentScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _currentScale = 1.0f;
        }

        private void Update()
        {
            _currentScale = Mathf.MoveTowards(_currentScale, 1.0f, _decay * Time.unscaledDeltaTime);
            transform.localScale = _baseScale * _currentScale;
        }

        /// <summary>Triggers a single visual pulse.</summary>
        public void Pulse()
        {
            _currentScale = _pulseScale;
        }
    }
}
