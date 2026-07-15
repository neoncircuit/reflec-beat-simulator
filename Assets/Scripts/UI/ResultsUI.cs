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
