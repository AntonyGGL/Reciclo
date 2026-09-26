using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint6
{
    /// <summary>
    /// Panel de configuracion: controles de volumen BGM/SFX,
    /// boton de mute, y boton de reiniciar progreso.
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        public static SettingsMenuUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Controles de Volumen")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text bgmValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Button muteButton;
        [SerializeField] private Text muteButtonText;

        [Header("Botones")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetProgressButton;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void Start()
        {
            if (bgmSlider != null)
            {
                bgmSlider.minValue = 0f;
                bgmSlider.maxValue = 1f;
                bgmSlider.value = AudioManager.Instance != null ? AudioManager.Instance.BGMVolume : 0.6f;
                bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.value = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1.0f;
                sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            }

            if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
            if (muteButton != null) muteButton.onClick.AddListener(OnMuteClicked);
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(OnResetProgress);

            UpdateVolumeTexts();
        }

        public void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            UpdateVolumeTexts();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.UIClick);
        }

        private void OnBGMSliderChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetBGMVolume(value);
            UpdateVolumeTexts();
        }

        private void OnSFXSliderChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
            UpdateVolumeTexts();
        }

        private void OnMuteClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMute();
                if (muteButtonText != null)
                    muteButtonText.text = AudioManager.Instance.IsMuted ? "DESMUTEAR" : "MUTEAR";
            }
        }

        private void OnResetProgress()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.ResetProgress();
                Debug.Log("[SettingsMenuUI] Progreso reiniciado.");
            }
        }

        private void UpdateVolumeTexts()
        {
            if (bgmValueText != null)
                bgmValueText.text = "Musica: " + Mathf.RoundToInt((bgmSlider != null ? bgmSlider.value : 0.6f) * 100f) + "%";
            if (sfxValueText != null)
                sfxValueText.text = "Efectos: " + Mathf.RoundToInt((sfxSlider != null ? sfxSlider.value : 1.0f) * 100f) + "%";
        }
    }
}