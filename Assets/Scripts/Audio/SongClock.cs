using System;
using UnityEngine;

namespace ReflecBeatSimulator.Audio
{
    /// <summary>
    /// Smoothed song clock: interpolates with the game clock between audio buffer
    /// updates, while correcting drift toward the DSP (audio hardware) clock.
    /// </summary>
    public sealed class SongClock
    {
        private readonly double _startDspTime;
        private readonly double _offsetSec;
        private readonly Func<double> _getDspTime;
        private double _smoothedTime;
        private double _lastUnscaledTime;
        private bool _initialized;

        /// <summary>
        /// Creates a clock tied to the real <see cref="AudioSettings.dspTime"/>.
        /// </summary>
        /// <param name="startDspTime">The dspTime passed to AudioSource.PlayScheduled.</param>
        /// <param name="offsetSec">Chart audioOffsetMs converted to seconds.</param>
        public SongClock(double startDspTime, double offsetSec)
            : this(startDspTime, offsetSec, () => AudioSettings.dspTime)
        {
        }

        /// <summary>
        /// Creates a clock with a custom DSP time provider, used by EditMode tests.
        /// </summary>
        public SongClock(double startDspTime, double offsetSec, Func<double> getDspTime)
        {
            _startDspTime = startDspTime;
            _offsetSec = offsetSec;
            _getDspTime = getDspTime ?? throw new ArgumentNullException(nameof(getDspTime));
            _smoothedTime = _getDspTime() - _startDspTime - _offsetSec;
        }

        /// <summary>Call once per frame. Returns current song position in seconds.</summary>
        /// <param name="unscaledTime">A frame-stable unscaled time such as Time.unscaledTimeAsDouble.</param>
        public double Tick(double unscaledTime)
        {
            double dspSongTime = _getDspTime() - _startDspTime - _offsetSec;

            if (!_initialized)
            {
                _initialized = true;
                _lastUnscaledTime = unscaledTime;
                _smoothedTime = dspSongTime;
                return _smoothedTime;
            }

            double delta = unscaledTime - _lastUnscaledTime;
            _lastUnscaledTime = unscaledTime;

            _smoothedTime += delta;

            // Correct 10% of the gap per frame; snap if badly diverged (e.g. app resume).
            double gap = dspSongTime - _smoothedTime;
            _smoothedTime += Math.Abs(gap) > 0.05 ? gap : gap * 0.1;
            return _smoothedTime;
        }
    }
}
