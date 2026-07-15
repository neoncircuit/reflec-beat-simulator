using System.Collections.Generic;

namespace ReflecBeatSimulator.Core
{
    /// <summary>A complete chart: metadata, timing, and note events.</summary>
    public sealed class Chart
    {
        public ChartMeta Meta { get; set; }
        public ChartTiming Timing { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();
        public RivalConfig Rival { get; set; }
    }
}
