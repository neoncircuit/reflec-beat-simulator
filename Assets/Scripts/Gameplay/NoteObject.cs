using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Visual representation of a single chart note. Position is driven by song time.</summary>
    public sealed class NoteObject : MonoBehaviour
    {
        private const float DefaultSpriteWidth = 1.28f;

        private static readonly Color LeftFirstColor = new Color(0.35f, 0.75f, 1f, 1f);
        private static readonly Color RightFirstColor = new Color(1f, 0.6f, 0.2f, 1f);

        [Header("Bounce (visual-only)")]
        [SerializeField] private bool _enableBounce = true;
        [SerializeField] private int _wallHits = 2;
        [SerializeField] private float _spriteWidth = DefaultSpriteWidth;
        [SerializeField] private Transform _directionIndicator;

        private Note _note;
        private PlayfieldRenderer _playfield;
        private double _approachTime;
        private float _visualX;
        private float _bounceDirection = 1f;
        private bool _isRival;

        public bool IsJudged { get; private set; }
        public bool IsHeld { get; private set; }
        public Note Note => _note;

        /// <summary>Initializes the note with its chart data and playfield context.</summary>
        public void Initialize(Note note, PlayfieldRenderer playfield, double approachTime, float visualX)
        {
            _note = note;
            _isRival = note != null && note.IsRival;
            _playfield = playfield;
            _approachTime = approachTime > 0.0 ? approachTime : Constants.DefaultApproachTime;
            _visualX = visualX;
            _bounceDirection = visualX < 0.5f ? 1f : -1f;
            _enableBounce = _enableBounce && !_isRival;
            transform.localScale = Vector3.one * (_playfield.NoteWorldWidth / _spriteWidth);
            ApplyTint(note);
            ApplyDirectionVisuals();
        }

        /// <summary>Updates position based on the current song time. Call from GameController.Update.</summary>
        public void UpdatePosition(double songTime)
        {
            if (IsJudged)
                return;

            if (IsHeld)
            {
                transform.position = GetTargetPosition();
                return;
            }
            double t = (_note.TimeSec - songTime) / _approachTime;
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            Vector3 spawn = GetSpawnPosition();
            Vector3 target = GetTargetPosition();
            Vector3 position = Vector3.Lerp(target, spawn, (float)t);

            if (ShouldBounce())
            {
                float progress = 1f - (float)t;
                float bouncedX = BounceMath.BounceX(_visualX, progress, _wallHits, _bounceDirection);
                position.x = _playfield.GetWorldPosition(bouncedX).x;
            }

            transform.position = position;
        }

        /// <summary>Marks the note as judged and removes its visual.</summary>
        public void MarkJudged()
        {
            IsJudged = true;
            Destroy(gameObject);
        }

        /// <summary>Locks the note at the judgment line while a long note is being held.</summary>
        public void MarkHeld()
        {
            IsHeld = true;
        }

        private bool IsTopNote()
        {
            return _note != null && _note.Type == NoteType.Top;
        }

        private void ApplyTint(Note note)
        {
            if (note == null)
                return;

            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
                return;

            if (note.IsGold)
                renderer.color = Color.yellow;
            else if (note.IsRival)
                renderer.color = new Color(1f, 0.3f, 0.3f);
        }

        private void ApplyDirectionVisuals()
        {
            if (!ShouldBounce())
                return;

            if (_directionIndicator != null)
                _directionIndicator.localScale = new Vector3(_bounceDirection, 1f, 1f);

            if (_note == null || _note.IsGold || _note.IsRival)
                return;

            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
                return;

            renderer.color = _bounceDirection > 0f ? LeftFirstColor : RightFirstColor;
        }

        private bool ShouldBounce()
        {
            if (!_enableBounce || _note == null)
                return false;

            return _note.Type != NoteType.Top && _note.Type != NoteType.BigBang;
        }

        private Vector3 GetSpawnPosition()
        {
            if (IsTopNote())
                return _playfield.GetTopSpawnPosition(_note.Zone);

            if (_isRival)
                return _playfield.GetRivalSpawnPosition(_visualX);

            return _playfield.GetSpawnPosition(_visualX);
        }

        private Vector3 GetTargetPosition()
        {
            if (IsTopNote())
                return _playfield.GetTopWorldPosition(_note.Zone);

            if (_isRival)
                return _playfield.GetRivalTargetPosition(_visualX);

            return _playfield.GetWorldPosition(_visualX);
        }
    }
}
