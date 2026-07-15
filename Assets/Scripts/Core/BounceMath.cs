namespace ReflecBeatSimulator.Core
{
    /// <summary>Pure C# helpers for visual wall-bounce motion.</summary>
    public static class BounceMath
    {
        /// <summary>
        /// Reflects a normalized X coordinate off the left (0) and right (1) walls
        /// like an air-hockey puck. The bounce is purely visual: vertical travel
        /// and judgment timing are unchanged.
        /// </summary>
        /// <param name="startX">Starting normalized X in [0, 1].</param>
        /// <param name="elapsed">Elapsed time since spawn, in seconds.</param>
        /// <param name="duration">Total travel duration, in seconds.</param>
        /// <param name="wallHits">Number of wall collisions during the travel.</param>
        /// <param name="direction">Direction multiplier: 1f moves right first, -1f moves left first.</param>
        /// <returns>Bounced normalized X in [0, 1].</returns>
        public static float BounceX(float startX, float elapsed, float duration, int wallHits, float direction = 1f)
        {
            if (duration <= 0f || wallHits <= 0)
                return startX;

            float t = Clamp01(elapsed / duration);
            float position = startX + direction * wallHits * t;
            return PingPong(position, 1f);
        }

        /// <summary>
        /// Progress-based variant of <see cref="BounceX"/>. The bounce curve is
        /// expressed as a normalized fraction of the travel, so wall hits land at
        /// predictable positions regardless of approach time.
        /// </summary>
        /// <param name="startX">Starting normalized X in [0, 1].</param>
        /// <param name="progress">Normalized travel progress from spawn (0) to judgment line (1).</param>
        /// <param name="wallHits">Number of wall collisions during the travel.</param>
        /// <param name="direction">Direction multiplier: 1f moves right first, -1f moves left first.</param>
        /// <returns>Bounced normalized X in [0, 1].</returns>
        public static float BounceX(float startX, float progress, int wallHits, float direction = 1f)
        {
            if (wallHits <= 0)
                return startX;

            float position = startX + direction * wallHits * Clamp01(progress);
            return PingPong(position, 1f);
        }

        /// <summary>Ping-pongs a value between 0 and length, reflecting at each boundary.</summary>
        public static float PingPong(float value, float length)
        {
            if (length <= 0f)
                return value;

            float mod = value % (2f * length);
            if (mod < 0f)
                mod += 2f * length;

            return mod <= length ? mod : 2f * length - mod;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;
            if (value > 1f)
                return 1f;
            return value;
        }
    }
}
