# PRODUCT.md — Reflec Beat Simulator

## What this is

A fan-made rhythm game for Android tablets, inspired by Konami's arcade game
Reflec Beat (2010–2020). The original was discontinued and its online services
shut down; this project exists to make that style of gameplay playable again on
a consumer touchscreen device.

## Who it's for

- Primary: the project owner, playing on a Samsung Galaxy Tab.
- Secondary: rhythm game fans who own the APK and want to author their own charts.

## The core experience

Music plays. Objects glide down the screen toward a judgment line. The player
taps them the instant they cross it. Accuracy is graded (JUST / GREAT / GOOD /
MISS) and the run ends with a score. The entire product lives or dies on one
feeling: **taps land exactly on the beat.** If sync feels off, nothing else matters.

## Feature roadmap (in strict order)

1. **M0 — Calibration spike:** metronome plays, player taps along, app reports
   timing deltas. Proves the device is viable.
2. **M1 — Playable baseline:** one built-in song, tap notes only, grading, score
   screen. Placeholder visuals are fine.
3. **M2 — Chart support:** load user charts (JSON) and audio from device storage;
   all note types (long, vertical, top, bigbang); BPM changes.
4. **M3 — Authoring:** a workflow for creating charts (editor or documented
   hand-authoring).
5. **M4 — Single-player vs CPU rival:** optional `rival`/`gold` notes, a
   deterministic CPU rival, Just Reflect gauge, HP bars, and duel outcome. Local
   two-player and online play remain out of scope.
6. **M5 — Game feel & visual clarity pass:** resize notes, readable wall-bounce
   motion, calibration nudging, sync-trust harness, object pooling, approach-time
   / rival-visibility options, and improved hit/results feedback.
7. **M6 — Persistence & session flow:** save gameplay settings and high scores,
   surface best results in chart select, and add retry/back navigation from the
   results screen. Prepares the simulator for repeated play on the target device.
8. **M7 — Visual feel & transitions:** a first polish pass on the default look.
   Redesign the chart-select screen, improve note visibility and hit feedback,
   and add smooth transitions between scenes and UI elements. No theming or
   skin system yet — one default presentation only.
9. **M8 — Device verification & build hardening:** validate calibration and
   audio-sync feel on the actual Galaxy Tab, lock Android build settings (DSP
   buffer size, orientation, package name), and establish a reproducible
   signed-APK build step.
10. **M9 — Documentation & authoring workflow:** improve the chart-authoring
    guide and in-editor preview workflow, add demo/tutorial charts, and finalize
    onboarding docs so a stranger can build, author, and play a chart from this
    repository alone.

## Explicit non-features (do not build)

- Local two-player or online play (M4 is single-player versus a CPU rival only)
- Accounts, profiles, unlocks, or any progression system
- Themes, skins, or visual polish before M1 is complete
- Any converter or importer for Konami's proprietary data

## Content & IP stance

This is a clean-room homage. Konami is credited as the original designers in the
README; credit is not a license. The product ships only original or freely
licensed music, charts, and art. Users may load their own local content.

## Success criteria

- After calibration, a mid-level player cannot perceive audio/judgment offset.
- A hand-authored chart plays start-to-finish in sync on the target tablet.
- A stranger can author and play a new chart using only the docs in this repo.
