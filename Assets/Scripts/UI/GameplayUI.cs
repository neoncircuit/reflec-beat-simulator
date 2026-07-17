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

        private void Awake()
        {
            if (_judgmentText != null)
                _judgmentRect = _judgmentText.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            ResetJudgmentScale();
        }

        private void OnDisable()
        {
            if (_judgmentPop != null)
            {
                StopCoroutine(_judgmentPop);
                _judgmentPop = null;
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

        public void ShowJudgment(Judgment grade)
        {
            if (_judgmentText == null)
                return;

            _judgmentText.gameObject.SetActive(true);
            _judgmentText.text = grade.ToString().ToUpperInvariant();
            _judgmentText.color = ColorForGrade(grade);
            _judgmentText.fontSize = JudgmentFontSize;
            _judgmentText.fontStyle = FontStyle.Bold;
            _judgmentTimer = JudgmentDisplayDuration;

            PopJudgment();
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

        private static Color ColorForGrade(Judgment grade)
        {
            switch (grade)
            {
                case Judgment.Just:
                    return new Color(1f, 0.84f, 0f); // gold
                case Judgment.Great:
                    return Color.green;
                case Judgment.Good:
                    return Color.cyan;
                case Judgment.Miss:
                    return Color.red;
                default:
                    return Color.white;
            }
        }
    }
}
