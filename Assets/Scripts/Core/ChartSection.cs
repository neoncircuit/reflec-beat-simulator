using System;
using System.Collections.Generic;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Pure helper for selecting a subset of notes inside a time range.</summary>
    public static class ChartSection
    {
        /// <summary>
        /// Returns notes whose head time is inside <paramref name="startSec"/> inclusive
        /// to <paramref name="endSec"/> exclusive. Notes must already have
        /// <see cref="Note.TimeSec"/> assigned (e.g. via <see cref="TimingConverter"/>).
        /// </summary>
        public static List<Note> GetNotesInRange(IList<Note> notes, double startSec, double endSec)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));
            if (startSec > endSec)
                throw new ArgumentException("startSec must be less than or equal to endSec.", nameof(startSec));

            List<Note> result = new List<Note>();
            foreach (Note note in notes)
            {
                if (note == null)
                    continue;

                if (note.TimeSec >= startSec && note.TimeSec < endSec)
                    result.Add(note);
            }

            return result;
        }
    }
}
