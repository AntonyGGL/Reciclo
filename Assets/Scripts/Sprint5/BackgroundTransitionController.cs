using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint2;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint5
{
    public enum DefeatBackgroundBehavior
    {
        FreezeInCurrentState,   // Congelar fondo en su estado actual
        RevertToDirtyGradual,   // Regresar gradualmente a estado sucio
        InstantDirty            // Cambio instantáneo a estado sucio
    }

    [System.Serializable]
    public class DistrictThemeData
    {
        public string districtName = "Plaza Central";
        public Sprite dirtySprite;
        public Sprite midSprite;
        public Sprite cleanSprite;
        public Color dirtyColor = new Color(0.28f, 0.25f, 0.28f, 1f);  // Contaminado / Gris oscuro
        public Color midColor = new Color(0.45f, 0.55f, 0.55f, 1f);    // En recuperación
        public Color cleanColor = new Color(0.25f, 0.75f, 0.95f, 1f);  // Ciudad Limpia / Azul cielo
    }

    /// <summary>
    /// Controlador de transición visual dinámica del escenario según el desempeño del jugador en tiempo real.
    /// Realiza crossfade de opacidad entre las capas: Sucio (0-35%), Intermedio (36-70%) y Limpio (71-100%).
    /// </summary>
    public class BackgroundTransitionController : MonoBehaviour
    {
        private static BackgroundTransitionController _instance;
        public static BackgroundTransitionController Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<BackgroundTransitionController>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Capas de Fondo (Crossfade UI)")]
        [SerializeField] private CanvasGroup dirtyCanvasGroup;
        [SerializeField] private CanvasGroup midCanvasGroup;
        [SerializeField] private CanvasGroup cleanCanvasGroup;

        [Header("Imágenes de las Capas")]
        [SerializeField] private UnityEngine.UI.Image dirtyImage;
        [SerializeField] private UnityEngine.UI.Image midImage;
        [SerializeField] private UnityEngine.UI.Image cleanImage;
        [SerializeField] private UnityEngine.UI.Text districtLabelText;
        [SerializeField] private UnityEngine.UI.Text cleanlinessStatusText;

        [Header("Umbrales de Estado Visual")]
        [Range(0.1f, 0.5f)] [SerializeField] private float dirtyThreshold = 0.35f; // 0% a 35%
        [Range(0.5f, 0.9f)] [SerializeField] private float midThreshold = 0.70f;   // 36% a 70%

        [Header("Comportamiento al Perder")]
        [SerializeField] private DefeatBackgroundBehavior defeatBehavior = DefeatBackgroundBehavior.RevertToDirtyGradual;
        [SerializeField] private float revertSpeed = 1.2f;

        [Header("Configuración de Transición")]
        [SerializeField] private float transitionSpeed = 3.0f;
        [SerializeField] private int targetLevelScore = 1000;

        [Header("Temas de los 5 Barrios")]
        [SerializeField] private List<DistrictThemeData> districtThemes = new List<DistrictThemeData>();
        [SerializeField] private int currentDistrictIndex = 0;

        [Header("Estado de Progreso en Tiempo Real")]
        [Range(0f, 1f)] [SerializeField] private float currentCleanlinessProgress = 0f;
        [Range(0f, 1f)] [SerializeField] private float targetCleanlinessProgress = 0f;
        [SerializeField] private bool isGameOverFrozen = false;

        public event Action<float> OnCleanlinessProgressChanged; // 0.0 a 1.0
        public event Action<string> OnDistrictChanged;

        public float CleanlinessProgress => currentCleanlinessProgress;
        public float CleanlinessPercent => currentCleanlinessProgress * 100f;
        public DefeatBackgroundBehavior DefeatBehavior { get => defeatBehavior; set => defeatBehavior = value; }

        private Coroutine revertRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultThemes();
        }

        private void Start()
        {
            // Suscripción a eventos centrales
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += HandleGameStart;
                GameManager.Instance.OnScoreChanged += HandleScoreChanged;
                GameManager.Instance.OnGameOver += HandleGameOver;
                GameManager.Instance.OnVictory += HandleVictory;
            }

            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.OnVeroDefeated += HandleVeroDefeated;
            }

            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.OnLevelSelected += HandleLevelSelected;
                var currentLevel = WorldMapManager.Instance.GetCurrentLevel();
                if (currentLevel != null)
                {
                    SetDistrict(currentLevel.levelIndex - 1);
                }
            }
            else
            {
                SetDistrict(0);
            }

            SetProgress(0f, true);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= HandleGameStart;
                GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
                GameManager.Instance.OnGameOver -= HandleGameOver;
                GameManager.Instance.OnVictory -= HandleVictory;
            }

            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.OnVeroDefeated -= HandleVeroDefeated;
            }

            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.OnLevelSelected -= HandleLevelSelected;
            }
        }

        private void Update()
        {
            if (isGameOverFrozen) return;

            // Interpolación suave y optimizada para evitar cambios bruscos
            if (!Mathf.Approximately(currentCleanlinessProgress, targetCleanlinessProgress))
            {
                currentCleanlinessProgress = Mathf.MoveTowards(
                    currentCleanlinessProgress,
                    targetCleanlinessProgress,
                    Time.deltaTime * transitionSpeed
                );

                ApplyLayerCrossfade(currentCleanlinessProgress);
                UpdateStatusText(currentCleanlinessProgress);
            }
        }

        /// <summary>
        /// Establece el progreso de limpieza deseado (0.0 a 1.0).
        /// </summary>
        public void SetProgress(float progress, bool immediate = false)
        {
            targetCleanlinessProgress = Mathf.Clamp01(progress);

            if (immediate)
            {
                currentCleanlinessProgress = targetCleanlinessProgress;
                ApplyLayerCrossfade(currentCleanlinessProgress);
                UpdateStatusText(currentCleanlinessProgress);
            }

            OnCleanlinessProgressChanged?.Invoke(targetCleanlinessProgress);
        }

        /// <summary>
        /// Calcula el progreso en base al puntaje y precisión del jugador durante la partida.
        /// </summary>
        public void EvaluatePlayerProgress(int score, int hits, int errors)
        {
            if (isGameOverFrozen) return;

            int total = hits + errors;
            float accuracyRatio = (total > 0) ? ((float)hits / total) : 0f;
            float scoreRatio = (targetLevelScore > 0) ? ((float)score / targetLevelScore) : 0f;

            // Ponderación: 60% peso del puntaje objetivo + 40% precisión
            float calculatedProgress = Mathf.Clamp01((scoreRatio * 0.6f) + (accuracyRatio * 0.4f));

            SetProgress(calculatedProgress, false);
        }

        /// <summary>
        /// Realiza el crossfade de opacidad entre las 3 capas: Sucio, Intermedio y Limpio.
        /// </summary>
        private void ApplyLayerCrossfade(float progress)
        {
            float dirtyAlpha = 0f;
            float midAlpha = 0f;
            float cleanAlpha = 0f;

            if (progress <= dirtyThreshold)
            {
                // 0% a 35%: Sucio puro a transición intermedia temprana
                float t = (dirtyThreshold > 0f) ? (progress / dirtyThreshold) : 0f;
                dirtyAlpha = 1f - (t * 0.5f);
                midAlpha = t * 0.5f;
                cleanAlpha = 0f;
            }
            else if (progress <= midThreshold)
            {
                // 36% a 70%: Transición intermedia a limpia
                float t = (progress - dirtyThreshold) / (midThreshold - dirtyThreshold);
                dirtyAlpha = Mathf.Lerp(0.5f, 0f, t);
                midAlpha = Mathf.Lerp(0.5f, 1f, t < 0.5f ? t * 2f : (1f - (t - 0.5f) * 2f));
                cleanAlpha = Mathf.Lerp(0f, 0.7f, t);
            }
            else
            {
                // 71% a 100%: Ciudad Limpia dominante
                float t = (progress - midThreshold) / (1f - midThreshold);
                dirtyAlpha = 0f;
                midAlpha = Mathf.Lerp(0.3f, 0f, t);
                cleanAlpha = Mathf.Lerp(0.7f, 1f, t);
            }

            if (dirtyCanvasGroup != null) dirtyCanvasGroup.alpha = dirtyAlpha;
            if (midCanvasGroup != null) midCanvasGroup.alpha = midAlpha;
            if (cleanCanvasGroup != null) cleanCanvasGroup.alpha = cleanAlpha;
        }

        private void UpdateStatusText(float progress)
        {
            if (cleanlinessStatusText == null) return;

            string status;
            Color statusColor;

            if (progress < dirtyThreshold)
            {
                status = $"🌫️ AMBIENTE CONTAMINADO ({Mathf.RoundToInt(progress * 100)}% Limpio)";
                statusColor = new Color(0.85f, 0.35f, 0.35f);
            }
            else if (progress < midThreshold)
            {
                status = $"🌱 EN RECUPERACIÓN ({Mathf.RoundToInt(progress * 100)}% Limpio)";
                statusColor = new Color(0.95f, 0.8f, 0.25f);
            }
            else
            {
                status = $"✨ CIUDAD LIMPIA ({Mathf.RoundToInt(progress * 100)}% Limpio)";
                statusColor = new Color(0.3f, 0.95f, 0.45f);
            }

            cleanlinessStatusText.text = status;
            cleanlinessStatusText.color = statusColor;
        }

        /// <summary>
        /// Cambia el barrio activo (0 a 4) aplicando sus respectivos fondos y paletas.
        /// </summary>
        public void SetDistrict(int districtIndex)
        {
            if (districtThemes == null || districtThemes.Count == 0) return;

            districtIndex = Mathf.Clamp(districtIndex, 0, districtThemes.Count - 1);
            currentDistrictIndex = districtIndex;
            var theme = districtThemes[districtIndex];

            if (dirtyImage != null)
            {
                dirtyImage.color = theme.dirtyColor;
                if (theme.dirtySprite != null) dirtyImage.sprite = theme.dirtySprite;
            }

            if (midImage != null)
            {
                midImage.color = theme.midColor;
                if (theme.midSprite != null) midImage.sprite = theme.midSprite;
            }

            if (cleanImage != null)
            {
                cleanImage.color = theme.cleanColor;
                if (theme.cleanSprite != null) cleanImage.sprite = theme.cleanSprite;
            }

            if (districtLabelText != null)
            {
                districtLabelText.text = $"📍 BARRIO: {theme.districtName.ToUpper()}";
            }

            OnDistrictChanged?.Invoke(theme.districtName);
            Debug.Log($"[BackgroundTransitionController] Barrio asignado: {theme.districtName}");
        }

        private void InitializeDefaultThemes()
        {
            if (districtThemes != null && districtThemes.Count >= 5) return;

            districtThemes = new List<DistrictThemeData>
            {
                new DistrictThemeData
                {
                    districtName = "Plaza Central",
                    dirtyColor = new Color(0.25f, 0.25f, 0.32f, 1f), // Gris azulado opaco
                    midColor = new Color(0.35f, 0.55f, 0.70f, 1f),   // Azul urbano
                    cleanColor = new Color(0.20f, 0.65f, 0.95f, 1f)  // Cielo azul despejado
                },
                new DistrictThemeData
                {
                    districtName = "Mercado Municipal",
                    dirtyColor = new Color(0.32f, 0.28f, 0.18f, 1f), // Ocre sucio
                    midColor = new Color(0.65f, 0.60f, 0.35f, 1f),   // Amarillo pálido
                    cleanColor = new Color(0.95f, 0.80f, 0.20f, 1f)  // Amarillo brillante y limpio
                },
                new DistrictThemeData
                {
                    districtName = "Parque Ecológico",
                    dirtyColor = new Color(0.22f, 0.28f, 0.20f, 1f), // Verde marchito/oscuro
                    midColor = new Color(0.35f, 0.60f, 0.30f, 1f),   // Verde en brote
                    cleanColor = new Color(0.20f, 0.85f, 0.35f, 1f)  // Verde esmeralda resplandeciente
                },
                new DistrictThemeData
                {
                    districtName = "Zona Industrial",
                    dirtyColor = new Color(0.30f, 0.20f, 0.15f, 1f), // Óxido / Hollín marrón
                    midColor = new Color(0.55f, 0.40f, 0.30f, 1f),   // Bronce filtrado
                    cleanColor = new Color(0.60f, 0.75f, 0.85f, 1f)  // Acero limpio y aire puro
                },
                new DistrictThemeData
                {
                    districtName = "Río y Periferia",
                    dirtyColor = new Color(0.30f, 0.12f, 0.35f, 1f), // Morado tóxico de Sr. Basura
                    midColor = new Color(0.35f, 0.45f, 0.65f, 1f),   // Agua en clarificación
                    cleanColor = new Color(0.10f, 0.75f, 0.85f, 1f)  // Agua cristalina turquesa
                }
            };
        }

        // --- Manejadores de Eventos del Juego ---
        private void HandleGameStart()
        {
            isGameOverFrozen = false;
            if (revertRoutine != null) StopCoroutine(revertRoutine);

            var currentLevel = (WorldMapManager.Instance != null) ? WorldMapManager.Instance.GetCurrentLevel() : null;
            if (currentLevel != null)
            {
                targetLevelScore = currentLevel.targetScore;
                SetDistrict(currentLevel.levelIndex - 1);
            }

            SetProgress(0.05f, true);
        }

        private void HandleScoreChanged(int currentScore)
        {
            if (isGameOverFrozen) return;

            int hits = (GameManager.Instance != null) ? GameManager.Instance.TotalHits : 0;
            int errors = (GameManager.Instance != null) ? GameManager.Instance.TotalErrors : 0;

            EvaluatePlayerProgress(currentScore, hits, errors);
        }

        private void HandleVeroDefeated()
        {
            ApplyDefeatBehavior();
        }

        private void HandleGameOver()
        {
            ApplyDefeatBehavior();
        }

        private void ApplyDefeatBehavior()
        {
            switch (defeatBehavior)
            {
                case DefeatBackgroundBehavior.FreezeInCurrentState:
                    isGameOverFrozen = true;
                    Debug.Log("[BackgroundTransitionController] Fondo congelado en su estado actual tras la derrota.");
                    break;

                case DefeatBackgroundBehavior.InstantDirty:
                    isGameOverFrozen = true;
                    SetProgress(0f, true);
                    Debug.Log("[BackgroundTransitionController] Fondo reiniciado instantáneamente a estado sucio.");
                    break;

                case DefeatBackgroundBehavior.RevertToDirtyGradual:
                    isGameOverFrozen = false;
                    if (revertRoutine != null) StopCoroutine(revertRoutine);
                    revertRoutine = StartCoroutine(RevertToDirtyRoutine());
                    Debug.Log("[BackgroundTransitionController] Regresando gradualmente a estado sucio.");
                    break;
            }
        }

        private IEnumerator RevertToDirtyRoutine()
        {
            while (currentCleanlinessProgress > 0f)
            {
                currentCleanlinessProgress = Mathf.MoveTowards(
                    currentCleanlinessProgress,
                    0f,
                    Time.unscaledDeltaTime * revertSpeed
                );

                ApplyLayerCrossfade(currentCleanlinessProgress);
                UpdateStatusText(currentCleanlinessProgress);
                yield return null;
            }
            isGameOverFrozen = true;
        }

        private void HandleVictory()
        {
            isGameOverFrozen = false;
            SetProgress(1.0f, false);
            Debug.Log("[BackgroundTransitionController] ¡Victoria lograda! Escenario en Ciudad Limpia al 100%.");
        }

        private void HandleLevelSelected(LevelData level)
        {
            if (level != null)
            {
                targetLevelScore = level.targetScore;
                SetDistrict(level.levelIndex - 1);
            }
        }
    }
}