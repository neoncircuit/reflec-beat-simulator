# Architecture

## Overview

The simulator is split into three layers:

- **`Core/`** — pure C#, Unity-agnostic. Parser, timing conversion, judgment, and scoring. Covered by EditMode tests.
- **`Audio/`** — DSP-clock audio scheduling and timekeeping (`AudioPlayback`, `SongClock`, `Metronome`).
- **`Gameplay/`** and **`UI/`** — Unity `MonoBehaviour`s that drive scenes, render the playfield, capture input, and load charts.
- **`Editor/`** (Editor-only) — Authoring helpers: `ChartAuthoringTools`, `ChartPreviewWindow`, and the `Reflec Beat / Chart Preview` menu item.

```
Chart (Core)
├── Meta, Timing, Notes, Rival
└── TimingConverter.ApplyTimes()   # tick → seconds

RivalConfig (Core)
└── Optional duel settings: accuracy, rival HP, player HP

DuelState (Core)
└── HP bars, Just Reflect gauge, outcome resolution

CpuRival (Core)
└── Deterministic judgment schedule for rival notes

ChartWriter (Core)
└── Write() / ToJson()             # Chart → *.rbchart.json

ChartSection (Core)
└── GetNotesInRange()              # filter notes by [start, end) seconds

Judge (Core)
├── JudgeTouch()                   # tap / press grades
├── JudgeRelease()                 # long-note tail grades
└── Worse()                        # combine head + tail

Score (Core)
└── Apply(grade) → total / combo / achievement rate

AudioPlayback / SongClock (Audio)
└── Schedule() / Tick()            # dspTime-based clock

ContentBootstrapper (Gameplay)
└── Copies StreamingAssets/charts/* → persistentDataPath/charts/

ChartRepository (Gameplay)
└── Discovers chart folders with *.rbchart.json

ChartSelectController + ChartSelectUI (Gameplay / UI)
└── Lists charts, loads selected chart+audio into ChartSession

GameController (Gameplay)
├── Reads ChartSession (chart + audio clip + autoplay flag)
├── Schedules music
├── Spawns NoteObjects (player and rival)
├── Processes autoplay touches/releases
├── Judges real touches from TouchInput
├── Drives CPU rival judgments in duel mode
├── Handles Just Reflect activation
└── Drives GameplayUI / DuelUI / ResultsUI

NoteObject (Gameplay)
└── Visual state + position update for one note (player or rival)

PlayfieldRenderer (Gameplay)
└── Spawn / judgment-line positions and top-zone layout

TouchInput (Gameplay)
└── Captures touches/mouse, forwards TouchData with dspTime
```

## Core scripts

### `ChartParser.cs`

Parses `*.rbchart.json` into a `Chart` object. Validates the schema from `DESIGN.md` §3.4 and throws `RbChartException` on malformed input.

### `TimingConverter.cs`

Walks the BPM map and computes each note's absolute `TimeSec`. Also computes `EndTick` and `EndTimeSec` for long notes.

### `Judge.cs`

Implements the judgment rules for all note types:

- `tap` / `vertical` / `top` / `bigbang`: single timing window.
- `long`: head press + tail release; final grade is the worse of the two.
- `top`: matched by horizontal zone only (`zone: 0|1|2`).
- `bigbang`: any touch in the time window counts, X is ignored.

### `Score.cs`

Tracks total score, current combo, max combo, and achievement rate. `JUST/GREAT/GOOD/MISS` values come from `Constants`.

### `RivalConfig.cs`

Optional chart-level duel configuration: CPU `Accuracy`, rival HP, and player HP. `ChartParser` validates the block; `ChartWriter` omits it when it matches the defaults.

### `DuelState.cs`

Pure C# state machine for the single-player duel layer. Tracks player HP, rival HP, Just Reflect gauge, and the final `DuelOutcome`. Applies gauge gain on player judgments, player damage on misses and CPU hits, and reflect damage when the gauge is full.

### `CpuRival.cs`

Deterministic CPU rival. At construction it builds a time-sorted judgment schedule from all rival notes, using a per-note seeded random roll to choose JUST/GREAT/GOOD based on `RivalConfig.Accuracy`. `Process(songTime, callback)` consumes due judgments each frame so the CPU stays locked to the song clock.

## Gameplay scripts

### `GameController.cs`

Central gameplay loop. Responsibilities:

1. Load chart/audio from `ChartSession`, falling back to a built-in demo chart if the session is empty.
2. Apply timing conversion to notes.
3. Schedule music playback via `AudioPlayback.Schedule`.
4. Spawn notes `approachTime` before their hit time (both player and rival notes).
5. Process autoplay input if enabled.
6. Judge touches from `TouchInput`.
7. Detect missed notes.
8. Drive the CPU rival and Just Reflect gauge when a duel chart is active.
9. Update `GameplayUI`, `DuelUI`, and show `ResultsUI` when the chart finishes.

### `NoteObject.cs`

Visual representation of a note. Initialized with its note data and playfield reference, then updated each frame to move from spawn point toward the judgment line. Rival notes travel in the opposite direction and are tinted red; gold notes are tinted yellow.

### `PlayfieldRenderer.cs`

Defines the playfield geometry: spawn position for a given normalized X, judgment-line position, and the three top-note zones.

### `TouchInput.cs`

Captures touch (Enhanced Touch Support) and mouse input, normalizes screen X to `[0, 1]`, and records DSP time for each touch begin/end. Emits `TouchData` events.

### `ChartLoader.cs` and `AudioLoader.cs`

Load a chart folder asynchronously: parse JSON, apply timing, and load the audio clip via `UnityWebRequestMultimedia.GetAudioClip` so Android paths work. `ChartLoader` also validates that the audio path stays inside the chart directory.

### `ChartSession.cs`

Static container used to pass the selected chart, audio clip, and autoplay flag from `ChartSelectScene` to `GameplayScene`. For editor previews it also stores the `IsPreview` flag and `[PreviewStartSec, PreviewEndSec)` range so `GameController` can play a section.

### `ContentBootstrapper.cs`

Runs on first launch (or when its version marker is stale) and copies the bundled demo chart/audio from `StreamingAssets/charts/` into `Application.persistentDataPath/charts/`. Uses `UnityWebRequest` on Android.

### `ChartRepository.cs`

Discovers chart folders under `Application.persistentDataPath/charts/` that contain at least one `*.rbchart.json` file.

### `ChartSelectController.cs` + `ChartSelectUI.cs`

Populate the chart-select screen with one button per discovered chart, plus toggles for autoplay, sync check, and rival-visibility, and an approach-time dropdown. On selection, the controller persists the current options via `SettingsStore`, loads the chart/audio into `ChartSession`, and loads `GameplayScene`. Chart labels include the stored best grade and achievement rate from `HighScoreStore`.

### `SettingsStore.cs`

Persists user-facing gameplay settings (approach time, hide rival visuals, autoplay, sync check) to a JSON file under `Application.persistentDataPath`. Loaded by `ChartSelectUI` on startup and saved when a chart is selected.

### `HighScoreStore.cs`

Persists the best result for each chart to a JSON file under `Application.persistentDataPath`. Saves a result only when it beats the existing entry by achievement rate or, on a tie, by max combo. Used to annotate chart-select labels and can be surfaced on the results screen.

### `CalibrationController.cs` + `CalibrationUI.cs`

Calibration scene: plays a metronome, captures tap deltas, stores an input offset in `CalibrationStore`, and proceeds to `ChartSelectScene`.

## UI scripts

### `GameplayUI.cs`

Updates score, combo, and judgment text during gameplay.

### `ResultsUI.cs`

Displays the final grade, score, max combo, and achievement rate when a chart finishes. In duel mode it also shows a colored WIN / LOSE / DRAW outcome. Provides Retry (reloads `GameplayScene`) and Back (returns to `ChartSelectScene`) buttons so the player can keep the loop tight.

### `DuelUI.cs`

HUD for the single-player duel layer. Shows player HP, rival HP, Just Reflect gauge, and the reflect button. Fires `OnReflectRequested` when the button is pressed, which `GameController` routes to `DuelState.ActivateJustReflect()`.

## Authoring and preview

M3 adds an editor-only authoring workflow. The components are:

### `ChartWriter.cs`

Serializes a `Chart` back to `*.rbchart.json`. It validates required fields, sorts notes and BPM changes by tick, omits derived times, and uses invariant culture so files round-trip cleanly.

### `ChartSection.cs`

Pure helper that returns the notes whose head time lies inside `[startSec, endSec)`. `GameController` uses this to limit preview playback to a section.

### `ChartAuthoringTools.cs`

Static editor helpers that create a chart folder under `Application.persistentDataPath/charts/<name>/`, copy in an audio file, and write a starter JSON via `ChartWriter`.

### `ChartPreviewWindow.cs`

`EditorWindow` at **`Reflec Beat / Chart Preview`**. It lists discovered chart folders, lets the author set a preview start time and duration, and launches `GameplayScene` in Play mode for that section. It temporarily disables domain reload so the static `ChartSession` survives entering Play mode.

See [`authoring.md`](./authoring.md) for the user-facing guide.

### Preview data flow

1. Author selects a chart and range in `ChartPreviewWindow`.
2. `ChartLoader.LoadAsync` parses the JSON and loads the audio clip.
3. `ChartSession.SetPreview(chart, clip, startSec, endSec)` stores chart, clip, range, and forces autoplay.
4. `GameplayScene` enters Play mode; `GameController.Start()` detects `ChartSession.IsPreview`.
5. `GameController` filters notes with `ChartSection.GetNotesInRange`, schedules audio from `PreviewStartSec`, and offsets `_audioOffsetSec` so the existing judgment formula stays correct.
6. Autoplay hits every note in the section; `ResultsUI` appears when it ends.


## Data flow

1. **First launch**: `ContentBootstrapper` copies demo chart/audio to persistent storage.
2. **Calibration**: player taps beats; offset is saved to `CalibrationStore`.
3. **Chart select**: `ChartRepository` lists charts; `ChartSelectController` loads the chosen one into `ChartSession`.
4. **Gameplay**: `GameController` schedules audio, spawns notes, judges input, drives the CPU rival and Just Reflect gauge in duel mode, and updates UI.
5. **Results**: `ResultsUI` shows the final `Score` and, for duel charts, the `DuelOutcome`.

## Test coverage

EditMode tests live in `Assets/Tests/EditMode/`:

- `SongClockTests` — DSP-clock smoothing and snap behavior.
- `TimingConverterTests` — BPM changes and tick-to-seconds conversion.
- `JudgeTests` — all note types and long-note release windows.
- `ScoreTests` — scoring and combo behavior.
- `ChartParserTests` — valid charts, invalid JSON, missing fields, and every note type.
- `ChartWriterTests` — round-trip serialization, derived-field omission, sorting, and validation.
- `ChartSectionTests` — filtering notes by preview range.
- `DuelStateTests` — gauge gain, damage, reflect activation, and outcome resolution.
- `CpuRivalTests` — deterministic scheduling, grade distribution, and ignoring player notes.
