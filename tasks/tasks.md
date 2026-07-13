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
| 7 | M0: Wire `CalibrationScene` in Unity | Pending | Needs Unity Editor to place objects/prefabs. |
| 8 | M0: Verify metronome sync on device | Pending | Requires Galaxy Tab build. |
| 9 | M1: Core chart model + timing/judge/score | Pending | Pure C#, EditMode tests first. |
| 10 | M1: Gameplay loop + scene | Pending | `NoteObject`, `PlayfieldRenderer`, `TouchInput`, `GameController`. |
| 11 | M1: Delete old prototype scripts | Pending | After replacements compile. |
| 12 | M2: JSON chart loader + content bootstrapper | Pending | Persistent data path, copy from StreamingAssets. |
| 13 | M2: All note types + chart select | Pending | Long, vertical, top, bigbang. |

## Legend

- **Completed**: Done and checked in.
- **In Progress**: Currently being worked on.
- **Pending**: Not started or blocked on Unity/device.
- **Blocked**: Waiting on something else.
