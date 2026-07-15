using ReflecBeatSimulator.Core;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Static container for the chart selected on the chart-select screen.</summary>
    public static class ChartSession
    {
        public static Chart Chart { get; set; }
        public static AudioClip AudioClip { get; set; }
        public static string ChartPath { get; set; }
        public static bool Autoplay { get; set; }
        public static bool SyncCheck { get; set; }
        public static double ApproachTime { get; set; } = Constants.DefaultApproachTime;
        public static bool HideRivalVisuals { get; set; }

        public static bool IsPreview { get; set; }
        public static double PreviewStartSec { get; set; }
        public static double PreviewEndSec { get; set; }

        public static bool HasChart => Chart != null;

        public static void SetPreview(Chart chart, AudioClip clip, double startSec, double endSec)
        {
            Chart = chart;
            AudioClip = clip;
            Autoplay = true;
            IsPreview = true;
            PreviewStartSec = startSec;
            PreviewEndSec = endSec;
        }

        public static void Clear()
        {
            Chart = null;
            AudioClip = null;
            ChartPath = null;
            Autoplay = false;
            SyncCheck = false;
            ApproachTime = Constants.DefaultApproachTime;
            HideRivalVisuals = false;
            IsPreview = false;
            PreviewStartSec = 0.0;
            PreviewEndSec = 0.0;
        }
    }
}
