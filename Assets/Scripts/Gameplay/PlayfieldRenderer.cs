using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Maps normalized playfield coordinates to world space.</summary>
    public sealed class PlayfieldRenderer : MonoBehaviour
    {
        [SerializeField] private float _width = 10f;
        [SerializeField] private float _judgmentLineY = 0f;
        [SerializeField] private float _spawnY = 10f;

        public float JudgmentLineY => _judgmentLineY;

        /// <summary>World position on the judgment line for the given normalized x (0.0–1.0).</summary>
        public Vector3 GetWorldPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _judgmentLineY, 0f);
        }

        /// <summary>World spawn position above the judgment line for the given normalized x.</summary>
        public Vector3 GetSpawnPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _spawnY, 0f);
        }
    }
}
