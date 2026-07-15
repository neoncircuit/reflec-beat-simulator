using System.Collections.Generic;
using NUnit.Framework;
using ReflecBeatSimulator.Core;
using ReflecBeatSimulator.Gameplay;
using UnityEngine;

namespace ReflecBeatSimulator.Tests
{
    public class NoteObjectPoolTests
    {
        [Test]
        public void Get_CreatesInstance_WhenPoolEmpty()
        {
            GameObject prefab = CreatePrefab();
            var pool = new NoteObjectPool(new Dictionary<string, GameObject> { { NoteType.Tap, prefab } });

            NoteObject noteObject = pool.Get(NoteType.Tap, Vector3.zero);

            Assert.That(noteObject, Is.Not.Null);
            Assert.That(noteObject.gameObject.activeInHierarchy, Is.True);

            Object.DestroyImmediate(noteObject.gameObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Release_ReusesInstance_OnNextGet()
        {
            GameObject prefab = CreatePrefab();
            var pool = new NoteObjectPool(new Dictionary<string, GameObject> { { NoteType.Tap, prefab } });

            NoteObject first = pool.Get(NoteType.Tap, Vector3.zero);
            pool.Release(first, NoteType.Tap);
            NoteObject second = pool.Get(NoteType.Tap, Vector3.zero);

            Assert.That(second, Is.SameAs(first));
            Assert.That(second.gameObject.activeInHierarchy, Is.True);

            Object.DestroyImmediate(second.gameObject);
            Object.DestroyImmediate(prefab);
        }

        [Test]
        public void Clear_DestroysPooledInstances()
        {
            GameObject prefab = CreatePrefab();
            var pool = new NoteObjectPool(new Dictionary<string, GameObject> { { NoteType.Tap, prefab } });

            NoteObject noteObject = pool.Get(NoteType.Tap, Vector3.zero);
            pool.Release(noteObject, NoteType.Tap);
            pool.Clear();

            // After Clear the pooled GameObject should be destroyed.
            Assert.That(noteObject == null || noteObject.gameObject == null, Is.True);

            Object.DestroyImmediate(prefab);
        }

        private static GameObject CreatePrefab()
        {
            GameObject go = new GameObject("NoteObjectPoolTestPrefab");
            go.AddComponent<NoteObject>();
            return go;
        }
    }
}
