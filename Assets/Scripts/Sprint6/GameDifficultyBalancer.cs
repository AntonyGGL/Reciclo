using UnityEngine;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint6
{
    public class GameDifficultyBalancer : MonoBehaviour
    {
        public static GameDifficultyBalancer Instance { get; private set; }

        [Header("Curva de Dificultad")]
        [SerializeField] private float baseFallSpeed = 3.0f;
        [SerializeField] private float maxFallSpeed = 8.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public float GetFallSpeedForLevel(int levelIndex)
        {
            // Ajuste progresivo de velocidad de caída de residuos por barrio (Nivel 1 a Nivel 5)
            float t = Mathf.Clamp01((levelIndex - 1) / 4.0f);
            return Mathf.Lerp(baseFallSpeed, maxFallSpeed, t);
        }

        public float GetErrorPenalizationForLevel(int levelIndex)
        {
            // En niveles avanzados (Zona Industrial y Río), los errores llenan más contaminación
            return 20f + (levelIndex - 1) * 2.5f;
        }

        public float GetSpawnIntervalForLevel(int levelIndex)
        {
            // Nivel 1: 2.5s -> Nivel 5: 1.2s
            return Mathf.Max(1.0f, 2.5f - (levelIndex - 1) * 0.3f);
        }
    }
}
