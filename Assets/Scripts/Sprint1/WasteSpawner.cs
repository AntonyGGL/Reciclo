using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint6;

namespace ReCiclo.Sprint1
{
    public class WasteSpawner : MonoBehaviour
    {
        public static WasteSpawner Instance { get; private set; }

        [Header("Configuracion de Generacion")]
        [SerializeField] private List<GameObject> wastePrefabs = new List<GameObject>();
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnInterval = 2.0f;
        [SerializeField] private bool isSpawning = false;
        [SerializeField] private RectTransform itemsContainer;

        private Coroutine spawnRoutine;

        private readonly (WasteCategory category, string name, Color color)[] sampleWasteData = new[]
        {
            (WasteCategory.Paper, "Periodico", new Color(0.25f, 0.55f, 0.95f)),
            (WasteCategory.Paper, "Caja Carton", new Color(0.35f, 0.65f, 1f)),
            (WasteCategory.Plastic, "Botella Plastica", new Color(1f, 0.85f, 0.2f)),
            (WasteCategory.Plastic, "Bolsa Snack", new Color(0.95f, 0.75f, 0.15f)),
            (WasteCategory.Glass, "Botella Vidrio", new Color(0.2f, 0.85f, 0.4f)),
            (WasteCategory.Glass, "Frasco Vidrio", new Color(0.15f, 0.75f, 0.35f)),
            (WasteCategory.Organic, "Cascara Platano", new Color(0.6f, 0.4f, 0.2f)),
            (WasteCategory.Organic, "Manzana", new Color(0.5f, 0.35f, 0.15f)),
            (WasteCategory.Electronic, "Pila Alcalina", new Color(0.95f, 0.25f, 0.25f)),
            (WasteCategory.Electronic, "Celular Viejo", new Color(0.85f, 0.2f, 0.3f))
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (itemsContainer == null)
            {
                itemsContainer = GetComponent<RectTransform>();
            }
        }

        public void StartSpawning(float interval)
        {
            spawnInterval = Mathf.Max(0.8f, interval);
            isSpawning = true;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(SpawnLoopRoutine());
            Debug.Log($"[WasteSpawner] Spawner iniciado con intervalo de {spawnInterval}s.");
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            Debug.Log("[WasteSpawner] Spawner detenido.");
        }

        private IEnumerator SpawnLoopRoutine()
        {
            // Spawn inmediato al iniciar
            SpawnSingleWasteItem();

            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);
                if (isSpawning)
                {
                    SpawnSingleWasteItem();
                }
            }
        }

        public GameObject SpawnSingleWasteItem()
        {
            Transform parent = (itemsContainer != null) ? itemsContainer : transform;

            // Si hay prefabs configurados
            if (wastePrefabs != null && wastePrefabs.Count > 0)
            {
                Transform spawnPoint = parent;
                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                }

                GameObject prefabToSpawn = wastePrefabs[Random.Range(0, wastePrefabs.Count)];
                GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity, parent);

                DraggableItem draggable = spawnedObj.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.Category = (WasteCategory)Random.Range(0, System.Enum.GetValues(typeof(WasteCategory)).Length);
                }

                return spawnedObj;
            }

            // Si no hay prefabs asignados, construir elemento UI dinamicamente
            return CreateDynamicWasteUI(parent);
        }

        private GameObject CreateDynamicWasteUI(Transform parent)
        {
            var data = sampleWasteData[Random.Range(0, sampleWasteData.Length)];

            GameObject wasteObj = new GameObject($"Waste_{data.name}");
            wasteObj.transform.SetParent(parent, false);

            RectTransform rect = wasteObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160, 160);

            // Posicion aleatoria en la zona central de juego
            float randomX = Random.Range(-280f, 280f);
            float randomY = Random.Range(-100f, 200f);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            // Fondo visual con borde
            Image img = wasteObj.AddComponent<Image>();
            img.color = data.color;
            img.raycastTarget = true;

            Outline outline = wasteObj.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, -3);

            // Texto descriptivo dentro del residuo
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(wasteObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text label = textObj.AddComponent<Text>();
            label.text = $"{data.name}\n({GetCategoryTag(data.category)})";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0, 0, 0, 0.8f);
            textShadow.effectDistance = new Vector2(2, -2);

            // Componentes de arrastre e interaccion
            CanvasGroup cg = wasteObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            DraggableItem draggable = wasteObj.AddComponent<DraggableItem>();
            draggable.Category = data.category;
            draggable.PointsValue = 100;

            return wasteObj;
        }

        private string GetCategoryTag(WasteCategory category)
        {
            switch (category)
            {
                case WasteCategory.Paper: return "PAPEL";
                case WasteCategory.Plastic: return "PLASTICO";
                case WasteCategory.Glass: return "VIDRIO";
                case WasteCategory.Organic: return "ORGANICO";
                case WasteCategory.Electronic: return "ELECTRONICO";
                default: return "";
            }
        }
    }
}
