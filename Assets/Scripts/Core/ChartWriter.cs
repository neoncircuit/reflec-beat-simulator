using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Serializes a <see cref="Chart"/> to a valid <c>*.rbchart.json</c> file.</summary>
    public static class ChartWriter
    {
        /// <summary>Writes <paramref name="chart"/> to <paramref name="path"/> as JSON.</summary>
        public static void Write(Chart chart, string path)
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path must not be empty.", nameof(path));

            string json = ToJson(chart);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        /// <summary>Returns the JSON representation of <paramref name="chart"/>.</summary>
        public static string ToJson(Chart chart)
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            Validate(chart);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine(Indent(1) + "\"version\": 1,");

            AppendMeta(sb, chart.Meta);
            AppendTiming(sb, chart.Timing);
            AppendRival(sb, chart.Rival);
            AppendNotes(sb, chart.Notes);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void Validate(Chart chart)
        {
            if (chart.Meta == null)
                throw new RbChartException("Missing required section: meta.");

            RequireMetaField(chart.Meta.Title, "meta.title");
            RequireMetaField(chart.Meta.Artist, "meta.artist");
            RequireMetaField(chart.Meta.Charter, "meta.charter");
            RequireMetaField(chart.Meta.AudioFile, "meta.audioFile");
            RequireMetaField(chart.Meta.Difficulty, "meta.difficulty");

            if (chart.Timing == null)
                throw new RbChartException("Missing required section: timing.");

            if (chart.Timing.Ppq <= 0)
                throw new RbChartException("timing.ppq must be positive.");

            if (chart.Timing.BpmChanges == null || chart.Timing.BpmChanges.Count == 0)
                throw new RbChartException("timing.bpmChanges must contain at least one entry.");

            foreach (BpmChange change in chart.Timing.BpmChanges)
            {
                if (change.Bpm <= 0.0)
                    throw new RbChartException("BPM values must be positive.");
            }

            if (chart.Notes == null)
                throw new RbChartException("Missing required section: notes.");

            foreach (Note note in chart.Notes)
            {
                if (note == null)
                    throw new RbChartException("Notes array must not contain null entries.");

                ValidateNote(note);
            }
        }

        private static void RequireMetaField(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new RbChartException($"Missing required field: {field}.");
        }

        private static void ValidateNote(Note note)
        {
            switch (note.Type)
            {
                case NoteType.Tap:
                case NoteType.Vertical:
                    ValidateX(note.X);
                    break;

                case NoteType.Long:
                    ValidateX(note.X);
                    if (note.DurationTicks <= 0)
                        throw new RbChartException("Long notes must have a positive durationTicks.");
                    break;

                case NoteType.Top:
                    if (note.Zone < 0 || note.Zone > 2)
                        throw new RbChartException("Top notes must have zone 0, 1, or 2.");
                    break;

                case NoteType.BigBang:
                    break;

                default:
                    throw new RbChartException($"Unknown note type: '{note.Type}'.");
            }
        }

        private static void ValidateX(float x)
        {
            if (x < 0f || x > 1f)
                throw new RbChartException("Note x must be between 0.0 and 1.0.");
        }

        private static void AppendMeta(StringBuilder sb, ChartMeta meta)
        {
            sb.AppendLine(Indent(1) + "\"meta\": {");
            sb.AppendLine(Indent(2) + $"\"title\": {Escape(meta.Title)},");
            sb.AppendLine(Indent(2) + $"\"artist\": {Escape(meta.Artist)},");
            sb.AppendLine(Indent(2) + $"\"charter\": {Escape(meta.Charter)},");
            sb.AppendLine(Indent(2) + $"\"audioFile\": {Escape(meta.AudioFile)},");
            sb.AppendLine(Indent(2) + $"\"difficulty\": {Escape(meta.Difficulty)},");
            sb.AppendLine(Indent(2) + $"\"level\": {meta.Level}");
            sb.AppendLine(Indent(1) + "},");
        }

        private static void AppendTiming(StringBuilder sb, ChartTiming timing)
        {
            sb.AppendLine(Indent(1) + "\"timing\": {");
            sb.AppendLine(Indent(2) + $"\"ppq\": {timing.Ppq},");
            sb.AppendLine(Indent(2) + $"\"audioOffsetMs\": {timing.AudioOffsetMs.ToString("R", CultureInfo.InvariantCulture)},");
            sb.AppendLine(Indent(2) + "\"bpmChanges\": [");

            List<BpmChange> ordered = timing.BpmChanges.OrderBy(c => c.Tick).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                BpmChange change = ordered[i];
                string comma = i < ordered.Count - 1 ? "," : "";
                sb.AppendLine(Indent(3) + "{ "
                    + $"\"tick\": {change.Tick}, "
                    + $"\"bpm\": {change.Bpm.ToString("R", CultureInfo.InvariantCulture)} }}"
                    + comma);
            }

            sb.AppendLine(Indent(2) + "]");
            sb.AppendLine(Indent(1) + "},");
        }

        private static void AppendRival(StringBuilder sb, RivalConfig rival)
        {
            if (rival == null || rival.IsDefault())
                return;

            sb.AppendLine(Indent(1) + "\"rival\": {");
            sb.AppendLine(Indent(2) + $"\"accuracy\": {rival.Accuracy.ToString("R", CultureInfo.InvariantCulture)},");
            sb.AppendLine(Indent(2) + $"\"rivalHp\": {rival.RivalHp},");
            sb.AppendLine(Indent(2) + $"\"playerHp\": {rival.PlayerHp}");
            sb.AppendLine(Indent(1) + "},");
        }

        private static void AppendNotes(StringBuilder sb, List<Note> notes)
        {
            sb.AppendLine(Indent(1) + "\"notes\": [");

            List<Note> ordered = notes.OrderBy(n => n.Tick).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                Note note = ordered[i];
                string comma = i < ordered.Count - 1 ? "," : "";
                sb.Append(Indent(2) + "{ "
                    + $"\"tick\": {note.Tick}, "
                    + $"\"type\": {Escape(note.Type)}");

                switch (note.Type)
                {
                    case NoteType.Tap:
                    case NoteType.Vertical:
                        sb.Append($", \"x\": {note.X.ToString("R", CultureInfo.InvariantCulture)}");
                        break;

                    case NoteType.Long:
                        sb.Append($", \"x\": {note.X.ToString("R", CultureInfo.InvariantCulture)}");
                        sb.Append($", \"durationTicks\": {note.DurationTicks}");
                        break;

                    case NoteType.Top:
                        sb.Append($", \"zone\": {note.Zone}");
                        break;
                }

                if (note.IsRival)
                    sb.Append(", \"rival\": true");
                if (note.IsGold)
                    sb.Append(", \"gold\": true");

                sb.AppendLine(" }" + comma);
            }

            sb.AppendLine(Indent(1) + "]");
        }

        private static string Escape(string value)
        {
            if (value == null)
                return "\"\"";

            StringBuilder sb = new StringBuilder(value.Length + 2);
            sb.Append('\"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (char.IsControl(c))
                            sb.Append($"\\u{(int)c:X4}");
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('\"');
            return sb.ToString();
        }

        private static string Indent(int level)
        {
            return new string(' ', level * 2);
        }
    }
}
