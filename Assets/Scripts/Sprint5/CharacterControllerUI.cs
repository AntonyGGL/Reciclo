using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint5
{
    public enum VeroState
    {
        Idle,
        Celebrating,
        Alert,
        Sad
    }

    public enum BossVisualState
    {
        Laughing,
        Hurt,
        Defeated
    }

    public class CharacterControllerUI : MonoBehaviour
    {
        public static CharacterControllerUI Instance { get; private set; }

        [Header("Referencias Vero")]
        [SerializeField] private Image veroImage;
        [SerializeField] private Sprite veroIdleSprite;
        [SerializeField] private Sprite veroCelebratingSprite;
        [SerializeField] private Sprite veroAlertSprite;
        [SerializeField] private Sprite veroSadSprite;

        [Header("Referencias Sr. Basura")]
        [SerializeField] private Image bossImage;
        [SerializeField] private Sprite bossLaughingSprite;
        [SerializeField] private Sprite bossHurtSprite;
        [SerializeField] private Sprite bossDefeatedSprite;

        [Header("Animación de Reacción")]
        [SerializeField] private float reactionDuration = 1.2f;

        private Coroutine veroReactionRoutine;
        private Coroutine bossReactionRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetVeroState(VeroState state, bool temporary = false)
        {
            if (veroImage == null) return;

            Sprite targetSprite = veroIdleSprite;
            switch (state)
            {
                case VeroState.Idle: targetSprite = veroIdleSprite; break;
                case VeroState.Celebrating: targetSprite = veroCelebratingSprite != null ? veroCelebratingSprite : veroIdleSprite; break;
                case VeroState.Alert: targetSprite = veroAlertSprite != null ? veroAlertSprite : veroIdleSprite; break;
                case VeroState.Sad: targetSprite = veroSadSprite != null ? veroSadSprite : veroIdleSprite; break;
            }

            if (targetSprite != null) veroImage.sprite = targetSprite;

            if (temporary)
            {
                if (veroReactionRoutine != null) StopCoroutine(veroReactionRoutine);
                veroReactionRoutine = StartCoroutine(ResetVeroStateRoutine());
            }
        }

        private IEnumerator ResetVeroStateRoutine()
        {
            yield return new WaitForSeconds(reactionDuration);
            SetVeroState(VeroState.Idle, false);
        }

        public void SetBossVisualState(BossVisualState state, bool temporary = false)
        {
            if (bossImage == null) return;

            Sprite targetSprite = bossLaughingSprite;
            switch (state)
            {
                case BossVisualState.Laughing: targetSprite = bossLaughingSprite; break;
                case BossVisualState.Hurt: targetSprite = bossHurtSprite != null ? bossHurtSprite : bossLaughingSprite; break;
                case BossVisualState.Defeated: targetSprite = bossDefeatedSprite != null ? bossDefeatedSprite : bossLaughingSprite; break;
            }

            if (targetSprite != null) bossImage.sprite = targetSprite;

            if (temporary && state != BossVisualState.Defeated)
            {
                if (bossReactionRoutine != null) StopCoroutine(bossReactionRoutine);
                bossReactionRoutine = StartCoroutine(ResetBossStateRoutine());
            }
        }

        private IEnumerator ResetBossStateRoutine()
        {
            yield return new WaitForSeconds(reactionDuration);
            SetBossVisualState(BossVisualState.Laughing, false);
        }

        public void OnPlayerSuccess()
        {
            SetVeroState(VeroState.Celebrating, true);
        }

        public void OnPlayerError()
        {
            SetVeroState(VeroState.Alert, true);
            SetBossVisualState(BossVisualState.Laughing, true);
        }

        public void OnBossDamaged()
        {
            SetBossVisualState(BossVisualState.Hurt, true);
        }
    }
}
