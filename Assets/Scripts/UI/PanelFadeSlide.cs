using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>
    /// Animates a UI panel in and out with a combined fade and slide.
    /// Works with the panel's root CanvasGroup; children layout themselves normally.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public sealed class PanelFadeSlide : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Vector2 _hiddenOffset = new Vector2(0f, -50f);
        [SerializeField] private float _duration = 0.25f;

        private CanvasGroup _canvasGroup;
        private Coroutine _activeTween;
        private Vector2 _originalPosition;
        private Vector2 _shownPosition;

        /// <summary>Current fade/slide duration in seconds.</summary>
        public float Duration => _duration;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform == null)
            {
                Debug.LogError($"{nameof(PanelFadeSlide)} requires a RectTransform on the same GameObject.", this);
                enabled = false;
                return;
            }

            _originalPosition = _rectTransform.anchoredPosition;
            _shownPosition = _originalPosition;
        }

        private void OnEnable()
        {
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPosition;
                _shownPosition = _originalPosition;
            }
        }

        /// <summary>Overrides duration and hidden offset for this panel's animations.</summary>
        public void Configure(float duration, Vector2 hiddenOffset)
        {
            _duration = Mathf.Max(0f, duration);
            _hiddenOffset = hiddenOffset;
        }

        /// <summary>Shows the panel with a fade+slide animation.</summary>
        public void Show(bool immediate = false)
        {
            gameObject.SetActive(true);
            Tween(1f, Vector2.zero, immediate);
        }

        /// <summary>Hides the panel with a fade+slide animation.</summary>
        public void Hide(bool immediate = false)
        {
            Tween(0f, _hiddenOffset, immediate, () => gameObject.SetActive(false));
        }

        private void Tween(float targetAlpha, Vector2 positionOffset, bool immediate, System.Action onComplete = null)
        {
            if (_activeTween != null)
                StopCoroutine(_activeTween);

            if (immediate)
            {
                Apply(targetAlpha, positionOffset);
                onComplete?.Invoke();
                return;
            }

            _activeTween = StartCoroutine(TweenRoutine(targetAlpha, positionOffset, onComplete));
        }

        private IEnumerator TweenRoutine(float targetAlpha, Vector2 positionOffset, System.Action onComplete)
        {
            float startAlpha = _canvasGroup.alpha;
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector2 targetPosition = _shownPosition + positionOffset;
            float elapsed = 0f;

            _canvasGroup.blocksRaycasts = targetAlpha > 0.5f;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            Apply(targetAlpha, positionOffset);
            onComplete?.Invoke();
            _activeTween = null;
        }

        private void Apply(float alpha, Vector2 positionOffset)
        {
            _canvasGroup.alpha = alpha;
            _canvasGroup.blocksRaycasts = alpha > 0.5f;
            _rectTransform.anchoredPosition = _shownPosition + positionOffset;
        }
    }
}
