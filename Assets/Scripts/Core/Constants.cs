namespace ReflecBeatSimulator.Core
{
    /// <summary>Central constants used by Core, Audio, Gameplay, and UI.</summary>
    public static class Constants
    {
        /// <summary>Ticks per quarter note (PPQ).</summary>
        public const int Ppq = 480;

        /// <summary>JUST judgment window in seconds.</summary>
        public const double JustWindow = 0.033;

        /// <summary>GREAT judgment window in seconds.</summary>
        public const double GreatWindow = 0.066;

        /// <summary>GOOD judgment window in seconds.</summary>
        public const double GoodWindow = 0.100;

        /// <summary>Default note approach time in seconds.</summary>
        public const double DefaultApproachTime = 1.0;

        /// <summary>Number of taps collected during calibration.</summary>
        public const int CalibrationTapCount = 16;
    }
}
