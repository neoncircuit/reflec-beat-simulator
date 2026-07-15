using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class ChartWriterTests
    {
        private string _tempFile;

        [SetUp]
        public void SetUp()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), $"rbchart-writer-test-{Guid.NewGuid()}.rbchart.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [Test]
        public void Write_RoundTripsMinimalChart()
        {
            Chart original = CreateMinimalChart();
            ChartWriter.Write(original, _tempFile);

            string json = File.ReadAllText(_tempFile);
            Chart roundTripped = ChartParser.Parse(json);

            Assert.That(roundTripped.Meta.Title, Is.EqualTo(original.Meta.Title));
            Assert.That(roundTripped.Meta.Artist, Is.EqualTo(original.Meta.Artist));
            Assert.That(roundTripped.Meta.Charter, Is.EqualTo(original.Meta.Charter));
            Assert.That(roundTripped.Meta.AudioFile, Is.EqualTo(original.Meta.AudioFile));
            Assert.That(roundTripped.Meta.Difficulty, Is.EqualTo(original.Meta.Difficulty));
            Assert.That(roundTripped.Meta.Level, Is.EqualTo(original.Meta.Level));

            Assert.That(roundTripped.Timing.Ppq, Is.EqualTo(original.Timing.Ppq));
            Assert.That(roundTripped.Timing.AudioOffsetMs, Is.EqualTo(original.Timing.AudioOffsetMs));
            Assert.That(roundTripped.Timing.BpmChanges.Count, Is.EqualTo(1));
            Assert.That(roundTripped.Timing.BpmChanges[0].Tick, Is.EqualTo(0));
            Assert.That(roundTripped.Timing.BpmChanges[0].Bpm, Is.EqualTo(120.0));

            Assert.That(roundTripped.Notes.Count, Is.EqualTo(1));
            Assert.That(roundTripped.Notes[0].Tick, Is.EqualTo(480));
            Assert.That(roundTripped.Notes[0].Type, Is.EqualTo(NoteType.Tap));
            Assert.That(roundTripped.Notes[0].X, Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(roundTripped.Notes[0].TimeSec, Is.EqualTo(0.5).Within(1e-9));
        }

        [Test]
        public void Write_RoundTripsAllNoteTypes()
        {
            Chart original = CreateChartWithAllNoteTypes();
            ChartWriter.Write(original, _tempFile);

            string json = File.ReadAllText(_tempFile);
            Chart roundTripped = ChartParser.Parse(json);

            Assert.That(roundTripped.Notes.Count, Is.EqualTo(5));

            Note tap = FindNote(roundTripped, 480);
            Assert.That(tap.Type, Is.EqualTo(NoteType.Tap));
            Assert.That(tap.X, Is.EqualTo(0.25f).Within(1e-6f));

            Note vertical = FindNote(roundTripped, 960);
            Assert.That(vertical.Type, Is.EqualTo(NoteType.Vertical));
            Assert.That(vertical.X, Is.EqualTo(0.75f).Within(1e-6f));

            Note top = FindNote(roundTripped, 1440);
            Assert.That(top.Type, Is.EqualTo(NoteType.Top));
            Assert.That(top.Zone, Is.EqualTo(2));

            Note bigBang = FindNote(roundTripped, 1920);
            Assert.That(bigBang.Type, Is.EqualTo(NoteType.BigBang));

            Note longNote = FindNote(roundTripped, 2400);
            Assert.That(longNote.Type, Is.EqualTo(NoteType.Long));
            Assert.That(longNote.X, Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(longNote.DurationTicks, Is.EqualTo(480));
            Assert.That(longNote.EndTimeSec, Is.EqualTo(3.0).Within(1e-9));
        }

        [Test]
        public void Write_DoesNotEmitDerivedTimes()
        {
            Chart chart = CreateChartWithAllNoteTypes();
            ChartWriter.Write(chart, _tempFile);

            string json = File.ReadAllText(_tempFile);

            Assert.That(json, Does.Not.Contain("TimeSec"));
            Assert.That(json, Does.Not.Contain("EndTimeSec"));
            Assert.That(json, Does.Not.Contain("EndTick"));
        }

        [Test]
        public void Write_OrdersNotesByTick()
        {
            Chart chart = CreateMinimalChart();
            chart.Notes = new List<Note>
            {
                new Note { Tick = 960, Type = NoteType.Tap, X = 0.2f },
                new Note { Tick = 240, Type = NoteType.Tap, X = 0.1f },
                new Note { Tick = 480, Type = NoteType.Tap, X = 0.3f }
            };

            ChartWriter.Write(chart, _tempFile);
            string json = File.ReadAllText(_tempFile);
            Chart parsed = ChartParser.Parse(json);

            List<int> ticks = parsed.Notes.Select(n => n.Tick).ToList();
            Assert.That(ticks, Is.EqualTo(new[] { 240, 480, 960 }));
        }

        [Test]
        public void Write_SortsBpmChangesByTick()
        {
            Chart chart = CreateMinimalChart();
            chart.Timing.BpmChanges = new List<BpmChange>
            {
                new BpmChange { Tick = 1920, Bpm = 180.0 },
                new BpmChange { Tick = 0, Bpm = 120.0 },
                new BpmChange { Tick = 960, Bpm = 150.0 }
            };

            ChartWriter.Write(chart, _tempFile);
            string json = File.ReadAllText(_tempFile);
            Chart parsed = ChartParser.Parse(json);

            List<int> ticks = parsed.Timing.BpmChanges.Select(b => b.Tick).ToList();
            Assert.That(ticks, Is.EqualTo(new[] { 0, 960, 1920 }));
        }

        [Test]
        public void Write_MissingRequiredMetaField_Throws()
        {
            Chart chart = CreateMinimalChart();
            chart.Meta.Title = null;

            Assert.That(() => ChartWriter.Write(chart, _tempFile), Throws.Exception);
        }

        [Test]
        public void Write_NullChart_Throws()
        {
            Assert.That(() => ChartWriter.Write(null, _tempFile), Throws.ArgumentNullException);
        }

        private static Chart CreateMinimalChart()
        {
            return new Chart
            {
                Meta = new ChartMeta
                {
                    Title = "Test Chart",
                    Artist = "Test Artist",
                    Charter = "Tester",
                    AudioFile = "beat.wav",
                    Difficulty = "EASY",
                    Level = 1
                },
                Timing = new ChartTiming
                {
                    Ppq = 480,
                    AudioOffsetMs = 50.0,
                    BpmChanges = new List<BpmChange>
                    {
                        new BpmChange { Tick = 0, Bpm = 120.0 }
                    }
                },
                Notes = new List<Note>
                {
                    new Note { Tick = 480, Type = NoteType.Tap, X = 0.5f }
                }
            };
        }

        private static Chart CreateChartWithAllNoteTypes()
        {
            Chart chart = CreateMinimalChart();
            chart.Notes = new List<Note>
            {
                new Note { Tick = 480, Type = NoteType.Tap, X = 0.25f },
                new Note { Tick = 960, Type = NoteType.Vertical, X = 0.75f },
                new Note { Tick = 1440, Type = NoteType.Top, Zone = 2 },
                new Note { Tick = 1920, Type = NoteType.BigBang },
                new Note { Tick = 2400, Type = NoteType.Long, X = 0.5f, DurationTicks = 480 }
            };
            return chart;
        }

        private static Note FindNote(Chart chart, int tick)
        {
            return chart.Notes.First(n => n.Tick == tick);
        }
    }
}
