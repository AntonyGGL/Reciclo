using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint4
{
    /// <summary>
    /// Barra visual de vida de Sr. Basura en el HUD con animaciones de daño y transiciones de color.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        private static BossHealthBar _instance;
        public static BossHealthBar Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<BossHealthBar>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Referencias UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private UnityEngine.UI.Image fillImage;
        [SerializeField] private UnityEngine.UI.Text hpText;
        [SerializeField] private RectTransform containerTransform;

        [Header("Colores por Estado de Vida")]
        [SerializeField] private Color highHealthColor = new Color(0.2f, 0.8f, 0.3f);  // Verde (> 50%)
        [SerializeField] private Color mediumHealthColor = new Color(0.95f, 0.75f, 0.1f); // Amarillo (25% - 50%)
        [SerializeField] private Color lowHealthColor = new Color(0.92f, 0.22f, 0.22f);   // Rojo Crítico (< 25%)

        [Header("Efectos")]
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 8f;

        [Header("Valores de Vida")]
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float currentHealth = 500f;

        public event Action<float, float> OnBossHealthChanged;
        public event Action<float> OnBossDamaged;
        public event Action OnBossDefeated;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private float targetHealth = 500f;
        private Coroutine shakeCoroutine;
        private Vector2 originalPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (containerTransform == null) containerTransform = GetComponent<RectTransform>();
            if (containerTransform != null) originalPosition = containerTransform.anchoredPosition;
        }

        public void InitializeBar(float maxHP)
        {
            maxHealth = maxHP;
            currentHealth = maxHP;
            targetHealth = maxHP;

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHP;
                healthSlider.value = maxHP;
            }
            UpdateHealthUI(maxHP);
            gameObject.SetActive(true);
        }

        public void SetHealth(float newHealth, bool triggerShake = true)
        {
            float prev = currentHealth;
            currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
            targetHealth = currentHealth;

            float damageAmount = prev - currentHealth;
            if (damageAmount > 0f)
            {
                OnBossDamaged?.Invoke(damageAmount);
                if (triggerShake && containerTransform != null)
                {
                    if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
                    shakeCoroutine = StartCoroutine(ShakeBarRoutine());
                }
            }

            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                OnBossDefeated?.Invoke();
            }
        }

        private void Update()
        {
            if (healthSlider != null && !Mathf.Approximately(healthSlider.value, targetHealth))
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);
                UpdateHealthUI(healthSlider.value);
            }
        }

        private void UpdateHealthUI(float currentValue)
        {
            if (maxHealth <= 0f) return;
            float ratio = currentValue / maxHealth;

            if (fillImage != null)
            {
                if (ratio > 0.5f)
                {
                    fillImage.color = Color.Lerp(mediumHealthColor, highHealthColor, (ratio - 0.5f) * 2f);
                }
                else
                {
                    fillImage.color = Color.Lerp(lowHealthColor, mediumHealthColor, ratio * 2f);
                }
            }

            if (hpText != null)
            {
                hpText.text = $"👑 SR. BASURA: {Mathf.CeilToInt(currentValue)} / {Mathf.CeilToInt(maxHealth)} HP";
            }
        }

        private IEnumerator ShakeBarRoutine()
        {
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;

                if (containerTransform != null)
                {
                    containerTransform.anchoredPosition = originalPosition + new Vector2(x, y);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (containerTransform != null)
            {
                containerTransform.anchoredPosition = originalPosition;
            }
        }
    }
}