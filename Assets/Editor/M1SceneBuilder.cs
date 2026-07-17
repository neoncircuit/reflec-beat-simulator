#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using ReflecBeatSimulator.Audio;
using ReflecBeatSimulator.Gameplay;
using ReflecBeatSimulator.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReflecBeatSimulator.EditorTools
{
    /// <summary>
    /// One-shot editor utility that builds the M0/M1 scenes and prefabs from the
    /// canonical scripts. Run via <b>Reflec Beat / Build M1 Scenes</b>.
    /// </summary>
    public static class M1SceneBuilder
    {
        private const string ScenesPath = "Assets/Scenes";
        private const string PrefabsPath = "Assets/Prefabs";
        private const string SpritesPath = "Assets/Sprites";
        private const string MaterialsPath = "Assets/Materials";
        private const string AudioClipPath = "Assets/Audio/PlaceholderBeat.wav";
        private const string RingSpritePath = "Assets/Sprites/NoteRing.png";
        private const string RingMaterialPath = "Assets/Materials/NoteRing.mat";

        private static readonly string[] OldPrototypeScripts =
        {
            "Assets/Scripts/NoteData.cs",
            "Assets/Scripts/Note.cs",
            "Assets/Scripts/TargetZone.cs",
            "Assets/Scripts/UIManager.cs",
            "Assets/Scripts/GameController.cs"
        };

        [MenuItem("Reflec Beat/Build M1 Scenes")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("M1SceneBuilder: Cannot build scenes while in Play mode. Exit Play mode and try again.");
                EditorUtility.DisplayDialog("Reflec Beat", "Cannot build scenes while in Play mode.\n\nExit Play mode and run Reflec Beat / Build M1 Scenes again.", "OK");
                return;
            }

            EnsureDirectory(ScenesPath);
            EnsureDirectory(PrefabsPath);
            EnsureDirectory(SpritesPath);
            EnsureDirectory(MaterialsPath);

            Sprite sprite = GetDefaultSprite();
            if (sprite == null)
            {
                Debug.LogError("M1SceneBuilder: Could not load the built-in UI sprite. Aborting.");
                return;
            }

            Sprite ringSprite = EnsureRingSprite();
            if (ringSprite == null)
            {
                Debug.LogError("M1SceneBuilder: Could not create ring sprite. Aborting.");
                return;
            }

            Material ringMaterial = EnsureRingMaterial();
            if (ringMaterial == null)
            {
                Debug.LogError("M1SceneBuilder: Could not create ring material. Aborting.");
                return;
            }

            Font font = GetDefaultFont();
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioClipPath);
            if (clip == null)
            {
                Debug.LogError($"M1SceneBuilder: Could not find audio clip at {AudioClipPath}");
                return;
            }

            var notePrefabs = CreateNotePrefabs(ringSprite, ringMaterial);
            if (notePrefabs.tap == null)
                return;

            GameObject beatMarkerPrefab = CreateBeatMarkerPrefab(sprite);
            if (beatMarkerPrefab == null)
                return;

            BuildGameplayScene(clip, notePrefabs, sprite, font);
            BuildCalibrationScene(clip, beatMarkerPrefab, sprite, font);
            BuildChartSelectScene(sprite, font);

            AddScenesToBuildSettings();
            DeleteOldPrototypeScripts();

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Reflec Beat", "M1 scenes and prefabs built successfully.", "OK");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.ImportAsset(path);
            }
        }

        private static Sprite GetDefaultSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static Font GetDefaultFont()
        {
            // Prefer the built-in fallback; otherwise AddComponent<Text>() will set a default font.
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }

        private static Sprite EnsureRingSprite()
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(RingSpritePath);
            if (existing != null)
                return existing;

            const int size = 128;
            const float outerRadius = 54f;
            const float innerRadius = 40f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Color clear = Color.clear;
            Color white = Color.white;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(center, new Vector2(x + 0.5f, y + 0.5f));
                    if (distance <= outerRadius && distance >= innerRadius)
                        texture.SetPixel(x, y, white);
                    else
                        texture.SetPixel(x, y, clear);
                }
            }

            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            File.WriteAllBytes(RingSpritePath, png);
            AssetDatabase.ImportAsset(RingSpritePath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(RingSpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(RingSpritePath);
        }

        private static Material EnsureRingMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(RingMaterialPath);
            if (existing != null)
                return existing;

            var material = new Material(Shader.Find("Sprites/Default"));
            AssetDatabase.CreateAsset(material, RingMaterialPath);
            return material;
        }

        private static (GameObject tap, GameObject longNote, GameObject vertical, GameObject top, GameObject bigBang)
            CreateNotePrefabs(Sprite ringSprite, Material ringMaterial)
        {
            GameObject tap = CreateSingleNotePrefab("TapNote", ringSprite, ringMaterial, Color.white);
            GameObject longNote = CreateSingleNotePrefab("LongNote", ringSprite, ringMaterial, new Color(0.4f, 1f, 0.4f));
            GameObject vertical = CreateSingleNotePrefab("VerticalNote", ringSprite, ringMaterial, Color.cyan);
            GameObject top = CreateSingleNotePrefab("TopNote", ringSprite, ringMaterial, new Color(1f, 0.9f, 0.2f));
            GameObject bigBang = CreateSingleNotePrefab("BigBangNote", ringSprite, ringMaterial, new Color(1f, 0.4f, 0.4f));

            return (tap, longNote, vertical, top, bigBang);
        }

        private static GameObject CreateSingleNotePrefab(string name, Sprite sprite, Material material, Color color)
        {
            var root = new GameObject(name);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.material = material;
            renderer.color = color;
            root.AddComponent<NoteObject>();

            string path = $"{PrefabsPath}/{name}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
            Object.DestroyImmediate(root);

            if (!ok)
            {
                Debug.LogError($"M1SceneBuilder: Failed to save {path}.");
                return null;
            }

            return savedPrefab;
        }

        private static GameObject CreateBeatMarkerPrefab(Sprite sprite)
        {
            var root = new GameObject("CalibrationBeatMarker");
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(1f, 0.8f, 0.2f);
            root.AddComponent<CalibrationBeatMarker>();

            string path = $"{PrefabsPath}/CalibrationBeatMarker.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
            Object.DestroyImmediate(root);

            if (!ok)
            {
                Debug.LogError($"M1SceneBuilder: Failed to save {path}.");
                return null;
            }

            return savedPrefab;
        }

        private static void BuildGameplayScene(
            AudioClip clip,
            (GameObject tap, GameObject longNote, GameObject vertical, GameObject top, GameObject bigBang) notePrefabs,
            Sprite sprite,
            Font font)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            CreateMainCamera();
            CreateTransitionManager();
            AudioSource audioSource = CreateAudioSource(clip);
            PlayfieldRenderer playfield = CreatePlayfield();
            TouchInput touchInput = CreateTouchInput();
            HitFeedbackEffect hitFeedback = CreateHitFeedback();
            (GameplayUI gameplayUi, GameObject canvas) = CreateGameplayUI(font);
            ResultsUI resultsUi = CreateResultsUI(canvas, sprite, font);
            DuelUI duelUi = CreateDuelUI(canvas, sprite, font);
            CreateGameController(audioSource, clip, playfield, touchInput, notePrefabs, gameplayUi, resultsUi, duelUi, hitFeedback);
            CreateEventSystem();

            if (!EditorSceneManager.SaveScene(scene, $"{ScenesPath}/GameplayScene.unity"))
                Debug.LogWarning("M1SceneBuilder: Failed to save GameplayScene.");
        }

        private static void BuildCalibrationScene(AudioClip clip, GameObject beatMarkerPrefab, Sprite sprite, Font font)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            CreateMainCamera();
            CreateTransitionManager();
            AudioSource audioSource = CreateAudioSource(clip);
            CalibrationBeatMarker marker = CreateBeatMarkerInstance(beatMarkerPrefab);
            CalibrationUI calibrationUi = CreateCalibrationUI(sprite, font);
            CreateCalibrationController(audioSource, clip, calibrationUi, marker);
            CreateEventSystem();

            if (!EditorSceneManager.SaveScene(scene, $"{ScenesPath}/CalibrationScene.unity"))
                Debug.LogWarning("M1SceneBuilder: Failed to save CalibrationScene.");
        }

        private static void CreateMainCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            go.AddComponent<AudioListener>();
        }

        private static void CreateTransitionManager()
        {
            var go = new GameObject("TransitionManager");
            go.AddComponent<TransitionManager>();
        }

        private static AudioSource CreateAudioSource(AudioClip clip)
        {
            var go = new GameObject("AudioSource");
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.clip = clip;
            return source;
        }

        private static PlayfieldRenderer CreatePlayfield()
        {
            var go = new GameObject("Playfield");
            var playfield = go.AddComponent<PlayfieldRenderer>();
            using (var so = new SerializedObject(playfield))
            {
                so.FindProperty("_judgmentLineY").floatValue = -4f;
                so.FindProperty("_spawnY").floatValue = 5f;
                so.ApplyModifiedProperties();
            }
            return playfield;
        }

        private static TouchInput CreateTouchInput()
        {
            var go = new GameObject("TouchInput");
            return go.AddComponent<TouchInput>();
        }

        private static HitFeedbackEffect CreateHitFeedback()
        {
            var go = new GameObject("HitFeedbackEffect");
            var effect = go.AddComponent<HitFeedbackEffect>();

            Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RingSpritePath);
            Material ringMaterial = AssetDatabase.LoadAssetAtPath<Material>(RingMaterialPath);

            using (var so = new SerializedObject(effect))
            {
                so.FindProperty("_ringSprite").objectReferenceValue = ringSprite;
                so.FindProperty("_ringMaterial").objectReferenceValue = ringMaterial;
                so.ApplyModifiedProperties();
            }

            return effect;
        }

        private static void CreateGameController(
            AudioSource audioSource,
            AudioClip clip,
            PlayfieldRenderer playfield,
            TouchInput touchInput,
            (GameObject tap, GameObject longNote, GameObject vertical, GameObject top, GameObject bigBang) notePrefabs,
            GameplayUI gameplayUi,
            ResultsUI resultsUi,
            DuelUI duelUi,
            HitFeedbackEffect hitFeedback)
        {
            var go = new GameObject("GameController");
            var controller = go.AddComponent<GameController>();

            using (var so = new SerializedObject(controller))
            {
                so.FindProperty("_audioSource").objectReferenceValue = audioSource;
                so.FindProperty("_audioClip").objectReferenceValue = clip;
                so.FindProperty("_playfield").objectReferenceValue = playfield;
                so.FindProperty("_touchInput").objectReferenceValue = touchInput;
                so.FindProperty("_tapNotePrefab").objectReferenceValue = notePrefabs.tap;
                so.FindProperty("_longNotePrefab").objectReferenceValue = notePrefabs.longNote;
                so.FindProperty("_verticalNotePrefab").objectReferenceValue = notePrefabs.vertical;
                so.FindProperty("_topNotePrefab").objectReferenceValue = notePrefabs.top;
                so.FindProperty("_bigBangNotePrefab").objectReferenceValue = notePrefabs.bigBang;
                so.FindProperty("_gameplayUI").objectReferenceValue = gameplayUi;
                so.FindProperty("_resultsUI").objectReferenceValue = resultsUi;
                so.FindProperty("_duelUI").objectReferenceValue = duelUi;
                so.FindProperty("_hitFeedback").objectReferenceValue = hitFeedback;
                so.ApplyModifiedProperties();
            }
        }

        private static (GameplayUI ui, GameObject canvas) CreateGameplayUI(Font font)
        {
            GameObject canvas = CreateCanvas("Canvas");

            var scoreText = CreateUiText(canvas, "ScoreText", "Rate: 0.00%",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -20f), new Vector2(400f, 60f), 36, TextAnchor.UpperLeft, font);

            var comboText = CreateUiText(canvas, "ComboText", "Combo: 0",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -80f), new Vector2(300f, 60f), 36, TextAnchor.UpperLeft, font);

            var judgmentText = CreateUiText(canvas, "JudgmentText", "",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 80f), new Vector2(400f, 100f), 64, TextAnchor.LowerCenter, font);
            judgmentText.gameObject.SetActive(false);

            var uiGo = new GameObject("GameplayUI");
            uiGo.transform.SetParent(canvas.transform, false);
            var ui = uiGo.AddComponent<GameplayUI>();

            using (var so = new SerializedObject(ui))
            {
                so.FindProperty("_scoreText").objectReferenceValue = scoreText;
                so.FindProperty("_comboText").objectReferenceValue = comboText;
                so.FindProperty("_judgmentText").objectReferenceValue = judgmentText;
                so.ApplyModifiedProperties();
            }

            return (ui, canvas);
        }

        private static ResultsUI CreateResultsUI(GameObject canvas, Sprite sprite, Font font)
        {
            var panel = CreateUiPanel(canvas, "ResultsPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(800f, 400f), sprite);
            panel.SetActive(false);

            panel.AddComponent<CanvasGroup>();
            var panelFade = panel.AddComponent<PanelFadeSlide>();

            var resultText = CreateUiText(panel, "ResultText", "",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(700f, 300f), 32, TextAnchor.MiddleCenter, font);

            var ui = panel.AddComponent<ResultsUI>();
            using (var so = new SerializedObject(ui))
            {
                so.FindProperty("_panel").objectReferenceValue = panel;
                so.FindProperty("_panelFade").objectReferenceValue = panelFade;
                so.FindProperty("_resultText").objectReferenceValue = resultText;
                so.ApplyModifiedProperties();
            }

            return ui;
        }

        private static DuelUI CreateDuelUI(GameObject canvas, Sprite sprite, Font font)
        {
            var panel = CreateUiPanel(canvas, "DuelPanel",
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-220f, -180f), new Vector2(400f, 320f), sprite);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            Text playerHpText = CreateUiText(panel, "PlayerHpText", "Player HP: 100/100",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(380f, 40f), 24, TextAnchor.UpperCenter, font);
            Slider playerHpBar = CreateSlider(panel, "PlayerHpBar",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(360f, 30f), sprite, new Color(0.2f, 0.8f, 0.2f));

            Text rivalHpText = CreateUiText(panel, "RivalHpText", "Rival HP: 100/100",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -100f), new Vector2(380f, 40f), 24, TextAnchor.UpperCenter, font);
            Slider rivalHpBar = CreateSlider(panel, "RivalHpBar",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(360f, 30f), sprite, new Color(0.9f, 0.2f, 0.2f));

            Text gaugeText = CreateUiText(panel, "GaugeText", "Gauge: 0/100",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -180f), new Vector2(380f, 40f), 24, TextAnchor.UpperCenter, font);
            Slider gaugeBar = CreateSlider(panel, "GaugeBar",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -220f), new Vector2(360f, 30f), sprite, new Color(1f, 0.9f, 0.2f));

            Button reflectButton = CreateButton(panel, "ReflectButton", "Reflect",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 50f), new Vector2(200f, 60f), sprite, font);

            var ui = panel.AddComponent<DuelUI>();
            using (var so = new SerializedObject(ui))
            {
                so.FindProperty("_playerHpBar").objectReferenceValue = playerHpBar;
                so.FindProperty("_rivalHpBar").objectReferenceValue = rivalHpBar;
                so.FindProperty("_gaugeBar").objectReferenceValue = gaugeBar;
                so.FindProperty("_reflectButton").objectReferenceValue = reflectButton;
                so.FindProperty("_playerHpText").objectReferenceValue = playerHpText;
                so.FindProperty("_rivalHpText").objectReferenceValue = rivalHpText;
                so.FindProperty("_gaugeText").objectReferenceValue = gaugeText;
                so.ApplyModifiedProperties();
            }

            panel.SetActive(true);
            return ui;
        }

        private static Slider CreateSlider(
            GameObject parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite,
            Color fillColor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var background = go.AddComponent<Image>();
            background.sprite = sprite;
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var fillGo = new GameObject("Fill", typeof(RectTransform));
            fillGo.transform.SetParent(go.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.anchoredPosition = Vector2.zero;
            fillRt.sizeDelta = Vector2.zero;

            var fillImage = fillGo.AddComponent<Image>();
            fillImage.sprite = sprite;
            fillImage.color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            return slider;
        }

        private static CalibrationBeatMarker CreateBeatMarkerInstance(GameObject prefab)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "CalibrationBeatMarker";
            return instance.GetComponent<CalibrationBeatMarker>();
        }

        private static CalibrationUI CreateCalibrationUI(Sprite sprite, Font font)
        {
            GameObject canvas = CreateCanvas("Canvas");

            var instruction = CreateUiText(canvas, "InstructionText", "Tap the beat when you hear the click.",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -50f), new Vector2(1200f, 80f), 36, TextAnchor.UpperCenter, font);

            var lastDelta = CreateUiText(canvas, "LastDeltaText", "Last: --",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -150f), new Vector2(600f, 60f), 36, TextAnchor.UpperCenter, font);

            var average = CreateUiText(canvas, "AverageText", "Average: --",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -220f), new Vector2(600f, 60f), 36, TextAnchor.UpperCenter, font);

            var storedOffset = CreateUiText(canvas, "StoredOffsetText", "Stored offset: --",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -290f), new Vector2(600f, 60f), 32, TextAnchor.UpperCenter, font);

            Button nudgeDownButton = CreateButton(canvas, "NudgeDownButton", "-5 ms",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-140f, -360f), new Vector2(160f, 60f), sprite, font);

            Button nudgeUpButton = CreateButton(canvas, "NudgeUpButton", "+5 ms",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(140f, -360f), new Vector2(160f, 60f), sprite, font);

            var doneGroup = CreateUiPanel(canvas, "DoneGroup",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(600f, 200f), sprite);
            doneGroup.SetActive(false);
            doneGroup.AddComponent<CanvasGroup>();
            var donePanelFade = doneGroup.AddComponent<PanelFadeSlide>();

            Button retryButton = CreateButton(doneGroup, "RetryButton", "Retry",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(-120f, 0f), new Vector2(180f, 80f), sprite, font);

            Button doneButton = CreateButton(doneGroup, "DoneButton", "Done",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(120f, 0f), new Vector2(180f, 80f), sprite, font);

            var uiGo = new GameObject("CalibrationUI");
            uiGo.transform.SetParent(canvas.transform, false);
            var ui = uiGo.AddComponent<CalibrationUI>();

            using (var so = new SerializedObject(ui))
            {
                so.FindProperty("_instructionText").objectReferenceValue = instruction;
                so.FindProperty("_lastDeltaText").objectReferenceValue = lastDelta;
                so.FindProperty("_averageText").objectReferenceValue = average;
                so.FindProperty("_storedOffsetText").objectReferenceValue = storedOffset;
                so.FindProperty("_doneGroup").objectReferenceValue = doneGroup;
                so.FindProperty("_donePanelFade").objectReferenceValue = donePanelFade;
                so.FindProperty("_retryButton").objectReferenceValue = retryButton;
                so.FindProperty("_doneButton").objectReferenceValue = doneButton;
                so.FindProperty("_nudgeUpButton").objectReferenceValue = nudgeUpButton;
                so.FindProperty("_nudgeDownButton").objectReferenceValue = nudgeDownButton;
                so.ApplyModifiedProperties();
            }

            return ui;
        }

        private static void BuildChartSelectScene(Sprite sprite, Font font)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            CreateMainCamera();
            CreateTransitionManager();

            GameObject canvas = CreateCanvas("Canvas");

            Text titleText = CreateUiText(canvas, "TitleText", "Select Chart",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(1200f, 80f), 48, TextAnchor.UpperCenter, font);

            Toggle autoplayToggle = CreateToggle(canvas, "AutoplayToggle", "Autoplay",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-520f, -160f), new Vector2(280f, 60f), sprite, font);

            Toggle syncCheckToggle = CreateToggle(canvas, "SyncCheckToggle", "Sync Check",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-170f, -160f), new Vector2(280f, 60f), sprite, font);

            Toggle hideRivalToggle = CreateToggle(canvas, "HideRivalToggle", "Hide Rival",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(180f, -160f), new Vector2(280f, 60f), sprite, font);

            Button calibrateButton = CreateButton(canvas, "CalibrateButton", "Calibrate",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(520f, -160f), new Vector2(200f, 60f), sprite, font);

            (Text approachLabel, Button approachPrev, Button approachNext) = CreateStepper(canvas, "ApproachStepper", "Approach: 1.00s",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -240f), new Vector2(360f, 60f), sprite, font);

            (GameObject scrollView, Transform content) = CreateScrollView(canvas,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -300f), new Vector2(1400f, 700f), sprite);

            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.anchoredPosition = Vector2.zero;
            scrollRectTransform.sizeDelta = Vector2.zero;
            scrollRectTransform.offsetMin = new Vector2(40f, 40f);
            scrollRectTransform.offsetMax = new Vector2(-40f, -300f);

            GameObject buttonPrefab = CreateChartButtonPrefab(sprite, font);

            var uiGo = new GameObject("ChartSelectUI");
            uiGo.transform.SetParent(canvas.transform, false);
            var ui = uiGo.AddComponent<ChartSelectUI>();

            using (var so = new SerializedObject(ui))
            {
                so.FindProperty("_titleText").objectReferenceValue = titleText;
                so.FindProperty("_content").objectReferenceValue = content;
                so.FindProperty("_chartButtonPrefab").objectReferenceValue = buttonPrefab;
                so.FindProperty("_autoplayToggle").objectReferenceValue = autoplayToggle;
                so.FindProperty("_syncCheckToggle").objectReferenceValue = syncCheckToggle;
                so.FindProperty("_hideRivalToggle").objectReferenceValue = hideRivalToggle;
                so.FindProperty("_calibrateButton").objectReferenceValue = calibrateButton;
                so.FindProperty("_approachTimeLabel").objectReferenceValue = approachLabel;
                so.FindProperty("_approachTimePrevButton").objectReferenceValue = approachPrev;
                so.FindProperty("_approachTimeNextButton").objectReferenceValue = approachNext;
                so.ApplyModifiedProperties();
            }

            var controllerGo = new GameObject("ChartSelectController");
            var controller = controllerGo.AddComponent<ChartSelectController>();
            using (var so = new SerializedObject(controller))
            {
                so.FindProperty("_ui").objectReferenceValue = ui;
                so.ApplyModifiedProperties();
            }

            var bootstrapperGo = new GameObject("ContentBootstrapper");
            bootstrapperGo.AddComponent<ContentBootstrapper>();

            CreateEventSystem();

            if (!EditorSceneManager.SaveScene(scene, $"{ScenesPath}/ChartSelectScene.unity"))
                Debug.LogWarning("M1SceneBuilder: Failed to save ChartSelectScene.");
        }

        private static (GameObject scrollView, Transform content) CreateScrollView(
            GameObject parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite)
        {
            var go = new GameObject("ScrollView", typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var scrollRect = go.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateUiPanel(go, "Viewport",
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, sprite);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage != null)
                viewportImage.color = new Color(0f, 0f, 0f, 0.3f);

            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = Vector2.one;
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            var layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            return (go, content.transform);
        }

        private static GameObject CreateChartButtonPrefab(Sprite sprite, Font font)
        {
            var root = new GameObject("ChartButtonPrefab", typeof(RectTransform));

            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1200f, 110f);

            var image = root.AddComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(0.2f, 0.22f, 0.28f, 0.95f);

            var button = root.AddComponent<Button>();
            button.targetGraphic = image;

            root.AddComponent<ButtonScaleEffect>();

            Text titleText = CreateUiText(root, "TitleText", "Title",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -10f), new Vector2(760f, 50f), 30, TextAnchor.UpperLeft, font);
            titleText.rectTransform.pivot = new Vector2(0f, 1f);

            Text artistText = CreateUiText(root, "ArtistText", "Artist",
                new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(20f, 10f), new Vector2(760f, 40f), 24, TextAnchor.LowerLeft, font);
            artistText.rectTransform.pivot = new Vector2(0f, 0f);

            Text difficultyText = CreateUiText(root, "DifficultyText", "Difficulty",
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -10f), new Vector2(280f, 40f), 24, TextAnchor.UpperRight, font);
            difficultyText.rectTransform.pivot = new Vector2(1f, 1f);

            Text bestText = CreateUiText(root, "BestText", "Best",
                new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-20f, 10f), new Vector2(280f, 40f), 22, TextAnchor.LowerRight, font);
            bestText.rectTransform.pivot = new Vector2(1f, 0f);

            string path = $"{PrefabsPath}/ChartButton.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path, out bool ok);
            Object.DestroyImmediate(root);

            if (!ok)
            {
                Debug.LogError($"M1SceneBuilder: Failed to save {path}.");
                return null;
            }

            return savedPrefab;
        }

        private static Toggle CreateToggle(
            GameObject parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite,
            Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(go.transform, false);
            var bgRt = background.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.5f);
            bgRt.anchorMax = new Vector2(0f, 0.5f);
            bgRt.pivot = new Vector2(0f, 0.5f);
            bgRt.anchoredPosition = new Vector2(0f, 0f);
            bgRt.sizeDelta = new Vector2(50f, 50f);

            var bgImage = background.AddComponent<Image>();
            bgImage.sprite = sprite;
            bgImage.color = Color.white;

            var checkmark = new GameObject("Checkmark", typeof(RectTransform));
            checkmark.transform.SetParent(background.transform, false);
            var cmRt = checkmark.GetComponent<RectTransform>();
            cmRt.anchorMin = Vector2.zero;
            cmRt.anchorMax = Vector2.one;
            cmRt.sizeDelta = Vector2.zero;

            var cmImage = checkmark.AddComponent<Image>();
            cmImage.sprite = sprite;
            cmImage.color = new Color(0.2f, 0.8f, 0.2f);

            var labelText = CreateUiText(go, "Label", label,
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleLeft, font);
            labelText.rectTransform.offsetMin = new Vector2(60f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(-10f, 0f);

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = cmImage;
            toggle.isOn = false;

            return toggle;
        }

        private static (Text label, Button prevButton, Button nextButton) CreateStepper(
            GameObject parent,
            string name,
            string initialLabel,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite,
            Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Button prevButton = CreateButton(go, "PrevButton", "<",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(60f, 60f), sprite, font);

            Button nextButton = CreateButton(go, "NextButton", ">",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0f, 0f), new Vector2(60f, 60f), sprite, font);

            Text label = CreateUiText(go, "Label", initialLabel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(sizeDelta.x - 140f, 60f), 30, TextAnchor.MiddleCenter, font);
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.raycastTarget = false;

            return (label, prevButton, nextButton);
        }

        private static void CreateCalibrationController(
            AudioSource audioSource,
            AudioClip clip,
            CalibrationUI ui,
            CalibrationBeatMarker marker)
        {
            var go = new GameObject("CalibrationController");
            var controller = go.AddComponent<CalibrationController>();

            using (var so = new SerializedObject(controller))
            {
                so.FindProperty("_audioSource").objectReferenceValue = audioSource;
                so.FindProperty("_tickClip").objectReferenceValue = clip;
                so.FindProperty("_ui").objectReferenceValue = ui;
                so.FindProperty("_beatMarker").objectReferenceValue = marker;
                so.ApplyModifiedProperties();
            }
        }

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static Text CreateUiText(
            GameObject parent,
            string name,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            TextAnchor alignment,
            Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = alignment;
            t.raycastTarget = false;
            if (font != null)
                t.font = font;

            return t;
        }

        private static GameObject CreateUiPanel(
            GameObject parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = new Color(0f, 0f, 0f, 0.8f);

            return go;
        }

        private static Button CreateButton(
            GameObject parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Sprite sprite,
            Font font)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            go.AddComponent<ButtonScaleEffect>();

            var labelText = CreateUiText(go, "Text", label,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, sizeDelta, 28, TextAnchor.MiddleCenter, font);
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.sizeDelta = Vector2.zero;

            return button;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static void AddScenesToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene($"{ScenesPath}/CalibrationScene.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/ChartSelectScene.unity", true),
                new EditorBuildSettingsScene($"{ScenesPath}/GameplayScene.unity", true)
            };

            // Preserve any other existing scenes, e.g. from the user.
            string[] managedPaths =
            {
                $"{ScenesPath}/CalibrationScene.unity",
                $"{ScenesPath}/ChartSelectScene.unity",
                $"{ScenesPath}/GameplayScene.unity"
            };

            foreach (var existing in EditorBuildSettings.scenes)
            {
                string normalizedExisting = existing.path.Replace('\\', '/');
                bool isManaged = false;
                foreach (string managed in managedPaths)
                {
                    if (normalizedExisting == managed)
                    {
                        isManaged = true;
                        break;
                    }
                }

                if (!isManaged)
                    scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void DeleteOldPrototypeScripts()
        {
            foreach (string path in OldPrototypeScripts)
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                {
                    if (!AssetDatabase.DeleteAsset(path))
                        Debug.LogWarning($"M1SceneBuilder: Failed to delete {path}");
                }
            }
        }
    }
}
#endif
