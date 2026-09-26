using System;
using UnityEngine;
using ReCiclo.Sprint1;
using ReCiclo.Sprint3;
using ReCiclo.Sprint4;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint2
{
    public enum GameState
    {
        Ready,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    /// <summary>
    /// Controlador central del ciclo de juego.
    /// Administra estados, pausas, transiciones y expone eventos C# desacoplados.
    /// Sprint 6: Integra PowerUpManager y EducationalPopup.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Estado del Juego")]
        [SerializeField] private GameState currentState = GameState.Ready;
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int totalHits = 0;
        [SerializeField] private int totalErrors = 0;

        // Eventos C# publicos segun especificacion de Sprint 2
        public event Action OnGameStart;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnVictory;
        public event Action OnGameOver;
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;

        public GameState CurrentState => currentState;
        public int CurrentScore => currentScore;
        public int TotalHits => totalHits;
        public int TotalErrors => totalErrors;

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

        public void SetReady()
        {
            SetState(GameState.Ready);
        }

        public void StartGame()
        {
            currentScore = 0;
            totalHits = 0;
            totalErrors = 0;

            SetState(GameState.Playing);

            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            float duration = currentLevel != null ? currentLevel.durationSeconds : 90f;

            if (LevelTimer.Instance != null)
            {
                LevelTimer.Instance.StartTimer(duration);
            }

            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.ResetHealth();
            }

            if (ComboSystem.Instance != null)
            {
                ComboSystem.Instance.ResetCombo();
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

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.LevelStart);

            OnGameStart?.Invoke();
            OnScoreChanged?.Invoke(currentScore);
            Debug.Log("[GameManager] Juego iniciado en estado PLAYING.");
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
            Debug.Log("[GameManager] Juego pausado.");
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
            Debug.Log("[GameManager] Juego reanudado.");
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing) PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }

        public void SetState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            if (newState == GameState.Playing || newState == GameState.Ready || newState == GameState.Victory || newState == GameState.GameOver)
            {
                Time.timeScale = 1f;
            }
        }

        public void AddScore(int basePoints)
        {
            if (currentState != GameState.Playing) return;

            totalHits++;
            int comboMultiplier = (ComboSystem.Instance != null) ? ComboSystem.Instance.CurrentMultiplier : 1;
            int powerUpMultiplier = (PowerUpManager.Instance != null) ? PowerUpManager.Instance.GetScoreMultiplier() : 1;
            int finalAdd = basePoints * comboMultiplier * powerUpMultiplier;
            currentScore += finalAdd;

            OnScoreChanged?.Invoke(currentScore);
        }

        public void RegisterError()
        {
            if (currentState != GameState.Playing) return;
            totalErrors++;
        }

        public void OnTimeExpired()
        {
            LevelData currentLevel = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            int target = currentLevel != null ? currentLevel.targetScore : 800;

            if (totalHits > 0 && currentScore >= Mathf.RoundToInt(target * 0.45f))
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
            if (currentState == GameState.Victory) return;
            SetState(GameState.Victory);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.Victory);
            if (VisualJuiceEffects.Instance != null) VisualJuiceEffects.Instance.PlayVictoryConfetti();

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

            // Sprint 6: Mostrar dato educativo aleatorio tras la victoria
            if (EducationalPopup.Instance != null)
            {
                EducationalPopup.Instance.ShowRandomFact();
            }

            OnVictory?.Invoke();
            Debug.Log($"[GameManager] VICTORIA! Puntaje: {currentScore}, Precision: {accuracy:F1}%");
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;
            SetState(GameState.GameOver);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.GameOver);

            if (EndGameUI.Instance != null)
            {
                EndGameUI.Instance.ShowGameOver(currentScore);
            }

            OnGameOver?.Invoke();
            Debug.Log($"[GameManager] GAME OVER. Puntaje final: {currentScore}");
        }
    }
}