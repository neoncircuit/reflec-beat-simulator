# Todo

## M0 — Calibration spike (in progress)

- [x] Initialize git repo and commit legacy baseline
- [x] Add Input System / Test Framework packages and project settings
- [x] Create `Core/Constants`
- [x] Create `Audio/SongClock`, `Audio/Metronome`, `Audio/AudioPlayback`
- [x] Write `SongClock` EditMode tests
- [x] Create calibration scripts (`CalibrationController`, `CalibrationUI`, `CalibrationStore`, `CalibrationBeatMarker`)
- [ ] Wire `CalibrationScene` in Unity (camera, AudioSource, UI, beat marker)
- [ ] Verify metronome sync in Editor
- [ ] Build to Galaxy Tab and measure tap deltas

## M1 — Playable baseline

- [ ] Core chart model (`Note`, `Chart`, `TimingConverter`, `Judge`, `Score`)
- [ ] EditMode tests for `TimingConverter`, `Judge`, `Score`
- [ ] `Gameplay/NoteObject`, `PlayfieldRenderer`, `TouchInput`, `GameController`
- [ ] `GameplayScene`, `TapNote` prefab, HUD / results UI
- [ ] Delete old prototype scripts (`NoteData`, `Note`, `TargetZone`, `UIManager`, old `GameController`)
- [ ] Update README and architecture docs

## M2 — Chart support

- [ ] `ChartParser` and `RbChartException`
- [ ] `ContentBootstrapper` to copy demo chart/audio on first launch
- [ ] All note types (long, vertical, top, bigbang)
- [ ] `ChartSelectScene` and persistent-data loading
- [ ] `ChartParser` EditMode tests

## Later / out of scope until M2 is done

- M3 authoring workflow
- Visual polish / themes
- iOS build support
- Online / versus features
