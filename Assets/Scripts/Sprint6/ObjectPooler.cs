using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReCiclo.Sprint6
{
    [Serializable]
    public class PoolItem
    {
        public string poolTag;
        public GameObject prefab;
        public int initialSize = 10;
        public bool allowGrowth = true;
    }

    /// <summary>
    /// Sistema de Object Pooling reutilizable (Sprint 7).
    /// Optimiza residuos, efectos y partículas evitando Instantiate y Destroy constantes.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        private static ObjectPooler _instance;
        public static ObjectPooler Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<ObjectPooler>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuración de Pools")]
        [SerializeField] private List<PoolItem> poolsToInitialize = new List<PoolItem>();

        private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var item in poolsToInitialize)
            {
                if (item.prefab == null || string.IsNullOrEmpty(item.poolTag)) continue;
                PreloadPool(item.poolTag, item.prefab, item.initialSize);
            }
        }

        public void PreloadPool(string poolTag, GameObject prefab, int count)
        {
            if (prefab == null || string.IsNullOrEmpty(poolTag)) return;

            if (!poolDictionary.ContainsKey(poolTag))
            {
                poolDictionary[poolTag] = new Queue<GameObject>();
            }

            prefabDictionary[poolTag] = prefab;

            GameObject poolParent = new GameObject($"[Pool] {poolTag}");
            poolParent.transform.SetParent(transform);

            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, poolParent.transform);
                obj.SetActive(false);
                poolDictionary[poolTag].Enqueue(obj);
            }

            Debug.Log($"[ObjectPooler] Pool '{poolTag}' inicializado con {count} objetos.");
        }

        public GameObject GetFromPool(string poolTag, Vector3 position = default, Quaternion rotation = default)
        {
            if (!poolDictionary.ContainsKey(poolTag))
            {
                Debug.LogWarning($"[ObjectPooler] No existe pool con la etiqueta: {poolTag}");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[poolTag];
            GameObject objToSpawn = null;

            while (queue.Count > 0 && objToSpawn == null)
            {
                objToSpawn = queue.Dequeue();
            }

            if (objToSpawn == null)
            {
                if (prefabDictionary.ContainsKey(poolTag))
                {
                    objToSpawn = Instantiate(prefabDictionary[poolTag], transform);
                }
            }

            if (objToSpawn != null)
            {
                objToSpawn.transform.position = position;
                objToSpawn.transform.rotation = rotation;
                objToSpawn.SetActive(true);
            }

            return objToSpawn;
        }

        public GameObject SpawnFromPool(string poolTag, Vector3 position, Quaternion rotation)
        {
            return GetFromPool(poolTag, position, rotation);
        }

        public void ReturnToPool(string poolTag, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (poolDictionary.ContainsKey(poolTag))
            {
                poolDictionary[poolTag].Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
