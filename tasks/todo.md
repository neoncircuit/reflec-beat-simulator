# Todo

## M0 — Calibration spike (completed)

- [x] Initialize git repo and commit legacy baseline
- [x] Add Input System / Test Framework packages and project settings
- [x] Create `Core/Constants`
- [x] Create `Audio/SongClock`, `Audio/Metronome`, `Audio/AudioPlayback`
- [x] Write `SongClock` EditMode tests
- [x] Create calibration scripts (`CalibrationController`, `CalibrationUI`, `CalibrationStore`, `CalibrationBeatMarker`)
- [x] Wire `CalibrationScene` in Unity (camera, AudioSource, UI, beat marker)
- [ ] Verify metronome sync in Editor
- [ ] Build to Galaxy Tab and measure tap deltas

## M1 — Playable baseline (completed)

- [x] Core chart model (`Note`, `Chart`, `TimingConverter`, `Judge`, `Score`)
- [x] EditMode tests for `TimingConverter`, `Judge`, `Score`
- [x] `Gameplay/NoteObject`, `PlayfieldRenderer`, `TouchInput`, `GameController`
- [x] `GameplayScene`, note prefabs, HUD / results UI
- [x] Delete old prototype scripts (`NoteData`, `Note`, `TargetZone`, `UIManager`, old `GameController`)
- [x] Update README and architecture docs

## M2 — Chart support (completed)

- [x] `ChartParser` and `RbChartException`
- [x] `ContentBootstrapper` to copy demo chart/audio on first launch
- [x] All note types (`long`, `vertical`, `top`, `bigbang`)
- [x] `ChartSelectScene` and persistent-data loading
- [x] `ChartParser` EditMode tests
- [x] Autoplay mode for sync verification

## M3 — Authoring (completed)

- [x] Decide editor vs documented hand-authoring workflow
- [x] Author can preview timing in place (play section with audio)
- [x] Export valid `*.rbchart.json`
- [x] EditMode tests for any authoring helpers

## Later / out of scope until M3 is done

- [x] CI/CD scaffolding (linting, security scan, dependabot)
- Visual polish / themes
- iOS build support
- Online / versus features

## M4 — Single-player vs CPU rival (completed)

- [x] Add duel data model (`Note.IsRival`, `Note.IsGold`, `Chart.Rival`, `RivalConfig`)
- [x] Add duel constants (`MaxGauge`, gauge gains, damage values, reflect damage)
- [x] Parse optional `rival`/`gold` note flags and top-level `rival` config
- [x] Write optional fields only when non-default
- [x] Implement `DuelState` (HP, gauge, reflect, outcome)
- [x] Implement deterministic `CpuRival`
- [x] Write EditMode tests for `DuelState` and `CpuRival`
- [x] Wire duel into `GameController`
- [x] Render rival notes (reverse movement, red/gold tints) in `NoteObject`/`PlayfieldRenderer`
- [x] Add `DuelUI` HUD and update `ResultsUI` for duel outcomes
- [x] Build `DuelUI` into `GameplayScene` via `M1SceneBuilder`
- [x] Update docs (`DESIGN.md`, `docs/authoring.md`, `docs/architecture.md`, `README.md`, `PRODUCT.md`) and task trackers
- [x] Run all EditMode tests in Unity

## M5 — Game feel & visual clarity pass (completed)

- [x] Resize notes and tighten playfield scale
- [x] Make wall-bounce readable and less uniform
- [x] Sync-trust harness (autoplay timing report)
- [x] Calibration discoverability and ±5 ms nudge
- [x] Note object pooling
- [x] Approach-time / scroll-speed option
- [x] Rival object visibility toggle
- [x] Hit feedback polish (colored flashes, pop effect, larger judgment text)
- [x] Results clarity (letter grade, colored duel banner)

## M6 — Persistence & session flow (completed)

- [x] Add `SettingsStore` for gameplay options
- [x] Add `HighScoreStore` for per-chart best results
- [x] Restore settings in `ChartSelectUI`
- [x] Save settings on chart selection
- [x] Show high-score grade/rate in chart-select labels
- [x] Add Retry / Back buttons to `ResultsUI`
- [x] Update docs and task trackers
