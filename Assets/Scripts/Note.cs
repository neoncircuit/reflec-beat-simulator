using UnityEngine;

namespace ReflecBeat
{
    /// <summary>
    /// State and behavior for a single moving note.
    /// The GameController drives movement each frame.
    /// </summary>
    public class Note : MonoBehaviour
    {
        public int Lane { get; set; }
        public float HitTime { get; set; }
        public float SpawnTime { get; set; }
        public Vector3 StartPosition { get; set; }
        public Vector3 TargetPosition { get; set; }

        public bool WasHit { get; set; }
        public bool WasMissed { get; set; }

        public void Initialize(int lane, float hitTime, float spawnTime,
                               Vector3 start, Vector3 target)
        {
            Lane = lane;
            HitTime = hitTime;
            SpawnTime = spawnTime;
            StartPosition = start;
            TargetPosition = target;
            WasHit = false;
            WasMissed = false;
        }
    }
}
