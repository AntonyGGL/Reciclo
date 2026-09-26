using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    /// <summary>
    /// Zona de reciclaje que acepta residuos correspondientes.
    /// Al aceptar recupera vida a Vero y suma combo; al rechazar daña a Vero y rompe combo.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Configuración de la Zona de Reciclaje")]
        [SerializeField] private WasteCategory acceptedCategory = WasteCategory.Paper;
        [SerializeField] private string zoneName = "Contenedor de Papel";
        [SerializeField] private Color normalColor = new Color(0.2f, 0.5f, 0.9f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.7f, 1f);

        [Header("Eventos")]
        public UnityEvent<DraggableItem> OnItemAccepted = new UnityEvent<DraggableItem>();
        public UnityEvent<DraggableItem> OnItemRejected = new UnityEvent<DraggableItem>();

        private Graphic targetGraphic;
        private Vector3 originalScale;
        private Coroutine pulseRoutine;

        public WasteCategory AcceptedCategory => acceptedCategory;
        public string ZoneName => zoneName;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            originalScale = transform.localScale;
            if (targetGraphic != null && normalColor.a > 0.05f)
            {
                targetGraphic.color = normalColor;
            }
        }

        public void SetAcceptedCategory(WasteCategory category, Color color, string name = "")
        {
            acceptedCategory = category;
            normalColor = color;
            if (!string.IsNullOrEmpty(name)) zoneName = name;
            if (targetGraphic != null) targetGraphic.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                if (targetGraphic != null) targetGraphic.color = highlightColor;
                transform.localScale = originalScale * 1.05f;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (targetGraphic != null) targetGraphic.color = normalColor;
            transform.localScale = originalScale;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (targetGraphic != null) targetGraphic.color = normalColor;
            transform.localScale = originalScale;

            DraggableItem item = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<DraggableItem>() : null;
            if (item != null)
            {
                ProcessItemDrop(item);
                return;
            }

            FallingWaste fallingWaste = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<FallingWaste>() : null;
            if (fallingWaste != null)
            {
                if (fallingWaste.Category == acceptedCategory)
                {
                    fallingWaste.TriggerSuccess();
                }
                else
                {
                    fallingWaste.TriggerMiss();
                }
            }
        }

        public bool ProcessItemDrop(DraggableItem item)
        {
            if (item == null) return false;

            bool isCorrect = (item.Category == acceptedCategory);

            if (isCorrect)
            {
                Debug.Log($"[DropZone] ✔ Éxito: {item.Category} depositado en {zoneName}");

                // Notificaciones de Acierto según Sprint 3
                if (GameManager.Instance != null) GameManager.Instance.AddScore(item.PointsValue);
                if (ComboSystem.Instance != null) ComboSystem.Instance.AddCombo();
                if (VeroHealthController.Instance != null) VeroHealthController.Instance.Heal();
                if (CharacterControllerUI.Instance != null) CharacterControllerUI.Instance.OnPlayerSuccess();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.DropSuccess);

                if (BossController.Instance != null && BossController.Instance.gameObject.activeInHierarchy)
                {
                    int combo = (ComboSystem.Instance != null) ? ComboSystem.Instance.CurrentCombo : 1;
                    BossController.Instance.OnPlayerClassified(false, combo);
                }
                if (SmogController.Instance != null && SmogController.Instance.IsSmogActive)
                {
                    SmogController.Instance.OnCleanHit();
                }

                OnItemAccepted?.Invoke(item);
                TriggerBounceEffect(true);
                item.OnDropAccepted(this);
                return true;
            }
            else
            {
                Debug.LogWarning($"[DropZone] ✖ Rechazado: {item.Category} no pertenece a {zoneName} ({acceptedCategory})");

                // Notificaciones de Error según Sprint 3
                if (GameManager.Instance != null) GameManager.Instance.RegisterError();
                if (ComboSystem.Instance != null) ComboSystem.Instance.ResetCombo();
                if (VeroHealthController.Instance != null) VeroHealthController.Instance.TakeDamage();
                if (CharacterControllerUI.Instance != null) CharacterControllerUI.Instance.OnPlayerError();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.DropError);

                OnItemRejected?.Invoke(item);
                TriggerBounceEffect(false);
                item.OnDropRejected(this);
                return false;
            }
        }

        private void TriggerBounceEffect(bool success)
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(BounceRoutine(success ? Color.green : Color.red));
        }

        private IEnumerator BounceRoutine(Color flashColor)
        {
            float elapsed = 0f;
            float duration = 0.22f;
            Color originalColor = targetGraphic != null ? targetGraphic.color : normalColor;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.12f;
                transform.localScale = originalScale * scale;

                if (targetGraphic != null)
                {
                    targetGraphic.color = Color.Lerp(flashColor, originalColor, progress);
                }
                yield return null;
            }

            transform.localScale = originalScale;
            if (targetGraphic != null) targetGraphic.color = normalColor;
            pulseRoutine = null;
        }
    }
}