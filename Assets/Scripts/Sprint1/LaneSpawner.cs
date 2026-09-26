using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;
using ReCiclo.Sprint6;

namespace ReCiclo.Sprint1
{
    /// <summary>
    /// Generador de residuos por carriles verticales estilo Guitar Hero.
    /// Controla la cadencia, velocidad progresiva y posiciones fijas de los carriles.
    /// </summary>
    public class LaneSpawner : MonoBehaviour
    {
        public static LaneSpawner Instance { get; private set; }

        [Header("Configuración de Carriles")]
        [Range(3, 5)]
        [SerializeField] private int laneCount = 5;
        [SerializeField] private float[] laneXPositions = new float[] { -340f, -170f, 0f, 170f, 340f };
        [SerializeField] private float spawnYPosition = 420f;
        [SerializeField] private RectTransform container;

        [Header("Velocidad y Cadencia")]
        [SerializeField] private float baseFallSpeed = 380f;
        [SerializeField] private float currentFallSpeed = 380f;
        [SerializeField] private float spawnInterval = 1.8f;
        [SerializeField] private float minSpawnInterval = 0.75f;
        [SerializeField] private float speedIncreaseRate = 2.5f; // Aumento de velocidad por segundo
        [SerializeField] private float intervalDecreaseRate = 0.012f;

        [Header("Prefabs y Datos de Residuos")]
        [SerializeField] private GameObject fallingWastePrefab;
        [SerializeField] private Sprite[] wasteSprites;

        private bool isSpawning = false;
        private Coroutine spawnLoopRoutine;
        private List<FallingWaste> activeWasteList = new List<FallingWaste>();

        private readonly (WasteCategory category, string name, Color color, int points)[] wastePalette = new[]
        {
            (WasteCategory.Paper, "Periódico", new Color(0.18f, 0.48f, 0.92f), 100),
            (WasteCategory.Plastic, "Botella PET", new Color(0.95f, 0.75f, 0.08f), 100),
            (WasteCategory.Glass, "Botella Vidrio", new Color(0.15f, 0.75f, 0.38f), 150),
            (WasteCategory.Organic, "Cáscara Plátano", new Color(0.65f, 0.38f, 0.18f), 90),
            (WasteCategory.Electronic, "Pila Gastada", new Color(0.92f, 0.22f, 0.22f), 200)
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (container == null) container = GetComponent<RectTransform>();
            EnsureLanePositions();
        }

        private void Start()
        {
            // Suscribirse a eventos de GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += HandleGameStart;
                GameManager.Instance.OnGamePaused += HandleGamePaused;
                GameManager.Instance.OnGameResumed += HandleGameResumed;
                GameManager.Instance.OnVictory += HandleGameEnd;
                GameManager.Instance.OnGameOver += HandleGameEnd;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= HandleGameStart;
                GameManager.Instance.OnGamePaused -= HandleGamePaused;
                GameManager.Instance.OnGameResumed -= HandleGameResumed;
                GameManager.Instance.OnVictory -= HandleGameEnd;
                GameManager.Instance.OnGameOver -= HandleGameEnd;
            }
        }

        private void Update()
        {
            if (!isSpawning) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Dificultad progresiva en tiempo real
            currentFallSpeed += speedIncreaseRate * Time.deltaTime;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - (intervalDecreaseRate * Time.deltaTime));
        }

        public void StartSpawning(float initialInterval, float initialSpeed)
        {
            spawnInterval = Mathf.Max(minSpawnInterval, initialInterval);
            baseFallSpeed = initialSpeed;
            currentFallSpeed = initialSpeed;
            isSpawning = true;

            if (spawnLoopRoutine != null) StopCoroutine(spawnLoopRoutine);
            spawnLoopRoutine = StartCoroutine(SpawnLoop());
            Debug.Log($"[LaneSpawner] Spawning iniciado en {laneCount} carriles. Intervalo: {spawnInterval:F2}s, Velocidad: {currentFallSpeed:F0}px/s");
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnLoopRoutine != null) StopCoroutine(spawnLoopRoutine);
            Debug.Log("[LaneSpawner] Spawner detenido.");
        }

        public void ClearAllActiveWaste()
        {
            activeWasteList.RemoveAll(w => w == null);
            foreach (var w in activeWasteList)
            {
                if (w != null)
                {
                    if (ObjectPooler.Instance != null) ObjectPooler.Instance.ReturnToPool("WasteItem", w.gameObject);
                    else Destroy(w.gameObject);
                }
            }
            activeWasteList.Clear();
        }

        private IEnumerator SpawnLoop()
        {
            // Spawn inicial inmediato
            SpawnNextWaste();

            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);

                if (isSpawning && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    SpawnNextWaste();
                }
            }
        }

        public FallingWaste SpawnNextWaste(int forcedLane = -1)
        {
            EnsureLanePositions();

            int lane = (forcedLane >= 0 && forcedLane < laneCount) ? forcedLane : Random.Range(0, laneCount);
            float posX = laneXPositions[lane];

            // El carril y la categoría se eligen de forma independiente.
            int wasteIndex = Random.Range(0, wastePalette.Length);
            var def = wastePalette[wasteIndex];

            GameObject wasteObj = new GameObject($"Waste_Lane{lane}_{def.name}");
            wasteObj.transform.SetParent((container != null) ? container : transform, false);

            RectTransform rect = wasteObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 150);
            rect.anchoredPosition = new Vector2(posX, spawnYPosition);

            // Aspecto visual
            UnityEngine.UI.Image img = wasteObj.AddComponent<UnityEngine.UI.Image>();
            img.color = def.color;

            UnityEngine.UI.Outline outline = wasteObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, -3);

            UnityEngine.UI.Shadow shadow = wasteObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);

            // Texto descriptivo
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(wasteObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            UnityEngine.UI.Text label = textObj.AddComponent<UnityEngine.UI.Text>();
            label.text = $"{def.name}\n<size=14><b>{def.category.ToString().ToUpper()}</b></size>";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 18;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.supportRichText = true;

            // Componente interactivo FallingWaste
            FallingWaste fallingWaste = wasteObj.AddComponent<FallingWaste>();
            Sprite spr = (wasteSprites != null && wasteIndex < wasteSprites.Length) ? wasteSprites[wasteIndex] : null;
            fallingWaste.Initialize(def.category, def.name, def.points, currentFallSpeed, lane, def.color, spr);

            activeWasteList.Add(fallingWaste);
            return fallingWaste;
        }

        private void EnsureLanePositions()
        {
            if (laneXPositions == null || laneXPositions.Length != laneCount)
            {
                laneXPositions = new float[laneCount];
                float startX = -360f;
                float spacing = (laneCount > 1) ? (720f / (laneCount - 1)) : 0f;
                for (int i = 0; i < laneCount; i++)
                {
                    laneXPositions[i] = startX + (i * spacing);
                }
            }
        }

        // --- Manejadores de Eventos del GameManager ---
        private void HandleGameStart()
        {
            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            float interval = currentLevel != null ? currentLevel.spawnIntervalSeconds : 2.0f;
            float speed = (GameDifficultyBalancer.Instance != null && currentLevel != null)
                ? GameDifficultyBalancer.Instance.GetFallSpeedForLevel(currentLevel.levelIndex) * 80f
                : 360f;

            ClearAllActiveWaste();
            StartSpawning(interval, speed);
        }

        private void HandleGamePaused() { }
        private void HandleGameResumed() { }
        private void HandleGameEnd()
        {
            StopSpawning();
        }

        public int LaneCount { get => laneCount; set { laneCount = Mathf.Clamp(value, 3, 5); EnsureLanePositions(); } }
        public float[] LaneXPositions => laneXPositions;
        public float CurrentFallSpeed => currentFallSpeed;
    }
}
