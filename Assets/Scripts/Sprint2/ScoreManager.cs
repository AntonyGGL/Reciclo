using System;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint6;

namespace ReCiclo.Sprint2
{
    /// <summary>
    /// Registra el puntaje, calcula la precisión y las estrellas obtenidas según las especificaciones.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("UI de Puntuación")]
        [SerializeField] private UnityEngine.UI.Text scoreText;
        [SerializeField] private UnityEngine.UI.Text highScoreText;
        [SerializeField] private UnityEngine.UI.Text accuracyText;

        public event Action<int> OnScoreUpdated;
        public event Action<int> OnStarsCalculated;

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
                GameManager.Instance.OnVictory += HandleVictory;
            }
            UpdateScoreDisplay(0);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                GameManager.Instance.OnVictory -= HandleVictory;
            }
        }

        public void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"PUNTAJE: {newScore}";
            }
            OnScoreUpdated?.Invoke(newScore);
        }

        public float CalculateAccuracy(int hits, int errors)
        {
            int total = hits + errors;
            if (total == 0) return 0f;
            return ((float)hits / total) * 100f;
        }

        public int CalculateStars(float accuracy, int finalScore, int targetScore)
        {
            // 1 estrella: completar nivel
            // 2 estrellas: buena precisión (>= 60%)
            // 3 estrellas: alta precisión (>= 85%) y puntaje alto
            if (accuracy >= 85f && finalScore >= Mathf.RoundToInt(targetScore * 0.9f))
            {
                return 3;
            }
            else if (accuracy >= 60f)
            {
                return 2;
            }
            return 1;
        }

        private void HandleVictory()
        {
            if (GameManager.Instance == null) return;

            int score = GameManager.Instance.CurrentScore;
            float acc = CalculateAccuracy(GameManager.Instance.TotalHits, GameManager.Instance.TotalErrors);
            int stars = CalculateStars(acc, score, 1000);

            if (accuracyText != null)
            {
                accuracyText.text = $"PRECISIÓN: {acc:F1}%";
            }

            OnStarsCalculated?.Invoke(stars);
        }
    }
}