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
        /// <summary>
        /// Horizontal position. When <see cref="IsScreenSpace"/> is true this is x / Screen.width;
        /// otherwise it is already playfield-normalized (0–1 on the judgment line).
        /// </summary>
        public readonly float NormalizedX;
        public readonly Vector2 ScreenPosition;
        public readonly double DspTime;
        public readonly bool IsScreenSpace;

        /// <summary>Creates a playfield-space touch (e.g. autoplay / synthetic).</summary>
        public TouchData(int touchId, float playfieldX, double dspTime)
        {
            TouchId = touchId;
            NormalizedX = playfieldX;
            ScreenPosition = default;
            DspTime = dspTime;
            IsScreenSpace = false;
        }

        /// <summary>Creates a screen-space touch from EnhancedTouch.</summary>
        public TouchData(int touchId, Vector2 screenPosition, double dspTime)
        {
            TouchId = touchId;
            ScreenPosition = screenPosition;
            NormalizedX = Screen.width > 0 ? screenPosition.x / Screen.width : 0.5f;
            DspTime = dspTime;
            IsScreenSpace = true;
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
            var touch = finger.currentTouch;
            int touchId = touch.valid ? touch.touchId : finger.index;
            OnTouchBegan?.Invoke(new TouchData(touchId, finger.screenPosition, AudioSettings.dspTime));
        }

        private void HandleFingerUp(Finger finger)
        {
            var touch = finger.currentTouch;
            int touchId = touch.valid ? touch.touchId : finger.index;
            OnTouchEnded?.Invoke(new TouchData(touchId, finger.screenPosition, AudioSettings.dspTime));
        }
    }
}
