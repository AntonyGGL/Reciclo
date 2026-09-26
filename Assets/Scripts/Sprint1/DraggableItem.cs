using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Configuración de Arrastre")]
        [SerializeField] private float dragScale = 1.18f;
        [SerializeField] private float returnSpeed = 16f;
        [SerializeField] private bool useElasticReturn = true;

        [Header("Datos del Residuo")]
        [SerializeField] private WasteCategory category = WasteCategory.Paper;
        [SerializeField] private int pointsValue = 100;
        [SerializeField] private string itemName = "Residuo";

        [Header("Eventos de Arrastre")]
        public UnityEvent OnDragStarted = new UnityEvent();
        public UnityEvent OnDragEnded = new UnityEvent();
        public UnityEvent OnElasticReturnCompleted = new UnityEvent();

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas rootCanvas;
        private Transform originalParent;
        private Vector2 originalAnchoredPosition;
        private Vector3 originalScale;
        private bool isDragging = false;
        private bool isRecycled = false;
        private Coroutine returnRoutine;

        public WasteCategory Category { get => category; set => category = value; }
        public int PointsValue { get => pointsValue; set => pointsValue = value; }
        public string ItemName { get => itemName; set => itemName = value; }
        public bool IsDragging => isDragging;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            rootCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalScale = transform.localScale;
            originalParent = transform.parent;
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }
        }

        public void SetItemData(WasteCategory newCategory, string name, int points, Color color)
        {
            category = newCategory;
            itemName = name;
            pointsValue = points;

            Image img = GetComponent<Image>();
            if (img != null) img.color = color;

            Text txt = GetComponentInChildren<Text>();
            if (txt != null) txt.text = name;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isRecycled) return;

            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }
            transform.localScale = originalScale * dragScale;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SoundType.GrabItem);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isRecycled) return;

            isDragging = true;
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.88f;
            }

            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform, true);
            }
            transform.SetAsLastSibling();

            OnDragStarted?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || rectTransform == null || isRecycled) return;

            float scaleFactor = (rootCanvas != null && rootCanvas.scaleFactor > 0.001f) ? rootCanvas.scaleFactor : 1f;
            rectTransform.anchoredPosition += eventData.delta / scaleFactor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDragging)
            {
                transform.localScale = originalScale;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isRecycled) return;

            isDragging = false;
            transform.localScale = originalScale;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1.0f;
            }

            OnDragEnded?.Invoke();

            // Si no fue reciclado en un DropZone/RecycleBin, retornar elásticamente a su posición de inicio
            if (gameObject.activeInHierarchy && !isRecycled)
            {
                if (originalParent != null && transform.parent != originalParent)
                {
                    transform.SetParent(originalParent, true);
                }

                if (useElasticReturn)
                {
                    if (returnRoutine != null) StopCoroutine(returnRoutine);
                    returnRoutine = StartCoroutine(ElasticReturnRoutine());
                }
            }
        }

        private IEnumerator ElasticReturnRoutine()
        {
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 targetPos = originalAnchoredPosition;
            float time = 0f;
            float duration = 0.35f;

            while (time < duration && !isDragging)
            {
                time += Time.deltaTime;
                float t = time / duration;
                // Easing elástico / overshoot suave: t^2 * ((s+1)*t - s)
                float s = 1.70158f;
                float curvedT = (t == 1f) ? 1f : 1f + (--t) * t * ((s + 1f) * t + s);

                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, Mathf.Clamp01(curvedT));
                yield return null;
            }

            if (!isDragging)
            {
                rectTransform.anchoredPosition = targetPos;
                OnElasticReturnCompleted?.Invoke();
            }
            returnRoutine = null;
        }

        public void OnDropAccepted(DropZone zone)
        {
            isRecycled = true;
            if (returnRoutine != null) StopCoroutine(returnRoutine);
            StartCoroutine(VanishAndDestroyRoutine(true));
        }

        public void OnDropRejected(DropZone zone)
        {
            // Retornar a la posición inicial
            if (originalParent != null && transform.parent != originalParent)
            {
                transform.SetParent(originalParent, true);
            }
            if (returnRoutine != null) StopCoroutine(returnRoutine);
            returnRoutine = StartCoroutine(ElasticReturnRoutine());
        }

        public void OnItemRecycled(bool isCorrect)
        {
            isRecycled = true;
            if (returnRoutine != null) StopCoroutine(returnRoutine);
            StartCoroutine(VanishAndDestroyRoutine(isCorrect));
        }

        private IEnumerator VanishAndDestroyRoutine(bool success)
        {
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                if (canvasGroup != null) canvasGroup.alpha = 1f - progress;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}