using ReCiclo.Sprint6;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    /// <summary>
    /// Residuo que cae verticalmente por un carril estilo Guitar Hero hacia la línea de impacto.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class FallingWaste : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Configuración del Residuo")]
        [SerializeField] private WasteCategory category = WasteCategory.Paper;
        [SerializeField] private string wasteName = "Periódico";
        [SerializeField] private int basePoints = 100;
        [SerializeField] private float damageToVero = 15f;
        [SerializeField] private float fallSpeed = 350f;
        [SerializeField] private int laneIndex = 0;

        [Header("Límites de Caída")]
        [SerializeField] private float missThresholdY = -600f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas rootCanvas;
        private bool isClassified = false;
        private bool isMissed = false;
        private bool isDragging = false;
        private Vector2 dragOffset;

        public event Action<FallingWaste> OnClassified;
        public event Action<FallingWaste> OnMiss;

        public WasteCategory Category => category;
        public string WasteName => wasteName;
        public int BasePoints => basePoints;
        public int LaneIndex => laneIndex;
        public float FallSpeed { get => fallSpeed; set => fallSpeed = value; }
        public bool IsClassified => isClassified;
        public bool IsMissed => isMissed;
        public RectTransform Rect => rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            rootCanvas = GetComponentInParent<Canvas>();
        }

        public void Initialize(WasteCategory cat, string name, int points, float speed, int lane, Color color, Sprite sprite = null)
        {
            category = cat;
            wasteName = name;
            basePoints = points;
            fallSpeed = speed;
            laneIndex = lane;
            isClassified = false;
            isMissed = false;
            isDragging = false;

            UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                if (sprite != null)
                {
                    img.sprite = sprite;
                    img.color = Color.white;
                }
                else
                {
                    img.color = color;
                }
            }

            UnityEngine.UI.Text txt = GetComponentInChildren<UnityEngine.UI.Text>();
            if (txt != null)
            {
                txt.text = $"{name}\n<size=15><b>{GetCategoryName(cat)}</b></size>";
            }
        }

        private void Update()
        {
            // Solo cae si el juego está en estado Playing y no está siendo arrastrado
            if (isClassified || isMissed || isDragging) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            // Movimiento vertical descendente constante
            rectTransform.anchoredPosition += Vector2.down * (fallSpeed * Time.deltaTime);

            // Verificar si sobrepasó la línea de impacto inferior sin ser clasificado
            if (rectTransform.anchoredPosition.y <= missThresholdY)
            {
                TriggerMiss();
            }
        }

        /// <summary>
        /// Se dispara cuando el residuo cruza la zona de impacto sin clasificarse (Fallo).
        /// </summary>
        public void TriggerMiss()
        {
            if (isMissed || isClassified) return;
            isMissed = true;

            Debug.Log($"[FallingWaste] MISS: {wasteName} ({category}) cruzó la línea de impacto sin reciclarse.");

            // Notificar a los sistemas centrales
            if (GameManager.Instance != null) GameManager.Instance.RegisterError();
            if (ComboSystem.Instance != null) ComboSystem.Instance.ResetCombo();
            if (VeroHealthController.Instance != null) VeroHealthController.Instance.TakeDamage(damageToVero);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.DropError);
            if (CharacterControllerUI.Instance != null) CharacterControllerUI.Instance.OnPlayerError();

            OnMiss?.Invoke(this);
            StartCoroutine(FadeAndDestroy(Color.red));
        }

        /// <summary>
        /// Se dispara cuando el jugador clasifica exitosamente el residuo.
        /// </summary>
        public void TriggerSuccess(float multiplier = 1f)
        {
            if (isClassified || isMissed) return;
            isClassified = true;

            int earnedPoints = Mathf.RoundToInt(basePoints * multiplier);
            if (GameManager.Instance != null) GameManager.Instance.AddScore(earnedPoints);
            if (ComboSystem.Instance != null) ComboSystem.Instance.AddCombo();
            if (VeroHealthController.Instance != null) VeroHealthController.Instance.Heal(2f); // Pequeña recuperación por acierto

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.DropSuccess);
            if (CharacterControllerUI.Instance != null) CharacterControllerUI.Instance.OnPlayerSuccess();
            if (VisualJuiceEffects.Instance != null) VisualJuiceEffects.Instance.PlaySuccessSparkles(transform.position);

            if (BossController.Instance != null && BossController.Instance.gameObject.activeInHierarchy)
            {
                int combo = (ComboSystem.Instance != null) ? ComboSystem.Instance.CurrentCombo : 1;
                bool isPerfect = (multiplier >= 1.4f);
                BossController.Instance.OnPlayerClassified(isPerfect, combo);
            }
            if (SmogController.Instance != null && SmogController.Instance.IsSmogActive)
            {
                SmogController.Instance.OnCleanHit();
            }

            OnClassified?.Invoke(this);
            StartCoroutine(FadeAndDestroy(Color.green));
        }

        private IEnumerator FadeAndDestroy(Color flashColor)
        {
            float elapsed = 0f;
            float duration = 0.18f;
            Vector3 startScale = transform.localScale;

            UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
            Color origColor = img != null ? img.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                if (img != null) img.color = Color.Lerp(flashColor, origColor, t);
                yield return null;
            }

            if (ObjectPooler.Instance != null)
            {
                transform.localScale = Vector3.one;
                if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = true; }
                ObjectPooler.Instance.ReturnToPool("WasteItem", gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Soporte para Interacción Táctil y Arrastre ---
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isClassified || isMissed) return;
            transform.localScale = Vector3.one * 1.15f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.GrabItem);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isClassified || isMissed) return;
            isDragging = true;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.85f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || isClassified || isMissed) return;
            float scaleFactor = (rootCanvas != null && rootCanvas.scaleFactor > 0.001f) ? rootCanvas.scaleFactor : 1f;
            rectTransform.anchoredPosition += eventData.delta / scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isClassified || isMissed) return;
            isDragging = false;
            transform.localScale = Vector3.one;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1.0f;
            }
        }

        private string GetCategoryName(WasteCategory cat)
        {
            switch (cat)
            {
                case WasteCategory.Paper: return "PAPEL";
                case WasteCategory.Plastic: return "PLÁSTICO";
                case WasteCategory.Glass: return "VIDRIO";
                case WasteCategory.Organic: return "ORGÁNICO";
                case WasteCategory.Electronic: return "ELECTRÓNICO";
                default: return "";
            }
        }
    }
}