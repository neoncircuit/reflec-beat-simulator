using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>In-game HUD for score, combo, and rate.</summary>
    public sealed class GameplayUI : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _comboText;

        public void SetScore(Score score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Rate: {score.AchievementRate:F2}%";

            if (_comboText != null)
                _comboText.text = $"Combo: {score.Combo}";
        }
    }
}
