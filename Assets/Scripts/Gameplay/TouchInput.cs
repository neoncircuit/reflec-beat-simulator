using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Data for a single touch that just began this frame.</summary>
    public readonly struct TouchData
    {
        public readonly int TouchId;
        public readonly float NormalizedX;
        public readonly double DspTime;

        public TouchData(int touchId, float normalizedX, double dspTime)
        {
            TouchId = touchId;
            NormalizedX = normalizedX;
            DspTime = dspTime;
        }
    }

    /// <summary>Captures new touches from EnhancedTouch and timestamps them against the DSP clock.</summary>
    public sealed class TouchInput : MonoBehaviour
    {
        public event Action<TouchData> OnTouchBegan;
        public event Action<TouchData> OnTouchEnded;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
            TouchSimulation.Enable();
#endif
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerUp += HandleFingerUp;
        }

        private void OnDisable()
        {
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerUp -= HandleFingerUp;
            EnhancedTouchSupport.Disable();
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
        }

        private void HandleFingerDown(Finger finger)
        {
            Vector2 screenPos = finger.screenPosition;
            var touch = finger.currentTouch;
            int touchId = touch.valid ? touch.touchId : finger.index;
            var data = new TouchData(
                touchId,
                screenPos.x / Screen.width,
                AudioSettings.dspTime);

            OnTouchBegan?.Invoke(data);
        }

        private void HandleFingerUp(Finger finger)
        {
            Vector2 screenPos = finger.screenPosition;
            var touch = finger.currentTouch;
            int touchId = touch.valid ? touch.touchId : finger.index;
            var data = new TouchData(
                touchId,
                screenPos.x / Screen.width,
                AudioSettings.dspTime);

            OnTouchEnded?.Invoke(data);
        }
    }
}
