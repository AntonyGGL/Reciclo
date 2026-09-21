using UnityEngine;
using UnityEngine.EventSystems;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint1
{
    public class RecycleBin : MonoBehaviour, IDropHandler
    {
        [Header("Configuracion del Contenedor")]
        [SerializeField] private WasteCategory acceptedCategory = WasteCategory.Paper;
        [SerializeField] private Color binColor = Color.blue;
        [SerializeField] private bool isWildcardActive = false;

        [Header("Efectos Visuales")]
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem errorParticles;

        public WasteCategory AcceptedCategory => acceptedCategory;

        public void SetWildcardMode(bool active)
        {
            isWildcardActive = active;
        }

        public void SetAcceptedCategory(WasteCategory category, Color color)
        {
            acceptedCategory = category;
            binColor = color;
        }

        public void OnDrop(PointerEventData eventData)
        {
            DraggableItem item = eventData.pointerDrag?.GetComponent<DraggableItem>();
            if (item == null) return;

            bool isCorrect = isWildcardActive || (item.Category == acceptedCategory);

            if (isCorrect)
            {
                Debug.Log($"[RecycleBin] ¡Acierto! Residuo {item.Category} depositado correctamente en contenedor {acceptedCategory}.");

                if (successParticles != null) successParticles.Play();

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

                item.OnItemRecycled(true);
            }
            else
            {
                Debug.LogWarning($"[RecycleBin] Error. Residuo {item.Category} NO pertenece a contenedor {acceptedCategory}.");

                if (errorParticles != null) errorParticles.Play();

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

                item.OnItemRecycled(false);
            }
        }
    }
}
