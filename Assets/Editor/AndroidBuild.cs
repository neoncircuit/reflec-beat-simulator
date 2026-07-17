#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ReflecBeatSimulator.EditorTools
{
    /// <summary>
    /// Reproducible signed-APK build entry point (M8). Run via
    /// <b>Reflec Beat/Build Android APK</b> or headless:
    /// <c>-executeMethod ReflecBeatSimulator.EditorTools.AndroidBuild.Build</c>.
    /// Regenerates scenes, applies and asserts the locked build settings, signs
    /// with the keystore from environment variables, and fails the build if any
    /// step cannot be verified. See docs/build-and-sign.md.
    /// </summary>
    public static class AndroidBuild
    {
        /// <summary>APK output path, relative to the project root (gitignored).</summary>
        public const string ApkPath = "Build/Android/reflec-beat-simulator.apk";

        private const string KeystorePathVar = "RB_KEYSTORE_PATH";
        private const string KeystorePassVar = "RB_KEYSTORE_PASS";
        private const string KeyaliasNameVar = "RB_KEYALIAS_NAME";
        private const string KeyaliasPassVar = "RB_KEYALIAS_PASS";

        /// <summary>Builds the signed Android APK end to end.</summary>
        [MenuItem("Reflec Beat/Build Android APK")]
        public static void Build()
        {
            M1SceneBuilder.Build();
            BuildConfig.Apply();
            BuildConfig.AssertLocked();
            ConfigureKeystore();

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new BuildFailedException("AndroidBuild: no enabled scenes in Build Settings.");

            string directory = Path.GetDirectoryName(ApkPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            BuildReport report = BuildPipeline.BuildPlayer(scenes, ApkPath, BuildTarget.Android, BuildOptions.None);

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"AndroidBuild: build failed with result {report.summary.result}.");

            if (!File.Exists(ApkPath))
                throw new BuildFailedException($"AndroidBuild: expected APK missing at {ApkPath}.");

            Debug.Log($"AndroidBuild: signed APK written to {ApkPath} " +
                      $"({new FileInfo(ApkPath).Length / (1024 * 1024)} MB).");
        }

        private static void ConfigureKeystore()
        {
            string path = Environment.GetEnvironmentVariable(KeystorePathVar);
            string storePass = Environment.GetEnvironmentVariable(KeystorePassVar);
            string alias = Environment.GetEnvironmentVariable(KeyaliasNameVar);
            string aliasPass = Environment.GetEnvironmentVariable(KeyaliasPassVar);

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(storePass) ||
                string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(aliasPass))
            {
                throw new BuildFailedException(
                    "AndroidBuild: signing env vars missing. Set RB_KEYSTORE_PATH, RB_KEYSTORE_PASS, " +
                    "RB_KEYALIAS_NAME, and RB_KEYALIAS_PASS. See docs/build-and-sign.md.");
            }

            path = ConvertWslPath(path);
            if (!File.Exists(path))
                throw new BuildFailedException($"AndroidBuild: keystore not found at {path}.");

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = path;
            PlayerSettings.Android.keystorePass = storePass;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPass;
        }

        // The build runs inside Windows Unity, but the wrapper scripts pass paths
        // from WSL2 (/mnt/d/...). Convert so File.Exists and Unity's signer agree.
        private static string ConvertWslPath(string path)
        {
            if (path.Length > 7 && path.StartsWith("/mnt/") && path[6] == '/')
            {
                char drive = char.ToUpperInvariant(path[5]);
                return $"{drive}:/{path.Substring(7)}";
            }

            return path;
        }
    }
}
#endif
