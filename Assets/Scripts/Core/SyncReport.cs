namespace ReflecBeatSimulator.Core
{
    /// <summary>
    /// Pure C# record of a single autoplay sync-check run. Used to verify that a
    /// chart can be judged within an acceptable timing window on the target device.
    /// </summary>
    public sealed class SyncReport
    {
        public double MaxTimingDeltaMs { get; private set; }
        public double MaxClockDriftMs { get; private set; }
        public int MissedNotes { get; private set; }
        public int JudgedNotes { get; private set; }

        public void RecordTimingDeltaMs(double deltaMs)
        {
            if (deltaMs > MaxTimingDeltaMs)
                MaxTimingDeltaMs = deltaMs;
        }

        public void RecordClockDriftMs(double driftMs)
        {
            if (driftMs > MaxClockDriftMs)
                MaxClockDriftMs = driftMs;
        }

        public void RecordMiss()
        {
            MissedNotes++;
        }

        public void RecordJudged()
        {
            JudgedNotes++;
        }

        /// <summary>
        /// Returns true if the run is within the target tolerances: no misses,
        /// timing delta under ~16 ms, and clock drift under ~16 ms.
        /// </summary>
        public bool IsWithinTarget()
        {
            return MissedNotes == 0 &&
                   MaxTimingDeltaMs < 16.0 &&
                   MaxClockDriftMs < 16.0;
        }
    }
}
