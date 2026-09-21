using System;
using UnityEngine;
using ReCiclo.Sprint1;
using ReCiclo.Sprint4;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint2
{
    public enum GameState
    {
        Init,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Estado del Juego")]
        [SerializeField] private GameState currentState = GameState.Init;
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int totalHits = 0;
        [SerializeField] private int totalErrors = 0;

        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            currentScore = 0;
            totalHits = 0;
            totalErrors = 0;
            SetState(GameState.Playing);

            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            float duration = currentLevel != null ? currentLevel.durationSeconds : 90f;
            float spawnInterval = currentLevel != null ? currentLevel.spawnIntervalSeconds : 2.5f;

            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StartTimer(duration);
            }

            if (PollutionBar.Instance != null)
            {
                PollutionBar.Instance.ResetBar();
            }

            if (WasteSpawner.Instance != null)
            {
                WasteSpawner.Instance.StartSpawning(spawnInterval);
            }

            if (currentLevel != null && currentLevel.isBossLevel && BossController.Instance != null)
            {
                BossController.Instance.StartBossBattle();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(true);
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM(false);
            }

            OnScoreChanged?.Invoke(currentScore);
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Victory:
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    break;
            }
        }

        public void AddScore(int amount)
        {
            totalHits++;
            int multiplier = ComboSystem.Instance != null ? ComboSystem.Instance.CurrentMultiplier : 1;
            int finalAdd = amount * multiplier;
            currentScore += finalAdd;

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.AddCombo();
            }

            OnScoreChanged?.Invoke(currentScore);
        }

        public void RegisterError()
        {
            totalErrors++;

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.ResetCombo();
            }

            if (PollutionBar.Instance != null)
            {
                PollutionBar.Instance.AddPollution(20f);
            }
        }

        public void OnTimeExpired()
        {
            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            int target = currentLevel != null ? currentLevel.targetScore : 800;

            // Para ganar, el jugador debe haber acertado residuos y haber alcanzado al menos el 50% del puntaje objetivo
            if (totalHits > 0 && currentScore >= Mathf.RoundToInt(target * 0.5f))
            {
                TriggerVictory();
            }
            else
            {
                TriggerGameOver();
            }
        }

        public void TriggerVictory()
        {
            SetState(GameState.Victory);

            if (WasteSpawner.Instance != null)
            {
                WasteSpawner.Instance.StopSpawning();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SoundType.Victory);
            }

            if (VisualJuiceEffects.Instance != null)
            {
                VisualJuiceEffects.Instance.PlayVictoryConfetti();
            }

            float accuracy = (totalHits + totalErrors > 0) ? ((float)totalHits / (totalHits + totalErrors)) * 100f : 0f;

            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            int levelIndex = currentLevel != null ? currentLevel.levelIndex : 1;

            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.CompleteLevel(levelIndex, currentScore, accuracy);
            }

            if (EndGameUI.Instance != null)
            {
                EndGameUI.Instance.ShowVictory(currentScore, accuracy);
            }
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);

            if (WasteSpawner.Instance != null)
            {
                WasteSpawner.Instance.StopSpawning();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SoundType.GameOver);
            }

            if (EndGameUI.Instance != null)
            {
                EndGameUI.Instance.ShowGameOver(currentScore);
            }
        }

        public int CurrentScore => currentScore;
        public GameState CurrentState => currentState;
    }
}
