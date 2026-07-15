using System.Collections.Generic;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class ChartSectionTests
    {
        [Test]
        public void GetNotesInRange_KeepsNotesInRange()
        {
            List<Note> notes = new List<Note>
            {
                new Note { TimeSec = 1.0 },
                new Note { TimeSec = 2.0 },
                new Note { TimeSec = 3.0 }
            };

            List<Note> result = ChartSection.GetNotesInRange(notes, 1.0, 4.0);

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetNotesInRange_ExcludesNotesBeforeStart()
        {
            List<Note> notes = new List<Note>
            {
                new Note { TimeSec = 0.5 },
                new Note { TimeSec = 1.5 }
            };

            List<Note> result = ChartSection.GetNotesInRange(notes, 1.0, 3.0);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TimeSec, Is.EqualTo(1.5));
        }

        [Test]
        public void GetNotesInRange_ExcludesNotesAtOrAfterEnd()
        {
            List<Note> notes = new List<Note>
            {
                new Note { TimeSec = 1.0 },
                new Note { TimeSec = 2.0 },
                new Note { TimeSec = 3.0 }
            };

            List<Note> result = ChartSection.GetNotesInRange(notes, 0.0, 2.0);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TimeSec, Is.EqualTo(1.0));
        }

        [Test]
        public void GetNotesInRange_EmptyInput_ReturnsEmpty()
        {
            List<Note> result = ChartSection.GetNotesInRange(new List<Note>(), 0.0, 10.0);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetNotesInRange_LongNoteHeadInRange_IsIncluded()
        {
            List<Note> notes = new List<Note>
            {
                new Note { Tick = 480, TimeSec = 1.0, Type = NoteType.Long, X = 0.5f, DurationTicks = 960, EndTimeSec = 3.0 }
            };

            List<Note> result = ChartSection.GetNotesInRange(notes, 0.5, 2.0);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].TimeSec, Is.EqualTo(1.0));
        }

        [Test]
        public void GetNotesInRange_StartGreaterThanEnd_Throws()
        {
            List<Note> notes = new List<Note> { new Note { TimeSec = 1.0 } };

            Assert.That(() => ChartSection.GetNotesInRange(notes, 5.0, 1.0), Throws.ArgumentException);
        }

        [Test]
        public void GetNotesInRange_NullNotes_Throws()
        {
            Assert.That(() => ChartSection.GetNotesInRange(null, 0.0, 1.0), Throws.ArgumentNullException);
        }
    }
}
