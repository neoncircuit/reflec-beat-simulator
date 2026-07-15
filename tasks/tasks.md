# Project Tasks

A running list of work items for the Reflec Beat Simulator.

| ID | Task | Status | Notes |
|----|------|--------|-------|
| 1 | Baseline repo in git | Completed | Legacy prototype + canonical docs committed as root baseline. |
| 2 | M0: Unity packages + project settings | Completed | Input System, Test Framework, landscape, Best-latency DSP. |
| 3 | M0: Core constants | Completed | `Constants.cs` with PPQ, judgment windows, approach time. |
| 4 | M0: Audio clock (`SongClock`, `Metronome`, `AudioPlayback`) | Completed | DSP-clock scheduling + smoothing. |
| 5 | M0: `SongClock` EditMode tests | Completed | Fake clock seam, smoothing, snap behavior. |
| 6 | M0: Calibration scripts | Completed | Controller, UI, store (JSON), beat marker. |
| 7 | M0: Wire `CalibrationScene` in Unity | Completed | Scene built via `Reflec Beat / Build M1 Scenes`. |
| 8 | M0: Verify metronome sync on device | Pending | Requires Galaxy Tab build. |
| 9 | M1: Core chart model + timing/judge/score | Completed | Pure C#, EditMode tests first. |
| 10 | M1: Gameplay loop + scene | Completed | `NoteObject`, `PlayfieldRenderer`, `TouchInput`, `GameController`. Scene built via editor tool. |
| 11 | M1: Delete old prototype scripts | Completed | `NoteData`, `Note`, `TargetZone`, `UIManager`, old `GameController` removed. |
| 12 | M2: JSON chart loader + content bootstrapper | Completed | `ChartParser`, `ChartLoader`, `AudioLoader`, `ContentBootstrapper`, `ChartRepository`. |
| 13 | M2: All note types + chart select | Completed | `long`, `vertical`, `top`, `bigbang`; `ChartSelectScene`, `ChartSelectController`, `ChartSelectUI`; autoplay mode. |
| 14 | M2: EditMode tests for parser + judgment | Completed | `ChartParserTests`, `JudgeTests` cover all note types and long release windows. |
| 15 | M3: Authoring workflow | Completed | Documented hand-authoring workflow via `Reflec Beat / Chart Preview`. |
| 16 | M3: Preview timing in place | Completed | `ChartPreviewWindow` launches `GameplayScene` with a `[start, end)` range and audio offset. |
| 17 | CI/CD scaffolding | Completed | Super-linter, TruffleHog, DevSkim, Dependabot. |
| 18 | M4: Duel data model + constants | Completed | `Note.IsRival`, `Note.IsGold`, `Chart.Rival`, `Constants` duel values. |
| 19 | M4: Parse/write rival/gold fields | Completed | `ChartParser` and `ChartWriter` support optional note flags and top-level `rival` config. |
| 20 | M4: Core duel logic | Completed | `DuelState`, `CpuRival`, `RivalConfig` with EditMode tests. |
| 21 | M4: Wire duel into gameplay | Completed | `GameController`, `NoteObject`, `PlayfieldRenderer`, `DuelUI`, `ResultsUI`, `M1SceneBuilder`. |
| 22 | M4: Documentation + task updates | Completed | `DESIGN.md`, `docs/authoring.md`, `docs/architecture.md`, `README.md`, `PRODUCT.md`, `tasks/*.md`. |
| 23 | M5: Game feel + visual clarity pass | Completed | Resize notes, bounce readability, calibration nudge, sync harness, pooling, approach-time/rival-visibility options, hit/results feedback. |
| 24 | M6: Settings persistence + high scores | Completed | `SettingsStore`, `HighScoreStore`, settings restore/save, high-score labels, results navigation. |
| 23 | M4: Run EditMode tests in Unity | Completed | All EditMode tests pass, including `DuelStateTests` and `CpuRivalTests`. |

## Legend

- **Completed**: Done and checked in.
- **In Progress**: Currently being worked on.
- **Pending**: Not started or blocked on Unity/device.
- **Blocked**: Waiting on something else.
