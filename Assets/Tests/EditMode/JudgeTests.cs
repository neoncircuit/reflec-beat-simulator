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
    }
}
