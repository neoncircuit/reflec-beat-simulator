using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReflecBeatSimulator.UI
{
    /// <summary>
    /// Singleton that fades the screen to black between scene loads.
    /// Created by the scene builder in every scene; the first instance survives
    /// scene changes and later duplicates destroy themselves.
    /// </summary>
    public sealed class TransitionManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _overlay;

        private static TransitionManager _instance;
        private bool _isFading;

        private const float DefaultDuration = 0.25f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureOverlay();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>Fades out and loads the named scene.</summary>
        public static void LoadScene(string sceneName, float duration = DefaultDuration)
        {
            if (_instance == null)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            _instance.StartCoroutine(_instance.FadeAndLoad(sceneName, duration));
        }

        /// <summary>Fades the overlay in (used for scripted transitions).</summary>
        public static Coroutine FadeIn(float duration = DefaultDuration)
        {
            if (_instance == null)
                return null;

            return _instance.StartCoroutine(_instance.Fade(1f, duration));
        }

        /// <summary>Fades the overlay out.</summary>
        public static Coroutine FadeOut(float duration = DefaultDuration)
        {
            if (_instance == null)
                return null;

            return _instance.StartCoroutine(_instance.Fade(0f, duration));
        }

        private IEnumerator FadeAndLoad(string sceneName, float duration)
        {
            if (_isFading)
                yield break;

            _isFading = true;
            _canvasGroup.blocksRaycasts = true;
            yield return StartCoroutine(Fade(1f, duration));

            SceneManager.LoadScene(sceneName);

            // After scene loads, fade back out. The new scene's TransitionManager
            // will be a duplicate and destroyed, so this instance stays alive.
            yield return null;
            _canvasGroup.alpha = 1f;
            yield return StartCoroutine(Fade(0f, duration));
            _canvasGroup.blocksRaycasts = false;
            _isFading = false;
        }

        private IEnumerator Fade(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = duration > 0f ? elapsed / duration : 1f;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        private void EnsureOverlay()
        {
            if (_canvasGroup != null)
                return;

            var canvasGo = new GameObject("TransitionCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var overlayGo = new GameObject("Overlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);

            var rect = overlayGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _overlay = overlayGo.AddComponent<Image>();
            _overlay.color = Color.black;

            _canvasGroup = overlayGo.AddComponent<CanvasGroup>();
        }
    }
}
