using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using ReCiclo.Sprint4;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Configuracion de Arrastre")]
        [SerializeField] private float dragScale = 1.15f;
        [SerializeField] private float returnSpeed = 15f;

        private Vector2 originalAnchoredPosition;
        private Transform originalParent;
        private Vector3 originalScale;
        private Canvas rootCanvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private bool isDragging = false;
        private Coroutine returnRoutine;

        public WasteCategory Category { get; set; } = WasteCategory.Paper;
        public int PointsValue { get; set; } = 100;

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

        public void OnPointerDown(PointerEventData eventData)
        {
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
            isDragging = true;
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            // Desactivar raycasts temporalmente para que el drop detecte el contenedor (RecycleBin) debajo
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.85f;
            }

            // Elevar al frente visual del Canvas
            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform, true);
            }
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || rectTransform == null) return;

            float scaleFactor = (rootCanvas != null && rootCanvas.scaleFactor > 0.001f) ? rootCanvas.scaleFactor : 1f;
            rectTransform.anchoredPosition += eventData.delta / scaleFactor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = originalScale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1.0f;
            }

            // Si no fue aceptado y reciclado por un contenedor, retorna a su posicion original
            if (gameObject.activeInHierarchy)
            {
                if (originalParent != null && transform.parent != originalParent)
                {
                    transform.SetParent(originalParent, true);
                }
                if (returnRoutine != null) StopCoroutine(returnRoutine);
                returnRoutine = StartCoroutine(ReturnToOriginalPosition());
            }
        }

        private IEnumerator ReturnToOriginalPosition()
        {
            while (!isDragging && Vector2.Distance(rectTransform.anchoredPosition, originalAnchoredPosition) > 1f)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, originalAnchoredPosition, Time.deltaTime * returnSpeed);
                yield return null;
            }
            if (!isDragging)
            {
                rectTransform.anchoredPosition = originalAnchoredPosition;
            }
            returnRoutine = null;
        }

        public void OnItemRecycled(bool isCorrect)
        {
            if (returnRoutine != null) StopCoroutine(returnRoutine);
            Destroy(gameObject);
        }
    }
}
