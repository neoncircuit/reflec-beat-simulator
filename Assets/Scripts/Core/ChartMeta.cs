namespace ReflecBeatSimulator.Core
{
    /// <summary>Chart metadata: the part humans read before playing.</summary>
    public sealed class ChartMeta
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Charter { get; set; }
        public string AudioFile { get; set; }
        public string Difficulty { get; set; }
        public int Level { get; set; }
    }
}
