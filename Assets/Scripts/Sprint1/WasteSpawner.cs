using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint6;

namespace ReCiclo.Sprint1
{
    public class WasteSpawner : MonoBehaviour
    {
        public static WasteSpawner Instance { get; private set; }

        [Header("Configuración de Generación Cadenciada")]
        [SerializeField] private float spawnInterval = 2.2f;
        [SerializeField] private float minSpawnInterval = 1.0f;
        [SerializeField] private bool autoStartSpawning = true;
        [SerializeField] private int maxActiveItems = 4;
        [SerializeField] private RectTransform itemsContainer;

        [Header("Prefabs Opcionales")]
        [SerializeField] private List<GameObject> wastePrefabs = new List<GameObject>();
        [SerializeField] private Transform[] spawnPoints;

        [Header("Eventos")]
        public UnityEvent<DraggableItem> OnWasteSpawned = new UnityEvent<DraggableItem>();

        private bool isSpawning = false;
        private Coroutine spawnRoutine;
        private List<DraggableItem> activeItems = new List<DraggableItem>();

        private readonly (WasteCategory category, string name, Color color, int points)[] wasteDefinitions = new[]
        {
            (WasteCategory.Paper, "Periódico", new Color(0.22f, 0.52f, 0.92f), 100),
            (WasteCategory.Paper, "Caja de Cartón", new Color(0.35f, 0.65f, 0.98f), 120),
            (WasteCategory.Plastic, "Botella PET", new Color(0.95f, 0.78f, 0.12f), 100),
            (WasteCategory.Plastic, "Bolsa Plástica", new Color(0.92f, 0.70f, 0.10f), 80),
            (WasteCategory.Glass, "Botella Vidrio", new Color(0.18f, 0.80f, 0.42f), 150),
            (WasteCategory.Glass, "Frasco Vidrio", new Color(0.14f, 0.72f, 0.38f), 140),
            (WasteCategory.Organic, "Cáscara Plátano", new Color(0.62f, 0.38f, 0.20f), 90),
            (WasteCategory.Organic, "Manzana", new Color(0.55f, 0.30f, 0.15f), 90),
            (WasteCategory.Electronic, "Pila Gastada", new Color(0.92f, 0.22f, 0.22f), 200),
            (WasteCategory.Electronic, "Celular Viejo", new Color(0.85f, 0.18f, 0.28f), 250)
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

        private void Start()
        {
            if (autoStartSpawning)
            {
                StartSpawning(spawnInterval);
            }
        }

        public void StartSpawning(float interval)
        {
            spawnInterval = Mathf.Max(minSpawnInterval, interval);
            isSpawning = true;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(SpawnCadenceRoutine());
            Debug.Log($"[WasteSpawner] Generación iniciada con cadencia de {spawnInterval:F1}s");
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            Debug.Log("[WasteSpawner] Generación detenida");
        }

        public void SetCadence(float interval)
        {
            spawnInterval = Mathf.Max(minSpawnInterval, interval);
        }

        private IEnumerator SpawnCadenceRoutine()
        {
            // Spawn inicial inmediato
            SpawnSingleWasteItem();

            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);

                // Limpiar lista de items destruidos
                activeItems.RemoveAll(item => item == null);

                if (isSpawning && activeItems.Count < maxActiveItems)
                {
                    SpawnSingleWasteItem();
                }
            }
        }

        public GameObject SpawnSingleWasteItem()
        {
            Transform parent = (itemsContainer != null) ? itemsContainer : transform;

            // Si hay prefabs asignados
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
                    activeItems.Add(draggable);
                    OnWasteSpawned?.Invoke(draggable);
                }

                return spawnedObj;
            }

            // Generar elemento interactivo procedural
            GameObject dynamicItem = CreateDynamicWasteUI(parent);
            DraggableItem dItem = dynamicItem.GetComponent<DraggableItem>();
            if (dItem != null)
            {
                activeItems.Add(dItem);
                OnWasteSpawned?.Invoke(dItem);
            }
            return dynamicItem;
        }

        private GameObject CreateDynamicWasteUI(Transform parent)
        {
            var def = wasteDefinitions[Random.Range(0, wasteDefinitions.Length)];

            GameObject wasteObj = new GameObject($"Waste_{def.name}");
            wasteObj.transform.SetParent(parent, false);

            RectTransform rect = wasteObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(170, 170);

            // Posición aleatoria dentro del área de spawn
            float randomX = Random.Range(-260f, 260f);
            float randomY = Random.Range(-80f, 160f);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            // Imagen de fondo con estilo moderno
            Image img = wasteObj.AddComponent<Image>();
            img.color = def.color;
            img.raycastTarget = true;

            Outline outline = wasteObj.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.9f);
            outline.effectDistance = new Vector2(3.5f, -3.5f);

            Shadow shadow = wasteObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(4f, -4f);

            // Contenedor de Texto y Categoría
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(wasteObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-10, -10);

            Text label = textObj.AddComponent<Text>();
            label.text = $"{def.name}\n<size=16><b>{GetCategoryDisplayName(def.category)}</b></size>";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.supportRichText = true;

            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0, 0, 0, 0.85f);
            textShadow.effectDistance = new Vector2(2, -2);

            // Componente de arrastre y datos
            DraggableItem draggable = wasteObj.AddComponent<DraggableItem>();
            draggable.SetItemData(def.category, def.name, def.points, def.color);

            return wasteObj;
        }

        private string GetCategoryDisplayName(WasteCategory category)
        {
            switch (category)
            {
                case WasteCategory.Paper: return "PAPEL";
                case WasteCategory.Plastic: return "PLÁSTICO";
                case WasteCategory.Glass: return "VIDRIO";
                case WasteCategory.Organic: return "ORGÁNICO";
                case WasteCategory.Electronic: return "ELECTRÓNICO";
                default: return "";
            }
        }
    }
}