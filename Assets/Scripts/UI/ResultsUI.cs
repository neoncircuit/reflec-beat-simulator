using System.Collections;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Cinematic soft-card results panel shown at the end of a chart.</summary>
    public sealed class ResultsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private PanelFadeSlide _panelFade;
        [SerializeField] private Text _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _backButton;

        private const float ButtonWidth = 280f;
        private const float ButtonHeight = 60f;
        private const float GradePopDuration = 0.20f;
        private const float GradePopScale = 1.14f;

        private RectTransform _resultRect;
        private Coroutine _gradePop;
        private Image _dimmer;

        private void Awake()
        {
            EnsureDimmer();
            EnsureButtons();

            if (_panelFade == null && _panel != null)
                _panelFade = _panel.GetComponent<PanelFadeSlide>();

            _panelFade?.Configure(DefaultVisualTheme.PanelShowDuration, DefaultVisualTheme.PanelHiddenOffset);

            if (_resultText != null)
            {
                _resultRect = _resultText.GetComponent<RectTransform>();
                _resultText.color = DefaultVisualTheme.MenuText;
                _resultText.supportRichText = true;
            }

            ApplyPanelTheme();
        }

        public void Show(Score score)
        {
            Show(score, null, null);
        }

        public void Show(Score score, DuelOutcome? outcome)
        {
            Show(score, outcome, null);
        }

        public void Show(Score score, DuelOutcome? outcome, SyncReport syncReport)
        {
            if (_dimmer != null)
                _dimmer.gameObject.SetActive(true);

            if (_panel != null)
                _panel.SetActive(true);

            _panelFade?.Show();

            if (_resultText == null)
                return;

            string clearLine = score.MissCount == 0
                ? $"<size=42><color=#{ColorUtility.ToHtmlStringRGB(DefaultVisualTheme.MenuAccent)}><b>CLEAR!!</b></color></size>\n"
                : $"<size=42><color=#{ColorUtility.ToHtmlStringRGB(DefaultVisualTheme.MenuAccentAlt)}><b>RESULT</b></color></size>\n";

            string outcomeLine = outcome.HasValue
                ? $"<size=36><color={DefaultVisualTheme.HexForOutcome(outcome.Value)}><b>{outcome.Value.ToString().ToUpperInvariant()}</b></color></size>\n"
                : string.Empty;

            string gradeLine =
                $"<size=40><b>RANK  {score.Grade}</b></size>    " +
                $"<size=34><color=#{ColorUtility.ToHtmlStringRGB(DefaultVisualTheme.MenuAccentAlt)}>" +
                $"AR  {score.AchievementRate:F1}%</color></size>\n\n";

            string syncLine = syncReport != null
                ? $"<size=22>Sync Check — max Δ {syncReport.MaxTimingDeltaMs:F1} ms · " +
                  $"drift {syncReport.MaxClockDriftMs:F1} ms · misses {syncReport.MissedNotes} · " +
                  $"{(syncReport.IsWithinTarget() ? "WITHIN TARGET" : "OUTSIDE TARGET")}</size>\n\n"
                : string.Empty;

            string just = DefaultVisualTheme.HexForGrade(Judgment.Just);
            string great = DefaultVisualTheme.HexForGrade(Judgment.Great);
            string good = DefaultVisualTheme.HexForGrade(Judgment.Good);
            string miss = DefaultVisualTheme.HexForGrade(Judgment.Miss);

            _resultText.text =
                syncLine +
                clearLine +
                outcomeLine +
                gradeLine +
                $"<color={just}>JUST</color>  {score.JustCount}     " +
                $"<color={great}>GREAT</color>  {score.GreatCount}     " +
                $"<color={good}>GOOD</color>  {score.GoodCount}     " +
                $"<color={miss}>MISS</color>  {score.MissCount}\n\n" +
                $"<b>MAX COMBO</b>  {score.MaxCombo}";

            PopGrade();
        }

        private void EnsureDimmer()
        {
            if (_panel == null || _dimmer != null)
                return;

            Transform parent = _panel.transform.parent;
            if (parent == null)
                return;

            Transform existing = parent.Find("ResultsDimmer");
            if (existing != null)
            {
                _dimmer = existing.GetComponent<Image>();
                return;
            }

            var go = new GameObject("ResultsDimmer");
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(_panel.transform.GetSiblingIndex());

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _dimmer = go.AddComponent<Image>();
            _dimmer.color = new Color(0.05f, 0.08f, 0.12f, 0.55f);
            _dimmer.raycastTarget = true;
            go.SetActive(false);
        }

        private void EnsureButtons()
        {
            if (_panel == null)
                return;

            if (_retryButton == null)
            {
                _retryButton = CreateButton(
                    "RetryButton",
                    "RETRY",
                    new Vector2(-150f, -150f),
                    () => TransitionManager.LoadScene("GameplayScene"),
                    accent: false);
            }

            if (_backButton == null)
            {
                _backButton = CreateButton(
                    "BackButton",
                    "MUSIC SELECT",
                    new Vector2(150f, -150f),
                    () => TransitionManager.LoadScene("ChartSelectScene"),
                    accent: true);
            }

            UiChrome.StyleMenuButton(_retryButton);
            UiChrome.StyleMenuButton(_backButton, accent: true);
        }

        private Button CreateButton(
            string name,
            string label,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction onClick,
            bool accent)
        {
            var buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(_panel.transform, false);

            var rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.one * 0.5f;
            rect.anchorMax = Vector2.one * 0.5f;
            rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var image = buttonGo.AddComponent<Image>();
            image.color = accent ? DefaultVisualTheme.MenuAccent : DefaultVisualTheme.MenuCardAlt;

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            buttonGo.AddComponent<ButtonScaleEffect>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.color = accent ? Color.white : DefaultVisualTheme.MenuText;
            text.alignment = TextAnchor.MiddleCenter;

            if (_resultText != null)
                text.font = _resultText.font;

            return button;
        }

        private void ApplyPanelTheme()
        {
            if (_panel == null)
                return;

            // Enlarge card for cinematic results layout.
            var rect = _panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(900f, 520f);
            }

            UiChrome.StyleSoftCard(_panel, 3.5f);
            UiChrome.ApplyHeaderBanner(_panel, "RESULT", _resultText != null ? _resultText.font : null);
        }

        private void PopGrade()
        {
            if (_resultRect == null)
                return;

            if (_gradePop != null)
                StopCoroutine(_gradePop);

            _gradePop = StartCoroutine(GradePopRoutine());
        }

        private IEnumerator GradePopRoutine()
        {
            _resultRect.localScale = Vector3.one * 0.96f;
            Vector3 target = Vector3.one * GradePopScale;
            float elapsed = 0f;

            while (elapsed < GradePopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / GradePopDuration);
                _resultRect.localScale = Vector3.Lerp(Vector3.one * 0.96f, target, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < GradePopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / GradePopDuration);
                _resultRect.localScale = Vector3.Lerp(target, Vector3.one, t);
                yield return null;
            }

            _resultRect.localScale = Vector3.one;
            _gradePop = null;
        }
    }
}
