# AGENTS.md — rules for coding agents in this repo

Read order: `PRODUCT.md` (what & why) → `DESIGN.md` (architecture & specs) →
this file (how to work). This file is canonical for agent behaviour; `CLAUDE.md`
just points here.

## Hard guardrails

1. **No Konami assets, ever.** No ripped audio, charts, art, fonts, or decompiled
   code may be committed, downloaded, or referenced. Original/CC content only.
2. **Stay inside the current milestone** (see PRODUCT.md roadmap). Do not build
   ahead, add features, or start visual polish before M1 is playable.
3. **Timing code uses the audio clock only.** `AudioSettings.dspTime` +
   `PlayScheduled`, via the `SongClock` abstraction. Never `Time.time`,
   `Time.deltaTime` accumulation, coroutines, or `Invoke` for anything
   timing-critical. Any PR that touches timing must say how it was verified.
4. **`Assets/Scripts/Core/` stays pure C#.** No `UnityEngine` gameplay types
   (`MonoBehaviour`, `GameObject`, scene state). Parser, timing conversion,
   judgment, and scoring live here so they can be unit tested.
5. **Tests before consumers.** `TimingConverter`, `Judge`, and `ChartParser`
   changes require EditMode tests (BPM-change math and judgment-window edges are
   where silent sync bugs hide). Run tests before declaring work done.
6. **Latency claims need a device.** Never assert that sync "works" from the
   editor alone; only on-device measurement (M0 harness) counts.

## Conventions

- **C# style:** standard .NET naming (PascalCase types/members, camelCase locals).
  Public members get XML doc comments (`/// <summary>`); use explicit types on
  public APIs — no `var` in public signatures.
- **Charts:** conform exactly to the `*.rbchart.json` spec in DESIGN.md §3.
  Never invent fields; propose spec changes in DESIGN.md first, code second.
- **File loading:** runtime content loads from `Application.persistentDataPath`.
  Do not read `StreamingAssets` with `System.IO` — it fails on Android.
- **Constants:** judgment windows, approach time, and offsets are named constants
  in one place (`Core/`), never magic numbers scattered in MonoBehaviours.
- **Commits:** small and scoped; message states the milestone (e.g. `M1: ...`).

## When unsure

- Spec ambiguity → check DESIGN.md; if still ambiguous, ask the owner rather
  than guessing. Do not silently extend the chart format or judgment rules.
- Conflict between files → PRODUCT.md defines scope, DESIGN.md defines
  architecture, this file defines process. Newer explicit owner instructions
  in the conversation override all three.
