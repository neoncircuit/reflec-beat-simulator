using System;
using System.Collections;
using System.Collections.Generic;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Cinematic soft-card chart-select list and option toggles.</summary>
    public sealed class ChartSelectUI : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _chartButtonPrefab;
        [SerializeField] private Toggle _autoplayToggle;
        [SerializeField] private Toggle _syncCheckToggle;
        [SerializeField] private Toggle _hideRivalToggle;
        [SerializeField] private Button _calibrateButton;
        [SerializeField] private Text _approachTimeLabel;
        [SerializeField] private Button _approachTimePrevButton;
        [SerializeField] private Button _approachTimeNextButton;
        [SerializeField] private PanelFadeSlide _rootFade;
        [SerializeField] private CanvasGroup _rootCanvasGroup;

        private readonly List<GameObject> _buttons = new List<GameObject>();
        private int _approachTimeIndex;

        public event Action<int> OnChartSelected;
        public event Action OnCalibrate;

        public bool Autoplay => _autoplayToggle != null && _autoplayToggle.isOn;
        public bool SyncCheck => _syncCheckToggle != null && _syncCheckToggle.isOn;
        public bool HideRivalVisuals => _hideRivalToggle != null && _hideRivalToggle.isOn;

        public double ApproachTime
        {
            get
            {
                if (_approachTimeIndex < 0 || _approachTimeIndex >= Constants.ApproachTimeOptions.Count)
                    return Constants.DefaultApproachTime;

                return Constants.ApproachTimeOptions[_approachTimeIndex];
            }
        }

        private void Awake()
        {
            EnsureRootFade();
            ApplyCinematicMenu();
        }

        private void Start()
        {
            ApplySettings(SettingsStore.Load());
            UpdateApproachTimeLabel();

            if (_calibrateButton != null)
                _calibrateButton.onClick.AddListener(() => OnCalibrate?.Invoke());

            if (_approachTimePrevButton != null)
                _approachTimePrevButton.onClick.AddListener(() => CycleApproachTime(-1));

            if (_approachTimeNextButton != null)
                _approachTimeNextButton.onClick.AddListener(() => CycleApproachTime(1));

            PlayEnterTransition();
        }

        private void OnDestroy()
        {
            if (_calibrateButton != null)
                _calibrateButton.onClick.RemoveAllListeners();

            if (_approachTimePrevButton != null)
                _approachTimePrevButton.onClick.RemoveAllListeners();

            if (_approachTimeNextButton != null)
                _approachTimeNextButton.onClick.RemoveAllListeners();
        }

        public void ApplySettings(SettingsStore.GameSettings settings)
        {
            if (settings == null)
                return;

            if (_autoplayToggle != null)
                _autoplayToggle.isOn = settings.Autoplay;

            if (_syncCheckToggle != null)
                _syncCheckToggle.isOn = settings.SyncCheck;

            if (_hideRivalToggle != null)
                _hideRivalToggle.isOn = settings.HideRivalVisuals;

            SetApproachTimeIndexFromValue(settings.ApproachTimeSec);
        }

        private void CycleApproachTime(int delta)
        {
            int count = Constants.ApproachTimeOptions.Count;
            if (count == 0)
                return;

            _approachTimeIndex = (_approachTimeIndex + delta + count) % count;
            UpdateApproachTimeLabel();
        }

        private void SetApproachTimeIndexFromValue(double approachTimeSec)
        {
            _approachTimeIndex = 0;
            for (int i = 0; i < Constants.ApproachTimeOptions.Count; i++)
            {
                if (Math.Abs(Constants.ApproachTimeOptions[i] - approachTimeSec) < 1e-6)
                {
                    _approachTimeIndex = i;
                    break;
                }
            }

            UpdateApproachTimeLabel();
        }

        private void UpdateApproachTimeLabel()
        {
            if (_approachTimeLabel != null)
            {
                _approachTimeLabel.text = $"Approach: {ApproachTime:0.00}s";
                _approachTimeLabel.color = DefaultVisualTheme.MenuText;
            }
        }

        public void SetTitle(string title)
        {
            if (_titleText != null)
                _titleText.text = title;
        }

        public void ClearButtons()
        {
            foreach (GameObject button in _buttons)
            {
                if (button == null)
                    continue;

                var buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                    buttonComponent.onClick.RemoveAllListeners();

                Destroy(button);
            }

            _buttons.Clear();
        }

        public void AddButton(string title, string artist, string difficulty, string best, int index)
        {
            if (_content == null)
            {
                Debug.LogError("ChartSelectUI: _content is not assigned; cannot add button.");
                return;
            }

            if (_chartButtonPrefab == null)
            {
                Debug.LogError("ChartSelectUI: _chartButtonPrefab is not assigned; cannot add button.");
                return;
            }

            GameObject button = Instantiate(_chartButtonPrefab, _content, false);
            button.name = $"ChartButton_{index}";

            SetButtonText(button, "TitleText", title, DefaultVisualTheme.MenuText, 30, FontStyle.Bold);
            SetButtonText(button, "ArtistText", artist, DefaultVisualTheme.MenuTextMuted, 22, FontStyle.Normal);
            SetButtonText(button, "DifficultyText", difficulty, DefaultVisualTheme.MenuAccent, 22, FontStyle.Bold);
            SetButtonText(button, "BestText", best, DefaultVisualTheme.MenuAccentAlt, 20, FontStyle.Normal);
            UiChrome.StyleChartCard(button);

            var buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                int capturedIndex = index;
                buttonComponent.onClick.AddListener(() => OnChartSelected?.Invoke(capturedIndex));
            }

            _buttons.Add(button);
            StartCoroutine(FadeInCard(button, index));
        }

        private static void SetButtonText(
            GameObject button,
            string childName,
            string text,
            Color color,
            int fontSize,
            FontStyle style)
        {
            Transform child = button.transform.Find(childName);
            if (child == null)
                return;

            var label = child.GetComponent<Text>();
            if (label == null)
                return;

            label.text = text ?? string.Empty;
            label.color = color;
            label.fontSize = fontSize;
            label.fontStyle = style;
        }

        public void SetInteractable(bool interactable)
        {
            foreach (GameObject button in _buttons)
            {
                if (button == null)
                    continue;

                var buttonComponent = button.GetComponent<Button>();
                if (buttonComponent != null)
                    buttonComponent.interactable = interactable;
            }
        }

        private void EnsureRootFade()
        {
            if (_rootFade != null)
                return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            if (_rootCanvasGroup == null)
            {
                _rootCanvasGroup = canvas.GetComponent<CanvasGroup>();
                if (_rootCanvasGroup == null)
                    _rootCanvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }

            _rootFade = canvas.GetComponent<PanelFadeSlide>();
            if (_rootFade == null)
                _rootFade = canvas.gameObject.AddComponent<PanelFadeSlide>();

            _rootFade.Configure(DefaultVisualTheme.ChartSelectEnterDuration, new Vector2(0f, 36f));
        }

        private void PlayEnterTransition()
        {
            if (_rootFade == null)
                return;

            if (_rootCanvasGroup != null)
                _rootCanvasGroup.alpha = 0f;

            _rootFade.Show();
        }

        private void ApplyCinematicMenu()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            UiChrome.EnsureMenuBackdrop(canvas);

            Camera cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = DefaultVisualTheme.MenuBackgroundDeep;

            if (_titleText != null)
            {
                _titleText.color = DefaultVisualTheme.MenuText;
                _titleText.fontStyle = FontStyle.Bold;
                _titleText.fontSize = Mathf.Max(_titleText.fontSize, 48);
                UiChrome.ApplyHeaderBanner(canvas != null ? canvas.gameObject : gameObject, "MUSIC SELECT", _titleText.font);
                _titleText.gameObject.SetActive(false);
            }

            if (_approachTimeLabel != null)
                _approachTimeLabel.color = DefaultVisualTheme.MenuText;

            UiChrome.StyleMenuButton(_calibrateButton, accent: true);
            UiChrome.StyleMenuButton(_approachTimePrevButton);
            UiChrome.StyleMenuButton(_approachTimeNextButton);
            ApplyToggleTheme(_autoplayToggle);
            ApplyToggleTheme(_syncCheckToggle);
            ApplyToggleTheme(_hideRivalToggle);
        }

        private static void ApplyToggleTheme(Toggle toggle)
        {
            if (toggle == null)
                return;

            var label = toggle.GetComponentInChildren<Text>();
            if (label != null)
                label.color = DefaultVisualTheme.MenuText;
        }

        private IEnumerator FadeInCard(GameObject button, int index)
        {
            var group = button.GetComponent<CanvasGroup>();
            if (group == null)
                group = button.AddComponent<CanvasGroup>();

            RectTransform rect = button.GetComponent<RectTransform>();
            Vector3 baseScale = Vector3.one;
            group.alpha = 0f;
            if (rect != null)
                rect.localScale = baseScale * 0.94f;

            float delay = index * DefaultVisualTheme.ChartCardStagger;
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            float elapsed = 0f;
            float duration = DefaultVisualTheme.ChartSelectEnterDuration;
            while (elapsed < duration)
            {
                if (button == null)
                    yield break;

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                group.alpha = t;
                if (rect != null)
                    rect.localScale = Vector3.Lerp(baseScale * 0.94f, baseScale, t);
                yield return null;
            }

            if (group != null)
                group.alpha = 1f;
            if (rect != null)
                rect.localScale = baseScale;
        }
    }
}
