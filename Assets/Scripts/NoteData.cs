using UnityEngine;

namespace ReflecBeat
{
    /// <summary>
    /// Serializable data for a single note in a chart.
    /// For the prototype, notes only know when they should be hit and which lane they travel to.
    /// </summary>
    [System.Serializable]
    public struct NoteData
    {
        public float hitTime; // seconds from song start
        public int lane;      // target zone index

        public NoteData(float hitTime, int lane)
        {
            this.hitTime = hitTime;
            this.lane = lane;
        }
    }
}
