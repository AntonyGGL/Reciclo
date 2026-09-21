using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint4
{
    public class BossHealthBar : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text hpText;
        [SerializeField] private RectTransform containerTransform;

        [Header("Colores por Estado de Vida")]
        [SerializeField] private Color highHealthColor = new Color(0.2f, 0.8f, 0.3f);  // Verde
        [SerializeField] private Color mediumHealthColor = new Color(0.9f, 0.7f, 0.1f); // Amarillo
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);    // Rojo

        [Header("Efectos")]
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 8f;

        private float targetHealth = 100f;
        private float maxHealth = 100f;
        private Coroutine shakeCoroutine;
        private Vector3 originalPosition;

        private void Awake()
        {
            if (containerTransform != null)
            {
                originalPosition = containerTransform.anchoredPosition;
            }
        }

        public void InitializeBar(float maxHP)
        {
            maxHealth = maxHP;
            targetHealth = maxHP;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHP;
                healthSlider.value = maxHP;
            }
            UpdateHealthUI(maxHP);
        }

        public void SetHealth(float currentHP, bool triggerShake = true)
        {
            targetHealth = Mathf.Clamp(currentHP, 0f, maxHealth);

            if (triggerShake && containerTransform != null)
            {
                if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
                shakeCoroutine = StartCoroutine(ShakeBarRoutine());
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
                hpText.text = $"SR. BASURA: {Mathf.CeilToInt(currentValue)} / {Mathf.CeilToInt(maxHealth)} HP";
            }
        }

        private IEnumerator ShakeBarRoutine()
        {
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;

                if (containerTransform != null)
                {
                    containerTransform.anchoredPosition = originalPosition + new Vector3(x, y, 0f);
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
