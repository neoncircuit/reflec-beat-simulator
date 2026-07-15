using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests.EditMode
{
    public class SyncReportTests
    {
        [Test]
        public void TracksMaxTimingDelta()
        {
            var report = new SyncReport();
            report.RecordTimingDeltaMs(2.0);
            report.RecordTimingDeltaMs(8.0);
            report.RecordTimingDeltaMs(5.0);

            Assert.That(report.MaxTimingDeltaMs, Is.EqualTo(8.0).Within(1e-9));
        }

        [Test]
        public void TracksMaxClockDrift()
        {
            var report = new SyncReport();
            report.RecordClockDriftMs(1.0);
            report.RecordClockDriftMs(4.0);

            Assert.That(report.MaxClockDriftMs, Is.EqualTo(4.0).Within(1e-9));
        }

        [Test]
        public void TracksMisses()
        {
            var report = new SyncReport();
            report.RecordMiss();
            report.RecordMiss();

            Assert.That(report.MissedNotes, Is.EqualTo(2));
        }

        [Test]
        public void WithinTarget_WhenAllTolerancesMet()
        {
            var report = new SyncReport();
            report.RecordTimingDeltaMs(10.0);
            report.RecordClockDriftMs(10.0);

            Assert.That(report.IsWithinTarget(), Is.True);
        }

        [Test]
        public void OutsideTarget_WhenTimingDeltaTooHigh()
        {
            var report = new SyncReport();
            report.RecordTimingDeltaMs(20.0);
            report.RecordClockDriftMs(5.0);

            Assert.That(report.IsWithinTarget(), Is.False);
        }

        [Test]
        public void OutsideTarget_WhenAnyMissOccurs()
        {
            var report = new SyncReport();
            report.RecordTimingDeltaMs(5.0);
            report.RecordClockDriftMs(5.0);
            report.RecordMiss();

            Assert.That(report.IsWithinTarget(), Is.False);
        }
    }
}
