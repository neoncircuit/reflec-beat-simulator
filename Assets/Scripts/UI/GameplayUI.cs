using System.Collections;
using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>In-game HUD for score, combo, rate, and judgment feedback.</summary>
    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _comboText;
        [SerializeField] private Text _judgmentText;

        private float _judgmentTimer;
        private const float JudgmentDisplayDuration = 0.5f;
        private const int JudgmentFontSize = 48;
        private const float JudgmentPopScale = 1.3f;
        private const float JudgmentPopDuration = 0.12f;

        private RectTransform _judgmentRect;
        private Coroutine _judgmentPop;
        private Coroutine _hudFade;
        private Camera _worldCamera;
        private CanvasGroup _scoreGroup;
        private CanvasGroup _comboGroup;
        private Image _scoreChrome;
        private Image _comboChrome;

        private void Awake()
        {
            if (_judgmentText != null)
                _judgmentRect = _judgmentText.GetComponent<RectTransform>();
            _worldCamera = Camera.main;
            EnsureHudChrome();
            ApplyTheme();
        }

        private void OnEnable()
        {
            ResetJudgmentScale();
            PlayHudFadeIn();
        }

        private void OnDisable()
        {
            if (_judgmentPop != null)
            {
                StopCoroutine(_judgmentPop);
                _judgmentPop = null;
            }

            if (_hudFade != null)
            {
                StopCoroutine(_hudFade);
                _hudFade = null;
            }

            ResetJudgmentScale();
        }

        private void Update()
        {
            if (_judgmentTimer > 0f)
            {
                _judgmentTimer -= Time.unscaledDeltaTime;
                if (_judgmentTimer <= 0f && _judgmentText != null)
                    _judgmentText.gameObject.SetActive(false);
            }
        }

        public void SetScore(Score score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Rate: {score.AchievementRate:F2}%";

            if (_comboText != null)
                _comboText.text = $"Combo: {score.Combo}";
        }

        /// <summary>Shows the judgment text at the default HUD position.</summary>
        public void ShowJudgment(Judgment grade)
        {
            ShowJudgment(grade, null);
        }

        /// <summary>Shows the judgment text at the world position of the note that was hit.</summary>
        public void ShowJudgment(Judgment grade, Vector3? worldPosition)
        {
            if (_judgmentText == null)
                return;

            _judgmentText.gameObject.SetActive(true);
            _judgmentText.text = grade.ToString().ToUpperInvariant();
            _judgmentText.color = DefaultVisualTheme.ColorForGrade(grade);
            _judgmentText.fontSize = JudgmentFontSize;
            _judgmentText.fontStyle = FontStyle.Bold;
            _judgmentTimer = JudgmentDisplayDuration;

            if (worldPosition.HasValue)
                PositionAtWorld(worldPosition.Value);

            PopJudgment();
        }

        private void PositionAtWorld(Vector3 worldPosition)
        {
            if (_judgmentRect == null)
                return;

            Camera camera = _worldCamera ?? Camera.main;
            if (camera == null)
                return;

            Canvas canvas = _judgmentRect.GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
                return;

            Vector3 screenPoint = camera.WorldToScreenPoint(worldPosition);
            Camera renderCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, renderCamera, out Vector2 localPoint))
                return;

            Vector2 pivotReference = new Vector2(
                Mathf.Lerp(canvasRect.rect.xMin, canvasRect.rect.xMax,
                    _judgmentRect.anchorMin.x + _judgmentRect.pivot.x * (_judgmentRect.anchorMax.x - _judgmentRect.anchorMin.x)),
                Mathf.Lerp(canvasRect.rect.yMin, canvasRect.rect.yMax,
                    _judgmentRect.anchorMin.y + _judgmentRect.pivot.y * (_judgmentRect.anchorMax.y - _judgmentRect.anchorMin.y)));

            _judgmentRect.anchoredPosition = localPoint - pivotReference;
        }

        private void PopJudgment()
        {
            if (_judgmentRect == null)
                return;

            if (_judgmentPop != null)
                StopCoroutine(_judgmentPop);

            _judgmentPop = StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            ResetJudgmentScale();
            Vector3 target = Vector3.one * JudgmentPopScale;
            float elapsed = 0f;

            while (elapsed < JudgmentPopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / JudgmentPopDuration;
                _judgmentRect.localScale = Vector3.Lerp(Vector3.one, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            _judgmentRect.localScale = target;
            elapsed = 0f;

            while (elapsed < JudgmentPopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / JudgmentPopDuration;
                _judgmentRect.localScale = Vector3.Lerp(target, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            _judgmentRect.localScale = Vector3.one;
            _judgmentPop = null;
        }

        private void ResetJudgmentScale()
        {
            if (_judgmentRect != null)
                _judgmentRect.localScale = Vector3.one;
        }

        private void EnsureHudChrome()
        {
            if (_scoreText != null)
            {
                _scoreGroup = EnsureCanvasGroup(_scoreText.gameObject);
                _scoreChrome = EnsureTextChrome(_scoreText);
            }

            if (_comboText != null)
            {
                _comboGroup = EnsureCanvasGroup(_comboText.gameObject);
                _comboChrome = EnsureTextChrome(_comboText);
            }
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            var group = go.GetComponent<CanvasGroup>();
            if (group == null)
                group = go.AddComponent<CanvasGroup>();
            return group;
        }

        private static Image EnsureTextChrome(Text text)
        {
            Transform parent = text.transform.parent;
            if (parent == null)
                return null;

            Transform existing = parent.Find(text.name + "Chrome");
            if (existing != null)
                return existing.GetComponent<Image>();

            var chromeGo = new GameObject(text.name + "Chrome");
            chromeGo.transform.SetParent(parent, false);
            chromeGo.transform.SetSiblingIndex(text.transform.GetSiblingIndex());

            var rect = chromeGo.AddComponent<RectTransform>();
            var textRect = text.rectTransform;
            rect.anchorMin = textRect.anchorMin;
            rect.anchorMax = textRect.anchorMax;
            rect.pivot = textRect.pivot;
            rect.anchoredPosition = textRect.anchoredPosition;
            rect.sizeDelta = textRect.sizeDelta + new Vector2(24f, 12f);

            var image = chromeGo.AddComponent<Image>();
            image.color = DefaultVisualTheme.HudChrome;
            image.raycastTarget = false;
            UiChrome.ApplyPanelBorder(chromeGo, DefaultVisualTheme.PanelBorderSoft, 1.5f);
            return image;
        }

        private void ApplyTheme()
        {
            if (_scoreText != null)
                _scoreText.color = DefaultVisualTheme.TextPrimary;

            if (_comboText != null)
                _comboText.color = DefaultVisualTheme.TextPrimary;

            if (_scoreChrome != null)
            {
                _scoreChrome.color = DefaultVisualTheme.HudChrome;
                UiChrome.ApplyPanelBorder(_scoreChrome.gameObject, DefaultVisualTheme.JudgmentLine, 1.5f);
            }

            if (_comboChrome != null)
            {
                _comboChrome.color = DefaultVisualTheme.HudChrome;
                UiChrome.ApplyPanelBorder(_comboChrome.gameObject, DefaultVisualTheme.TopJudgmentLine, 1.5f);
            }
        }

        private void PlayHudFadeIn()
        {
            if (_hudFade != null)
                StopCoroutine(_hudFade);

            _hudFade = StartCoroutine(HudFadeRoutine());
        }

        private IEnumerator HudFadeRoutine()
        {
            SetHudAlpha(0f);
            float elapsed = 0f;
            float duration = DefaultVisualTheme.HudFadeDuration;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetHudAlpha(Mathf.SmoothStep(0f, 1f, elapsed / duration));
                yield return null;
            }

            SetHudAlpha(1f);
            _hudFade = null;
        }

        private void SetHudAlpha(float alpha)
        {
            if (_scoreGroup != null)
                _scoreGroup.alpha = alpha;

            if (_comboGroup != null)
                _comboGroup.alpha = alpha;

            if (_scoreChrome != null)
            {
                Color c = _scoreChrome.color;
                c.a = DefaultVisualTheme.HudChrome.a * alpha;
                _scoreChrome.color = c;
            }

            if (_comboChrome != null)
            {
                Color c = _comboChrome.color;
                c.a = DefaultVisualTheme.HudChrome.a * alpha;
                _comboChrome.color = c;
            }
        }
    }
}
