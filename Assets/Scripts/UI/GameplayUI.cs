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
            _judgmentTimer = JudgmentDisplayDuration;
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
