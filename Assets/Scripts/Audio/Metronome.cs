using System;
using UnityEngine;

namespace ReflecBeatSimulator.Audio
{
    /// <summary>
    /// Schedules metronome clicks on the DSP clock. Each click is queued slightly
    /// ahead of time so AudioSettings.dspTime jitter does not cause missed beats.
    /// </summary>
    public sealed class Metronome
    {
        private readonly AudioSource _source;
        private readonly AudioClip _tick;
        private readonly double _secondsPerBeat;
        private readonly double _startDspTime;
        private readonly int? _maxBeats;

        private double _nextBeatDsp;
        private int _beatCount;

        private const double Lookahead = 0.5;

        /// <summary>
        /// Creates a metronome.
        /// </summary>
        /// <param name="source">AudioSource used for scheduling clicks.</param>
        /// <param name="tick">Click clip.</param>
        /// <param name="bpm">Beats per minute.</param>
        /// <param name="startDspTime">DSP time of the first click.</param>
        /// <param name="maxBeats">Optional maximum number of clicks to schedule.</param>
        public Metronome(AudioSource source, AudioClip tick, double bpm, double startDspTime, int? maxBeats = null)
        {
            if (bpm <= 0.0)
                throw new ArgumentException("BPM must be positive", nameof(bpm));

            _source = source ?? throw new ArgumentNullException(nameof(source));
            _tick = tick ?? throw new ArgumentNullException(nameof(tick));
            _secondsPerBeat = 60.0 / bpm;
            _startDspTime = startDspTime;
            _maxBeats = maxBeats;
            _nextBeatDsp = startDspTime;
            _beatCount = 0;
        }

        /// <summary>Raised each time a beat click is scheduled.</summary>
        public event Action<int> BeatScheduled;

        /// <summary>
        /// Schedules upcoming clicks. Call from Update once per frame.
        /// </summary>
        public void Update()
        {
            if (_maxBeats.HasValue && _beatCount >= _maxBeats.Value)
                return;

            double scheduleLimit = AudioSettings.dspTime + Lookahead;
            while (_nextBeatDsp < scheduleLimit && (!_maxBeats.HasValue || _beatCount < _maxBeats.Value))
            {
                _source.clip = _tick;
                _source.PlayScheduled(_nextBeatDsp);
                _beatCount++;
                BeatScheduled?.Invoke(_beatCount);
                _nextBeatDsp += _secondsPerBeat;
            }
        }

        /// <summary>True if all scheduled beats have finished (only when maxBeats was set).</summary>
        public bool IsFinished => _maxBeats.HasValue && _beatCount >= _maxBeats.Value;
    }
}
