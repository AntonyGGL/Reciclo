using System;
using System.Collections.Generic;
using UnityEngine;
using ReCiclo.Sprint1;
using ReCiclo.Sprint2;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint6
{
    [Serializable]
    public class DistrictBalanceConfig
    {
        public int levelIndex;
        public string districtName;
        public float fallSpeed = 320f;
        public float spawnInterval = 2.0f;
        public int wastePerWave = 1;
        public float levelDuration = 90f;
        public float errorDamage = 15f;
        public int targetScore = 1000;
        public bool hasBoss = false;
        public float bossHealth = 500f;
    }

    /// <summary>
    /// Balanceador de dificultad progresiva por barrio (Sprint 7).
    /// Controla curvas de velocidad, cadencia de oleadas, penalización de error y batalla final contra Sr. Basura.
    /// </summary>
    public class GameDifficultyBalancer : MonoBehaviour
    {
        private static GameDifficultyBalancer _instance;
        public static GameDifficultyBalancer Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<GameDifficultyBalancer>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuración de Curvas por Barrio")]
        [SerializeField] private List<DistrictBalanceConfig> districtConfigs = new List<DistrictBalanceConfig>();

        [Header("Nivel Activo")]
        [SerializeField] private int activeLevel = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultCurves();
        }

        private void InitializeDefaultCurves()
        {
            if (districtConfigs != null && districtConfigs.Count > 0) return;

            districtConfigs = new List<DistrictBalanceConfig>
            {
                // Nivel 1: Plaza Central (Introducción amigable)
                new DistrictBalanceConfig
                {
                    levelIndex = 1,
                    districtName = "Plaza Central",
                    fallSpeed = 300f,
                    spawnInterval = 2.2f,
                    wastePerWave = 1,
                    levelDuration = 90f,
                    errorDamage = 10f,
                    targetScore = 800,
                    hasBoss = false
                },
                // Nivel 2: Barrio Comercial
                new DistrictBalanceConfig
                {
                    levelIndex = 2,
                    districtName = "Barrio Comercial",
                    fallSpeed = 360f,
                    spawnInterval = 1.8f,
                    wastePerWave = 1,
                    levelDuration = 85f,
                    errorDamage = 15f,
                    targetScore = 1200,
                    hasBoss = false
                },
                // Nivel 3: Zona Residencial
                new DistrictBalanceConfig
                {
                    levelIndex = 3,
                    districtName = "Zona Residencial",
                    fallSpeed = 420f,
                    spawnInterval = 1.5f,
                    wastePerWave = 2,
                    levelDuration = 80f,
                    errorDamage = 20f,
                    targetScore = 1600,
                    hasBoss = false
                },
                // Nivel 4: Zona Industrial
                new DistrictBalanceConfig
                {
                    levelIndex = 4,
                    districtName = "Zona Industrial",
                    fallSpeed = 480f,
                    spawnInterval = 1.25f,
                    wastePerWave = 2,
                    levelDuration = 75f,
                    errorDamage = 25f,
                    targetScore = 2000,
                    hasBoss = false
                },
                // Nivel 5: Vertedero / Río (Batalla Final contra Sr. Basura)
                new DistrictBalanceConfig
                {
                    levelIndex = 5,
                    districtName = "Río Huaycoloro - Vertedero",
                    fallSpeed = 540f,
                    spawnInterval = 1.0f,
                    wastePerWave = 3,
                    levelDuration = 120f,
                    errorDamage = 30f,
                    targetScore = 3000,
                    hasBoss = true,
                    bossHealth = 500f
                }
            };
        }

        public DistrictBalanceConfig GetConfigForLevel(int levelIndex)
        {
            if (districtConfigs == null || districtConfigs.Count == 0) InitializeDefaultCurves();
            var cfg = districtConfigs.Find(c => c.levelIndex == levelIndex);
            return cfg ?? districtConfigs[0];
        }

        public void ApplyDifficultyToLevel(int levelIndex)
        {
            activeLevel = levelIndex;
            DistrictBalanceConfig cfg = GetConfigForLevel(levelIndex);

            // 1. Spawner de carriles
            if (LaneSpawner.Instance != null)
            {
                LaneSpawner.Instance.StartSpawning(cfg.spawnInterval, cfg.fallSpeed);
            }

            // 2. Temporizador
            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StartTimer(cfg.levelDuration);
            }

            // 3. Daño de Vero
            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.SetErrorDamage(cfg.errorDamage);
            }

            // 4. Jefe Sr. Basura
            if (BossController.Instance != null)
            {
                BossController.Instance.gameObject.SetActive(cfg.hasBoss);
                if (cfg.hasBoss)
                {
                    BossController.Instance.InitializeBoss(cfg.bossHealth);
                }
            }

            Debug.Log($"[GameDifficultyBalancer] Dificultad aplicada para Nivel {levelIndex} ({cfg.districtName}): Vel={cfg.fallSpeed}, Spawn={cfg.spawnInterval}s, Daño={cfg.errorDamage}, Jefe={cfg.hasBoss}");
        }

        public float GetFallSpeedForLevel(int levelIndex) => GetConfigForLevel(levelIndex).fallSpeed;
        public float GetSpawnIntervalForLevel(int levelIndex) => GetConfigForLevel(levelIndex).spawnInterval;
        public float GetErrorPenalizationForLevel(int levelIndex) => GetConfigForLevel(levelIndex).errorDamage;
    }
}
