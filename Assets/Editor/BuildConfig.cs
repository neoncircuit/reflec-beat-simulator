#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ReflecBeatSimulator.EditorTools
{
    /// <summary>
    /// Applies and enforces the locked M8 Android build settings. Run manually via
    /// <b>Reflec Beat/Apply M8 Build Settings</b>; <see cref="AndroidBuild"/> calls
    /// <see cref="Apply"/> and <see cref="AssertLocked"/> before every build so the
    /// settings cannot silently drift.
    /// </summary>
    public static class BuildConfig
    {
        /// <summary>Locked Android application identifier (package name).</summary>
        public const string ApplicationIdentifier = "com.neoncircuit.reflecbeat";

        /// <summary>Locked human-readable version string.</summary>
        public const string BundleVersion = "0.8.0";

        /// <summary>Locked Android versionCode. Bump manually per released APK.</summary>
        public const int BundleVersionCode = 8;

        private const string CompanyName = "neoncircuit";
        private const string ProductName = "Reflec Beat Simulator";
        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";
        private const string AudioManagerPath = "ProjectSettings/AudioManager.asset";

        // AudioManager m_DSPBufferSize enum: 0 = Default, 1 = Best latency.
        private const int RequiredDspBufferSize = 1;

        // No PlayerSettings API exists for these; they are locked in the YAML and
        // verified by AssertLocked: AndroidEnableSustainedPerformanceMode: 1.

        /// <summary>Applies every locked build setting and saves the project assets.</summary>
        [MenuItem("Reflec Beat/Apply M8 Build Settings")]
        public static void Apply()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = BundleVersion;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationIdentifier);

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = BundleVersionCode;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            Debug.Log($"BuildConfig: applied M8 settings ({ApplicationIdentifier}, IL2CPP/ARM64, " +
                      $"v{BundleVersion} code {BundleVersionCode}, LandscapeLeft).");
        }

        /// <summary>
        /// Throws <see cref="BuildFailedException"/> if any locked setting has drifted
        /// from the values applied by <see cref="Apply"/>.
        /// </summary>
        public static void AssertLocked()
        {
            Require(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == ApplicationIdentifier,
                $"application identifier must be {ApplicationIdentifier}");
            Require(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP,
                "scripting backend must be IL2CPP");
            Require(PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64,
                "target architectures must be ARM64 only");
            Require(PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel22,
                "min SDK must be 22");
            Require(PlayerSettings.Android.bundleVersionCode == BundleVersionCode,
                $"bundleVersionCode must be {BundleVersionCode}");
            Require(PlayerSettings.bundleVersion == BundleVersion,
                $"bundleVersion must be {BundleVersion}");
            Require(PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft,
                "orientation must be locked to LandscapeLeft");
            Require(ReadYamlInt(ProjectSettingsPath, "AndroidEnableSustainedPerformanceMode") == 1,
                "sustained performance mode must be enabled (ProjectSettings.asset)");
            Require(ReadYamlInt(AudioManagerPath, "m_DSPBufferSize") == RequiredDspBufferSize,
                "DSP buffer size must be Best latency (AudioManager m_DSPBufferSize: 1)");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new BuildFailedException($"BuildConfig: locked setting violated — {message}. " +
                                               "Run 'Reflec Beat/Apply M8 Build Settings' and rebuild.");
        }

        private static int ReadYamlInt(string path, string field)
        {
            if (!File.Exists(path))
                return -1;

            string prefix = field + ":";
            foreach (string line in File.ReadLines(path))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith(prefix) &&
                    int.TryParse(trimmed.Substring(prefix.Length).Trim(), out int value))
                {
                    return value;
                }
            }

            return -1;
        }
    }
}
#endif
