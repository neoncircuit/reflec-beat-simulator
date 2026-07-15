# Authoring charts for Reflec Beat Simulator

This guide describes how to create a `*.rbchart.json` chart from scratch and preview it inside the Unity editor. You do **not** need to write any code.

## Quick workflow

1. Open Unity and load the project.
2. Choose **`Reflec Beat / Chart Preview`** from the menu.
3. In the **New Chart** section, give the chart a name, browse to an audio file (`.wav`, `.mp3`, or `.ogg`), and click **Create Chart**.
4. Edit the generated JSON file with your favorite text editor.
5. Return to the **Chart Preview** window, select the chart, set a start time and duration, and click **Preview Section**.
6. The editor opens `GameplayScene`, enters Play mode, and plays only the notes inside the selected range with autoplay enabled.
7. Iterate on the JSON and repeat the preview until the chart feels right.
8. Play the full chart from `ChartSelectScene` to verify the complete file.

---

## Where charts live

At runtime the simulator reads charts from:

```
Application.persistentDataPath/charts/<chart-folder>/
    <chart-name>.rbchart.json
    <audio-file referenced by meta.audioFile>
```

The **Chart Preview** window creates this folder layout for you. If you prefer to do it by hand, create the folder and place both files there.

### Finding `persistentDataPath`

- **Unity Editor on Windows:** `C:\Users\<you>\AppData\Local\Unity\DefaultCompany\ReflecBeatSimulator`
- **Unity Editor on Linux/WSL:** `~/.config/unity3d/DefaultCompany/ReflecBeatSimulator`
- **Android:** `/storage/emulated/0/Android/data/<your-package-name>/files/`

Use the **Refresh** button in the Chart Preview window after adding folders manually.

---

## JSON format reference

A chart is one JSON object with three required sections (`meta`, `timing`, and `notes`) plus an optional `rival` section for duel charts.

```jsonc
{
  "version": 1,
  "meta": {
    "title": "My Song",
    "artist": "My Artist",
    "charter": "My Name",
    "audioFile": "beat.wav",
    "difficulty": "EASY",
    "level": 3
  },
  "timing": {
    "ppq": 480,
    "audioOffsetMs": 0,
    "bpmChanges": [
      { "tick": 0, "bpm": 128.0 }
    ]
  },
  "rival": {
    "accuracy": 0.95,
    "rivalHp": 100,
    "playerHp": 100
  },
  "notes": [
    { "tick": 480, "type": "tap", "x": 0.25 },
    { "tick": 960, "type": "tap", "x": 0.75, "gold": true },
    { "tick": 1440, "type": "tap", "x": 0.5, "rival": true }
  ]
}
```

The top-level `rival` block is optional and only needed for duel charts.

### `meta`

| Field | Type | Meaning |
|---|---|---|
| `title` | string | Song title shown in the chart list. |
| `artist` | string | Song artist. |
| `charter` | string | Person who wrote the chart. |
| `audioFile` | string | Filename of the audio file inside the chart folder. |
| `difficulty` | string | Difficulty label, e.g. `EASY`, `MEDIUM`, `HARD`. |
| `level` | integer | Numeric level. |

### `timing`

| Field | Type | Meaning |
|---|---|---|
| `ppq` | integer | Ticks per quarter note. The simulator expects `480`. |
| `audioOffsetMs` | number | Where tick 0 occurs in the audio, in milliseconds. Positive = tick 0 is after the audio start; negative = tick 0 is before the audio start. |
| `bpmChanges` | array | At least one `{ "tick": int, "bpm": number }`. The first entry should normally be at tick `0`. Later entries change tempo mid-song. |

### `notes`

Every note has `tick` and `type`. Other fields depend on the type.

| Type | Extra fields | Meaning |
|---|---|---|
| `tap` | `x` (0.0–1.0) | Tap when it reaches the judgment line. |
| `vertical` | `x` (0.0–1.0) | Falls straight down at fixed `x`. |
| `long` | `x` (0.0–1.0), `durationTicks` (int) | Press at the head, hold, release at the tail. |
| `top` | `zone` (0, 1, or 2) | Hit in one of the three top zones. |
| `bigbang` | none | Full-width; any touch in the time window counts. |

`x` is normalized across the judgment line: `0.0` is the left edge, `0.5` is center, `1.0` is the right edge.

Every note can also carry two optional booleans:

- `rival` — `true` means the CPU rival owns the note. Rival notes travel upward and are judged automatically by the CPU.
- `gold` — `true` means the note gives a bigger gauge reward on a player hit, or extra damage when the CPU hits a rival note.

Both default to `false`. See [Duel charts](#duel-charts) below for a full example.

---

## Tick math

With `ppq = 480`:

- 1 quarter note = `480` ticks
- 1 eighth note = `240` ticks
- 1 sixteenth note = `120` ticks
- 1 measure in 4/4 = `1920` ticks

So a note on beat 2 of measure 5 is at tick `(4 measures * 1920) + (1 beat * 480) = 8160`.

BPM changes are expressed in ticks too. A tempo change on beat 3 of measure 8 is at tick `(7 * 1920) + (2 * 480) = 14400`.

---

## Audio offset

`audioOffsetMs` is the single value that lines the chart up with the music.

- Set it to `0` first and place a `tap` note at tick `0`.
- Preview the first few seconds. If the note appears **before** the beat, increase `audioOffsetMs` (move tick 0 later in the audio). If it appears **after** the beat, decrease it.
- Adjust in 10–50 ms steps until the first beat is visually on the judgment line at the moment you hear it.

Do not shift notes forward/backward in the `notes` array to fix sync — change `audioOffsetMs` instead.

---

## Note-type examples

### `tap`

```json
{ "tick": 480, "type": "tap", "x": 0.5 }
```

### `long`

```json
{ "tick": 960, "type": "long", "x": 0.25, "durationTicks": 960 }
```

The head is at tick `960`; the tail is at tick `960 + 960 = 1920`.

### `vertical`

```json
{ "tick": 1440, "type": "vertical", "x": 0.75 }
```

### `top`

```json
{ "tick": 1920, "type": "top", "zone": 1 }
```

`zone` 0 is left, 1 is center, 2 is right.

### `bigbang`

```json
{ "tick": 2400, "type": "bigbang" }
```

---

## Duel charts

A chart becomes a single-player duel against a CPU rival as soon as at least one note has `"rival": true`. The player plays their own notes as usual, while the CPU automatically judges the rival notes travelling in the opposite direction.

### Optional duel config

If you omit the top-level `rival` block, these defaults are used:

| Field | Default | Meaning |
|---|---|---|
| `accuracy` | `0.95` | How likely the CPU is to earn a JUST on each rival note. |
| `rivalHp` | `100` | Starting HP for the CPU rival. |
| `playerHp` | `100` | Starting HP for the player. |

### Duel example

```jsonc
{
  "version": 1,
  "meta": { /* ... */ },
  "timing": { /* ... */ },
  "rival": {
    "accuracy": 0.95,
    "rivalHp": 100,
    "playerHp": 100
  },
  "notes": [
    { "tick": 480,  "type": "tap", "x": 0.25 },
    { "tick": 960,  "type": "tap", "x": 0.75, "gold": true },
    { "tick": 1440, "type": "tap", "x": 0.50, "rival": true },
    { "tick": 1920, "type": "tap", "x": 0.50, "rival": true, "gold": true }
  ]
}
```

### How the duel plays

- Player notes still move toward the judgment line and are tapped normally.
- Rival notes move upward from the judgment line to the spawn edge and are judged by the CPU.
- Every player hit builds a **Just Reflect gauge**; gold notes build it faster.
- When the gauge is full, a **Just Reflect** button appears. Pressing it fires a reflect attack that damages the rival.
- If the player misses a note, or the CPU lands a rival hit, the player loses HP.
- The duel ends when the song finishes or one side's HP reaches zero. The result is shown on the results screen.

### Authoring tips

- Start with only a few rival notes and test the HP/gauge balance before adding more.
- Keep rival note density lower than player note density; the CPU never misses, so too many rival notes can feel unfair.
- Use gold notes sparingly. On the player side they accelerate gauge gain; on the rival side they deal extra damage.

---

## BPM changes

```jsonc
"bpmChanges": [
  { "tick": 0, "bpm": 128.0 },
  { "tick": 7680, "bpm": 160.0 }
]
```

Every note after tick `7680` uses the new BPM. Notes before it keep the old BPM.

---

## Previewing a section

1. Open **`Reflec Beat / Chart Preview`**.
2. Select the chart in the list.
3. Set **Start Time** and **Duration** in seconds. A good first preview is `Start = 0`, `Duration = 10`.
4. Leave **Autoplay** on.
5. Click **Preview Section**.

The editor opens `GameplayScene`, starts Play mode, and plays only the notes whose head time is inside `[start, start + duration)`. Autoplay will hit every note with **JUST**, so you can verify sync by ear and eye. The results panel appears when the section ends.

### Preview boundaries

- Notes whose head is before the start time are skipped.
- Notes whose head is at or after the end time are skipped.
- Place section boundaries on measure lines and avoid previewing inside a long note.

---

## Playing the full chart

After editing, verify the complete chart the same way a player would:

1. Stop Play mode.
2. Open `Assets/Scenes/ChartSelectScene`.
3. Press **Play**.
4. Select your chart from the list.

The simulator loads the full chart and audio and plays from the beginning.

---

## Content reminder

Only use audio, charts, and artwork you have the right to distribute. The bundled `PlaceholderBeat.wav` and demo chart are original generated content. Recreating official Reflec Beat patterns from memory or transcription is still someone else's intellectual property; do not commit it.

---

## Troubleshooting

| Problem | Likely cause | Fix |
|---|---|---|
| Chart does not appear in the list | No `*.rbchart.json` in the folder, or folder is not under `persistentDataPath/charts/`. | Check the folder layout and click **Refresh**. |
| Preview fails to load audio | Audio file is missing or `audioFile` name does not match. | Verify the audio file exists in the chart folder and the filename matches `meta.audioFile` exactly. |
| Notes are off-sync | Wrong `audioOffsetMs`. | Place a tap at tick 0 and adjust `audioOffsetMs` in small steps. |
| `ChartParser` throws an exception | JSON syntax error or missing required field. | Run the JSON through a validator and compare against the reference above. |
| Long notes are not released correctly | Negative or zero `durationTicks`. | Use a positive integer. |
| `top` note is not judged | `zone` outside `0–2`. | Use `0`, `1`, or `2`. |

---

## File layout example

```
persistentDataPath/charts/my_song/
    my_song.rbchart.json
    beat.wav
```

`my_song.rbchart.json`:

```jsonc
{
  "version": 1,
  "meta": {
    "title": "My Song",
    "artist": "Me",
    "charter": "Me",
    "audioFile": "beat.wav",
    "difficulty": "EASY",
    "level": 1
  },
  "timing": {
    "ppq": 480,
    "audioOffsetMs": 0,
    "bpmChanges": [ { "tick": 0, "bpm": 120.0 } ]
  },
  "notes": [
    { "tick": 480, "type": "tap", "x": 0.5 }
  ]
}
```
