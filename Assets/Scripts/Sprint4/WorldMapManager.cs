using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReCiclo.Sprint4
{
    [System.Serializable]
    public enum WasteCategory
    {
        Paper,       // Azul - Nivel 1: Plaza Central
        Plastic,     // Amarillo - Nivel 2: Mercado
        Glass,       // Verde - Nivel 3: Parque
        Organic,     // Marrón - Nivel 4: Zona Industrial
        Electronic   // Rojo - Nivel 5: Río / Periferia (Jefe Final)
    }

    [System.Serializable]
    public class LevelData
    {
        public int levelIndex;
        public string levelName;
        public string districtName;
        public WasteCategory primaryCategory;
        public Color themeColor;
        public float durationSeconds = 90f;
        public int targetScore = 1000;
        public float spawnIntervalSeconds = 2.5f;
        public bool isBossLevel = false;
        public bool isUnlocked = false;
        public int starsEarned = 0; // 0 a 3 estrellas
    }

    public class WorldMapManager : MonoBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Configuración de Barrios y Niveles")]
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private int currentLevelIndex = 0;

        public event Action<int> OnLevelUnlocked;
        public event Action<int, int> OnStarsUpdated; // levelIndex, stars
        public event Action<LevelData> OnLevelSelected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDefaultLevels();
        }

        private void InitializeDefaultLevels()
        {
            if (levels != null && levels.Count > 0) return;

            levels = new List<LevelData>
            {
                new LevelData
                {
                    levelIndex = 1,
                    levelName = "Nivel 1: Plaza Central",
                    districtName = "Plaza Central",
                    primaryCategory = WasteCategory.Paper,
                    themeColor = new Color(0.2f, 0.5f, 0.9f), // Azul
                    durationSeconds = 90f,
                    targetScore = 800,
                    spawnIntervalSeconds = 2.5f,
                    isBossLevel = false,
                    isUnlocked = true,
                    starsEarned = 0
                },
                new LevelData
                {
                    levelIndex = 2,
                    levelName = "Nivel 2: Mercado",
                    districtName = "Mercado Municipal",
                    primaryCategory = WasteCategory.Plastic,
                    themeColor = new Color(0.95f, 0.8f, 0.2f), // Amarillo
                    durationSeconds = 90f,
                    targetScore = 1200,
                    spawnIntervalSeconds = 2.2f,
                    isBossLevel = false,
                    isUnlocked = false,
                    starsEarned = 0
                },
                new LevelData
                {
                    levelIndex = 3,
                    levelName = "Nivel 3: Parque",
                    districtName = "Parque Ecológico",
                    primaryCategory = WasteCategory.Glass,
                    themeColor = new Color(0.2f, 0.75f, 0.3f), // Verde
                    durationSeconds = 100f,
                    targetScore = 1500,
                    spawnIntervalSeconds = 2.0f,
                    isBossLevel = false,
                    isUnlocked = false,
                    starsEarned = 0
                },
                new LevelData
                {
                    levelIndex = 4,
                    levelName = "Nivel 4: Zona Industrial",
                    districtName = "Zona Industrial",
                    primaryCategory = WasteCategory.Organic,
                    themeColor = new Color(0.55f, 0.35f, 0.2f), // Marrón
                    durationSeconds = 110f,
                    targetScore = 2000,
                    spawnIntervalSeconds = 1.8f,
                    isBossLevel = false,
                    isUnlocked = false,
                    starsEarned = 0
                },
                new LevelData
                {
                    levelIndex = 5,
                    levelName = "Nivel 5: Río / Periferia - Batalla de Jefe",
                    districtName = "Río y Periferia",
                    primaryCategory = WasteCategory.Electronic,
                    themeColor = new Color(0.9f, 0.25f, 0.25f), // Rojo
                    durationSeconds = 120f,
                    targetScore = 2500,
                    spawnIntervalSeconds = 1.5f,
                    isBossLevel = true,
                    isUnlocked = false,
                    starsEarned = 0
                }
            };
        }

        public LevelData GetCurrentLevel()
        {
            if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            {
                return levels[currentLevelIndex];
            }
            return levels[0];
        }

        public LevelData GetLevel(int index)
        {
            if (index >= 0 && index < levels.Count)
            {
                return levels[index];
            }
            return null;
        }

        public List<LevelData> GetAllLevels()
        {
            return levels;
        }

        public bool SelectLevel(int levelIndex)
        {
            int listIndex = levelIndex - 1;
            if (listIndex >= 0 && listIndex < levels.Count)
            {
                if (levels[listIndex].isUnlocked)
                {
                    currentLevelIndex = listIndex;
                    OnLevelSelected?.Invoke(levels[listIndex]);
                    Debug.Log($"[WorldMapManager] Nivel seleccionado: {levels[listIndex].levelName}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[WorldMapManager] El nivel {levelIndex} está bloqueado.");
                }
            }
            return false;
        }

        public void CompleteLevel(int levelIndex, int scoreAchieved, float precisionPercent)
        {
            int listIndex = levelIndex - 1;
            if (listIndex < 0 || listIndex >= levels.Count) return;

            LevelData level = levels[listIndex];
            int stars = CalculateStars(scoreAchieved, level.targetScore, precisionPercent);

            if (stars > level.starsEarned)
            {
                level.starsEarned = stars;
                OnStarsUpdated?.Invoke(levelIndex, stars);
            }

            // Desbloquear siguiente nivel si fue superado con al menos 1 estrella
            if (stars > 0 && listIndex + 1 < levels.Count)
            {
                if (!levels[listIndex + 1].isUnlocked)
                {
                    levels[listIndex + 1].isUnlocked = true;
                    OnLevelUnlocked?.Invoke(listIndex + 2);
                    Debug.Log($"[WorldMapManager] ¡Nuevo barrio desbloqueado! Nivel {listIndex + 2}: {levels[listIndex + 1].districtName}");
                }
            }
        }

        private int CalculateStars(int score, int targetScore, float precisionPercent)
        {
            if (score < targetScore * 0.5f) return 0;
            if (score < targetScore || precisionPercent < 60f) return 1;
            if (score < targetScore * 1.3f || precisionPercent < 85f) return 2;
            return 3;
        }
    }
}
