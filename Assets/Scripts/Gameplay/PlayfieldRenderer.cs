using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Maps normalized playfield coordinates to world space and builds the visual framing.</summary>
    public sealed class PlayfieldRenderer : MonoBehaviour
    {
        [Header("Playfield Geometry")]
        [SerializeField] private float _width = 10f;
        [SerializeField] private float _judgmentLineY = -4f;
        [SerializeField] private float _spawnY = 5f;
        [SerializeField] private float _noteWorldWidth = 0.75f;

        [Header("Top Targets")]
        [SerializeField] private float _topJudgmentLineY = -2.5f;
        [SerializeField] private float _topSpawnY = 5f;
        [SerializeField] private float _topCircleRadius = 0.6f;

        [Header("Visuals")]
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private Color _lineColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color _lineGlowColor = new Color(0.2f, 0.9f, 1f, 0.25f);
        [SerializeField] private Color _backgroundColor = new Color(0.05f, 0.05f, 0.08f, 0.85f);
        [SerializeField] private Color _laneDividerColor = new Color(0.2f, 0.2f, 0.25f, 0.5f);
        [SerializeField] private Color _topCircleColor = new Color(1f, 0.9f, 0.2f, 0.6f);
        [SerializeField] private Color _topCircleActiveColor = new Color(1f, 0.95f, 0.5f, 0.9f);

        public float JudgmentLineY => _judgmentLineY;
        public float TopJudgmentLineY => _topJudgmentLineY;
        public float NoteWorldWidth => _noteWorldWidth;
        public float Width => _width;

        private readonly List<LineRenderer> _topCircleRenderers = new List<LineRenderer>();
        private readonly Dictionary<LineRenderer, Coroutine> _activeFlashes = new Dictionary<LineRenderer, Coroutine>();
        private LineRenderer _judgmentLineRenderer;
        private LineRenderer _judgmentGlowRenderer;

        private void Awake()
        {
            CreateBackground();
            CreateLaneDividers();
            CreateJudgmentLine();
            CreateTopCircles();
        }

        /// <summary>Returns the world position on the main judgment line for a normalized X.</summary>
        public Vector3 GetWorldPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _judgmentLineY, 0f);
        }

        /// <summary>Returns the spawn position above the main judgment line for a normalized X.</summary>
        public Vector3 GetSpawnPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _spawnY, 0f);
        }

        /// <summary>Returns the rival spawn position at the main judgment line for a normalized X.</summary>
        public Vector3 GetRivalSpawnPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _judgmentLineY, 0f);
        }

        /// <summary>Returns the rival target position above the main judgment line for a normalized X.</summary>
        public Vector3 GetRivalTargetPosition(float normalizedX)
        {
            return new Vector3((normalizedX - 0.5f) * _width, _spawnY, 0f);
        }

        /// <summary>Returns the world position of the top target circle for the given zone.</summary>
        public Vector3 GetTopWorldPosition(int zone)
        {
            return new Vector3((GetZoneX(zone) - 0.5f) * _width, _topJudgmentLineY, 0f);
        }

        /// <summary>Returns the spawn position above the top target circle for the given zone.</summary>
        public Vector3 GetTopSpawnPosition(int zone)
        {
            return new Vector3((GetZoneX(zone) - 0.5f) * _width, _topSpawnY, 0f);
        }

        /// <summary>Returns the normalized X position for a top-note zone (0=left, 1=center, 2=right).</summary>
        public static float GetZoneX(int zone)
        {
            return zone switch
            {
                0 => 1f / 6f,
                2 => 5f / 6f,
                _ => 0.5f
            };
        }

        /// <summary>Briefly flashes the main judgment line.</summary>
        public void FlashJudgmentLine()
        {
            if (_judgmentLineRenderer != null)
                FlashRenderer(_judgmentLineRenderer, _lineColor, Color.white, 0.1f);
            if (_judgmentGlowRenderer != null)
                FlashRenderer(_judgmentGlowRenderer, _lineGlowColor, new Color(1f, 1f, 1f, 0.5f), 0.1f);
        }

        /// <summary>Briefly flashes the top target circle for the given zone.</summary>
        public void FlashTopCircle(int zone)
        {
            if (zone < 0 || zone >= _topCircleRenderers.Count)
                return;

            LineRenderer circle = _topCircleRenderers[zone];
            if (circle != null)
                FlashRenderer(circle, _topCircleColor, _topCircleActiveColor, 0.1f);
        }

        private Coroutine FlashRenderer(LineRenderer renderer, Color baseColor, Color flashColor, float duration)
        {
            if (_activeFlashes.TryGetValue(renderer, out Coroutine old) && old != null)
                StopCoroutine(old);

            IEnumerator routine = FlashRendererCoroutine(renderer, baseColor, flashColor, duration);
            Coroutine coroutine = StartCoroutine(routine);
            _activeFlashes[renderer] = coroutine;
            return coroutine;
        }

        private System.Collections.IEnumerator FlashRendererCoroutine(LineRenderer renderer, Color baseColor, Color flashColor, float duration)
        {
            renderer.startColor = flashColor;
            renderer.endColor = flashColor;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                renderer.startColor = Color.Lerp(flashColor, baseColor, t);
                renderer.endColor = Color.Lerp(flashColor, baseColor, t);
                yield return null;
            }

            renderer.startColor = baseColor;
            renderer.endColor = baseColor;
            _activeFlashes.Remove(renderer);
        }

        private void CreateBackground()
        {
            var panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = "PlayfieldBackground";
            panel.transform.SetParent(transform, false);
            panel.transform.localPosition = new Vector3(0f, (_spawnY + _judgmentLineY) * 0.5f, 1f);
            panel.transform.localScale = new Vector3(_width + 0.5f, _spawnY - _judgmentLineY + 1f, 1f);

            Destroy(panel.GetComponent<Collider>());

            Material material = CreateSpriteMaterial();
            if (material == null)
                return;

            material.color = _backgroundColor;
            panel.GetComponent<Renderer>().material = material;
        }

        private void CreateLaneDividers()
        {
            for (int i = 1; i < 6; i++)
            {
                float x = (i / 6f - 0.5f) * _width;
                var lineGo = new GameObject($"LaneDivider{i}");
                lineGo.transform.SetParent(transform, false);

                var lineRenderer = lineGo.AddComponent<LineRenderer>();
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = _lineWidth * 0.5f;
                lineRenderer.endWidth = _lineWidth * 0.5f;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = CreateSpriteMaterial();
                lineRenderer.startColor = _laneDividerColor;
                lineRenderer.endColor = _laneDividerColor;
                lineRenderer.SetPosition(0, new Vector3(x, _spawnY, 0f));
                lineRenderer.SetPosition(1, new Vector3(x, _judgmentLineY, 0f));
            }
        }

        private void CreateJudgmentLine()
        {
            // Glow line behind the main line.
            var glowGo = new GameObject("JudgmentLineGlow");
            glowGo.transform.SetParent(transform, false);
            _judgmentGlowRenderer = glowGo.AddComponent<LineRenderer>();
            _judgmentGlowRenderer.positionCount = 2;
            _judgmentGlowRenderer.startWidth = _lineWidth * 4f;
            _judgmentGlowRenderer.endWidth = _lineWidth * 4f;
            _judgmentGlowRenderer.useWorldSpace = true;
            _judgmentGlowRenderer.material = CreateSpriteMaterial();
            _judgmentGlowRenderer.startColor = _lineGlowColor;
            _judgmentGlowRenderer.endColor = _lineGlowColor;
            _judgmentGlowRenderer.SetPosition(0, new Vector3(-_width * 0.5f, _judgmentLineY, 0.05f));
            _judgmentGlowRenderer.SetPosition(1, new Vector3(_width * 0.5f, _judgmentLineY, 0.05f));

            // Main judgment line.
            var lineGo = new GameObject("JudgmentLine");
            lineGo.transform.SetParent(transform, false);
            _judgmentLineRenderer = lineGo.AddComponent<LineRenderer>();
            _judgmentLineRenderer.positionCount = 2;
            _judgmentLineRenderer.startWidth = _lineWidth;
            _judgmentLineRenderer.endWidth = _lineWidth;
            _judgmentLineRenderer.useWorldSpace = true;
            _judgmentLineRenderer.material = CreateSpriteMaterial();
            _judgmentLineRenderer.startColor = _lineColor;
            _judgmentLineRenderer.endColor = _lineColor;
            _judgmentLineRenderer.SetPosition(0, new Vector3(-_width * 0.5f, _judgmentLineY, 0f));
            _judgmentLineRenderer.SetPosition(1, new Vector3(_width * 0.5f, _judgmentLineY, 0f));

            // Tick marks.
            for (int i = 0; i <= 6; i++)
            {
                float x = (i / 6f - 0.5f) * _width;
                var tickGo = new GameObject($"JudgmentTick{i}");
                tickGo.transform.SetParent(transform, false);

                var tickRenderer = tickGo.AddComponent<LineRenderer>();
                tickRenderer.positionCount = 2;
                tickRenderer.startWidth = _lineWidth * 0.8f;
                tickRenderer.endWidth = _lineWidth * 0.8f;
                tickRenderer.useWorldSpace = true;
                tickRenderer.material = CreateSpriteMaterial();
                tickRenderer.startColor = _lineColor;
                tickRenderer.endColor = _lineColor;
                tickRenderer.SetPosition(0, new Vector3(x, _judgmentLineY - 0.15f, 0f));
                tickRenderer.SetPosition(1, new Vector3(x, _judgmentLineY + 0.15f, 0f));
            }
        }

        private void CreateTopCircles()
        {
            for (int zone = 0; zone < 3; zone++)
            {
                Vector3 center = GetTopWorldPosition(zone);
                var circleGo = new GameObject($"TopCircle{zone}");
                circleGo.transform.SetParent(transform, false);

                var lineRenderer = circleGo.AddComponent<LineRenderer>();
                lineRenderer.positionCount = 32;
                lineRenderer.startWidth = _lineWidth;
                lineRenderer.endWidth = _lineWidth;
                lineRenderer.useWorldSpace = true;
                lineRenderer.loop = true;
                lineRenderer.material = CreateSpriteMaterial();
                lineRenderer.startColor = _topCircleColor;
                lineRenderer.endColor = _topCircleColor;

                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    float angle = i * Mathf.PI * 2f / lineRenderer.positionCount;
                    Vector3 point = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * _topCircleRadius;
                    lineRenderer.SetPosition(i, point);
                }

                _topCircleRenderers.Add(lineRenderer);
            }
        }

        private Material CreateSpriteMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("PlayfieldRenderer: Sprites/Default shader not found.");
                return null;
            }

            return new Material(shader);
        }
    }
}
