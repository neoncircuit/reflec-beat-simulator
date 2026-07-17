using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Applies device-level app settings once at startup: frame-rate cap, v-sync,
    /// and screen sleep. Present in every scene; the first instance survives scene
    /// loads and later duplicates destroy themselves.
    /// This affects visual frame pacing only — gameplay timing is driven
    /// exclusively by the audio clock (SongClock / AudioSettings.dspTime).
    /// </summary>
    public sealed class ApplicationBootstrapper : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private int _vSyncCount = 0;

        private static ApplicationBootstrapper _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = _vSyncCount;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
