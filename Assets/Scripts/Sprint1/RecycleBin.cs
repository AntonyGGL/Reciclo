using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    [RequireComponent(typeof(DropZone))]
    public class RecycleBin : MonoBehaviour
    {
        [Header("Configuración de Contenedor")]
        [SerializeField] private WasteCategory acceptedCategory = WasteCategory.Paper;
        [SerializeField] private string binTitle = "PAPEL";
        [SerializeField] private Color binColor = new Color(0.2f, 0.5f, 0.9f);
        [SerializeField] private bool isWildcardActive = false;

        [Header("Sprites de Estados (Sprint 6)")]
        [SerializeField] private UnityEngine.UI.Image binImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite successSprite;
        [SerializeField] private Sprite errorSprite;
        [SerializeField] private Sprite wildcardSprite;

        [Header("Efectos Visuales y Partículas")]
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem errorParticles;

        [Header("Eventos de Contenedor")]
        public UnityEvent<DraggableItem> OnItemAccepted = new UnityEvent<DraggableItem>();
        public UnityEvent<DraggableItem> OnItemRejected = new UnityEvent<DraggableItem>();

        private DropZone dropZone;

        public WasteCategory AcceptedCategory => acceptedCategory;
        public string BinTitle => binTitle;

        private void Awake()
        {
            dropZone = GetComponent<DropZone>();
            if (dropZone == null)
            {
                dropZone = gameObject.AddComponent<DropZone>();
            }

            dropZone.SetAcceptedCategory(acceptedCategory, binColor, binTitle);
            dropZone.OnItemAccepted.AddListener(HandleAccepted);
            dropZone.OnItemRejected.AddListener(HandleRejected);
        }

        public void SetWildcardMode(bool active)
        {
            isWildcardActive = active;
            if (binImage != null)
            {
                if (active && wildcardSprite != null) binImage.sprite = wildcardSprite;
                else if (!active && normalSprite != null) binImage.sprite = normalSprite;
            }
        }

        public void SetAcceptedCategory(WasteCategory category, Color color, string title = "")
        {
            acceptedCategory = category;
            binColor = color;
            if (!string.IsNullOrEmpty(title)) binTitle = title;

            if (dropZone != null)
            {
                dropZone.SetAcceptedCategory(category, color, binTitle);
            }
        }

        private void HandleAccepted(DraggableItem item)
        {
            if (successParticles != null) successParticles.Play();
            if (binImage != null && successSprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(ShowFeedbackSprite(successSprite));
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SoundType.DropSuccess);
            }

            if (CharacterControllerUI.Instance != null)
            {
                CharacterControllerUI.Instance.OnPlayerSuccess();
            }

            if (VisualJuiceEffects.Instance != null)
            {
                VisualJuiceEffects.Instance.PlaySuccessSparkles(transform.position);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(item.PointsValue);
            }

            if (BossController.Instance != null && BossController.Instance.gameObject.activeInHierarchy)
            {
                BossController.Instance.TakeDamage(25f);
                if (CharacterControllerUI.Instance != null)
                {
                    CharacterControllerUI.Instance.OnBossDamaged();
                }
            }

            OnItemAccepted?.Invoke(item);
        }

        private void HandleRejected(DraggableItem item)
        {
            if (errorParticles != null) errorParticles.Play();
            if (binImage != null && errorSprite != null)
            {
                StopAllCoroutines();
                StartCoroutine(ShowFeedbackSprite(errorSprite));
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(SoundType.DropError);
            }

            if (CharacterControllerUI.Instance != null)
            {
                CharacterControllerUI.Instance.OnPlayerError();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterError();
            }

            OnItemRejected?.Invoke(item);
        }

        private System.Collections.IEnumerator ShowFeedbackSprite(Sprite feedback)
        {
            binImage.sprite = feedback;
            yield return new WaitForSeconds(0.6f);
            if (isWildcardActive && wildcardSprite != null) binImage.sprite = wildcardSprite;
            else if (normalSprite != null) binImage.sprite = normalSprite;
        }
    }
}