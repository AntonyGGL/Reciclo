using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReCiclo.Sprint6
{
    [Serializable]
    public class LevelProgressData
    {
        public int levelIndex;
        public bool isUnlocked;
        public int starsEarned;
        public int highScore;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public List<LevelProgressData> levelsProgress = new List<LevelProgressData>();
        public float bgmVolume = 0.6f;
        public float sfxVolume = 1.0f;
        public bool tutorialCompleted = false;
        public string lastSaveTime;
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SAVE_KEY = "ReCiclo_PlayerData_v1";

        [SerializeField] private PlayerSaveData currentData;

        public event Action OnDataLoaded;
        public event Action OnDataSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGame();
        }

        public void SaveGame()
        {
            currentData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(currentData, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveSystem] Datos guardados exitosamente localmente.");
            OnDataSaved?.Invoke();
        }

        public void LoadGame()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    currentData = JsonUtility.FromJson<PlayerSaveData>(json);
                    Debug.Log($"[SaveSystem] Datos cargados. Último guardado: {currentData.lastSaveTime}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Error al cargar JSON: {e.Message}. Creando nuevos datos.");
                    CreateNewSaveData();
                }
            }
            else
            {
                CreateNewSaveData();
            }

            OnDataLoaded?.Invoke();
        }

        private void CreateNewSaveData()
        {
            currentData = new PlayerSaveData();
            for (int i = 1; i <= 5; i++)
            {
                currentData.levelsProgress.Add(new LevelProgressData
                {
                    levelIndex = i,
                    isUnlocked = (i == 1), // Nivel 1 desbloqueado por defecto
                    starsEarned = 0,
                    highScore = 0
                });
            }
            SaveGame();
        }

        public LevelProgressData GetLevelData(int levelIndex)
        {
            return currentData.levelsProgress.Find(l => l.levelIndex == levelIndex);
        }

        public void SaveLevelResult(int levelIndex, int stars, int score)
        {
            LevelProgressData data = GetLevelData(levelIndex);
            if (data != null)
            {
                if (stars > data.starsEarned) data.starsEarned = stars;
                if (score > data.highScore) data.highScore = score;

                // Desbloquear siguiente nivel
                LevelProgressData nextLevelData = GetLevelData(levelIndex + 1);
                if (nextLevelData != null && stars > 0)
                {
                    nextLevelData.isUnlocked = true;
                }

                SaveGame();
            }
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            CreateNewSaveData();
            Debug.Log("[SaveSystem] Progreso reiniciado por el usuario.");
        }

        public PlayerSaveData CurrentData => currentData;
    }
}
