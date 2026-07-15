using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class CpuRivalTests
    {
        private static Note PlayerNoteAt(double time) => new Note { TimeSec = time, Type = NoteType.Tap, X = 0.5f, IsRival = false };

        private static Note RivalNoteAt(int tick, double time, bool gold = false) => new Note
        {
            Tick = tick,
            TimeSec = time,
            Type = NoteType.Tap,
            X = 0.25f,
            IsRival = true,
            IsGold = gold
        };

        [Test]
        public void Construct_IgnoresPlayerNotes()
        {
            var notes = new List<Note>
            {
                PlayerNoteAt(1.0),
                RivalNoteAt(0, 2.0),
                PlayerNoteAt(3.0)
            };

            var cpu = new CpuRival(notes, RivalConfig.Default);

            Assert.That(cpu.Count, Is.EqualTo(1));
        }

        [Test]
        public void Construct_OrdersByTime()
        {
            var notes = new List<Note>
            {
                RivalNoteAt(2, 3.0),
                RivalNoteAt(0, 1.0),
                RivalNoteAt(1, 2.0)
            };

            var cpu = new CpuRival(notes, RivalConfig.Default);
            var seen = new List<Note>();
            cpu.Process(10.0, (note, grade) => seen.Add(note));

            Assert.That(seen[0].Tick, Is.EqualTo(0));
            Assert.That(seen[1].Tick, Is.EqualTo(1));
            Assert.That(seen[2].Tick, Is.EqualTo(2));
        }

        [Test]
        public void Process_OnlyConsumesDueJudgments()
        {
            var notes = new List<Note>
            {
                RivalNoteAt(0, 1.0),
                RivalNoteAt(1, 3.0)
            };

            var cpu = new CpuRival(notes, RivalConfig.Default);
            var seen = new List<Note>();

            cpu.Process(1.5, (note, grade) => seen.Add(note));
            Assert.That(seen.Count, Is.EqualTo(1));
            Assert.That(seen[0].Tick, Is.EqualTo(0));

            cpu.Process(3.5, (note, grade) => seen.Add(note));
            Assert.That(seen.Count, Is.EqualTo(2));
            Assert.That(seen[1].Tick, Is.EqualTo(1));
        }

        [Test]
        public void Process_DoesNotReconsumeJudgments()
        {
            var notes = new List<Note> { RivalNoteAt(0, 1.0) };
            var cpu = new CpuRival(notes, RivalConfig.Default);

            var seen = new List<Note>();
            cpu.Process(2.0, (note, grade) => seen.Add(note));
            cpu.Process(2.0, (note, grade) => seen.Add(note));

            Assert.That(seen.Count, Is.EqualTo(1));
        }

        [Test]
        public void ComputeGrade_IsDeterministic()
        {
            var first = CpuRival.ComputeGrade(12345, 0.95);
            var second = CpuRival.ComputeGrade(12345, 0.95);
            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void ComputeGrade_HigherAccuracyProducesMoreJusts()
        {
            const int sampleSize = 1000;
            int justLow = 0;
            int justHigh = 0;

            for (int i = 0; i < sampleSize; i++)
            {
                if (CpuRival.ComputeGrade(i, 0.5) == Judgment.Just)
                    justLow++;
                if (CpuRival.ComputeGrade(i, 0.99) == Judgment.Just)
                    justHigh++;
            }

            Assert.That(justHigh, Is.GreaterThan(justLow));
        }

        [Test]
        public void ComputeGrade_NeverMisses()
        {
            for (int i = 0; i < 100; i++)
            {
                var grade = CpuRival.ComputeGrade(i, 0.0);
                Assert.That(grade, Is.Not.EqualTo(Judgment.Miss));
                Assert.That(grade == Judgment.Good || grade == Judgment.Great || grade == Judgment.Just, Is.True);
            }
        }

        [Test]
        public void ComputeGrade_InvalidAccuracyThrows()
        {
            Assert.That(() => CpuRival.ComputeGrade(0, -0.1), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => CpuRival.ComputeGrade(0, 1.1), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Process_GoldFlagIsPreserved()
        {
            var notes = new List<Note> { RivalNoteAt(0, 1.0, gold: true) };
            var cpu = new CpuRival(notes, RivalConfig.Default);

            Note judgedNote = null;
            cpu.Process(2.0, (note, grade) => judgedNote = note);

            Assert.That(judgedNote, Is.Not.Null);
            Assert.That(judgedNote.IsGold, Is.True);
        }

        [Test]
        public void SameChartProducesSameSchedule()
        {
            var notes = new List<Note>
            {
                RivalNoteAt(0, 1.0),
                RivalNoteAt(480, 2.0),
                RivalNoteAt(960, 3.0)
            };

            var firstRun = new List<(Note note, Judgment grade)>();
            var secondRun = new List<(Note note, Judgment grade)>();

            var cpu1 = new CpuRival(notes, RivalConfig.Default);
            cpu1.Process(10.0, (note, grade) => firstRun.Add((note, grade)));

            var cpu2 = new CpuRival(notes, RivalConfig.Default);
            cpu2.Process(10.0, (note, grade) => secondRun.Add((note, grade)));

            Assert.That(firstRun.Count, Is.EqualTo(secondRun.Count));
            for (int i = 0; i < firstRun.Count; i++)
            {
                Assert.That(firstRun[i].note.Tick, Is.EqualTo(secondRun[i].note.Tick));
                Assert.That(firstRun[i].grade, Is.EqualTo(secondRun[i].grade));
            }
        }
    }
}
