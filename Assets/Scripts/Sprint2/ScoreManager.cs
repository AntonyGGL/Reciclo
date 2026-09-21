using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("UI Puntuación")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;

        private int displayedScore = 0;

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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            }
            UpdateScoreDisplay(0);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            }
        }

        public void UpdateScoreDisplay(int newScore)
        {
            displayedScore = newScore;
            if (scoreText != null)
            {
                scoreText.text = $"PUNTAJE: {displayedScore}";
            }
        }
    }
}
