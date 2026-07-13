# Tech Stack

## Decision

The prototype is built with **Unity 2022.3 LTS** using **C#**.

## Why Unity?

| Requirement | How Unity helps |
|-------------|-----------------|
| Android-first, iOS later | Cross-platform builds from one project. |
| Touch-based rhythm game | Built-in multi-touch input and 2D tooling. |
| Audio/visual sync | `AudioSettings.dspTime` and `AudioSource.PlayScheduled` enable sample-accurate timing. |
| Rapid prototyping | Large asset store, editor tooling, and huge community. |
| Tablet and phone | Canvas and camera systems adapt to different screen sizes. |

## Alternatives considered

- **Godot**: Excellent open-source choice. Would be the alternative if avoiding proprietary tooling.
- **Native Android (Kotlin)**: Good for Android-only, but would require a full rewrite for iOS.
- **C/C++ from scratch**: Too slow for a solo prototype; unnecessary for this level of performance.
- **React Native / Flutter**: Not game engines; audio sync and animation would be painful for a rhythm game.

## Language

Gameplay code is written in **C#**. It is easier to learn and debug than C/C++ for this use case, and it is the native language of Unity.

## Target platforms

1. Android (primary)
2. iOS (future)
3. Windows/macOS/Linux Editor for development and testing
