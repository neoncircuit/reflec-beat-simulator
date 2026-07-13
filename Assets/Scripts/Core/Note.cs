using System;

namespace ReflecBeatSimulator.Core
{
    /// <summary>A single note event in musical time. Seconds are derived at load, never stored.</summary>
    public sealed class Note
    {
        /// <summary>Musical time in ticks (PPQ-relative).</summary>
        public int Tick { get; set; }

        /// <summary>One of the <see cref="NoteType"/> constants.</summary>
        public string Type { get; set; }

        /// <summary>Target position on the judgment line, 0.0 (left) to 1.0 (right). Unused for top/bigbang.</summary>
        public float X { get; set; }

        /// <summary>Hold length for long notes, in ticks.</summary>
        public int DurationTicks { get; set; }

        /// <summary>3TOP zone index (0–2) for top notes.</summary>
        public int Zone { get; set; }

        /// <summary>Absolute hit time in seconds, computed at load from the BPM map.</summary>
        public double TimeSec { get; set; }
    }
}
