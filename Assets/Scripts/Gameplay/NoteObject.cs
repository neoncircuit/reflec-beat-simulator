using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Visual representation of a single chart note. Position is driven by song time.</summary>
    public sealed class NoteObject : MonoBehaviour
    {
        private Note _note;
        private PlayfieldRenderer _playfield;
        private double _approachTime;

        public bool IsJudged { get; private set; }
        public Note Note => _note;

        public void Initialize(Note note, PlayfieldRenderer playfield, double approachTime)
        {
            _note = note;
            _playfield = playfield;
            _approachTime = approachTime > 0.0 ? approachTime : Constants.DefaultApproachTime;
        }

        /// <summary>Updates position based on the current song time. Call from GameController.Update.</summary>
        public void UpdatePosition(double songTime)
        {
            if (IsJudged)
                return;

            double t = (_note.TimeSec - songTime) / _approachTime;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            Vector3 spawn = _playfield.GetSpawnPosition(_note.X);
            Vector3 line = _playfield.GetWorldPosition(_note.X);
            transform.position = Vector3.Lerp(line, spawn, (float)t);
        }

        /// <summary>Marks the note as judged and removes its visual.</summary>
        public void MarkJudged()
        {
            IsJudged = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
