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

        // Duel / CPU rival constants (M4)
        /// <summary>Max Just Reflect gauge for the player.</summary>
        public const int MaxGauge = 100;

        /// <summary>Gauge gained on a JUST judgment.</summary>
        public const int JustGaugeGain = 15;

        /// <summary>Gauge gained on a GREAT judgment.</summary>
        public const int GreatGaugeGain = 10;

        /// <summary>Gauge gained on a GOOD judgment.</summary>
        public const int GoodGaugeGain = 5;

        /// <summary>Extra gauge gained on a gold note (added to the grade gain).</summary>
        public const int GoldGaugeBonus = 10;

        /// <summary>Damage dealt to the player when the CPU hits a rival note.</summary>
        public const int CpuHitDamage = 8;

        /// <summary>Extra damage dealt to the player when the CPU hits a gold rival note.</summary>
        public const int CpuGoldDamageBonus = 4;

        /// <summary>Damage dealt to the player when the player misses a note.</summary>
        public const int PlayerMissDamage = 5;

        /// <summary>Damage dealt to the rival when the player activates Just Reflect.</summary>
        public const int ReflectDamage = 30;
    }
}
