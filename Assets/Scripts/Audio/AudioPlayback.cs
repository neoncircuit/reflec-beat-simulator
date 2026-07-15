using UnityEngine;

namespace ReflecBeatSimulator.Audio
{
    /// <summary>
    /// Small helper for scheduling audio playback on the DSP clock.
    /// </summary>
    public static class AudioPlayback
    {
        /// <summary>
        /// Schedules <paramref name="clip"/> on <paramref name="source"/> to start
        /// after <paramref name="delaySec"/> seconds and returns the scheduled DSP time.
        /// </summary>
        public static double Schedule(AudioSource source, AudioClip clip, double delaySec = 0.5)
        {
            double startDsp = AudioSettings.dspTime + delaySec;
            source.clip = clip;
            source.PlayScheduled(startDsp);
            return startDsp;
        }

        /// <summary>
        /// Schedules <paramref name="clip"/> on <paramref name="source"/> to start
        /// after <paramref name="delaySec"/> seconds, beginning playback at
        /// <paramref name="startOffsetSec"/> seconds into the clip.
        /// </summary>
        public static double Schedule(AudioSource source, AudioClip clip, double delaySec, double startOffsetSec)
        {
            double startDsp = AudioSettings.dspTime + delaySec;
            source.clip = clip;
            source.time = (float)startOffsetSec;
            source.PlayScheduled(startDsp);
            return startDsp;
        }
    }
}
