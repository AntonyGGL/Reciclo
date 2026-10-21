using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Inicio del nivel")]
        [SerializeField] private float countdownStepSeconds = 1f;

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

        private Coroutine startRoutine;
        private GameObject countdownOverlay;
        private Text countdownText;

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
            if (startRoutine != null) StopCoroutine(startRoutine);
            startRoutine = StartCoroutine(StartLevelSequence());
        }

        private IEnumerator StartLevelSequence()
        {
            Time.timeScale = 1f;
            currentScore = 0;
            totalHits = 0;
            totalErrors = 0;

            SetState(GameState.Ready);

            if (LevelTimer.Instance != null) LevelTimer.Instance.StopTimer();
            if (LaneSpawner.Instance != null)
            {
                LaneSpawner.Instance.StopSpawning();
                LaneSpawner.Instance.ClearAllActiveWaste();
            }

            EnsureCountdownUI();
            if (countdownOverlay == null || countdownText == null)
            {
                startRoutine = null;
                StartGameplay();
                yield break;
            }
            countdownOverlay.SetActive(true);
            countdownOverlay.transform.SetAsLastSibling();

            string[] steps = { "3", "2", "1", "¡VAMOS!" };
            foreach (string step in steps)
            {
                countdownText.text = step;
                countdownText.fontSize = step == "¡VAMOS!" ? 108 : 180;
                countdownText.transform.localScale = Vector3.one;
                yield return StartCoroutine(AnimateCountdownStep(countdownStepSeconds));
            }

            countdownOverlay.SetActive(false);
            startRoutine = null;
            StartGameplay();
        }

        private IEnumerator AnimateCountdownStep(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Lerp(1.35f, .9f, progress);
                countdownText.transform.localScale = Vector3.one * scale;
                Color color = countdownText.color;
                color.a = progress < .75f ? 1f : 1f - ((progress - .75f) / .25f);
                countdownText.color = color;
                yield return null;
            }
            Color resetColor = countdownText.color;
            resetColor.a = 1f;
            countdownText.color = resetColor;
        }

        private void EnsureCountdownUI()
        {
            if (countdownOverlay != null) return;
            GameObject canvasObject = GameObject.Find("GameCanvas");
            Canvas canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            countdownOverlay = new GameObject("LevelCountdown", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            countdownOverlay.transform.SetParent(canvas.transform, false);
            RectTransform overlayRect = countdownOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            countdownOverlay.GetComponent<Image>().color = new Color(.02f, .08f, .10f, .48f);

            GameObject textObject = new GameObject("CountdownText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(countdownOverlay.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(.2f, .3f);
            textRect.anchorMax = new Vector2(.8f, .75f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            countdownText = textObject.GetComponent<Text>();
            countdownText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countdownText.fontStyle = FontStyle.Bold;
            countdownText.alignment = TextAnchor.MiddleCenter;
            countdownText.color = new Color32(255, 239, 104, 255);
            countdownText.resizeTextForBestFit = true;
            countdownText.resizeTextMinSize = 48;
            countdownText.resizeTextMaxSize = 180;
            Outline outline = textObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, .18f, .12f, 1f);
            outline.effectDistance = new Vector2(5f, -5f);
        }

        private void StartGameplay()
        {
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

            if (BossController.Instance != null)
            {
                BossController.Instance.RecoverHealthFromPlayerError();
            }
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
            if (LevelTimer.Instance != null) LevelTimer.Instance.StopTimer();

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
            if (LevelTimer.Instance != null) LevelTimer.Instance.StopTimer();

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
