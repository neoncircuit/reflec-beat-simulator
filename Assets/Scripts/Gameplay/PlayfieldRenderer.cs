using System.Collections;
using System.Collections.Generic;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
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
        [SerializeField] private float _lineWidth = 0.12f;
        [SerializeField] private float _frameWidth = 0.10f;
        [SerializeField] private float _outerFrameWidth = 0.05f;
        [SerializeField] private Color _lineColor = new Color(1f, 0.40f, 0.58f, 1f);
        [SerializeField] private Color _lineGlowColor = new Color(1f, 0.35f, 0.55f, 0.45f);
        [SerializeField] private Color _backgroundColor = new Color(0.03f, 0.08f, 0.14f, 0.96f);
        [SerializeField] private Color _frameColor = new Color(0.70f, 0.88f, 1f, 0.75f);
        [SerializeField] private Color _outerFrameColor = new Color(0.40f, 0.60f, 0.85f, 0.40f);
        [SerializeField] private Color _topCircleColor = new Color(0.40f, 0.95f, 0.75f, 0.88f);
        [SerializeField] private Color _topCircleActiveColor = new Color(0.65f, 1f, 0.85f, 0.98f);

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
            CreateSubtleGrid();
            CreateFrame();
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

        /// <summary>
        /// Maps a screen-pixel position to playfield-normalized X (0–1 on the judgment line).
        /// Uses the camera's world projection so aspect ratio does not break hit detection.
        /// </summary>
        public float ScreenToNormalizedX(Vector2 screenPosition, Camera camera)
        {
            if (camera == null)
                camera = Camera.main;

            if (camera == null || _width <= 0f)
                return Mathf.Clamp01(screenPosition.x / Mathf.Max(1f, Screen.width));

            float depth = Mathf.Abs(camera.transform.position.z);
            Vector3 world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            return Mathf.Clamp01(world.x / _width + 0.5f);
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

        /// <summary>Briefly flashes the main judgment line with the judgment color.</summary>
        public void FlashJudgmentLine(Judgment grade)
        {
            Color flash = DefaultVisualTheme.ColorForGrade(grade);
            if (_judgmentLineRenderer != null)
                FlashRenderer(_judgmentLineRenderer, _lineColor, flash, 0.1f);
            if (_judgmentGlowRenderer != null)
                FlashRenderer(_judgmentGlowRenderer, _lineGlowColor, new Color(flash.r, flash.g, flash.b, 0.5f), 0.1f);
        }

        /// <summary>Briefly flashes the top target circle for the given zone with the judgment color.</summary>
        public void FlashTopCircle(int zone, Judgment grade)
        {
            if (zone < 0 || zone >= _topCircleRenderers.Count)
                return;

            LineRenderer circle = _topCircleRenderers[zone];
            if (circle != null)
                FlashRenderer(circle, _topCircleColor, DefaultVisualTheme.ColorForGrade(grade), 0.1f);
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

            if (Application.isPlaying)
                Destroy(panel.GetComponent<Collider>());
            else
                DestroyImmediate(panel.GetComponent<Collider>());

            Material material = CreateSpriteMaterial();
            if (material == null)
                return;

            material.color = _backgroundColor;
            panel.GetComponent<Renderer>().material = material;

            var band = GameObject.CreatePrimitive(PrimitiveType.Quad);
            band.name = "PlayfieldAccentBand";
            band.transform.SetParent(transform, false);
            band.transform.localPosition = new Vector3(0f, _judgmentLineY + 0.8f, 0.9f);
            band.transform.localScale = new Vector3(_width + 0.3f, 2.2f, 1f);

            if (Application.isPlaying)
                Destroy(band.GetComponent<Collider>());
            else
                DestroyImmediate(band.GetComponent<Collider>());

            Material bandMaterial = CreateSpriteMaterial();
            if (bandMaterial == null)
                return;

            bandMaterial.color = new Color(
                DefaultVisualTheme.JudgmentLineGlow.r,
                DefaultVisualTheme.JudgmentLineGlow.g,
                DefaultVisualTheme.JudgmentLineGlow.b,
                0.16f);
            band.GetComponent<Renderer>().material = bandMaterial;

            // Secondary cool wash near top targets for cinematic depth.
            var topWash = GameObject.CreatePrimitive(PrimitiveType.Quad);
            topWash.name = "PlayfieldTopWash";
            topWash.transform.SetParent(transform, false);
            topWash.transform.localPosition = new Vector3(0f, _topJudgmentLineY + 0.4f, 0.95f);
            topWash.transform.localScale = new Vector3(_width + 0.3f, 1.6f, 1f);

            if (Application.isPlaying)
                Destroy(topWash.GetComponent<Collider>());
            else
                DestroyImmediate(topWash.GetComponent<Collider>());

            Material topWashMat = CreateSpriteMaterial();
            if (topWashMat != null)
            {
                topWashMat.color = new Color(
                    DefaultVisualTheme.TopJudgmentLineGlow.r,
                    DefaultVisualTheme.TopJudgmentLineGlow.g,
                    DefaultVisualTheme.TopJudgmentLineGlow.b,
                    0.10f);
                topWash.GetComponent<Renderer>().material = topWashMat;
            }
        }

        private void CreateFrame()
        {
            float left = -_width * 0.5f;
            float right = _width * 0.5f;
            float bottom = _judgmentLineY - 0.45f;
            float top = _spawnY + 0.35f;
            float inset = 0.12f;

            // Outer muted frame
            CreateFrameSegment("OuterFrameLeft", new Vector3(left - inset, bottom - inset, 0.25f), new Vector3(left - inset, top + inset, 0.25f), _outerFrameWidth, _outerFrameColor);
            CreateFrameSegment("OuterFrameRight", new Vector3(right + inset, bottom - inset, 0.25f), new Vector3(right + inset, top + inset, 0.25f), _outerFrameWidth, _outerFrameColor);
            CreateFrameSegment("OuterFrameTop", new Vector3(left - inset, top + inset, 0.25f), new Vector3(right + inset, top + inset, 0.25f), _outerFrameWidth, _outerFrameColor);
            CreateFrameSegment("OuterFrameBottom", new Vector3(left - inset, bottom - inset, 0.25f), new Vector3(right + inset, bottom - inset, 0.25f), _outerFrameWidth, _outerFrameColor);

            // Inner accent frame
            CreateFrameSegment("FrameLeft", new Vector3(left, bottom, 0.2f), new Vector3(left, top, 0.2f), _frameWidth, _frameColor);
            CreateFrameSegment("FrameRight", new Vector3(right, bottom, 0.2f), new Vector3(right, top, 0.2f), _frameWidth, _frameColor);
            CreateFrameSegment("FrameTop", new Vector3(left, top, 0.2f), new Vector3(right, top, 0.2f), _frameWidth, _frameColor);
            CreateFrameSegment("FrameBottom", new Vector3(left, bottom, 0.2f), new Vector3(right, bottom, 0.2f), _frameWidth, _frameColor);

            CreateCornerAccent("CornerBL", new Vector3(left, bottom, 0.15f));
            CreateCornerAccent("CornerBR", new Vector3(right, bottom, 0.15f));
            CreateCornerAccent("CornerTL", new Vector3(left, top, 0.15f));
            CreateCornerAccent("CornerTR", new Vector3(right, top, 0.15f));
        }

        private void CreateSubtleGrid()
        {
            for (int i = 1; i < 6; i++)
            {
                float x = (i / 6f - 0.5f) * _width;
                CreateFrameSegment(
                    $"GridV{i}",
                    new Vector3(x, _judgmentLineY, 0.8f),
                    new Vector3(x, _spawnY, 0.8f),
                    0.03f,
                    DefaultVisualTheme.GridLine);
            }
        }

        private void CreateCornerAccent(string name, Vector3 center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var renderer = go.AddComponent<LineRenderer>();
            renderer.positionCount = 12;
            renderer.startWidth = _frameWidth * 1.2f;
            renderer.endWidth = _frameWidth * 1.2f;
            renderer.useWorldSpace = true;
            renderer.loop = true;
            renderer.material = CreateSpriteMaterial();
            renderer.startColor = DefaultVisualTheme.CornerAccent;
            renderer.endColor = DefaultVisualTheme.CornerAccent;

            const float radius = 0.14f;
            for (int i = 0; i < renderer.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / renderer.positionCount;
                renderer.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
        }

        private void CreateFrameSegment(string name, Vector3 start, Vector3 end)
        {
            CreateFrameSegment(name, start, end, _frameWidth, _frameColor);
        }

        private void CreateFrameSegment(string name, Vector3 start, Vector3 end, float width, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var renderer = go.AddComponent<LineRenderer>();
            renderer.positionCount = 2;
            renderer.startWidth = width;
            renderer.endWidth = width;
            renderer.useWorldSpace = true;
            renderer.material = CreateSpriteMaterial();
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.SetPosition(0, start);
            renderer.SetPosition(1, end);
        }

        private void CreateJudgmentLine()
        {
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
