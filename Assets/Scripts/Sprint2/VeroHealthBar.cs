using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    /// <summary>
    /// Componente de HUD que muestra la barra de vida de Vero, animaciones de daño y transiciones de color.
    /// </summary>
    public class VeroHealthBar : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private UnityEngine.UI.Text healthText;
        [SerializeField] private RectTransform containerRect;

        [Header("Colores según Nivel de Vida")]
        [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.85f, 0.4f);   // Verde (> 50%)
        [SerializeField] private Color midHealthColor = new Color(0.95f, 0.75f, 0.1f);   // Amarillo (25% - 50%)
        [SerializeField] private Color lowHealthColor = new Color(0.92f, 0.22f, 0.22f);  // Rojo Crítico (< 25%)

        [Header("Animación")]
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private float shakeMagnitude = 6f;

        private float targetHealth = 100f;
        private float maxHealth = 100f;
        private Coroutine shakeRoutine;
        private Vector2 originalPos;

        private void Awake()
        {
            if (containerRect == null) containerRect = GetComponent<RectTransform>();
            EnsureVisibleLayout();
            if (containerRect != null) originalPos = containerRect.anchoredPosition;
        }

        private void OnEnable()
        {
            EnsureVisibleLayout();
        }

        private void EnsureVisibleLayout()
        {
            if (containerRect == null) containerRect = GetComponent<RectTransform>();
            if (containerRect == null) return;

            Canvas rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (rootCanvas != null && transform.parent != rootCanvas.transform)
            {
                transform.SetParent(rootCanvas.transform, false);
            }

            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(0f, 1f);
            containerRect.pivot = new Vector2(0f, 1f);
            containerRect.anchoredPosition = new Vector2(24f, -70f);
            containerRect.sizeDelta = new Vector2(440f, 52f);
            transform.SetAsLastSibling();

            Canvas overlayCanvas = GetComponent<Canvas>();
            if (overlayCanvas == null) overlayCanvas = gameObject.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 500;
            overlayCanvas.pixelPerfect = true;

            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            if (healthText != null)
            {
                healthText.fontSize = 24;
                healthText.fontStyle = FontStyle.Bold;
                healthText.color = Color.white;
            }
        }

        private void Start()
        {
            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.OnHealthChanged += HandleHealthChanged;
                VeroHealthController.Instance.OnVeroDamaged += HandleVeroDamaged;
                HandleHealthChanged(VeroHealthController.Instance.CurrentHealth, VeroHealthController.Instance.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (VeroHealthController.Instance != null)
            {
                VeroHealthController.Instance.OnHealthChanged -= HandleHealthChanged;
                VeroHealthController.Instance.OnVeroDamaged -= HandleVeroDamaged;
            }
        }

        private void Update()
        {
            if (healthSlider != null && !Mathf.Approximately(healthSlider.value, targetHealth))
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);
                UpdateVisualColors(healthSlider.value);
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            targetHealth = current;
            maxHealth = max;

            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            UpdateVisualColors(current);

            if (healthText != null)
            {
                healthText.text = $"VIDA DE VERO: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";
            }
        }

        private void HandleVeroDamaged(float damageAmount)
        {
            if (containerRect != null)
            {
                if (shakeRoutine != null) StopCoroutine(shakeRoutine);
                shakeRoutine = StartCoroutine(ShakeRoutine());
            }
        }

        private void UpdateVisualColors(float value)
        {
            if (fillImage == null || maxHealth <= 0f) return;

            float ratio = value / maxHealth;
            if (ratio > 0.5f)
            {
                fillImage.color = Color.Lerp(midHealthColor, fullHealthColor, (ratio - 0.5f) * 2f);
            }
            else
            {
                fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, ratio * 2f);
            }
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
                float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
                containerRect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
                yield return null;
            }

            containerRect.anchoredPosition = originalPos;
            shakeRoutine = null;
        }
    }
}
