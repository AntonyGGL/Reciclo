using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint6
{
    /// <summary>
    /// Menu de pausa con opciones de reanudar, reiniciar, ajustes y salir.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;

        [Header("Botones")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private string worldMapSceneName = "MapaMundial";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (pausePanel != null) pausePanel.SetActive(false);
            EnsureMapButton();
            BindButtons();
        }

        private void BindButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
                resumeButton.onClick.AddListener(OnResumeClicked);
            }
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            if (mapButton != null)
            {
                mapButton.onClick.RemoveAllListeners();
                mapButton.onClick.AddListener(OnMapClicked);
            }
        }

        private void EnsureMapButton()
        {
            if (mapButton != null || settingsButton == null) return;

            GameObject mapButtonObject = Instantiate(settingsButton.gameObject, settingsButton.transform.parent);
            mapButtonObject.name = "Btn_Map";
            mapButton = mapButtonObject.GetComponent<Button>();

            RectTransform mapRect = mapButtonObject.GetComponent<RectTransform>();
            RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
            if (mapRect != null && settingsRect != null)
            {
                mapRect.anchoredPosition = settingsRect.anchoredPosition + new Vector2(0f, -100f);
            }

            Image mapImage = mapButtonObject.GetComponent<Image>();
            if (mapImage != null) mapImage.color = new Color(0.12f, 0.55f, 0.68f, 1f);

            Text mapLabel = mapButtonObject.GetComponentInChildren<Text>(true);
            if (mapLabel != null) mapLabel.text = "VOLVER AL MAPA";
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (pauseButton != null && game != null)
            {
                pauseButton.interactable = game.CurrentState == GameState.Playing;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && game != null)
            {
                if (game.CurrentState == GameState.Playing) OnPauseClicked();
                else if (game.CurrentState == GameState.Paused) OnResumeClicked();
            }
        }

        private void OnPauseClicked()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.CurrentState != GameState.Playing) return;

            game.PauseGame();
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                pausePanel.transform.SetAsLastSibling();
            }
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);
        }

        private void OnResumeClicked()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.CurrentState != GameState.Paused) return;

            game.ResumeGame();
            if (pausePanel != null) pausePanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void OnSettingsClicked()
        {
            if (SettingsMenuUI.Instance != null) SettingsMenuUI.Instance.OpenSettings();
        }

        private void OnMapClicked()
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);

            string sceneName = string.IsNullOrWhiteSpace(worldMapSceneName)
                ? "MapaMundial"
                : worldMapSceneName;
            SceneManager.LoadScene(sceneName);
        }
    }
}
