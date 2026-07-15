using System;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class ChartParserTests
    {
        private const string MinimalMeta = @"
    ""title"": ""Demo"",
    ""artist"": ""Synthetic"",
    ""charter"": ""Test"",
    ""audioFile"": ""beat.wav"",
    ""difficulty"": ""EASY"",
    ""level"": 1";

        private const string MinimalTiming = @"
    ""ppq"": 480,
    ""audioOffsetMs"": 0,
    ""bpmChanges"": [{""tick"": 0, ""bpm"": 120}]";

        private static string Wrap(string meta, string timing, string notes) => $@"{{
  ""version"": 1,
  ""meta"": {{{meta}}},
  ""timing"": {{{timing}}},
  ""notes"": [{notes}]
}}";

        private static string MinimalChart(string notes) => Wrap(MinimalMeta, MinimalTiming, notes);

        [Test]
        public void Parse_ValidChart_LoadsMeta()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart, Is.Not.Null);
            Assert.That(chart.Meta.Title, Is.EqualTo("Demo"));
            Assert.That(chart.Meta.Artist, Is.EqualTo("Synthetic"));
            Assert.That(chart.Meta.Charter, Is.EqualTo("Test"));
            Assert.That(chart.Meta.AudioFile, Is.EqualTo("beat.wav"));
            Assert.That(chart.Meta.Difficulty, Is.EqualTo("EASY"));
            Assert.That(chart.Meta.Level, Is.EqualTo(1));
        }

        [Test]
        public void Parse_ValidChart_LoadsTiming()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Timing.Ppq, Is.EqualTo(480));
            Assert.That(chart.Timing.AudioOffsetMs, Is.EqualTo(0.0));
            Assert.That(chart.Timing.BpmChanges.Count, Is.EqualTo(1));
            Assert.That(chart.Timing.BpmChanges[0].Tick, Is.EqualTo(0));
            Assert.That(chart.Timing.BpmChanges[0].Bpm, Is.EqualTo(120.0));
        }

        [Test]
        public void Parse_TapNote_ComputesTime()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Notes.Count, Is.EqualTo(1));
            Note note = chart.Notes[0];
            Assert.That(note.Type, Is.EqualTo(NoteType.Tap));
            Assert.That(note.Tick, Is.EqualTo(960));
            Assert.That(note.X, Is.EqualTo(0.5f).Within(1e-6f));
            // 960 ticks at 480 PPQ = 2 beats = 1 second at 120 BPM
            Assert.That(note.TimeSec, Is.EqualTo(1.0).Within(1e-9));
        }

        [Test]
        public void Parse_LongNote_ComputesEndTime()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""long"", ""x"": 0.25, ""durationTicks"": 480 }");
            Chart chart = ChartParser.Parse(json);

            Note note = chart.Notes[0];
            Assert.That(note.Type, Is.EqualTo(NoteType.Long));
            Assert.That(note.DurationTicks, Is.EqualTo(480));
            Assert.That(note.EndTick, Is.EqualTo(1440));
            Assert.That(note.TimeSec, Is.EqualTo(1.0).Within(1e-9));
            Assert.That(note.EndTimeSec, Is.EqualTo(1.5).Within(1e-9));
        }

        [Test]
        public void Parse_VerticalNote_LoadsX()
        {
            string json = MinimalChart(@"{ ""tick"": 480, ""type"": ""vertical"", ""x"": 0.75 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Notes[0].Type, Is.EqualTo(NoteType.Vertical));
            Assert.That(chart.Notes[0].X, Is.EqualTo(0.75f).Within(1e-6f));
        }

        [Test]
        public void Parse_TopNote_LoadsZone()
        {
            string json = MinimalChart(@"{ ""tick"": 480, ""type"": ""top"", ""zone"": 2 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Notes[0].Type, Is.EqualTo(NoteType.Top));
            Assert.That(chart.Notes[0].Zone, Is.EqualTo(2));
        }

        [Test]
        public void Parse_BigBangNote_RequiresNoPosition()
        {
            string json = MinimalChart(@"{ ""tick"": 480, ""type"": ""bigbang"" }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Notes[0].Type, Is.EqualTo(NoteType.BigBang));
        }

        [Test]
        public void Parse_BpmChanges_ComputesSecondsAcrossChanges()
        {
            string timing = @"
    ""ppq"": 480,
    ""audioOffsetMs"": 0,
    ""bpmChanges"": [
      {""tick"": 0, ""bpm"": 120},
      {""tick"": 960, ""bpm"": 60}
    ]";
            string json = Wrap(MinimalMeta, timing, @"{ ""tick"": 1920, ""type"": ""tap"", ""x"": 0.5 }");
            Chart chart = ChartParser.Parse(json);

            // 0-960 ticks = 1s at 120 BPM; 960-1920 ticks = 2s at 60 BPM
            Assert.That(chart.Notes[0].TimeSec, Is.EqualTo(3.0).Within(1e-9));
        }

        [Test]
        public void Parse_DefaultPpq_WhenOmitted()
        {
            string timing = @"
    ""audioOffsetMs"": 0,
    ""bpmChanges"": [{""tick"": 0, ""bpm"": 120}]";
            string json = Wrap(MinimalMeta, timing, @"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }");
            Chart chart = ChartParser.Parse(json);

            Assert.That(chart.Timing.Ppq, Is.EqualTo(Constants.Ppq));
        }

        [Test]
        public void Parse_MissingVersion_Throws()
        {
            string json = Wrap(MinimalMeta, MinimalTiming, @"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }").Replace("\"version\": 1,", "");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("version"));
        }

        [Test]
        public void Parse_UnsupportedVersion_Throws()
        {
            string json = Wrap(MinimalMeta, MinimalTiming, @"{ ""tick"": 960, ""type"": ""tap"", ""x"": 0.5 }").Replace("\"version\": 1", "\"version\": 2");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("version"));
        }

        [Test]
        public void Parse_InvalidJson_Throws()
        {
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse("{ not json"));
            Assert.That(ex.Message, Does.Contain("Invalid JSON"));
        }

        [Test]
        public void Parse_MissingMeta_Throws()
        {
            string json = "{\"version\":1,\"timing\":{" + MinimalTiming + "},\"notes\":[]}";
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("meta"));
        }

        [Test]
        public void Parse_MissingTiming_Throws()
        {
            string json = "{\"version\":1,\"meta\":{" + MinimalMeta + "},\"notes\":[]}";
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("timing"));
        }

        [Test]
        public void Parse_EmptyBpmChanges_Throws()
        {
            string timing = @"
    ""ppq"": 480,
    ""audioOffsetMs"": 0,
    ""bpmChanges"": []";
            string json = Wrap(MinimalMeta, timing, "");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("bpmChanges"));
        }

        [Test]
        public void Parse_UnorderedBpmChanges_Throws()
        {
            string timing = @"
    ""ppq"": 480,
    ""audioOffsetMs"": 0,
    ""bpmChanges"": [
      {""tick"": 960, ""bpm"": 60},
      {""tick"": 0, ""bpm"": 120}
    ]";
            string json = Wrap(MinimalMeta, timing, "");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("ordered"));
        }

        [Test]
        public void Parse_NonPositiveBpm_Throws()
        {
            string timing = @"
    ""ppq"": 480,
    ""audioOffsetMs"": 0,
    ""bpmChanges"": [{""tick"": 0, ""bpm"": 0}]";
            string json = Wrap(MinimalMeta, timing, "");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("BPM"));
        }

        [Test]
        public void Parse_TapMissingX_Throws()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""tap"" }");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("x"));
        }

        [Test]
        public void Parse_TapXOutOfRange_Throws()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""tap"", ""x"": 1.5 }");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("0.0"));
        }

        [Test]
        public void Parse_TopInvalidZone_Throws()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""top"", ""zone"": 5 }");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("zone"));
        }

        [Test]
        public void Parse_UnknownNoteType_Throws()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""slide"" }");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("unknown type"));
        }

        [Test]
        public void Parse_LongMissingDuration_Throws()
        {
            string json = MinimalChart(@"{ ""tick"": 960, ""type"": ""long"", ""x"": 0.5 }");
            var ex = Assert.Throws<RbChartException>(() => ChartParser.Parse(json));
            Assert.That(ex.Message, Does.Contain("durationTicks"));
        }
    }
}
