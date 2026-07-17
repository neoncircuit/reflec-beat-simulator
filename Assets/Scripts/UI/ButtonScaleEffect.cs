using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>
    /// Subtle scale feedback for uGUI buttons: grow on hover, shrink on press,
    /// and spring back on release. Safe for touch and pointer input.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressScale = 0.95f;
        [SerializeField] private float _duration = 0.1f;

        private Transform _target;
        private Button _button;
        private Coroutine _activeTween;
        private bool _isHovered;
        private bool _isPressed;

        private void Awake()
        {
            _target = transform;
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _target.localScale = Vector3.one;
            _isHovered = false;
            _isPressed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable)
                return;

            _isHovered = true;
            TweenTo(_isPressed ? _pressScale : _hoverScale);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            TweenTo(1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable)
                return;

            _isPressed = true;
            TweenTo(_pressScale);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            TweenTo(_isHovered ? _hoverScale : 1f);
        }

        private void TweenTo(Vector3 target)
        {
            if (_activeTween != null)
                StopCoroutine(_activeTween);

            _activeTween = StartCoroutine(TweenScale(target));
        }

        private void TweenTo(float uniformScale)
        {
            TweenTo(Vector3.one * uniformScale);
        }

        private IEnumerator TweenScale(Vector3 target)
        {
            Vector3 start = _target.localScale;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _duration;
                _target.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            _target.localScale = target;
            _activeTween = null;
        }
    }
}
