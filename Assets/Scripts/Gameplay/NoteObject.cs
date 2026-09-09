using System.Collections;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Visual representation of a single chart note. Position is driven by song time.</summary>
    public sealed class NoteObject : MonoBehaviour
    {
        private const float DefaultSpriteWidth = 1.28f;

        [Header("Bounce (visual-only)")]
        [SerializeField] private bool _enableBounce = true;
        [SerializeField] private int _wallHits = 2;
        [SerializeField] private float _spriteWidth = DefaultSpriteWidth;
        [SerializeField] private Transform _directionIndicator;

        [Header("Bounce Glow")]
        [SerializeField] private Color _goldGlowColor = new Color(1f, 0.82f, 0.40f, 1f);
        [SerializeField] private float _goldGlowDuration = 0.18f;

        private Note _note;
        private PlayfieldRenderer _playfield;
        private NoteObjectPool _pool;
        private double _approachTime;
        private float _visualX;
        private float _bounceDirection = 1f;
        private bool _isRival;
        private bool _baseEnableBounce;
        private bool _isPlayingEffect;
        private int _lastBounceSegment;
        private Color _baseColor;
        private Coroutine _activeGlow;

        private const float HitEffectDuration = 0.12f;
        private const float HitEffectScale = 1.35f;

        public bool IsJudged { get; private set; }
        public bool IsHeld { get; private set; }
        public Note Note => _note;

        private void Awake()
        {
            _baseEnableBounce = _enableBounce;
        }

        /// <summary>Initializes the note with its chart data and playfield context.</summary>
        public void Initialize(Note note, PlayfieldRenderer playfield, double approachTime, float visualX, NoteObjectPool pool = null)
        {
            _note = note;
            _pool = pool;
            _isRival = note != null && note.IsRival;
            _playfield = playfield;
            _approachTime = approachTime > 0.0 ? approachTime : Constants.DefaultApproachTime;
            _visualX = visualX;
            _bounceDirection = visualX < 0.5f ? 1f : -1f;
            _enableBounce = _baseEnableBounce && !_isRival;
            _lastBounceSegment = -1;
            transform.localScale = Vector3.one * (_playfield.NoteWorldWidth / _spriteWidth);

            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
                renderer.color = ColorForType(note);

            ApplyTint(note);
            ApplyDirectionVisuals();

            if (renderer != null)
                _baseColor = renderer.color;
        }

        private static Color ColorForType(Note note)
        {
            if (note == null)
                return DefaultVisualTheme.NoteTap;

            switch (note.Type)
            {
                case NoteType.Long:
                    return DefaultVisualTheme.NoteLong;
                case NoteType.Vertical:
                    return DefaultVisualTheme.NoteVertical;
                case NoteType.Top:
                    return DefaultVisualTheme.NoteTop;
                case NoteType.BigBang:
                    return DefaultVisualTheme.NoteBigBang;
                default:
                    return DefaultVisualTheme.NoteTap;
            }
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

                int segment = Mathf.FloorToInt(progress * _wallHits);
                if (segment > _lastBounceSegment)
                {
                    _lastBounceSegment = segment;
                    TriggerBounceGlow();
                }
            }

            transform.position = position;
        }

        /// <summary>Marks the note as judged and plays a brief pop effect before returning it to the pool.</summary>
        public void MarkJudged()
        {
            if (IsJudged || _isPlayingEffect)
                return;

            IsJudged = true;
            StartCoroutine(HitEffectCoroutine());
        }

        /// <summary>Returns the object to its pool without marking it judged.</summary>
        public void ReturnToPool()
        {
            if (_pool != null && _note != null)
                _pool.Release(this, _note.Type);
            else
                Destroy(gameObject);
        }

        /// <summary>Resets runtime state so the object can be reused from the pool.</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            _activeGlow = null;
            IsJudged = false;
            IsHeld = false;
            _isPlayingEffect = false;
            _lastBounceSegment = -1;
        }

        /// <summary>Locks the note at the judgment line while a long note is being held.</summary>
        public void MarkHeld()
        {
            IsHeld = true;
        }

        private IEnumerator HitEffectCoroutine()
        {
            _isPlayingEffect = true;
            Vector3 baseScale = transform.localScale;
            Vector3 targetScale = baseScale * HitEffectScale;
            float elapsed = 0f;

            while (elapsed < HitEffectDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / HitEffectDuration;
                transform.localScale = Vector3.Lerp(baseScale, targetScale, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            transform.localScale = baseScale;
            _isPlayingEffect = false;
            ReturnToPool();
        }

        private void TriggerBounceGlow()
        {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
                return;

            if (_activeGlow != null)
            {
                StopCoroutine(_activeGlow);
                _activeGlow = null;
            }

            _activeGlow = StartCoroutine(GlowCoroutine(renderer, _goldGlowColor, _goldGlowDuration));
        }

        private IEnumerator GlowCoroutine(SpriteRenderer renderer, Color glowColor, float duration)
        {
            renderer.color = glowColor;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                renderer.color = Color.Lerp(glowColor, _baseColor, t);
                yield return null;
            }

            renderer.color = _baseColor;
            _activeGlow = null;
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
                renderer.color = DefaultVisualTheme.NoteGold;
            else if (note.IsRival)
                renderer.color = DefaultVisualTheme.NoteRival;
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

            renderer.color = _bounceDirection > 0f
                ? DefaultVisualTheme.NoteBounceLeft
                : DefaultVisualTheme.NoteBounceRight;
        }

        private bool ShouldBounce()
        {
            if (!_enableBounce || _note == null)
                return false;

            // Only special (gold) notes bounce in the original; straight-shot types never do.
            if (!_note.IsGold)
                return false;

            return _note.Type != NoteType.Top
                && _note.Type != NoteType.BigBang
                && _note.Type != NoteType.Vertical;
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
