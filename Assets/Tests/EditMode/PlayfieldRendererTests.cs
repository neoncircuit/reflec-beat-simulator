using System.Reflection;
using NUnit.Framework;
using ReflecBeatSimulator.Gameplay;
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
