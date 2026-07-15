using System.Collections.Generic;
using UnityEngine;

namespace ReflecBeatSimulator.Gameplay
{
    /// <summary>
    /// Pools NoteObject instances per note type to avoid per-note Instantiate/Destroy
    /// allocations during gameplay.
    /// </summary>
    public sealed class NoteObjectPool
    {
        private readonly Dictionary<string, Stack<NoteObject>> _pools = new Dictionary<string, Stack<NoteObject>>();
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private readonly Transform _parent;

        /// <summary>
        /// Creates a pool using the supplied prefab mapping. Unknown note types fall
        /// back to the tap-note prefab if no exact mapping exists.
        /// </summary>
        public NoteObjectPool(IReadOnlyDictionary<string, GameObject> prefabsByType, Transform parent = null)
        {
            _parent = parent;
            foreach (KeyValuePair<string, GameObject> pair in prefabsByType)
                _prefabs[pair.Key] = pair.Value;
        }

        /// <summary>Borrows a NoteObject of the given type at the requested position.</summary>
        public NoteObject Get(string noteType, Vector3 position)
        {
            if (!_pools.TryGetValue(noteType, out Stack<NoteObject> stack))
            {
                stack = new Stack<NoteObject>();
                _pools[noteType] = stack;
            }

            NoteObject noteObject = null;
            while (stack.Count > 0 && noteObject == null)
                noteObject = stack.Pop();

            if (noteObject != null)
            {
                noteObject.gameObject.SetActive(true);
                noteObject.transform.position = position;
                noteObject.ResetState();
                return noteObject;
            }

            GameObject prefab = ResolvePrefab(noteType);
            if (prefab == null)
                return null;

            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity, _parent);
            noteObject = go.GetComponent<NoteObject>();
            if (noteObject == null)
            {
                Object.Destroy(go);
                return null;
            }

            return noteObject;
        }

        /// <summary>Returns a NoteObject to the pool for later reuse.</summary>
        public void Release(NoteObject noteObject, string noteType)
        {
            if (noteObject == null)
                return;

            noteObject.ResetState();
            noteObject.gameObject.SetActive(false);

            if (!_pools.TryGetValue(noteType, out Stack<NoteObject> stack))
            {
                stack = new Stack<NoteObject>();
                _pools[noteType] = stack;
            }

            stack.Push(noteObject);
        }

        /// <summary>Destroys every pooled instance and clears the pool.</summary>
        public void Clear()
        {
            foreach (Stack<NoteObject> stack in _pools.Values)
            {
                while (stack.Count > 0)
                {
                    NoteObject noteObject = stack.Pop();
                    if (noteObject != null)
                        Object.Destroy(noteObject.gameObject);
                }
            }

            _pools.Clear();
        }

        private GameObject ResolvePrefab(string noteType)
        {
            if (_prefabs.TryGetValue(noteType, out GameObject prefab) && prefab != null)
                return prefab;

            if (_prefabs.TryGetValue(NoteType.Tap, out GameObject fallback) && fallback != null)
                return fallback;

            return null;
        }
    }
}
