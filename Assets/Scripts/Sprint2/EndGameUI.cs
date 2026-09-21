using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    public class EndGameUI : MonoBehaviour
    {
        public static EndGameUI Instance { get; private set; }

        [Header("Modal de Victoria")]
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private Text victoryScoreText;
        [SerializeField] private Text victoryAccuracyText;
        [SerializeField] private GameObject[] starObjects;

        [Header("Modal de Derrota")]
        [SerializeField] private GameObject gameOverModal;
        [SerializeField] private Text gameOverScoreText;

        [Header("Botones")]
        [SerializeField] private Button victoryRestartButton;
        [SerializeField] private Button gameOverRestartButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        public void ShowVictory(int finalScore, float accuracy)
        {
            if (victoryModal != null) victoryModal.SetActive(true);
            if (gameOverModal != null) gameOverModal.SetActive(false);

            if (victoryScoreText != null) victoryScoreText.text = $"Puntaje: {finalScore}";
            if (victoryAccuracyText != null) victoryAccuracyText.text = $"Precision: {accuracy:F1}%";

            int stars = 1;
            if (accuracy >= 85f && finalScore >= 1200) stars = 3;
            else if (accuracy >= 60f && finalScore >= 800) stars = 2;

            if (starObjects != null)
            {
                for (int i = 0; i < starObjects.Length; i++)
                {
                    if (starObjects[i] != null)
                    {
                        starObjects[i].SetActive(i < stars);
                    }
                }
            }
        }

        public void ShowGameOver(int finalScore)
        {
            if (gameOverModal != null) gameOverModal.SetActive(true);
            if (victoryModal != null) victoryModal.SetActive(false);

            if (gameOverScoreText != null) gameOverScoreText.text = $"Puntaje Final: {finalScore}";
        }

        public void OnRestartButtonClicked()
        {
            if (victoryModal != null) victoryModal.SetActive(false);
            if (gameOverModal != null) gameOverModal.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
