using System;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Achievement-rate scoring and combo tracking.</summary>
    public sealed class Score
    {
        private readonly int _maxPossible;
        private int _earned;

        public Score(int totalNotes)
        {
            if (totalNotes < 0)
                throw new ArgumentOutOfRangeException(nameof(totalNotes));

            TotalNotes = totalNotes;
            _maxPossible = totalNotes * 100;

            if (totalNotes == 0)
                AchievementRate = 100.0;
        }

        /// <summary>Total number of notes in the chart.</summary>
        public int TotalNotes { get; }

        public int JustCount { get; private set; }
        public int GreatCount { get; private set; }
        public int GoodCount { get; private set; }
        public int MissCount { get; private set; }

        /// <summary>Current combo (resets on MISS).</summary>
        public int Combo { get; private set; }

        /// <summary>Highest combo achieved so far.</summary>
        public int MaxCombo { get; private set; }

        /// <summary>Achievement rate from 0.0 to 100.0.</summary>
        public double AchievementRate { get; private set; }

        /// <summary>Applies a judgment to the running score.</summary>
        public void Apply(Judgment judgment)
        {
            int points = judgment switch
            {
                Judgment.Just => 100,
                Judgment.Great => 70,
                Judgment.Good => 40,
                Judgment.Miss => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(judgment))
            };

            _earned += points;

            switch (judgment)
            {
                case Judgment.Just: JustCount++; break;
                case Judgment.Great: GreatCount++; break;
                case Judgment.Good: GoodCount++; break;
                case Judgment.Miss: MissCount++; break;
            }

            if (judgment == Judgment.Miss)
                Combo = 0;
            else
                Combo++;

            if (Combo > MaxCombo)
                MaxCombo = Combo;

            AchievementRate = _maxPossible > 0
                ? (_earned / (double)_maxPossible) * 100.0
                : 100.0;
        }
    }
}
