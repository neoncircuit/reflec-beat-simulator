# Architecture

## Overview

The prototype uses a simple component-based architecture. Each script has one clear responsibility.

```
GameController
├── spawns Notes from a hardcoded chart
├── moves Notes each frame
├── judges taps received from TargetZones
└── tells UIManager what to display

TargetZone (4 instances)
├── detects touch/mouse/keyboard input
└── calls GameController.HandleTargetTap(laneIndex)

Note (spawned at runtime)
├── stores its lane, hit time, and start/target positions
└── is moved by GameController

UIManager
└── updates score, combo, and judgment text
```

## Scripts

### `NoteData.cs`

A small data struct representing one note in a chart:

- `hitTime`: when the note should reach the target
- `lane`: which target zone the note travels to

### `Note.cs`

Runtime state for a single note. It does not move itself; `GameController` updates its transform each frame.

### `TargetZone.cs`

Attached to each of the four target zones. Handles:

- Touch input on Android
- Mouse clicks in the Editor
- A debug keyboard key for quick testing

When a valid input is detected, it tells `GameController` which lane was tapped.

### `GameController.cs`

The central game loop. Responsibilities:

1. Build a hardcoded chart in `BuildHardcodedChart()`.
2. Schedule music playback with `AudioSettings.dspTime`.
3. Spawn notes `travelTime` seconds before their hit time.
4. Move notes based on elapsed song time.
5. Judge taps from `TargetZone`.
6. Detect missed notes.
7. Track score and combo.

### `UIManager.cs`

Updates TextMeshPro UI elements after each judgment.

## Data flow

1. `GameController.Start()` builds the chart and schedules music.
2. Each frame, `GameController.Update()` spawns, moves, and checks notes.
3. Player taps a `TargetZone`.
4. `TargetZone` calls `GameController.HandleTargetTap()`.
5. `GameController` finds the closest unhit note in that lane and applies a judgment.
6. `GameController` calls `UIManager.ShowJudgment()`.
7. `UIManager` updates the screen.

## Future additions

- **Chart loader**: replace `BuildHardcodedChart()` with a JSON loader.
- **Chart editor**: a separate Unity scene or tool for authoring charts.
- **Note types**: hold notes, chain notes, simultaneous notes.
- **Settings**: audio offset, note speed, calibration.
