# Device Verification — Galaxy Tab Checklist (M8)

**When to run this:** after the build already feels right in the **Unity Editor**
(Play Mode + touch simulation; optional Editor Android/emulator smoke if needed).
Do **not** use the Galaxy Tab for daily iteration — avoid reinstalling every
UI or chart change. Treat the Tab as a **late / final certification gate** for
things only hardware can prove (real audio latency, multi-touch).

Build hardening (player settings, signed APK pipeline) can stay done ahead of
time; the checklist below is the on-device half of M8.

Run this on the physical Samsung Galaxy Tab after building the signed APK
(`docs/build-and-sign.md`). This closes out tasks #8 and #29 — the two items
that cannot be verified without hardware.

## 0. Install

```sh
adb install -r Build/Android/reflec-beat-simulator.apk
```

(Or copy the APK to the tablet and sideload.) Launch "Reflec Beat Simulator".

## 1. Smoke checks (2 minutes)

- [ ] App launches directly into landscape and **stays in landscape** when the
      tablet is rotated (orientation is locked to LandscapeLeft).
- [ ] Chart select loads; all bundled charts appear; high-score labels show.
- [ ] Start a chart: notes scroll, music plays, judgments register.
- [ ] Screen does not dim or sleep mid-song (`Screen.sleepTimeout = NeverSleep`).
- [ ] Frame pacing looks smooth; no visible hitching when many notes are
      on screen (targetFrameRate 60, vSync off, sustained performance mode on).

## 2. Calibration (metronome tap test)

In **CalibrationScene**:

1. Use **wired headphones or the built-in speakers** — never Bluetooth
   (Bluetooth adds 100 ms+ of audio latency the offset cannot reliably absorb;
   see DESIGN.md §4.4).
2. Tap along with the metronome until the reading stabilizes.
3. Confirm the stored offset: `CalibrationStore` writes
   `{ "inputOffsetSec": <double, seconds> }` as JSON under
   `Application.persistentDataPath`. Check via logcat or the calibration UI
   readout.
   - Expect a small positive value (tens of ms) on speakers, smaller on wired.
   - Re-run and confirm the value is **stable across runs** (±10 ms). Large
     drift means background load — close other apps and re-test.
4. Play a chart immediately after calibrating and judge whether taps *feel*
   centered — this is the primary audio-sync acceptance test (task #8).

## 3. Autoplay sync-trust harness

1. Enable **autoplay** in chart select and run a full chart.
2. The results judgment counts should be essentially all top-window hits
   (Marvelous/Perfect). Any systematic skew toward early or late indicates a
   clock or offset problem — investigate before trusting manual feel tests.

## 4. Input & gameplay feel

- [ ] Single taps, long notes (hold + release timing), vertical/top/bigbang
      notes all judge correctly.
- [ ] **4-finger chords**: simultaneous taps on 4 targets all register (no
      dropped touches). If drops occur, check the tablet's touch settings for
      gesture interception (e.g. multi-finger system gestures) and disable them.
- [ ] Rival/duel mode runs; rival notes render on the rival side; the
      Hide-rival toggle works.
- [ ] Hit feedback (flash/scale) and results screen transitions feel right.

## 5. Regression sanity

- [ ] Settings persist across an app restart (approach time, hide-rival).
- [ ] High scores update after a clear and appear in chart select.
- [ ] Retry/Back navigation from results works.

## Recording results

Note the final `inputOffsetSec`, the device model/Android version, and any
feel issues in the task folder for this check (`tasks/active/<task-slug>/notes.md`),
then mark the device items in the sprint task list as done.
