# Reflec Beat Simulator — Prototype

A minimal Unity prototype recreating the core feel of *Reflec Beat*: notes spawn at the center of a circular playfield and travel toward edge target zones, which the player taps in time with the music.

This is a **draft prototype**. The chart is hardcoded in `GameController.cs` so the focus is on gameplay feel, not file formats or editors.

## Legal note

This project does **not** include any Konami copyrighted music, charts, artwork, or assets. The included `PlaceholderBeat.wav` is a generated 120 BPM metronome loop. If you want to recreate official Reflec Beat patterns, you must obtain the rights or create original content.

## Requirements

- [Unity Hub](https://unity.com/download)
- Unity Editor **2022.3 LTS** or newer
- Windows, macOS, or Linux version of Unity Editor

The project is stored at `/mnt/d/GitHub/reflec-beat-simulator`. On Windows, this path maps to `D:\GitHub\reflec-beat-simulator`, so you can open it directly with Windows Unity Hub.

## Project structure

```
Assets/
  Scripts/
    NoteData.cs       # Data for one note
    Note.cs           # Note movement/state
    TargetZone.cs     # Touch/mouse/keyboard input for a target
    GameController.cs # Spawning, judging, scoring
    UIManager.cs      # Score/combo/judgment display
  Audio/
    PlaceholderBeat.wav # Generated 120 BPM loop
  Scenes/
    (create GameScene.unity here)
  Prefabs/
    (create Note.prefab here)
```

## Setup instructions

### 1. Install Unity

1. Download and install **Unity Hub**.
2. In Hub, install **Unity 2022.3 LTS** (or newer) with the **Android Build Support** module if you plan to build for Android.

### 2. Open this project

1. In Unity Hub, click **Open**.
2. Navigate to `D:\GitHub\reflec-beat-simulator` (Windows) or `/mnt/d/GitHub/reflec-beat-simulator` (Linux/WSL).
3. Open the project. Unity will import assets and generate the `Library/` folder.

### 3. Create the Note prefab

1. In the Project window, go to `Assets > Create > Sprites > Circle` to create a circle sprite.
2. In the Hierarchy, create an empty GameObject named `Note`.
3. Add a **Sprite Renderer** and assign the circle sprite.
4. Add the `Note` script (`Assets/Scripts/Note.cs`).
5. Set Scale to `(0.6, 0.6, 0.6)`.
6. Drag the `Note` GameObject into `Assets/Prefabs/` to create a prefab.
7. Delete the `Note` GameObject from the Hierarchy.

### 4. Create the scene

1. Open `Assets/Scenes/GameScene.unity` (or create a new scene and save it there).
2. Select **Main Camera** and set:
   - **Projection**: Orthographic
   - **Size**: `5`
   - **Background**: dark color
3. Create an empty GameObject named `SpawnPoint` at position `(0, 0, 0)`.
4. Create four empty GameObjects named `TargetZone_0` through `TargetZone_3` at these positions:
   - `TargetZone_0`: `(-3.5, 0, 0)`
   - `TargetZone_1`: `(0, 3.5, 0)`
   - `TargetZone_2`: `(3.5, 0, 0)`
   - `TargetZone_3`: `(0, -3.5, 0)`
5. For each target zone:
   - Add a visible sprite (circle or ring).
   - Add the `TargetZone` script.
   - Set `Lane Index` to `0`, `1`, `2`, `3` respectively.
   - Set `Debug Key` to `D`, `F`, `J`, `K` respectively.
6. Create an empty GameObject named `GameController` at `(0, 0, 0)`.
   - Add the `GameController` script.
   - Assign references:
     - **Music Source**: create a child GameObject with an **AudioSource**, load `PlaceholderBeat.wav`, and uncheck **Play On Awake**. Drag it here.
     - **Note Prefab**: the `Note` prefab.
     - **Spawn Point**: the `SpawnPoint` transform.
     - **Target Zones**: all four `TargetZone` objects, in order 0–3.
7. Create a **UI Canvas**:
   - Right-click → **UI → Canvas**.
   - Add three **TextMeshPro - Text (UI)** objects:
     - `ScoreText` (top-left): text `Score: 0`
     - `ComboText` (top-center): text `Combo: 0`
     - `JudgmentText` (center): text `READY`
8. Create an empty GameObject named `UIManager`.
   - Add the `UIManager` script.
   - Assign the three TextMeshPro text objects.
   - Drag `UIManager` into the **Ui Manager** field on `GameController`.

### 5. Test in the Editor

1. Press **Play**.
2. After a 2-second countdown, notes will spawn at the center and move toward the targets.
3. Tap the targets with the mouse or press `D`, `F`, `J`, `K` as notes arrive.
4. You should see `PERFECT`, `GREAT`, `GOOD`, or `MISS`, and the score/combo should update.

### 6. Build for Android

1. Go to **File > Build Settings**.
2. Switch platform to **Android**.
3. Add `GameScene` to **Scenes In Build**.
4. In **Player Settings**:
   - Set **Default Orientation** to **Landscape Left**.
   - Set a package name, e.g., `com.yourname.reflecbeatprototype`.
5. Connect an Android device with USB debugging enabled.
6. Click **Build And Run**.

## How it works

- **Music sync**: The song starts using `AudioSource.PlayScheduled(AudioSettings.dspTime + 2.0f)`. All note timing is measured against the audio DSP clock for sample-accurate sync.
- **Spawning**: Each note is spawned `travelTime` seconds before its hit time.
- **Movement**: Notes move from `SpawnPoint` to their target zone using `Vector3.Lerp`, driven by elapsed song time.
- **Input**: Each `TargetZone` detects touch, mouse, or keyboard input and forwards it to `GameController`.
- **Judging**: The closest unhit note in the tapped lane is judged by `|songTime - hitTime|`.
  - `PERFECT`: ±0.050s
  - `GREAT`: ±0.100s
  - `GOOD`: ±0.150s
  - `MISS`: ±0.200s or unhit past that
- **Scoring**: `PERFECT=100`, `GREAT=70`, `GOOD=40`, `MISS=0` and resets combo.

## Tuning the prototype

Open `Assets/Scripts/GameController.cs` and adjust these values in the Inspector:

- `travelTime` — how long notes take to reach targets
- `perfectWindow`, `greatWindow`, `goodWindow`, `missWindow` — judgment strictness
- `BuildHardcodedChart()` — change the hardcoded note pattern or BPM

## Next steps

Once the prototype feels good:

1. Design a JSON chart format.
2. Build a chart editor scene.
3. Replace the hardcoded chart with loaded chart files.
4. Add more note types (hold notes, chains).
5. Add menus and song select.
6. Add iOS build support.
