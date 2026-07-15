using ReflecBeatSimulator.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>Plain-text results panel shown at the end of a chart.</summary>
    public sealed class ResultsUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _resultText;

        public void Show(Score score)
        {
            Show(score, null);
        }

        public void Show(Score score, DuelOutcome? outcome)
        {
            if (_panel != null)
                _panel.SetActive(true);

            if (_resultText != null)
            {
                string outcomeLine = outcome.HasValue
                    ? $"Duel Result: {outcome.Value}\n"
                    : string.Empty;

                _resultText.text =
                    outcomeLine +
                    $"Achievement Rate: {score.AchievementRate:F2}%\n" +
                    $"JUST {score.JustCount}  GREAT {score.GreatCount}  GOOD {score.GoodCount}  MISS {score.MissCount}\n" +
                    $"Max Combo: {score.MaxCombo}";
            }
        }
    }
}
