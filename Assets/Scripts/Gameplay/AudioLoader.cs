using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>Platform-safe audio clip loader. Uses <see cref="UnityWebRequest"/> so Android APK paths work.</summary>
    public static class AudioLoader
    {
        /// <summary>Loads an audio file from an absolute filesystem path into an <see cref="AudioClip"/>.</summary>
        public static async Task<AudioClip> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            string uri = new Uri(path).AbsoluteUri;
            using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN);
            request.SendWebRequest();

            using (cancellationToken.Register(() => request.Abort()))
            {
                while (!request.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

#if UNITY_2020_1_OR_NEWER
            bool failed = request.result != UnityWebRequest.Result.Success;
#else
            bool failed = request.isNetworkError || request.isHttpError;
#endif
            if (failed)
                throw new Exception($"Failed to load audio '{path}': {request.error}");

            return DownloadHandlerAudioClip.GetContent(request);
        }
    }
}
