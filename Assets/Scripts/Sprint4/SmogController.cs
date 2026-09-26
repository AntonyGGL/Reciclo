using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint4
{
    /// <summary>
    /// Gestiona la mecánica de nube de smog temporal durante la batalla de jefe.
    /// Reduce parcialmente la visibilidad de los carriles sin bloquear la jugabilidad.
    /// </summary>
    public class SmogController : MonoBehaviour
    {
        private static SmogController _instance;
        public static SmogController Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<SmogController>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Referencias Visuales")]
        [SerializeField] private CanvasGroup smogCanvasGroup;
        [SerializeField] private UnityEngine.UI.Image smogImage;
        [SerializeField] private UnityEngine.UI.Text smogWarningText;

        [Header("Configuración de Smog")]
        [Range(0.2f, 0.85f)]
        [SerializeField] private float maxSmogAlpha = 0.65f;
        [SerializeField] private float defaultDuration = 5.0f;
        [SerializeField] private float fadeSpeed = 3.0f;
        [SerializeField] private float cleanHitReduction = 1.0f;

        [Header("Estado")]
        [SerializeField] private bool isSmogActive = false;
        [SerializeField] private float remainingSmogTime = 0f;

        public event Action OnSmogStarted;
        public event Action OnSmogCleared;

        public bool IsSmogActive => isSmogActive;
        public float RemainingSmogTime => remainingSmogTime;

        private Coroutine smogRoutine;
        private Coroutine pulseRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (smogCanvasGroup == null) smogCanvasGroup = GetComponent<CanvasGroup>();
            if (smogCanvasGroup != null)
            {
                smogCanvasGroup.alpha = 0f;
                smogCanvasGroup.blocksRaycasts = false;
                smogCanvasGroup.interactable = false;
            }
        }

        /// <summary>
        /// Activa la nube de smog durante un tiempo determinado.
        /// </summary>
        public void ActivateSmog(float duration = -1f, float targetAlpha = -1f)
        {
            float dur = (duration > 0f) ? duration : defaultDuration;
            float alpha = (targetAlpha > 0f) ? targetAlpha : maxSmogAlpha;

            remainingSmogTime = dur;
            isSmogActive = true;

            if (smogRoutine != null) StopCoroutine(smogRoutine);
            smogRoutine = StartCoroutine(SmogCycleRoutine(alpha));

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(SmogPulseRoutine());

            OnSmogStarted?.Invoke();
            Debug.Log($"[SmogController] ¡Alerta de Smog Tóxico! Visibilidad reducida por {dur}s.");
        }

        /// <summary>
        /// Disipa el smog inmediatamente o con transición suave.
        /// </summary>
        public void ClearSmog(bool immediate = false)
        {
            remainingSmogTime = 0f;
            isSmogActive = false;

            if (smogRoutine != null) StopCoroutine(smogRoutine);
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);

            if (immediate)
            {
                if (smogCanvasGroup != null) smogCanvasGroup.alpha = 0f;
                if (smogWarningText != null) smogWarningText.gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(FadeOutRoutine());
            }

            OnSmogCleared?.Invoke();
            Debug.Log("[SmogController] Smog disipado. Visibilidad restablecida al 100%.");
        }

        /// <summary>
        /// Reduce el tiempo de smog al clasificar correctamente un residuo.
        /// </summary>
        public void OnCleanHit()
        {
            if (!isSmogActive) return;

            remainingSmogTime -= cleanHitReduction;
            if (remainingSmogTime <= 0f)
            {
                ClearSmog(false);
            }
        }

        private IEnumerator SmogCycleRoutine(float targetAlpha)
        {
            if (smogWarningText != null)
            {
                smogWarningText.gameObject.SetActive(true);
                smogWarningText.text = "⚠️ ¡HUMO TÓXICO DE SR. BASURA! ⚠️\n<size=16>¡Clasifica para disipar el smog!</size>";
            }

            // Fade In
            while (smogCanvasGroup != null && smogCanvasGroup.alpha < targetAlpha)
            {
                smogCanvasGroup.alpha = Mathf.MoveTowards(smogCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
                yield return null;
            }

            // Mantener durante la duración restante
            while (remainingSmogTime > 0f)
            {
                remainingSmogTime -= Time.deltaTime;
                yield return null;
            }

            // Fade Out
            yield return FadeOutRoutine();
            isSmogActive = false;
            OnSmogCleared?.Invoke();
        }

        private IEnumerator FadeOutRoutine()
        {
            if (smogWarningText != null)
            {
                smogWarningText.gameObject.SetActive(false);
            }

            while (smogCanvasGroup != null && smogCanvasGroup.alpha > 0f)
            {
                smogCanvasGroup.alpha = Mathf.MoveTowards(smogCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
                yield return null;
            }
        }

        private IEnumerator SmogPulseRoutine()
        {
            while (isSmogActive)
            {
                if (smogImage != null)
                {
                    float wave = Mathf.Sin(Time.time * 2f) * 0.08f;
                    smogImage.color = new Color(0.2f + wave, 0.15f, 0.25f + wave, 0.9f);
                }
                yield return null;
            }
        }
    }
}