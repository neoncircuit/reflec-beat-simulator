# Reflec Beat Simulator — Documentation

This folder contains project documentation for the Reflec Beat Simulator prototype.

## Documents

- [`preliminary-report.md`](./preliminary-report.md) — What Reflec Beat is, its mechanics, its arcade and mobile version history, and how the simulator maps those ideas.
- [`tech-stack.md`](./tech-stack.md) — Why Unity/C# was chosen and what alternatives were considered.
- [`architecture.md`](./architecture.md) — How the Core, Audio, Gameplay, and UI layers fit together.
- [`authoring.md`](./authoring.md) — How to write and preview `*.rbchart.json` charts in the Unity editor.
- [`build-and-sign.md`](./build-and-sign.md) — Locked Android settings, keystore setup, and the reproducible signed-APK pipeline.
- [`device-verification.md`](./device-verification.md) — On-device Galaxy Tab checklist (calibration, audio-sync feel, orientation, frame rate).

## Project README

For setup instructions and how to run the simulator, see the root [`README.md`](../README.md).

## Prototype status

The focus right now is on a functional prototype, not visual polish. Placeholder shapes and default UI are intentional.

- **M0 — Calibration spike**: complete. The calibration scene measures tap deltas and stores an input offset. Device verification is pending.
- **M1 — Playable baseline**: complete. `GameplayScene` supports tap notes, judgment, scoring, and a results screen.
- **M2 — Chart support**: complete. Charts load from `*.rbchart.json` + audio in `Application.persistentDataPath/charts/`, all note types are implemented, and a chart-select UI with autoplay mode is wired.
- **M3 — Authoring**: complete. Hand-authoring workflow with in-Unity preview; see [`authoring.md`](./authoring.md).
- **M4 — Single-player vs CPU rival**: complete. Optional `rival`/`gold` notes, deterministic CPU rival, Just Reflect gauge, HP bars, and duel outcome.
- **M5 — Game feel & visual clarity pass**: complete. Approach-time / rival-visibility options, object pooling, sync-trust harness, calibration nudge, hit feedback polish.
- **M6 — Persistence & session flow**: complete. Saved gameplay settings, per-chart high scores, Retry/Back navigation.
- **M7 — Visual feel & transitions**: complete. Chart-select redesign, scene/UI transitions, note visibility polish.
- **M8 — Device verification & build hardening**: build half complete (locked settings, signed-APK pipeline, headless tests — see [`build-and-sign.md`](./build-and-sign.md)); on-device verification pending ([`device-verification.md`](./device-verification.md)).

See [`../tasks/todo.md`](../tasks/todo.md) and [`../PRODUCT.md`](../PRODUCT.md) for the full roadmap.
