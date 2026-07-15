using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Copies bundled demo charts from StreamingAssets into persistent data on first launch.</summary>
    public sealed class ContentBootstrapper : MonoBehaviour
    {
        private const string VersionKey = "ReflecBeatBootstrapVersion";
        private const int CurrentVersion = 1;

        /// <summary>Raised once when the bootstrap coroutine finishes (or immediately if no bootstrap is needed).</summary>
        public static event System.Action OnBootstrapComplete;

        /// <summary>Raised once if the bootstrap coroutine fails to copy one or more files.</summary>
        public static event System.Action OnBootstrapFailed;

        /// <summary>True once bootstrap has finished successfully or was skipped because the marker was up to date.</summary>
        public static bool IsComplete { get; private set; }

        // Relative paths inside StreamingAssets/charts/ that must be copied.
        private static readonly string[] DemoFiles =
        {
            "demo/demo.rbchart.json",
            "demo/PlaceholderBeat.wav"
        };

        private void Awake()
        {
            Debug.Log($"ContentBootstrapper: charts root = {ChartRepository.ChartsRoot}");
            if (NeedsBootstrap())
            {
                Debug.Log("ContentBootstrapper: starting demo copy from StreamingAssets.");
                StartCoroutine(BootstrapCoroutine());
            }
            else
            {
                Debug.Log("ContentBootstrapper: demo files already present; skipping copy.");
                IsComplete = true;
                OnBootstrapComplete?.Invoke();
            }
        }

        private static bool NeedsBootstrap()
        {
            if (PlayerPrefs.GetInt(VersionKey, 0) < CurrentVersion)
                return true;

            // Re-run if any bundled demo file is missing (e.g. user cleared persistent data).
            string destRoot = ChartRepository.ChartsRoot;
            foreach (string relativePath in DemoFiles)
            {
                string destPath = Path.Combine(destRoot, relativePath);
                if (!File.Exists(destPath))
                    return true;
            }

            return false;
        }

        private IEnumerator BootstrapCoroutine()
        {
            string sourceRoot = Application.streamingAssetsPath;
            string destRoot = ChartRepository.ChartsRoot;
            bool success = true;

            foreach (string relativePath in DemoFiles)
            {
                string sourceUri = sourceRoot + "/charts/" + relativePath;
                string destPath = Path.Combine(destRoot, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                if (Application.platform == RuntimePlatform.Android)
                {
                    var operation = new AndroidCopyOperation(sourceUri, destPath);
                    while (operation.MoveNext())
                        yield return operation.Current;

                    if (!operation.Success)
                        success = false;
                }
                else
                {
                    if (!CopyFromStreamingAssetsLocal(sourceUri, destPath))
                        success = false;
                }
            }

            if (!success)
            {
                Debug.LogError($"ContentBootstrapper: One or more demo files failed to copy to {destRoot}.");
                OnBootstrapFailed?.Invoke();
                yield break;
            }

            PlayerPrefs.SetInt(VersionKey, CurrentVersion);
            PlayerPrefs.Save();

            Debug.Log($"ContentBootstrapper: demo copied to {destRoot}.");
            IsComplete = true;
            OnBootstrapComplete?.Invoke();
        }

        private class AndroidCopyOperation : IEnumerator
        {
            private readonly string _sourceUri;
            private readonly string _destPath;
            private IEnumerator _inner;

            public bool Success { get; private set; } = true;
            public object Current => _inner?.Current;

            public AndroidCopyOperation(string sourceUri, string destPath)
            {
                _sourceUri = sourceUri;
                _destPath = destPath;
                _inner = Run().GetEnumerator();
            }

            public bool MoveNext()
            {
                return _inner.MoveNext();
            }

            public void Reset()
            {
                _inner.Reset();
            }

            private IEnumerable Run()
            {
                using var request = UnityWebRequest.Get(_sourceUri);
                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool failed = request.result != UnityWebRequest.Result.Success;
#else
                bool failed = request.isNetworkError || request.isHttpError;
#endif
                if (failed)
                {
                    Debug.LogError($"Bootstrap failed for {_sourceUri}: {request.error}");
                    Success = false;
                    yield break;
                }

                File.WriteAllBytes(_destPath, request.downloadHandler.data);
            }
        }

        private static bool CopyFromStreamingAssetsLocal(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, overwrite: true);
                return true;
            }

            Debug.LogWarning($"Bootstrap: missing bundled file {sourcePath}");
            return false;
        }
    }
}
