using System.Reflection;
using NUnit.Framework;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using ReflecBeatSimulator.UI;
using UnityEngine;

namespace ReflecBeatSimulator.Tests
{
    public class PlayfieldRendererTests
    {
        [Test]
        public void GetZoneX_MapsZonesToLeftCenterRight()
        {
            Assert.That(PlayfieldRenderer.GetZoneX(0), Is.EqualTo(1f / 6f).Within(1e-6f));
            Assert.That(PlayfieldRenderer.GetZoneX(1), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(PlayfieldRenderer.GetZoneX(2), Is.EqualTo(5f / 6f).Within(1e-6f));
        }

        [Test]
        public void GetZoneX_DefaultsToCenterForUnknownZone()
        {
            Assert.That(PlayfieldRenderer.GetZoneX(-1), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(PlayfieldRenderer.GetZoneX(99), Is.EqualTo(0.5f).Within(1e-6f));
        }

        [Test]
        public void GetWorldPosition_MapsNormalizedXToWorldSpace()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 12f, judgmentLineY: -4f);

            Vector3 center = playfield.GetWorldPosition(0.5f);
            Vector3 left = playfield.GetWorldPosition(0f);
            Vector3 right = playfield.GetWorldPosition(1f);

            Assert.That(center.x, Is.EqualTo(0f).Within(1e-6f));
            Assert.That(center.y, Is.EqualTo(-4f).Within(1e-6f));
            Assert.That(left.x, Is.EqualTo(-6f).Within(1e-6f));
            Assert.That(right.x, Is.EqualTo(6f).Within(1e-6f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void GetSpawnPosition_UsesSpawnY()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f, spawnY: 6f);

            Vector3 spawn = playfield.GetSpawnPosition(0.5f);

            Assert.That(spawn.x, Is.EqualTo(0f).Within(1e-6f));
            Assert.That(spawn.y, Is.EqualTo(6f).Within(1e-6f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void GetTopWorldPosition_MapsZoneToTopJudgmentLine()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 12f, judgmentLineY: -4f, topJudgmentLineY: -2f);

            Vector3 center = playfield.GetTopWorldPosition(1);

            Assert.That(center.x, Is.EqualTo(0f).Within(1e-6f));
            Assert.That(center.y, Is.EqualTo(-2f).Within(1e-6f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void GetTopSpawnPosition_UsesTopSpawnY()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f, topSpawnY: 7f);

            Vector3 spawn = playfield.GetTopSpawnPosition(0);

            Assert.That(spawn.y, Is.EqualTo(7f).Within(1e-6f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void NoteWorldWidth_IsExposed()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f);
            SetPrivateField(playfield, "_noteWorldWidth", 0.75f);

            Assert.That(playfield.NoteWorldWidth, Is.EqualTo(0.75f).Within(1e-6f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void FlashTopCircle_Great_SetsCircleColorToThemeGreat()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f);
            Color expected = DefaultVisualTheme.ColorForGrade(Judgment.Great);

            playfield.FlashTopCircle(1, Judgment.Great);

            Transform topCircle = playfield.transform.Find("TopCircle1");
            Assert.That(topCircle, Is.Not.Null, "TopCircle1 child not found.");
            var renderer = topCircle.GetComponent<LineRenderer>();
            Assert.That(renderer, Is.Not.Null, "TopCircle1 has no LineRenderer.");
            Assert.That(renderer.startColor.r, Is.EqualTo(expected.r).Within(0.1f));
            Assert.That(renderer.startColor.g, Is.EqualTo(expected.g).Within(0.1f));
            Assert.That(renderer.startColor.b, Is.EqualTo(expected.b).Within(0.1f));
            Assert.That(renderer.startColor.a, Is.EqualTo(expected.a).Within(0.1f));

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void Awake_CreatesPlayfieldFrameSegments()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f);

            Assert.That(playfield.transform.Find("FrameLeft"), Is.Not.Null);
            Assert.That(playfield.transform.Find("FrameRight"), Is.Not.Null);
            Assert.That(playfield.transform.Find("FrameTop"), Is.Not.Null);
            Assert.That(playfield.transform.Find("FrameBottom"), Is.Not.Null);
            Assert.That(playfield.transform.Find("OuterFrameLeft"), Is.Not.Null);
            Assert.That(playfield.transform.Find("PlayfieldTopWash"), Is.Not.Null);
            Assert.That(playfield.transform.Find("CornerBL"), Is.Not.Null);

            Object.DestroyImmediate(playfield.gameObject);
        }

        [Test]
        public void ScreenToNormalizedX_MapsWorldAlignedScreenPointToPlayfieldX()
        {
            PlayfieldRenderer playfield = CreatePlayfield(width: 10f, judgmentLineY: -4f);
            var cameraGo = new GameObject("TestCamera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            // World X = 0 should be playfield 0.5
            Vector3 screenCenter = camera.WorldToScreenPoint(new Vector3(0f, -4f, 0f));
            float xCenter = playfield.ScreenToNormalizedX(screenCenter, camera);
            Assert.That(xCenter, Is.EqualTo(0.5f).Within(0.02f));

            // World X = -2.5 (playfield 0.25)
            Vector3 screenLeft = camera.WorldToScreenPoint(new Vector3(-2.5f, -4f, 0f));
            float xLeft = playfield.ScreenToNormalizedX(screenLeft, camera);
            Assert.That(xLeft, Is.EqualTo(0.25f).Within(0.02f));

            Object.DestroyImmediate(cameraGo);
            Object.DestroyImmediate(playfield.gameObject);
        }

        private static PlayfieldRenderer CreatePlayfield(float width, float judgmentLineY, float spawnY = 5f,
            float topJudgmentLineY = -2.5f, float topSpawnY = 5f)
        {
            var go = new GameObject("PlayfieldTest");
            var playfield = go.AddComponent<PlayfieldRenderer>();

            SetPrivateField(playfield, "_width", width);
            SetPrivateField(playfield, "_judgmentLineY", judgmentLineY);
            SetPrivateField(playfield, "_spawnY", spawnY);
            SetPrivateField(playfield, "_topJudgmentLineY", topJudgmentLineY);
            SetPrivateField(playfield, "_topSpawnY", topSpawnY);

            // EditMode tests do not call Awake automatically; build visuals explicitly.
            if (playfield.transform.Find("TopCircle0") == null)
            {
                MethodInfo awake = playfield.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                awake?.Invoke(playfield, null);
            }

            return playfield;
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"Field '{name}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
