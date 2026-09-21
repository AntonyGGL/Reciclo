using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint4
{
    public class EnvironmentController : MonoBehaviour
    {
        public static EnvironmentController Instance { get; private set; }

        [Header("Referencias Visuales del Entorno")]
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private Image uiBackgroundImage;
        [SerializeField] private ParticleSystem smogParticleSystem;
        [SerializeField] private ParticleSystem cleanSparklesParticleSystem;

        [Header("Colores del Entorno")]
        [SerializeField] private Color pollutedColor = new Color(0.35f, 0.35f, 0.35f, 1f); // Basuralia (Gris/Smog)
        [SerializeField] private Color cleanColor = new Color(0.45f, 0.85f, 1f, 1f);       // Ciudad Limpia (Cielo Claro)

        [Header("Configuración de Animación")]
        [SerializeField] private float transitionSpeed = 2f;
        [Range(0f, 100f)] [SerializeField] private float currentPollutionPercent = 50f;

        private Coroutine transitionCoroutine;

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
            UpdateEnvironmentVisuals(currentPollutionPercent, true);
        }

        /// <summary>
        /// Actualiza la apariencia del fondo según el % de contaminación.
        /// 100% = Totalmente gris y con smog (Basuralia).
        /// 0% = Totalmente azul vibrante y limpio (Ciudad Limpia).
        /// </summary>
        public void SetPollutionLevel(float pollutionPercent, bool immediate = false)
        {
            pollutionPercent = Mathf.Clamp(pollutionPercent, 0f, 100f);
            currentPollutionPercent = pollutionPercent;

            if (immediate)
            {
                UpdateEnvironmentVisuals(pollutionPercent, true);
            }
            else
            {
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }
                transitionCoroutine = StartCoroutine(AnimateEnvironmentTransition(pollutionPercent));
            }
        }

        private IEnumerator AnimateEnvironmentTransition(float targetPollution)
        {
            float t = 0f;
            Color startColor = GetCurrentColor();
            Color targetColor = Color.Lerp(cleanColor, pollutedColor, targetPollution / 100f);

            while (t < 1f)
            {
                t += Time.deltaTime * transitionSpeed;
                Color interpolatedColor = Color.Lerp(startColor, targetColor, t);

                ApplyColorToBackground(interpolatedColor);
                yield return null;
            }

            ApplyColorToBackground(targetColor);
            UpdateParticles(targetPollution);
        }

        private void UpdateEnvironmentVisuals(float pollutionPercent, bool forceInstant = false)
        {
            Color targetColor = Color.Lerp(cleanColor, pollutedColor, pollutionPercent / 100f);
            ApplyColorToBackground(targetColor);
            UpdateParticles(pollutionPercent);
        }

        private void ApplyColorToBackground(Color color)
        {
            if (backgroundRenderer != null)
            {
                backgroundRenderer.color = color;
            }
            if (uiBackgroundImage != null)
            {
                uiBackgroundImage.color = color;
            }
        }

        private Color GetCurrentColor()
        {
            if (backgroundRenderer != null) return backgroundRenderer.color;
            if (uiBackgroundImage != null) return uiBackgroundImage.color;
            return cleanColor;
        }

        private void UpdateParticles(float pollutionPercent)
        {
            // Partículas de humo/smog cuando la contaminación es alta
            if (smogParticleSystem != null)
            {
                var mainSmog = smogParticleSystem.main;
                if (pollutionPercent > 40f)
                {
                    if (!smogParticleSystem.isPlaying) smogParticleSystem.Play();
                    mainSmog.maxParticles = Mathf.RoundToInt(Mathf.Lerp(10, 100, (pollutionPercent - 40f) / 60f));
                }
                else
                {
                    if (smogParticleSystem.isPlaying) smogParticleSystem.Stop();
                }
            }

            // Partículas de destellos de limpieza cuando la ciudad está recuperada (< 30% contaminación)
            if (cleanSparklesParticleSystem != null)
            {
                if (pollutionPercent < 30f)
                {
                    if (!cleanSparklesParticleSystem.isPlaying) cleanSparklesParticleSystem.Play();
                }
                else
                {
                    if (cleanSparklesParticleSystem.isPlaying) cleanSparklesParticleSystem.Stop();
                }
            }
        }
    }
}
