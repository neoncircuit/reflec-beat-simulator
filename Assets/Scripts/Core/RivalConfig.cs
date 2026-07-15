using System;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Optional chart-level configuration for the CPU rival duel layer.</summary>
    public sealed class RivalConfig
    {
        public const double DefaultAccuracy = 0.95;
        public const int DefaultRivalHp = 100;
        public const int DefaultPlayerHp = 100;

        /// <summary>Likelihood the CPU will earn a JUST judgment on a rival note.</summary>
        public double Accuracy { get; set; } = DefaultAccuracy;

        /// <summary>Starting HP for the CPU rival.</summary>
        public int RivalHp { get; set; } = DefaultRivalHp;

        /// <summary>Starting HP for the player.</summary>
        public int PlayerHp { get; set; } = DefaultPlayerHp;

        public static RivalConfig Default => new RivalConfig();

        /// <summary>Validates the config and throws if values are out of range.</summary>
        public void Validate()
        {
            if (Accuracy < 0.0 || Accuracy > 1.0)
                throw new RbChartException("rival.accuracy must be between 0.0 and 1.0.");

            if (RivalHp <= 0)
                throw new RbChartException("rival.rivalHp must be positive.");

            if (PlayerHp <= 0)
                throw new RbChartException("rival.playerHp must be positive.");
        }

        /// <summary>True if the config differs from the default values.</summary>
        public bool IsDefault()
        {
            return Math.Abs(Accuracy - DefaultAccuracy) < double.Epsilon
                && RivalHp == DefaultRivalHp
                && PlayerHp == DefaultPlayerHp;
        }
    }
}
