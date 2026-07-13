using System.Collections.Generic;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Timing metadata that binds a chart to its audio.</summary>
    public sealed class ChartTiming
    {
        /// <summary>Ticks per quarter note. Defaults to <see cref="Constants.Ppq"/>.</summary>
        public int Ppq { get; set; } = Constants.Ppq;

        /// <summary>Where tick 0 sits inside the audio file, in milliseconds.</summary>
        public double AudioOffsetMs { get; set; }

        /// <summary>Ordered list of BPM changes. Must contain at least one entry.</summary>
        public List<BpmChange> BpmChanges { get; set; } = new List<BpmChange>();
    }
}
