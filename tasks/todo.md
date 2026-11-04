# Backlog

Work that is not in a sprint. Do not start this work unless the owner tells
you to. Items are in roadmap order. For finished work before sprint-001, see
`archive/`.

## M0 — Calibration spike

- [ ] Verify metronome sync in the Editor
- [ ] Build to the Galaxy Tab and measure tap deltas (same check as the M8
  device items). Blocked: no device (see M8).

## M8 — Device verification & build hardening

These are late gates. Do them after the owner accepts Editor playability.
The checklist is `docs/device-verification.md` (sections 1 to 5).

On 2026-11-04 the owner did not have a test device. So the checks are in two
parts: the checks that do not need a device in hand, and the checks that do.

### Android checks without a device

These checks show that the app works on Android. They cannot show that taps
are on the beat.

- [ ] Make a dev build that has x86_64 and ARM64, for the Android emulator on
  Windows. The release build is ARM64 only, and an ARM-only Unity build is
  not reliable on an x86_64 emulator. Do not change the locked release
  settings (`docs/build-and-sign.md`).
- [ ] On the emulator, do the smoke checks (section 1) and the regression
  checks (section 5).
- [ ] In Samsung Remote Test Lab (free, real Galaxy devices in a browser),
  install the signed release APK on a Galaxy Tab. Do sections 1 and 5, and
  the single-tap checks of section 4. Remote touch is mouse input, so it
  cannot test 4-finger chords or timing. ADB is available through Remote
  Debug Bridge, for logcat and for the `CalibrationStore` file.
- [ ] Optional: add a Firebase Test Lab Game Loop scenario. The app runs a
  chart on autoplay and writes the judgment counts to a log file. Test Lab
  runs it on a physical device and uploads the log. This is the autoplay
  check of section 3 (clock and offset), not a check of the feel. First,
  check which Samsung tablets are in the Test Lab device list.

### Checks that need a device in hand

Blocked: the owner has no device. An emulator or a remote lab cannot measure
audio and touch latency (`AGENTS.md`, guardrail 6). When a device is
available (a Galaxy Tab, or a used Galaxy Tab A), do these checks. If no
device is available, the owner can decide in `PRODUCT.md` to defer them and
continue with M9.

- [ ] Verify calibration tap deltas on the device (section 2)
- [ ] Verify audio-sync feel on the device: calibrate, then play a chart, and
  play a chart on autoplay (sections 2 and 3)
- [ ] Verify 4-finger chords and long-note release timing (section 4)

## M9 — Documentation & authoring workflow

- [ ] Improve `docs/authoring.md` with screenshots and examples
- [ ] Polish the in-editor `Chart Preview` workflow
- [ ] Add demo and tutorial charts
- [ ] Finalize onboarding docs for building and playing

## Maintenance

No open items.

## Later (not in the roadmap)

- iOS build support
- User-selectable theme packs (after the default identity is done)

Local two-player and online play are non-features. See `PRODUCT.md`.
