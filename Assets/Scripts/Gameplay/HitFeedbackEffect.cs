using System.Collections;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Spawns a brief expanding ring at the judgment line when a note is hit.
    /// This is a visual-only effect; it does not affect judgment or scoring.
    /// </summary>
    public sealed class HitFeedbackEffect : MonoBehaviour
    {
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private Material _ringMaterial;
        [SerializeField] private int _poolSize = 16;
        [SerializeField] private float _duration = 0.25f;
        [SerializeField] private float _maxScale = 2f;

        private Transform[] _pool;
        private SpriteRenderer[] _renderers;
        private Coroutine[] _activeCoroutines;
        private int _nextIndex;

        private void Awake()
        {
            _pool = new Transform[_poolSize];
            _renderers = new SpriteRenderer[_poolSize];
            _activeCoroutines = new Coroutine[_poolSize];

            for (int i = 0; i < _poolSize; i++)
            {
                GameObject go = new GameObject($"HitRing{i}");
                go.transform.SetParent(transform, false);
                go.SetActive(false);

                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _ringSprite;
                renderer.material = _ringMaterial != null ? new Material(_ringMaterial) : null;
                renderer.sortingOrder = 10;

                _pool[i] = go.transform;
                _renderers[i] = renderer;
            }
        }

        private void OnDisable()
        {
            if (_activeCoroutines == null)
                return;

            for (int i = 0; i < _activeCoroutines.Length; i++)
            {
                if (_activeCoroutines[i] != null)
                    StopCoroutine(_activeCoroutines[i]);
            }

            for (int i = 0; i < _activeCoroutines.Length; i++)
                _activeCoroutines[i] = null;
        }

        /// <summary>Plays a hit ring at the given world position with the given color.</summary>
        public void Play(Vector3 position, Color color)
        {
            int index = _nextIndex;
            _nextIndex = (_nextIndex + 1) % _poolSize;

            Transform ring = _pool[index];
            SpriteRenderer renderer = _renderers[index];

            if (ring.gameObject.activeSelf && _activeCoroutines[index] != null)
                StopCoroutine(_activeCoroutines[index]);

            ring.position = position;
            ring.localScale = Vector3.one * 0.5f;
            renderer.color = color;
            ring.gameObject.SetActive(true);

            _activeCoroutines[index] = StartCoroutine(Animate(index));
        }

        private IEnumerator Animate(int index)
        {
            Transform ring = _pool[index];
            SpriteRenderer renderer = _renderers[index];
            Vector3 startScale = ring.localScale;
            Vector3 endScale = Vector3.one * _maxScale;
            Color startColor = renderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _duration;
                ring.localScale = Vector3.Lerp(startScale, endScale, t);
                renderer.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            ring.gameObject.SetActive(false);
            _activeCoroutines[index] = null;
        }
    }
}
