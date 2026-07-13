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

        /// <summary>
        /// Half-width of a tap note's judgment region on the normalized judgment line (0.0–1.0).
        /// A touch within this distance of a note's x can judge it.
        /// </summary>
        public const float TapHitHalfWidth = 0.15f;

        /// <summary>Number of taps collected during calibration.</summary>
        public const int CalibrationTapCount = 16;
    }
}
