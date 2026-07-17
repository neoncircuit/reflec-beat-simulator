# Build & Sign — Android APK (M8)

How to produce a reproducible, signed APK of the Reflec Beat Simulator from a
clean checkout. The whole pipeline is headless: WSL2 drives the Windows Unity
editor in batch mode.

## Locked build settings

`Assets/Editor/BuildConfig.cs` owns these values. Every build calls
`BuildConfig.Apply()` and then `BuildConfig.AssertLocked()`, which fails the
build (`BuildFailedException`) if any setting has drifted:

| Setting | Value |
|---|---|
| Package name | `com.neoncircuit.reflecbeat` |
| Version | `0.8.0` (versionCode `8`) |
| Scripting backend | IL2CPP (required for ARM64) |
| Architectures | ARM64 only |
| Min SDK | 22 (Android 5.1) |
| Target SDK | Automatic (highest installed) |
| Orientation | LandscapeLeft only |
| DSP buffer size | Best latency (`AudioManager.asset` → `m_DSPBufferSize: 1`) |
| Sustained performance mode | On (`ProjectSettings.asset` → `AndroidEnableSustainedPerformanceMode: 1`) |

To apply them by hand inside the editor: **Reflec Beat → Apply M8 Build Settings**.

## One-time keystore setup

The signing key is **never committed** (`*.keystore` is gitignored). Keep it
outside the repo:

```sh
keytool -genkeypair -v \
  -keystore /mnt/d/reflec-beat-simulator.keystore \
  -alias reflecbeat -keyalg RSA -keysize 2048 -validity 10000 \
  -storepass '<your-store-password>' -keypass '<your-key-password>' \
  -dname "CN=neoncircuit dev, OU=dev, O=neoncircuit, C=US"
```

(Unity's bundled `keytool.exe` under
`Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/` works too.)

Back the keystore up somewhere safe — an app can only ever be updated by an APK
signed with the same key.

## Environment variables

The build reads four env vars and **fails hard** if any is missing (there is no
unsigned fallback — M8's deliverable is a signed APK):

```sh
export RB_KEYSTORE_PATH=/mnt/d/reflec-beat-simulator.keystore
export RB_KEYSTORE_PASS='<your-store-password>'
export RB_KEYALIAS_NAME=reflecbeat
export RB_KEYALIAS_PASS='<your-key-password>'
```

Or keep them in a gitignored env file **outside** the repo and source it:

```sh
source /mnt/d/reflec-beat-keystore.env
```

Do not commit these exports. `scripts/build-android.sh` forwards them to the
Windows Unity process via `WSLENV`; WSL-style paths (`/mnt/d/...`) are converted
to Windows paths inside `AndroidBuild.cs`.

## Building

```sh
./scripts/build-android.sh
```

The script runs Unity batch mode with
`-executeMethod ReflecBeatSimulator.EditorTools.AndroidBuild.Build`, which:

1. Regenerates the scenes (`M1SceneBuilder.Build()`).
2. Applies and asserts the locked settings above.
3. Configures the keystore from the env vars.
4. Builds `Build/Android/reflec-beat-simulator.apk` (gitignored).
5. Fails the build unless the APK exists and the result is `Succeeded`.

`UNITY_EXE` overrides the Unity editor path if the install moves.

### Version bumps

Before each release-worthy APK, bump **both** in `BuildConfig.cs`:
`BundleVersion` (e.g. `"0.9.0"`) and `BundleVersionCode` (integer, must
strictly increase for any store/sideload update flow).

## Verifying the APK

```sh
apksigner verify --print-certs Build/Android/reflec-beat-simulator.apk
```

(`apksigner` ships with the Android SDK build-tools, e.g. under Unity's
`AndroidPlayer/SDK/build-tools/*/apksigner.bat`.) Expect one signer with the
`CN=neoncircuit dev` subject.

Check for unexpected permissions:

```sh
unzip -p Build/Android/reflec-beat-simulator.apk AndroidManifest.xml | strings | grep -i permission
```

The game needs no runtime permissions; the manifest should only carry what
Unity injects by default.

## Running the tests

```sh
./scripts/run-editmode-tests.sh
```

Produces `TestResults.xml` (gitignored), prints
`total/passed/failed/result`, and exits non-zero unless every EditMode test
passes.

## Device verification

See `docs/device-verification.md` for the on-device checklist (calibration,
audio-sync feel, orientation, frame rate).
