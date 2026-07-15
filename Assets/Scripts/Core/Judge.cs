using System;
using System.Collections.Generic;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Judgment logic: time windows and note matching.</summary>
    public static class Judge
    {
        /// <summary>Returns the grade for an absolute time delta, ignoring sign.</summary>
        public static Judgment Grade(double absDeltaSec)
        {
            if (absDeltaSec <= Constants.JustWindow)
                return Judgment.Just;
            if (absDeltaSec <= Constants.GreatWindow)
                return Judgment.Great;
            if (absDeltaSec <= Constants.GoodWindow)
                return Judgment.Good;
            return Judgment.Miss;
        }

        /// <summary>
        /// Finds the best unjudged note for a touch. Returns null if no candidate is within
        /// the time and spatial windows.
        /// </summary>
        public static (Note note, Judgment grade)? JudgeTouch(
            double touchTime,
            double touchX,
            IEnumerable<Note> candidates)
        {
            Note best = null;
            double bestAbs = double.MaxValue;

            foreach (var note in candidates)
            {
                if (note == null)
                    continue;

                if (!CanMatchType(note.Type))
                    continue;

                double dt = Math.Abs(touchTime - note.TimeSec);
                if (dt > Constants.GoodWindow)
                    continue;

                if (!IsInSpatialWindow(note, touchX))
                    continue;

                if (dt < bestAbs)
                {
                    best = note;
                    bestAbs = dt;
                }
            }

            if (best == null)
                return null;

            return (best, Grade(bestAbs));
        }

        private static bool CanMatchType(string type)
        {
            return type == NoteType.Tap ||
                   type == NoteType.Long ||
                   type == NoteType.Vertical ||
                   type == NoteType.Top ||
                   type == NoteType.BigBang;
        }

        /// <summary>Returns the worse of two grades, where MISS is worst and JUST is best.</summary>
        public static Judgment Worse(Judgment a, Judgment b)
        {
            return Rank(a) > Rank(b) ? a : b;
        }

        private static int Rank(Judgment grade)
        {
            switch (grade)
            {
                case Judgment.Just: return 0;
                case Judgment.Great: return 1;
                case Judgment.Good: return 2;
                case Judgment.Miss: return 3;
                default: return 3;
            }
        }

        /// <summary>Grades a long-note release. The caller combines this with the head grade via <see cref="Worse"/>.</summary>
        public static Judgment JudgeRelease(double releaseTime, Note note)
        {
            if (note.Type != NoteType.Long)
                throw new ArgumentException("Release grading only applies to long notes.", nameof(note));

            return Grade(Math.Abs(releaseTime - note.EndTimeSec));
        }

        private static bool IsInSpatialWindow(Note note, double touchX)
        {
            if (note.Type == NoteType.BigBang)
                return true;

            if (note.Type == NoteType.Top)
                return NoteZoneAtX(touchX) == note.Zone;

            return Math.Abs(touchX - note.X) <= Constants.TapHitHalfWidth;
        }

        private static int NoteZoneAtX(double x)
        {
            if (x < 1.0 / 3.0)
                return 0;
            if (x < 2.0 / 3.0)
                return 1;
            return 2;
        }
    }
}
