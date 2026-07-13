using System.Collections.Generic;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class TimingConverterTests
    {
        [Test]
        public void SingleBpm_ConvertsTicksToSeconds()
        {
            var changes = new List<BpmChange>
            {
                new BpmChange { Tick = 0, Bpm = 120 }
            };
            var tc = new TimingConverter(changes);

            Assert.That(tc.TickToSeconds(0), Is.EqualTo(0.0).Within(1e-9));
            Assert.That(tc.TickToSeconds(480), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tc.TickToSeconds(960), Is.EqualTo(1.0).Within(1e-9));
        }

        [Test]
        public void BpmChange_AppliesNewTempo()
        {
            var changes = new List<BpmChange>
            {
                new BpmChange { Tick = 0, Bpm = 120 },
                new BpmChange { Tick = 480, Bpm = 240 }
            };
            var tc = new TimingConverter(changes);

            Assert.That(tc.TickToSeconds(480), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(tc.TickToSeconds(960), Is.EqualTo(0.75).Within(1e-9));
        }

        [Test]
        public void ApplyTimes_SetsNoteTimeSec()
        {
            var timing = new ChartTiming
            {
                Ppq = 480,
                BpmChanges = new List<BpmChange>
                {
                    new BpmChange { Tick = 0, Bpm = 120 },
                    new BpmChange { Tick = 960, Bpm = 240 }
                }
            };
            var notes = new List<Note>
            {
                new Note { Tick = 480 },
                new Note { Tick = 1440 }
            };

            new TimingConverter(timing).ApplyTimes(notes);

            Assert.That(notes[0].TimeSec, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(notes[1].TimeSec, Is.EqualTo(1.25).Within(1e-9));
        }

        [Test]
        public void UnsortedBpmChanges_GetsSorted()
        {
            var changes = new List<BpmChange>
            {
                new BpmChange { Tick = 480, Bpm = 240 },
                new BpmChange { Tick = 0, Bpm = 120 }
            };
            var tc = new TimingConverter(changes);

            Assert.That(tc.TickToSeconds(960), Is.EqualTo(0.75).Within(1e-9));
        }
    }
}
