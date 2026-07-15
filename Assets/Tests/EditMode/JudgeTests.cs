using System.Collections.Generic;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class JudgeTests
    {
        private static Note NoteAt(double time, double x, string type = NoteType.Tap) =>
            new Note { TimeSec = time, X = (float)x, Type = type };

        [Test]
        public void Grade_JustAtZero()
        {
            Assert.That(Judge.Grade(0.0), Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void Grade_WindowsAtBoundaries()
        {
            Assert.That(Judge.Grade(Constants.JustWindow), Is.EqualTo(Judgment.Just));
            Assert.That(Judge.Grade(Constants.JustWindow + 1e-9), Is.EqualTo(Judgment.Great));
            Assert.That(Judge.Grade(Constants.GreatWindow), Is.EqualTo(Judgment.Great));
            Assert.That(Judge.Grade(Constants.GoodWindow), Is.EqualTo(Judgment.Good));
            Assert.That(Judge.Grade(Constants.GoodWindow + 0.001), Is.EqualTo(Judgment.Miss));
        }

        [Test]
        public void JudgeTouch_PicksNearestNoteWithinWindow()
        {
            var notes = new List<Note>
            {
                NoteAt(1.0, 0.2),
                NoteAt(1.1, 0.2)
            };
            var result = Judge.JudgeTouch(1.0, 0.2, notes);

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.note.TimeSec, Is.EqualTo(1.0).Within(1e-9));
            Assert.That(result.Value.grade, Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void JudgeTouch_RespectsXWindow()
        {
            var notes = new List<Note> { NoteAt(1.0, 0.0) };
            var result = Judge.JudgeTouch(1.0, 0.5, notes);

            Assert.That(result.HasValue, Is.False);
        }

        [Test]
        public void JudgeTouch_NullWhenNoCandidate()
        {
            var notes = new List<Note> { NoteAt(10.0, 0.5) };
            var result = Judge.JudgeTouch(1.0, 0.5, notes);

            Assert.That(result.HasValue, Is.False);
        }

        [Test]
        public void JudgeTouch_MatchesVerticalNote()
        {
            var notes = new List<Note> { NoteAt(1.0, 0.3, NoteType.Vertical) };
            var result = Judge.JudgeTouch(1.0, 0.3, notes);

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.grade, Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void JudgeTouch_MatchesTopNoteByZone()
        {
            var notes = new List<Note> { new Note { TimeSec = 1.0, Type = NoteType.Top, Zone = 1 } };
            var result = Judge.JudgeTouch(1.0, 0.5, notes); // zone 1 is 1/3..2/3

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.grade, Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void JudgeTouch_TopNoteWrongZoneMisses()
        {
            var notes = new List<Note> { new Note { TimeSec = 1.0, Type = NoteType.Top, Zone = 0 } };
            var result = Judge.JudgeTouch(1.0, 0.5, notes);

            Assert.That(result.HasValue, Is.False);
        }

        [Test]
        public void JudgeTouch_MatchesBigBangRegardlessOfX()
        {
            var notes = new List<Note> { new Note { TimeSec = 1.0, Type = NoteType.BigBang } };
            var result = Judge.JudgeTouch(1.0, 0.99, notes);

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.grade, Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void JudgeTouch_MatchesLongNoteHead()
        {
            var notes = new List<Note> { NoteAt(1.0, 0.5, NoteType.Long) };
            var result = Judge.JudgeTouch(1.0, 0.5, notes);

            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value.grade, Is.EqualTo(Judgment.Just));
        }

        [Test]
        public void JudgeRelease_GradesTailByEndTime()
        {
            var note = new Note { Type = NoteType.Long, TimeSec = 1.0, EndTimeSec = 2.0 };
            Assert.That(Judge.JudgeRelease(2.0, note), Is.EqualTo(Judgment.Just));
            Assert.That(Judge.JudgeRelease(2.05, note), Is.EqualTo(Judgment.Great));
            Assert.That(Judge.JudgeRelease(2.08, note), Is.EqualTo(Judgment.Good));
            Assert.That(Judge.JudgeRelease(2.2, note), Is.EqualTo(Judgment.Miss));
        }

        [Test]
        public void Worse_PicksMissOverJust()
        {
            Assert.That(Judge.Worse(Judgment.Just, Judgment.Miss), Is.EqualTo(Judgment.Miss));
            Assert.That(Judge.Worse(Judgment.Good, Judgment.Great), Is.EqualTo(Judgment.Good));
            Assert.That(Judge.Worse(Judgment.Just, Judgment.Just), Is.EqualTo(Judgment.Just));
        }
    }
}
