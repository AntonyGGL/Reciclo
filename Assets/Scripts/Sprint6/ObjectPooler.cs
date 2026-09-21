using System.Collections.Generic;
using UnityEngine;

namespace ReCiclo.Sprint6
{
    [System.Serializable]
    public class PoolItem
    {
        public string poolTag;
        public GameObject prefab;
        public int initialSize = 10;
        public bool allowGrowth = true;
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

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

                Queue<GameObject> objectQueue = new Queue<GameObject>();
                prefabDictionary[item.poolTag] = item.prefab;

                for (int i = 0; i < item.initialSize; i++)
                {
                    GameObject obj = Instantiate(item.prefab, transform);
                    obj.SetActive(false);
                    objectQueue.Enqueue(obj);
                }

                poolDictionary[item.poolTag] = objectQueue;
            }
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPooler] No existe pool con la etiqueta: {tag}");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[tag];
            GameObject objToSpawn = null;

            if (queue.Count > 0)
            {
                objToSpawn = queue.Dequeue();
            }
            else
            {
                // Si la cola está vacía y se permite crecimiento, instanciar nuevo
                if (prefabDictionary.ContainsKey(tag))
                {
                    objToSpawn = Instantiate(prefabDictionary[tag], transform);
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

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (poolDictionary.ContainsKey(tag))
            {
                poolDictionary[tag].Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
