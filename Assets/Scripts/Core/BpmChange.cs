namespace ReflecBeatSimulator.Core
{
    /// <summary>A single BPM change at a given tick.</summary>
    public sealed class BpmChange
    {
        /// <summary>Tick at which the BPM takes effect.</summary>
        public int Tick { get; set; }

        /// <summary>New beats-per-minute value. Must be positive.</summary>
        public double Bpm { get; set; }
    }
}
