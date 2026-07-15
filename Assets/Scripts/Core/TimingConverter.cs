using System;
using System.Collections.Generic;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Converts musical ticks to absolute seconds using the chart's BPM map.</summary>
    public sealed class TimingConverter
    {
        private readonly int _ppq;
        private readonly List<BpmChange> _bpmChanges;

        public TimingConverter(ChartTiming timing)
        {
            if (timing == null)
                throw new ArgumentNullException(nameof(timing));

            _ppq = timing.Ppq > 0 ? timing.Ppq : Constants.Ppq;
            _bpmChanges = timing.BpmChanges != null
                ? new List<BpmChange>(timing.BpmChanges)
                : new List<BpmChange>();

            Validate();
        }

        public TimingConverter(IList<BpmChange> bpmChanges, int ppq = Constants.Ppq)
        {
            _ppq = ppq > 0 ? ppq : Constants.Ppq;
            _bpmChanges = bpmChanges != null
                ? new List<BpmChange>(bpmChanges)
                : new List<BpmChange>();

            Validate();
        }

        private void Validate()
        {
            _bpmChanges.Sort((a, b) => a.Tick.CompareTo(b.Tick));

            if (_bpmChanges.Count == 0)
                throw new ArgumentException("At least one BPM change is required.", nameof(_bpmChanges));

            foreach (var change in _bpmChanges)
            {
                if (change.Bpm <= 0.0)
                    throw new ArgumentException("BPM values must be positive.", nameof(_bpmChanges));
            }
        }

        /// <summary>Returns the absolute time in seconds for the given tick.</summary>
        public double TickToSeconds(int tick)
        {
            double time = 0.0;
            int currentTick = 0;
            double currentBpm = _bpmChanges[0].Bpm;

            for (int i = 1; i < _bpmChanges.Count; i++)
            {
                int nextTick = _bpmChanges[i].Tick;
                if (tick <= nextTick)
                {
                    time += SegmentSeconds(currentTick, tick, currentBpm);
                    return time;
                }

                time += SegmentSeconds(currentTick, nextTick, currentBpm);
                currentTick = nextTick;
                currentBpm = _bpmChanges[i].Bpm;
            }

            time += SegmentSeconds(currentTick, tick, currentBpm);
            return time;
        }

        /// <summary>Computes and assigns absolute seconds for every note, including long-note tails.</summary>
        public void ApplyTimes(IList<Note> notes)
        {
            if (notes == null)
                return;

            foreach (var note in notes)
            {
                note.TimeSec = TickToSeconds(note.Tick);

                if (note.Type == NoteType.Long)
                {
                    note.EndTick = note.Tick + note.DurationTicks;
                    note.EndTimeSec = TickToSeconds(note.EndTick);
                }
            }
        }

        private double SegmentSeconds(int fromTick, int toTick, double bpm)
        {
            if (toTick < fromTick)
                return 0.0;

            return (toTick - fromTick) / (double)_ppq * (60.0 / bpm);
        }
    }
}
