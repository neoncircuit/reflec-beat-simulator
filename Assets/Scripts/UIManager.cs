using TMPro;
using UnityEngine;

namespace ReflecBeat
{
    /// <summary>
    /// Updates the on-screen score, combo, and last judgment text.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Text Fields")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI comboText;
        public TextMeshProUGUI judgmentText;

        private void Start()
        {
            ShowJudgment("READY", 0, 0);
        }

        public void ShowJudgment(string judgment, int combo, int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (comboText != null)
                comboText.text = $"Combo: {combo}";

            if (judgmentText != null)
                judgmentText.text = judgment;
        }
    }
}
