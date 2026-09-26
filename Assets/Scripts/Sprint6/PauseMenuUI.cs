using UnityEngine;
using UnityEngine.UI;
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

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        private void Start()
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnPauseClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
                if (pausePanel != null) pausePanel.SetActive(true);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);
            }
        }

        private void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
                if (pausePanel != null) pausePanel.SetActive(false);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);
            }
        }

        private void OnRestartClicked()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
                GameManager.Instance.StartGame();
            }
        }

        private void OnSettingsClicked()
        {
            if (SettingsMenuUI.Instance != null) SettingsMenuUI.Instance.OpenSettings();
        }
    }
}