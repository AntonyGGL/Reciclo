using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint2
{
    public class PollutionBar : MonoBehaviour
    {
        public static PollutionBar Instance { get; private set; }

        [Header("Configuración de Contaminación")]
        [SerializeField] private float maxPollution = 100f;
        [SerializeField] private float currentPollution = 0f;
        [SerializeField] private Slider pollutionSlider;
        [SerializeField] private Image fillImage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ResetBar()
        {
            currentPollution = 0f;
            UpdateUI();
        }

        public void AddPollution(float amount)
        {
            currentPollution += amount;
            currentPollution = Mathf.Clamp(currentPollution, 0f, maxPollution);
            UpdateUI();

            if (EnvironmentController.Instance != null)
            {
                EnvironmentController.Instance.SetPollutionLevel(currentPollution, false);
            }

            if (currentPollution >= maxPollution)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerGameOver();
                }
            }
        }

        public void ReducePollution(float amount)
        {
            currentPollution -= amount;
            currentPollution = Mathf.Clamp(currentPollution, 0f, maxPollution);
            UpdateUI();

            if (EnvironmentController.Instance != null)
            {
                EnvironmentController.Instance.SetPollutionLevel(currentPollution, false);
            }
        }

        private void UpdateUI()
        {
            if (pollutionSlider != null)
            {
                pollutionSlider.maxValue = maxPollution;
                pollutionSlider.value = currentPollution;
            }

            if (fillImage != null)
            {
                float t = currentPollution / maxPollution;
                fillImage.color = Color.Lerp(Color.yellow, Color.red, t);
            }
        }
    }
}
