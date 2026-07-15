using NUnit.Framework;
using ReflecBeatSimulator.Audio;

namespace ReflecBeatSimulator.Tests.EditMode
{
    public class SongClockTests
    {
        [Test]
        public void Tick_Initial_ReturnsDspSongTime()
        {
            double dsp = 100.0;
            var clock = new SongClock(100.0, 0.0, () => dsp);

            double t = clock.Tick(0.0);

            Assert.That(t, Is.EqualTo(0.0).Within(1e-9));
        }

        [Test]
        public void Tick_AppliesOffset()
        {
            double dsp = 50.0;
            var clock = new SongClock(40.0, 0.1, () => dsp);

            double t = clock.Tick(0.0);

            Assert.That(t, Is.EqualTo(50.0 - 40.0 - 0.1).Within(1e-9));
        }

        [Test]
        public void Tick_SmoothsSmallGap()
        {
            double dsp = 100.0;
            var clock = new SongClock(100.0, 0.0, () => dsp);

            clock.Tick(0.0);

            // Simulate one frame where unscaled time advances 16 ms but dsp only 15 ms.
            dsp += 0.015;
            double t = clock.Tick(0.016);

            // Smoothed time should be between the pure extrapolation (0.016)
            // and the dsp song time (0.015), closer to dsp because of the 10% blend.
            Assert.That(t, Is.LessThan(0.016));
            Assert.That(t, Is.GreaterThan(0.015));
        }

        [Test]
        public void Tick_SnapsLargeGap()
        {
            double dsp = 100.0;
            var clock = new SongClock(100.0, 0.0, () => dsp);

            clock.Tick(0.0);

            // Simulate a big discontinuity, e.g. app resume adding 200 ms.
            dsp += 0.2;
            double t = clock.Tick(0.2);

            Assert.That(t, Is.EqualTo(0.2).Within(0.001));
        }

        [Test]
        public void Tick_ConvergesToDspTime()
        {
            double dsp = 0.0;
            var clock = new SongClock(0.0, 0.0, () => dsp);

            clock.Tick(0.0);

            // Run many frames where unscaled time and dsp drift apart slowly.
            double unscaled = 0.0;
            for (int i = 0; i < 60; i++)
            {
                unscaled += 0.016;
                dsp += 0.01601; // dsp runs 10 us per frame faster
                clock.Tick(unscaled);
            }

            dsp += 0.01601;
            double final = clock.Tick(unscaled + 0.016);

            Assert.That(final, Is.EqualTo(dsp).Within(0.001));
        }
    }
}
