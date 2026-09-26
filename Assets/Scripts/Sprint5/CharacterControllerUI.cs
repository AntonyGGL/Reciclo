using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint5
{
    public enum VeroState
    {
        Idle,           // Esperando
        Happy,          // Acierto / Celebración
        Hurt,           // Error / Daño
        Defeated        // Vida a 0 / Derrota permanente
    }

    public enum BossVisualState
    {
        Laughing,
        Hurt,
        Defeated
    }

    /// <summary>
    /// Gestiona las animaciones visuales, expresiones y reacciones de Vero el Robot y Sr. Basura.
    /// </summary>
    public class CharacterControllerUI : MonoBehaviour
    {
                private static CharacterControllerUI _instance;
        public static CharacterControllerUI Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<CharacterControllerUI>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Referencias Vero el Robot")]
        [SerializeField] private UnityEngine.UI.Image veroImage;
        [SerializeField] private UnityEngine.UI.Text veroStatusText;
        [SerializeField] private Sprite veroIdleSprite;
        [SerializeField] private Sprite veroHappySprite;
        [SerializeField] private Sprite veroHurtSprite;
        [SerializeField] private Sprite veroDefeatedSprite;

        [Header("Referencias Sr. Basura")]
        [SerializeField] private UnityEngine.UI.Image bossImage;
        [SerializeField] private Sprite bossLaughingSprite;
        [SerializeField] private Sprite bossHurtSprite;
        [SerializeField] private Sprite bossDefeatedSprite;

        [Header("Configuración de Animación")]
        [SerializeField] private float reactionDuration = 1.0f;

        private VeroState currentVeroState = VeroState.Idle;
        private Coroutine veroReactionRoutine;
        private Coroutine bossReactionRoutine;

        public VeroState CurrentVeroState => currentVeroState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetVeroState(VeroState.Idle);
        }

        /// <summary>
        /// Cambia el estado y sprite de Vero.
        /// Si Vero ya está derrotado, evita que animaciones menores interrumpan la derrota.
        /// </summary>
        public void SetVeroState(VeroState state, bool temporary = false)
        {
            // Regla Sprint 3: Evitar que una animación menor interrumpa la animación de derrota
            if (currentVeroState == VeroState.Defeated && state != VeroState.Idle)
            {
                return;
            }

            currentVeroState = state;

            // Actualizar Sprite o Icono representativo
            Sprite targetSprite = veroIdleSprite;
            string statusIcon = "🤖 VERO: OK";
            Color statusColor = Color.white;

            switch (state)
            {
                case VeroState.Idle:
                    targetSprite = veroIdleSprite;
                    statusIcon = "🤖 VERO: LISTO";
                    statusColor = new Color(0.3f, 0.85f, 0.5f);
                    break;
                case VeroState.Happy:
                    targetSprite = (veroHappySprite != null) ? veroHappySprite : veroIdleSprite;
                    statusIcon = "⭐ VERO: ¡FELIZ!";
                    statusColor = new Color(0.2f, 0.9f, 1f);
                    break;
                case VeroState.Hurt:
                    targetSprite = (veroHurtSprite != null) ? veroHurtSprite : veroIdleSprite;
                    statusIcon = "💥 VERO: ¡DAÑADO!";
                    statusColor = new Color(1f, 0.3f, 0.3f);
                    break;
                case VeroState.Defeated:
                    targetSprite = (veroDefeatedSprite != null) ? veroDefeatedSprite : veroHurtSprite;
                    statusIcon = "💀 VERO: APAGADO";
                    statusColor = new Color(0.6f, 0.6f, 0.6f);
                    break;
            }

            if (veroImage != null && targetSprite != null)
            {
                veroImage.sprite = targetSprite;
            }

            if (veroStatusText != null)
            {
                veroStatusText.text = statusIcon;
                veroStatusText.color = statusColor;
            }

            // Efecto punch de escala
            StartCoroutine(PunchScale(veroImage != null ? veroImage.transform : transform, state));

            if (temporary && state != VeroState.Defeated)
            {
                if (veroReactionRoutine != null) StopCoroutine(veroReactionRoutine);
                veroReactionRoutine = StartCoroutine(ResetVeroToIdleRoutine());
            }
        }

        private IEnumerator ResetVeroToIdleRoutine()
        {
            yield return new WaitForSeconds(reactionDuration);
            if (currentVeroState != VeroState.Defeated)
            {
                SetVeroState(VeroState.Idle, false);
            }
        }

        private IEnumerator PunchScale(Transform target, VeroState state)
        {
            if (target == null) yield break;
            Vector3 originalScale = Vector3.one;
            float punch = (state == VeroState.Happy) ? 1.25f : (state == VeroState.Hurt) ? 0.85f : 1.0f;
            target.localScale = originalScale * punch;

            float elapsed = 0f;
            float duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(originalScale * punch, originalScale, elapsed / duration);
                yield return null;
            }
            target.localScale = originalScale;
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
            SetVeroState(VeroState.Happy, true);
        }

        public void OnPlayerError()
        {
            SetVeroState(VeroState.Hurt, true);
            SetBossVisualState(BossVisualState.Laughing, true);
        }

        public void OnBossDamaged()
        {
            SetBossVisualState(BossVisualState.Hurt, true);
        }
    }
}