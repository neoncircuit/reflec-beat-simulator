using UnityEngine;

namespace ReflecBeat
{
    /// <summary>
    /// Detects touch, mouse, and keyboard input for one target zone.
    /// Forwards valid taps to GameController for judgment.
    /// </summary>
    public class TargetZone : MonoBehaviour
    {
        [Tooltip("Index matching this lane in GameController.targetZones.")]
        public int laneIndex;

        [Tooltip("Radius in world units within which a touch counts as hitting this zone.")]
        public float hitRadius = 1.0f;

        [Tooltip("Keyboard key for testing in the Editor without a touch device.")]
        public KeyCode debugKey = KeyCode.Space;

        void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleTouchInput()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
                    worldPos.z = 0f;
                    if (IsWithinZone(worldPos))
                    {
                        GameController.Instance.HandleTargetTap(laneIndex);
                    }
                }
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0f;
                if (IsWithinZone(worldPos))
                {
                    GameController.Instance.HandleTargetTap(laneIndex);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(debugKey))
            {
                GameController.Instance.HandleTargetTap(laneIndex);
            }
        }

        private bool IsWithinZone(Vector3 worldPos)
        {
            return Vector3.Distance(worldPos, transform.position) <= hitRadius;
        }
    }
}
