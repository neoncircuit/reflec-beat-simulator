using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Parses a <see cref="Chart"/> from a <see cref="*.rbchart.json"/> string.</summary>
    public static class ChartParser
    {
        /// <summary>Parses JSON chart text into a validated <see cref="Chart"/> with absolute seconds.</summary>
        public static Chart Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new RbChartException("Chart JSON is empty.");

            JsonValue root;
            try
            {
                root = JsonReader.Read(json);
            }
            catch (Exception ex)
            {
                throw new RbChartException($"Invalid JSON: {ex.Message}", ex);
            }

            if (root.Kind != JsonValue.JsonKind.Object)
                throw new RbChartException("Chart root must be a JSON object.");

            var rootObject = root.AsObject();

            if (!rootObject.TryGetValue("version", out JsonValue versionValue))
                throw new RbChartException("Missing required field: version.");

            if (!versionValue.IsNumber || (int)versionValue.AsNumber() != 1)
                throw new RbChartException($"Unsupported chart version: {versionValue}.");

            Chart chart = new Chart
            {
                Meta = ParseMeta(rootObject),
                Timing = ParseTiming(rootObject),
                Notes = ParseNotes(rootObject),
                Rival = ParseRivalConfig(rootObject)
            };

            TimingConverter converter = new TimingConverter(chart.Timing);
            converter.ApplyTimes(chart.Notes);

            foreach (Note note in chart.Notes)
            {
                if (note.Type == NoteType.Long)
                {
                    note.EndTick = note.Tick + note.DurationTicks;
                    note.EndTimeSec = converter.TickToSeconds(note.EndTick);
                }
            }

            return chart;
        }

        private static ChartMeta ParseMeta(Dictionary<string, JsonValue> root)
        {
            if (!root.TryGetValue("meta", out JsonValue metaValue))
                throw new RbChartException("Missing required section: meta.");

            if (metaValue.Kind != JsonValue.JsonKind.Object)
                throw new RbChartException("Section 'meta' must be an object.");

            Dictionary<string, JsonValue> meta = metaValue.AsObject();

            ChartMeta result = new ChartMeta
            {
                Title = GetString(meta, "title", "meta"),
                Artist = GetString(meta, "artist", "meta"),
                Charter = GetString(meta, "charter", "meta"),
                AudioFile = GetString(meta, "audioFile", "meta"),
                Difficulty = GetString(meta, "difficulty", "meta"),
                Level = GetInt(meta, "level", "meta")
            };

            return result;
        }

        private static ChartTiming ParseTiming(Dictionary<string, JsonValue> root)
        {
            if (!root.TryGetValue("timing", out JsonValue timingValue))
                throw new RbChartException("Missing required section: timing.");

            if (timingValue.Kind != JsonValue.JsonKind.Object)
                throw new RbChartException("Section 'timing' must be an object.");

            Dictionary<string, JsonValue> timing = timingValue.AsObject();

            ChartTiming result = new ChartTiming
            {
                Ppq = timing.TryGetValue("ppq", out JsonValue ppqValue)
                    ? GetPositiveInt(ppqValue, "timing.ppq")
                    : Constants.Ppq,
                AudioOffsetMs = timing.TryGetValue("audioOffsetMs", out JsonValue offsetValue)
                    ? GetNumber(offsetValue, "timing.audioOffsetMs")
                    : 0.0
            };

            if (!timing.TryGetValue("bpmChanges", out JsonValue bpmChangesValue))
                throw new RbChartException("Missing required field: timing.bpmChanges.");

            if (bpmChangesValue.Kind != JsonValue.JsonKind.Array)
                throw new RbChartException("Field 'timing.bpmChanges' must be an array.");

            List<JsonValue> bpmArray = bpmChangesValue.AsArray();
            if (bpmArray.Count == 0)
                throw new RbChartException("Field 'timing.bpmChanges' must not be empty.");

            int lastTick = int.MinValue;
            foreach (JsonValue item in bpmArray)
            {
                if (item.Kind != JsonValue.JsonKind.Object)
                    throw new RbChartException("Every entry in 'timing.bpmChanges' must be an object.");

                Dictionary<string, JsonValue> change = item.AsObject();
                int tick = GetInt(change, "tick", "timing.bpmChanges.tick");
                double bpm = GetNumber(change, "bpm", "timing.bpmChanges.bpm");

                if (tick < 0)
                    throw new RbChartException("BPM change tick must be non-negative.");
                if (bpm <= 0.0)
                    throw new RbChartException("BPM values must be positive.");
                if (tick < lastTick)
                    throw new RbChartException("BPM changes must be ordered by tick.");

                lastTick = tick;
                result.BpmChanges.Add(new BpmChange { Tick = tick, Bpm = bpm });
            }

            return result;
        }

        private static List<Note> ParseNotes(Dictionary<string, JsonValue> root)
        {
            if (!root.TryGetValue("notes", out JsonValue notesValue))
                throw new RbChartException("Missing required section: notes.");

            if (notesValue.Kind != JsonValue.JsonKind.Array)
                throw new RbChartException("Section 'notes' must be an array.");

            List<JsonValue> notesArray = notesValue.AsArray();
            List<Note> notes = new List<Note>(notesArray.Count);

            for (int i = 0; i < notesArray.Count; i++)
            {
                JsonValue item = notesArray[i];
                if (item.Kind != JsonValue.JsonKind.Object)
                    throw new RbChartException($"Note at index {i} must be an object.");

                notes.Add(ParseNote(item.AsObject(), i));
            }

            return notes;
        }

        private static RivalConfig ParseRivalConfig(Dictionary<string, JsonValue> root)
        {
            if (!root.TryGetValue("rival", out JsonValue rivalValue))
                return null;

            if (rivalValue.Kind != JsonValue.JsonKind.Object)
                throw new RbChartException("Section 'rival' must be an object.");

            Dictionary<string, JsonValue> rival = rivalValue.AsObject();
            RivalConfig config = new RivalConfig();

            if (rival.TryGetValue("accuracy", out JsonValue accuracyValue))
                config.Accuracy = GetNumber(accuracyValue, "rival.accuracy");

            if (rival.TryGetValue("rivalHp", out JsonValue rivalHpValue))
                config.RivalHp = GetPositiveInt(rivalHpValue, "rival.rivalHp");

            if (rival.TryGetValue("playerHp", out JsonValue playerHpValue))
                config.PlayerHp = GetPositiveInt(playerHpValue, "rival.playerHp");

            config.Validate();
            return config;
        }

        private static Note ParseNote(Dictionary<string, JsonValue> noteObject, int index)
        {
            int tick = GetInt(noteObject, "tick", $"notes[{index}].tick");
            if (tick < 0)
                throw new RbChartException($"Note at index {index} has a negative tick.");

            string type = GetString(noteObject, "type", $"notes[{index}].type");

            Note note = new Note
            {
                Tick = tick,
                Type = ValidateNoteType(type, index)
            };

            switch (type)
            {
                case NoteType.Tap:
                case NoteType.Vertical:
                    note.X = GetNormalizedX(noteObject, "x", $"notes[{index}].x");
                    break;

                case NoteType.Long:
                    note.X = GetNormalizedX(noteObject, "x", $"notes[{index}].x");
                    note.DurationTicks = GetPositiveInt(noteObject, "durationTicks", $"notes[{index}].durationTicks");
                    break;

                case NoteType.Top:
                    note.Zone = GetZone(noteObject, "zone", $"notes[{index}].zone");
                    break;

                case NoteType.BigBang:
                    // No position data required.
                    break;
            }

            note.IsRival = GetOptionalBoolean(noteObject, "rival", false);
            note.IsGold = GetOptionalBoolean(noteObject, "gold", false);

            return note;
        }

        private static string ValidateNoteType(string type, int index)
        {
            if (type == NoteType.Tap ||
                type == NoteType.Long ||
                type == NoteType.Vertical ||
                type == NoteType.Top ||
                type == NoteType.BigBang)
            {
                return type;
            }

            throw new RbChartException($"Note at index {index} has unknown type '{type}'.");
        }

        private static string GetString(Dictionary<string, JsonValue> obj, string key, string path)
        {
            if (!obj.TryGetValue(key, out JsonValue value))
                throw new RbChartException($"Missing required field: {path}.");

            if (value.Kind != JsonValue.JsonKind.String)
                throw new RbChartException($"Field '{path}' must be a string.");

            return value.AsString();
        }

        private static int GetInt(Dictionary<string, JsonValue> obj, string key, string path)
        {
            if (!obj.TryGetValue(key, out JsonValue value))
                throw new RbChartException($"Missing required field: {path}.");

            return GetInt(value, path);
        }

        private static int GetInt(JsonValue value, string path)
        {
            if (!value.IsNumber)
                throw new RbChartException($"Field '{path}' must be an integer.");

            double number = value.AsNumber();
            if (number != Math.Floor(number))
                throw new RbChartException($"Field '{path}' must be an integer.");

            return (int)number;
        }

        private static int GetPositiveInt(Dictionary<string, JsonValue> obj, string key, string path)
        {
            int value = GetInt(obj, key, path);
            if (value <= 0)
                throw new RbChartException($"Field '{path}' must be positive.");
            return value;
        }

        private static int GetPositiveInt(JsonValue value, string path)
        {
            int result = GetInt(value, path);
            if (result <= 0)
                throw new RbChartException($"Field '{path}' must be positive.");
            return result;
        }

        private static double GetNumber(Dictionary<string, JsonValue> obj, string key, string path)
        {
            if (!obj.TryGetValue(key, out JsonValue value))
                throw new RbChartException($"Missing required field: {path}.");

            return GetNumber(value, path);
        }

        private static double GetNumber(JsonValue value, string path)
        {
            if (!value.IsNumber)
                throw new RbChartException($"Field '{path}' must be a number.");

            return value.AsNumber();
        }

        private static float GetNormalizedX(Dictionary<string, JsonValue> obj, string key, string path)
        {
            double value = GetNumber(obj, key, path);
            if (value < 0.0 || value > 1.0)
                throw new RbChartException($"Field '{path}' must be between 0.0 and 1.0.");

            return (float)value;
        }

        private static int GetZone(Dictionary<string, JsonValue> obj, string key, string path)
        {
            int value = GetInt(obj, key, path);
            if (value < 0 || value > 2)
                throw new RbChartException($"Field '{path}' must be 0, 1, or 2.");

            return value;
        }

        private static bool GetOptionalBoolean(Dictionary<string, JsonValue> obj, string key, bool defaultValue)
        {
            if (!obj.TryGetValue(key, out JsonValue value))
                return defaultValue;

            if (value.Kind != JsonValue.JsonKind.Boolean)
                throw new RbChartException($"Field '{key}' must be a boolean.");

            return value.AsBoolean();
        }

        /// <summary>Tiny JSON reader for the chart format. Supports objects, arrays, strings, numbers, booleans, null.</summary>
        private static class JsonReader
        {
            public static JsonValue Read(string json)
            {
                Parser parser = new Parser(json);
                JsonValue value = parser.ParseValue();
                parser.SkipWhitespace();
                if (!parser.IsAtEnd)
                    throw new Exception("Unexpected data after JSON value.");
                return value;
            }

            private sealed class Parser
            {
                private readonly string _text;
                private int _pos;

                public Parser(string text)
                {
                    _text = text;
                }

                public bool IsAtEnd => _pos >= _text.Length;

                public JsonValue ParseValue()
                {
                    SkipWhitespace();
                    if (IsAtEnd)
                        throw new Exception("Unexpected end of JSON.");

                    char c = _text[_pos];
                    switch (c)
                    {
                        case '{': return ParseObject();
                        case '[': return ParseArray();
                        case '"': return ParseString();
                        case 't':
                        case 'f': return ParseBoolean();
                        case 'n': return ParseNull();
                        default:
                            if (c == '-' || char.IsDigit(c))
                                return ParseNumber();
                            throw new Exception($"Unexpected character '{c}' at position {_pos}.");
                    }
                }

                private JsonValue ParseObject()
                {
                    Expect('{');
                    Dictionary<string, JsonValue> obj = new Dictionary<string, JsonValue>();

                    SkipWhitespace();
                    if (Peek() == '}')
                    {
                        _pos++;
                        return JsonValue.Object(obj);
                    }

                    while (true)
                    {
                        SkipWhitespace();
                        JsonValue keyValue = ParseString();
                        string key = keyValue.AsString();

                        SkipWhitespace();
                        Expect(':');

                        SkipWhitespace();
                        JsonValue value = ParseValue();
                        obj[key] = value;

                        SkipWhitespace();
                        char c = Peek();
                        if (c == ',')
                        {
                            _pos++;
                            continue;
                        }

                        if (c == '}')
                        {
                            _pos++;
                            return JsonValue.Object(obj);
                        }

                        throw new Exception($"Expected ',' or '}}' at position {_pos}.");
                    }
                }

                private JsonValue ParseArray()
                {
                    Expect('[');
                    List<JsonValue> array = new List<JsonValue>();

                    SkipWhitespace();
                    if (Peek() == ']')
                    {
                        _pos++;
                        return JsonValue.Array(array);
                    }

                    while (true)
                    {
                        SkipWhitespace();
                        array.Add(ParseValue());

                        SkipWhitespace();
                        char c = Peek();
                        if (c == ',')
                        {
                            _pos++;
                            continue;
                        }

                        if (c == ']')
                        {
                            _pos++;
                            return JsonValue.Array(array);
                        }

                        throw new Exception($"Expected ',' or ']' at position {_pos}.");
                    }
                }

                private JsonValue ParseString()
                {
                    Expect('"');
                    StringBuilder sb = new StringBuilder();

                    while (!IsAtEnd)
                    {
                        char c = _text[_pos++];
                        if (c == '"')
                            return JsonValue.String(sb.ToString());

                        if (c == '\\')
                        {
                            if (IsAtEnd)
                                throw new Exception("Unexpected end of string.");

                            char escape = _text[_pos++];
                            switch (escape)
                            {
                                case '"': sb.Append('"'); break;
                                case '\\': sb.Append('\\'); break;
                                case '/': sb.Append('/'); break;
                                case 'b': sb.Append('\b'); break;
                                case 'f': sb.Append('\f'); break;
                                case 'n': sb.Append('\n'); break;
                                case 'r': sb.Append('\r'); break;
                                case 't': sb.Append('\t'); break;
                                case 'u':
                                    if (_pos + 4 > _text.Length)
                                        throw new Exception("Incomplete unicode escape.");
                                    string hex = _text.Substring(_pos, 4);
                                    _pos += 4;
                                    int code = int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                    sb.Append((char)code);
                                    break;
                                default:
                                    throw new Exception($"Unknown escape '\\{escape}'.");
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    throw new Exception("Unterminated string.");
                }

                private JsonValue ParseNumber()
                {
                    int start = _pos;
                    if (Peek() == '-')
                        _pos++;

                    while (!IsAtEnd && char.IsDigit(_text[_pos]))
                        _pos++;

                    if (!IsAtEnd && _text[_pos] == '.')
                    {
                        _pos++;
                        while (!IsAtEnd && char.IsDigit(_text[_pos]))
                            _pos++;
                    }

                    if (!IsAtEnd && (_text[_pos] == 'e' || _text[_pos] == 'E'))
                    {
                        _pos++;
                        if (!IsAtEnd && (_text[_pos] == '+' || _text[_pos] == '-'))
                            _pos++;
                        while (!IsAtEnd && char.IsDigit(_text[_pos]))
                            _pos++;
                    }

                    string numberText = _text.Substring(start, _pos - start);
                    if (!double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        throw new Exception($"Invalid number '{numberText}'.");

                    return JsonValue.Number(value);
                }

                private JsonValue ParseBoolean()
                {
                    if (MatchLiteral("true"))
                        return JsonValue.Boolean(true);
                    if (MatchLiteral("false"))
                        return JsonValue.Boolean(false);
                    throw new Exception($"Unexpected token at position {_pos}.");
                }

                private JsonValue ParseNull()
                {
                    if (MatchLiteral("null"))
                        return JsonValue.Null;
                    throw new Exception($"Unexpected token at position {_pos}.");
                }

                private bool MatchLiteral(string literal)
                {
                    if (_pos + literal.Length > _text.Length)
                        return false;

                    if (_text.Substring(_pos, literal.Length) != literal)
                        return false;

                    _pos += literal.Length;
                    return true;
                }

                private void Expect(char expected)
                {
                    if (IsAtEnd || _text[_pos] != expected)
                        throw new Exception($"Expected '{expected}' at position {_pos}.");
                    _pos++;
                }

                private char Peek()
                {
                    return IsAtEnd ? '\0' : _text[_pos];
                }

                public void SkipWhitespace()
                {
                    while (!IsAtEnd)
                    {
                        char c = _text[_pos];
                        if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                            _pos++;
                        else
                            break;
                    }
                }
            }
        }

        /// <summary>A lightweight JSON value used only by <see cref="ChartParser"/>.</summary>
        private sealed class JsonValue
        {
            public enum JsonKind { Object, Array, String, Number, Boolean, Null }

            public JsonKind Kind { get; }
            private readonly object _data;

            private JsonValue(JsonKind kind, object data)
            {
                Kind = kind;
                _data = data;
            }

            public bool IsNumber => Kind == JsonKind.Number;

            public Dictionary<string, JsonValue> AsObject()
            {
                if (Kind != JsonKind.Object)
                    throw new InvalidOperationException("Value is not an object.");
                return (Dictionary<string, JsonValue>)_data;
            }

            public List<JsonValue> AsArray()
            {
                if (Kind != JsonKind.Array)
                    throw new InvalidOperationException("Value is not an array.");
                return (List<JsonValue>)_data;
            }

            public string AsString()
            {
                if (Kind != JsonKind.String)
                    throw new InvalidOperationException("Value is not a string.");
                return (string)_data;
            }

            public double AsNumber()
            {
                if (Kind != JsonKind.Number)
                    throw new InvalidOperationException("Value is not a number.");
                return (double)_data;
            }

            public bool AsBoolean()
            {
                if (Kind != JsonKind.Boolean)
                    throw new InvalidOperationException("Value is not a boolean.");
                return (bool)_data;
            }

            public static JsonValue Object(Dictionary<string, JsonValue> value) => new JsonValue(JsonKind.Object, value);
            public static JsonValue Array(List<JsonValue> value) => new JsonValue(JsonKind.Array, value);
            public static JsonValue String(string value) => new JsonValue(JsonKind.String, value);
            public static JsonValue Number(double value) => new JsonValue(JsonKind.Number, value);
            public static JsonValue Boolean(bool value) => new JsonValue(JsonKind.Boolean, value);
            public static readonly JsonValue Null = new JsonValue(JsonKind.Null, null);

            public override string ToString()
            {
                switch (Kind)
                {
                    case JsonKind.String: return AsString();
                    case JsonKind.Number: return AsNumber().ToString(CultureInfo.InvariantCulture);
                    case JsonKind.Boolean: return AsBoolean().ToString().ToLowerInvariant();
                    case JsonKind.Null: return "null";
                    default: return Kind.ToString();
                }
            }
        }
    }
}
