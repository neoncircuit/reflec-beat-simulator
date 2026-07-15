using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Plain-text results panel shown at the end of a chart.</summary>
    public sealed class ResultsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _resultText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _backButton;

        private const float ButtonWidth = 240f;
        private const float ButtonHeight = 56f;

        private void Start()
        {
            EnsureButtons();
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
            if (_panel != null)
                _panel.SetActive(true);

            if (_resultText == null)
                return;

            string outcomeLine = outcome.HasValue
                ? $"<size=48><color={ColorForOutcome(outcome.Value)}><b>{outcome.Value.ToString().ToUpperInvariant()}</b></color></size>\n"
                : string.Empty;

            string gradeLine = $"<size=36><b>Grade {score.Grade}</b></size>\n";

            string syncLine = syncReport != null
                ? $"Sync Check\nMax timing delta: {syncReport.MaxTimingDeltaMs:F2} ms\nMax clock drift: {syncReport.MaxClockDriftMs:F2} ms\nMisses: {syncReport.MissedNotes}\n{(syncReport.IsWithinTarget() ? "WITHIN TARGET" : "OUTSIDE TARGET")}\n\n"
                : string.Empty;

            _resultText.text =
                syncLine +
                outcomeLine +
                gradeLine +
                $"Achievement Rate: {score.AchievementRate:F2}%\n" +
                $"JUST {score.JustCount}  GREAT {score.GreatCount}  GOOD {score.GoodCount}  MISS {score.MissCount}\n" +
                $"Max Combo: {score.MaxCombo}";
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
                    new Vector2(0f, -100f),
                    () => SceneManager.LoadScene("GameplayScene"));
            }

            if (_backButton == null)
            {
                _backButton = CreateButton(
                    "BackButton",
                    "BACK",
                    new Vector2(0f, -170f),
                    () => SceneManager.LoadScene("ChartSelectScene"));
            }
        }

        private Button CreateButton(string name, string label, Vector2 anchoredPosition, UnityAction onClick)
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
            image.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

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
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            if (_resultText != null)
                text.font = _resultText.font;

            return button;
        }

        private static string ColorForOutcome(DuelOutcome outcome)
        {
            switch (outcome)
            {
                case DuelOutcome.Win:
                    return "#00FF00FF";
                case DuelOutcome.Lose:
                    return "#FF0000FF";
                default:
                    return "#FFFFFFFF";
            }
        }
    }
}
