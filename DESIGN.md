# Reflec Beat Simulator — DESIGN.md (v2)

A fan-made, clean-room simulator inspired by Konami's Reflec Beat (2010–2020). This document is the source of truth for implementation. Agents working in this repo: follow the locked decisions below, do not expand scope beyond the current milestone, and prioritise the audio-sync baseline over any visual work.

**IP rule (hard constraint):** this project reimplements the gameplay *concept* only. No Konami audio, charts, art, fonts, or code may be used, extracted, or committed. Konami is credited in the README as the original designers; credit is not a license. All bundled content must be original or freely licensed.

---

## 1. Locked decisions

| Decision | Value |
|---|---|
| Engine | Unity (existing project in this repo) |
| Target device | Samsung Galaxy Tab (Android build target) |
| Language | C# |
| Master clock | `AudioSettings.dspTime` (audio hardware clock) — never `Time.time` |
| Chart format | JSON, engine-agnostic, spec in §3 |
| Visual scope | Baseline only. No theming, skins, or portrait/landscape debates until M1 is playable. Use Unity's default landscape orientation and placeholder primitives. |
| Multiplayer / online play | Out of scope; M4 is single-player versus a CPU rival only |

---

## 2. Priority order

1. **M0 — Sync spike.** No gameplay. One scene: play an audio clip via `PlayScheduled`, render a metronome tick, capture touches, log `(touchTime − beatTime)` deltas on the actual tablet. This validates the entire timing model before anything else is built.
2. **M1 — Playable baseline.** One hardcoded chart, `tap` notes only, judgment line, JUST/GREAT/GOOD/MISS grading, plain text results. Placeholder circles for notes.
3. **M2 — Chart format.** JSON loader, BPM changes, remaining note types, chart selection from device storage. Includes an **autoplay mode** (engine judges every note perfectly) — the fastest way to verify a chart's sync by ear with no human in the loop.
4. **M3 — Authoring.** Documented hand-authoring workflow plus an in-Unity preview `EditorWindow` (`Reflec Beat / Chart Preview`). A `ChartWriter` serializes `Chart` to valid `*.rbchart.json`, `ChartSection` filters notes by time range, and `GameController` reuses the normal playback path to preview any `[start, end)` section with audio. Authoring guide: `docs/authoring.md`.
5. **M4 — Single-player vs CPU rival.** Optional chart fields mark notes as rival-owned and/or gold. A deterministic CPU rival plays rival notes, the player builds a Just Reflect gauge, and a full gauge can strike back. Non-rival charts are unaffected.
6. **M5 — Game feel & visual clarity pass.** Polish note sizing/bounce readability, calibration UX, sync verification, object pooling, scroll-speed/rival-visibility options, and hit/results feedback.
7. **M6 — Persistence & session flow.** Save gameplay settings and per-chart high scores, show bests in chart select, and provide retry/back navigation from results. Keeps the loop tight for repeated play on the target tablet.
8. **M7 — Visual feel & transitions.** A first polish pass on the default presentation: redesign the chart-select screen, improve note visibility and hit feedback, and add smooth scene/UI transitions. No theming or skin system yet.
9. **M8 — Device verification & build hardening.** Validate calibration and audio-sync feel on the actual Galaxy Tab, lock Android build settings, and establish a reproducible signed-APK build step.
10. **M9 — Documentation & authoring workflow.** Improve the chart-authoring guide and in-editor preview workflow, add demo/tutorial charts, and finalize onboarding docs.

Anything not listed above is out of scope until these are done.

---

## 3. Chart format (how track patterns are coded)

A chart is **a timed event list plus timing metadata**. Notes are stored in *musical time* (ticks), not milliseconds, so charts survive BPM changes and align to a beat grid for authoring. Milliseconds are computed once at load.

### 3.1 Timing model

- **Resolution:** 480 ticks per quarter note (PPQ).
- **BPM map:** ordered list of `(tick, bpm)` changes. Tick→seconds conversion walks the map at load and caches an absolute time per note.
- **`audioOffsetMs`:** where tick 0 sits inside the audio file. This single number binds the chart to the soundtrack.

### 3.2 Playfield model

Reflec Beat is not a fixed-lane game. Objects spawn at the far edge, travel in a straight line, and are tapped when they cross the **judgment line** near the player. Therefore a note stores only:

- `tick` — when it must be hit (musical time)
- `x` — target position on the judgment line, normalised `0.0–1.0`
- `type` — see below

Trajectory (spawn point, travel path, approach speed) is derived by the engine from `tick`, `x`, and a global approach time — it is presentation, not chart data.

### 3.3 Note types

| Type | Behaviour | Milestone |
|---|---|---|
| `tap` | Tap when it crosses the judgment line | M1 |
| `long` | `tick` + `durationTicks`; press at head, release at tail | M2 |
| `vertical` | Falls straight down at fixed `x` | M2 |
| `top` | Hit in one of 3 zones at the top of the player area (`zone: 0\|1\|2`) | M2 |
| `bigbang` | Full-width; any touch in its window counts | M2 |

### 3.4 File format: `*.rbchart.json`

```jsonc
{
  "version": 1,
  "meta": {
    "title": "Example Track",
    "artist": "Original Artist",
    "charter": "jamtan",
    "audioFile": "example.ogg",
    "difficulty": "MEDIUM",
    "level": 7
  },
  "timing": {
    "ppq": 480,
    "audioOffsetMs": 132,
    "bpmChanges": [ { "tick": 0, "bpm": 172 } ]
  },
  "rival": {
    "accuracy": 0.95,
    "rivalHp": 100,
    "playerHp": 100
  },
  "notes": [
    { "tick": 0,    "type": "tap",  "x": 0.25 },
    { "tick": 240,  "type": "tap",  "x": 0.75, "gold": true },
    { "tick": 480,  "type": "long", "x": 0.5, "durationTicks": 960 },
    { "tick": 1920, "type": "top",  "zone": 1 },
    { "tick": 2400, "type": "tap",  "x": 0.5, "rival": true }
  ]
}
```

Optional fields:

- `rival` (note-level `bool`): `true` means the CPU rival owns the note. Defaults to `false`.
- `gold` (note-level `bool`): `true` gives bonus gauge on a player hit or extra damage on a CPU hit. Defaults to `false`.
- `rival` (top-level object): optional chart-level duel config. If omitted, defaults are used.

```jsonc
"rival": {
  "accuracy": 0.95,
  "rivalHp": 100,
  "playerHp": 100
}
```

### 3.5 Data model (pure C#, no Unity dependencies)

```csharp
/// <summary>A single note event in musical time. Seconds are derived at load, never stored.</summary>
public sealed class Note
{
    /// <summary>Musical time in ticks (PPQ-relative).</summary>
    public int Tick { get; set; }

    /// <summary>One of: "tap", "long", "vertical", "top", "bigbang".</summary>
    public string Type { get; set; }

    /// <summary>Target position on the judgment line, 0.0 (left) to 1.0 (right). Unused for top/bigbang.</summary>
    public float X { get; set; }

    /// <summary>Hold length for long notes, in ticks.</summary>
    public int DurationTicks { get; set; }

    /// <summary>3TOP zone index (0–2) for top notes.</summary>
    public int Zone { get; set; }

    /// <summary>Absolute hit time in seconds, computed at load from the BPM map + audioOffsetMs.</summary>
    public double TimeSec { get; internal set; }

    /// <summary>True if this note is owned by the CPU rival (single-player duel mode).</summary>
    public bool IsRival { get; set; }

    /// <summary>True if this note is a gold note (bonus gauge / extra damage).</summary>
    public bool IsGold { get; set; }
}
```

Optional chart-level duel configuration:

```csharp
public sealed class RivalConfig
{
    public double Accuracy { get; set; } = 0.95;
    public int RivalHp { get; set; } = 100;
    public int PlayerHp { get; set; } = 100;
}
```

### 3.6 Android file-loading gotcha

On Android, `StreamingAssets` lives inside the APK and **cannot** be read with `System.IO.File` — it requires `UnityWebRequest`. To avoid that entirely, load charts and audio from `Application.persistentDataPath/charts/`, and copy any bundled demo chart there on first launch.

---

## 4. Audio sync architecture (the critical part)

### 4.1 The one rule

**The audio hardware clock is the single source of truth.** Never `Time.time`, `Time.deltaTime` accumulation, coroutine waits, or `Invoke` for anything timing-critical — they drift relative to audio playback.

### 4.2 Pipeline

1. **Load** the clip (decompressed in memory for the playing song, not streaming, to keep playback latency predictable).
2. **Start** with a scheduled play so the start time is a known clock value:
   ```csharp
   double startDsp = AudioSettings.dspTime + 0.5;
   audioSource.PlayScheduled(startDsp);
   ```
3. **Song clock:** `songTime = AudioSettings.dspTime − startDsp − audioOffsetSec`. Every note's on-screen position each frame is a pure function of `note.TimeSec − songTime`; notes are never animated independently. This keeps hundreds of objects locked to the music even through frame drops.
4. **Judge:** on touch, compare touch time against the song clock, find the nearest unjudged note within the widest window near that `x`, grade it.

### 4.3 dspTime smoothing (known Unity quirk)

`AudioSettings.dspTime` advances in audio-buffer-sized steps, not continuously, so driving visuals from it raw causes micro-stutter. Use a hybrid clock: interpolate with `Time.unscaledTime` between frames and gently correct toward `dspTime` when they diverge.

```csharp
/// <summary>
/// Smoothed song clock: interpolates with the game clock between audio buffer
/// updates, while correcting drift toward the DSP (audio hardware) clock.
/// </summary>
public sealed class SongClock
{
    private readonly double _startDspTime;
    private readonly double _offsetSec;
    private double _smoothedTime;
    private double _lastUnscaledTime;

    /// <param name="startDspTime">The dspTime passed to AudioSource.PlayScheduled.</param>
    /// <param name="offsetSec">Chart audioOffsetMs converted to seconds.</param>
    public SongClock(double startDspTime, double offsetSec)
    {
        _startDspTime = startDspTime;
        _offsetSec = offsetSec;
    }

    /// <summary>Call once per frame. Returns current song position in seconds.</summary>
    public double Tick(double unscaledTime)
    {
        double dspSongTime = AudioSettings.dspTime - _startDspTime - _offsetSec;
        double delta = unscaledTime - _lastUnscaledTime;
        _lastUnscaledTime = unscaledTime;

        _smoothedTime += delta;
        // Correct 10% of the gap per frame; snap if badly diverged (e.g. app resume).
        double gap = dspSongTime - _smoothedTime;
        _smoothedTime += Math.Abs(gap) > 0.05 ? gap : gap * 0.1;
        return _smoothedTime;
    }
}
```

### 4.4 Android latency mitigations

- **Project Settings → Audio → DSP Buffer Size = "Best latency."** The single biggest lever on Android; the default buffer adds noticeable output delay.
- Wired/built-in speakers only during play — Bluetooth audio adds 100 ms+ and varies; detect and warn if feasible.
- **Calibration screen (non-negotiable):** play a metronome, user taps ~16 times, store the average `(tapTime − beatTime)` as `inputOffsetSec`, applied to all judgments. This absorbs the device's combined audio-output + touch-input latency, which cannot be computed, only measured.

### 4.5 Input

Use the Input System's `EnhancedTouch` API. Track `touch.touchId` per finger (dense patterns need 4-finger chords) and timestamp against the song clock in the same frame the touch begins.

### 4.6 Judgment windows (tunable constants)

| Grade | Window |
|---|---|
| JUST | ±33 ms |
| GREAT | ±66 ms |
| GOOD | ±100 ms |
| MISS | beyond, or note passes unhit |

Scoring: achievement-rate style (JUST 100% / GREAT 70% / GOOD 40%), scaled to a fixed total per chart.

---

## 5. Code architecture within the existing Unity project

Keep the core pure so it is unit-testable and engine-portable:

```
Assets/
├── Scripts/
│   ├── Core/          # pure C#, NO UnityEngine gameplay deps:
│   │   ├── ChartParser.cs      # JSON → Chart model, validation
│   │   ├── TimingConverter.cs  # tick → seconds via BPM map
│   │   ├── Judge.cs            # windows, note matching, grading
│   │   └── Score.cs
│   ├── Audio/         # SongClock, scheduled playback, calibration
│   ├── Gameplay/      # MonoBehaviours: note spawning, judgment line, touch routing
│   └── UI/            # placeholder-only until M1 complete
├── Tests/EditMode/    # TimingConverter, Judge, ChartParser round-trip tests
└── (charts + audio live in persistentDataPath at runtime, see §3.6)
```

Rule for agents: `Core/` classes must not reference `MonoBehaviour`, `GameObject`, or scene state. `TimingConverter` and `Judge` get EditMode tests before gameplay code consumes them — BPM-change math and window-edge cases are exactly where silent sync bugs hide.

---

## 6. Risks

| Risk | Mitigation |
|---|---|
| Android touch/audio latency too high even with Best-latency DSP | M0 measures real deltas on the actual Galaxy Tab before further investment |
| Multi-touch ghosting on dense patterns | Per-`touchId` tracking; test 4-finger chords in M1 |
| dspTime jitter causing visual stutter | SongClock smoothing (§4.3) |
| Scope creep into visuals/themes | Locked decision: placeholders until M1 is playable |
| Ripped Konami content entering the repo | Hard IP rule (top of doc); original/CC content only |

---

## 8. Prior art: REFLET MUSE + RMSCreator (c. 2014)

A fan-made Windows Reflec Beat simulator (REFLET MUSE) and chart editor
(RMSCreator) by a Japanese developer (gdlife.sakura.ne.jp), discussed on the
Zenius-I-vanisher forums (threadid 7176). Lessons this design incorporates:

- **Sync was its weakest point.** It had no explicit offset field — audio was
  aligned by choosing where in the chart the music starts — and even experienced
  charters reported being unable to sync charts properly. Hence our explicit
  `audioOffsetMs` (§3.1) and mandatory calibration (§4.4).
- **No in-editor timing preview.** Charts could only be timing-checked by
  loading them into the simulator. Hence the M3 preview requirement (§2).
- **Loose judgment windows** relative to the arcade. Hence windows as tunable
  named constants (§4.6), so they can be tightened against feel-testing.
- **Autoplay via config files** proved valuable to its community for chart
  showcases and verification. Hence the M2 autoplay mode (§2).

We are not importing its chart format or code; it is referenced for lessons
only. A community-format importer could be reconsidered post-M3 if demand exists.
