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
        public bool isMuted = false;
        public bool tutorialCompleted = false;
        public string lastSaveTime;
    }

    /// <summary>
    /// Sistema de guardado y persistencia local offline (Sprint 7).
    /// Guarda estrellas, puntuaciones récord, niveles desbloqueados y ajustes de audio.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem _instance;
        public static SaveSystem Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<SaveSystem>();
                return _instance;
            }
            private set => _instance = value;
        }

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

            LoadProgress();
        }

        public void SaveProgress()
        {
            if (currentData == null) currentData = new PlayerSaveData();
            currentData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(currentData, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveSystem] Progreso guardado localmente (Offline).");
            OnDataSaved?.Invoke();
        }

        public void SaveGame() => SaveProgress();

        public void LoadProgress()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    currentData = JsonUtility.FromJson<PlayerSaveData>(json);
                    Debug.Log($"[SaveSystem] Progreso cargado. Último guardado: {currentData.lastSaveTime}");
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

        public void LoadGame() => LoadProgress();

        public void SaveAudioSettings(float bgmVolume, float sfxVolume, bool isMuted = false)
        {
            if (currentData == null) currentData = new PlayerSaveData();
            currentData.bgmVolume = Mathf.Clamp01(bgmVolume);
            currentData.sfxVolume = Mathf.Clamp01(sfxVolume);
            currentData.isMuted = isMuted;
            SaveProgress();
        }

        public void LoadAudioSettings(out float bgmVolume, out float sfxVolume, out bool isMuted)
        {
            if (currentData == null) LoadProgress();
            bgmVolume = currentData.bgmVolume;
            sfxVolume = currentData.sfxVolume;
            isMuted = currentData.isMuted;
        }

        public void SaveLevelResult(int levelIndex, int stars, int score)
        {
            LevelProgressData data = GetLevelData(levelIndex);
            if (data != null)
            {
                if (stars > data.starsEarned) data.starsEarned = stars;
                if (score > data.highScore) data.highScore = score;

                // Desbloquear siguiente nivel si obtuvo al menos 1 estrella
                if (stars > 0)
                {
                    UnlockLevel(levelIndex + 1);
                }

                SaveProgress();
            }
        }

        public void UnlockLevel(int levelIndex)
        {
            LevelProgressData next = GetLevelData(levelIndex);
            if (next != null)
            {
                next.isUnlocked = true;
                Debug.Log($"[SaveSystem] ¡Nivel {levelIndex} desbloqueado!");
            }
        }

        public bool IsLevelUnlocked(int levelIndex)
        {
            LevelProgressData data = GetLevelData(levelIndex);
            return data != null && data.isUnlocked;
        }

        public int GetHighestUnlockedLevel()
        {
            if (currentData == null || currentData.levelsProgress == null) return 1;
            int highest = 1;
            foreach (var lvl in currentData.levelsProgress)
            {
                if (lvl.isUnlocked && lvl.levelIndex > highest) highest = lvl.levelIndex;
            }
            return highest;
        }

        public int GetLevelStars(int levelIndex)
        {
            LevelProgressData data = GetLevelData(levelIndex);
            return data != null ? data.starsEarned : 0;
        }

        public int GetLevelHighScore(int levelIndex)
        {
            LevelProgressData data = GetLevelData(levelIndex);
            return data != null ? data.highScore : 0;
        }

        public LevelProgressData GetLevelData(int levelIndex)
        {
            if (currentData == null || currentData.levelsProgress == null) return null;
            return currentData.levelsProgress.Find(l => l.levelIndex == levelIndex);
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            CreateNewSaveData();
            Debug.Log("[SaveSystem] Progreso reiniciado a estado inicial.");
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
            SaveProgress();
        }

        public PlayerSaveData CurrentData => currentData;
    }
}
