using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Deterministic CPU rival that schedules judgments for rival notes.</summary>
    public sealed class CpuRival
    {
        private readonly List<ScheduledJudgment> _schedule;
        private int _nextIndex;

        /// <summary>True while at least one rival judgment has not yet been consumed.</summary>
        public bool HasPending => _nextIndex < _schedule.Count;

        /// <summary>Number of rival judgments in the schedule.</summary>
        public int Count => _schedule.Count;

        public CpuRival(IEnumerable<Note> notes, RivalConfig config)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _schedule = notes
                .Where(n => n != null && n.IsRival)
                .OrderBy(n => n.TimeSec)
                .Select(n => new ScheduledJudgment
                {
                    Note = n,
                    Grade = ComputeGrade(n.Tick, config.Accuracy),
                    TimeSec = n.TimeSec
                })
                .ToList();
        }

        /// <summary>Consumes all rival judgments whose time has passed and invokes the callback for each.</summary>
        public void Process(double songTime, Action<Note, Judgment> onJudgment)
        {
            if (onJudgment == null)
                throw new ArgumentNullException(nameof(onJudgment));

            while (HasPending && _schedule[_nextIndex].TimeSec <= songTime)
            {
                ScheduledJudgment entry = _schedule[_nextIndex++];
                onJudgment(entry.Note, entry.Grade);
            }
        }

        /// <summary>Returns the grade distribution the CPU would use for the given note tick.</summary>
        public static Judgment ComputeGrade(int tick, double accuracy)
        {
            if (accuracy < 0.0 || accuracy > 1.0)
                throw new ArgumentOutOfRangeException(nameof(accuracy), "Accuracy must be between 0.0 and 1.0.");

            Random rng = new Random(tick);
            double roll = rng.NextDouble();

            if (roll < accuracy)
                return Judgment.Just;

            double greatThreshold = accuracy + (1.0 - accuracy) * 0.5;
            if (roll < greatThreshold)
                return Judgment.Great;

            return Judgment.Good;
        }

        private sealed class ScheduledJudgment
        {
            public Note Note;
            public Judgment Grade;
            public double TimeSec;
        }
    }
}
