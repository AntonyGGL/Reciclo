using System;
using System.Collections;
using UnityEngine;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint2
{
    /// <summary>
    /// Controlador de vida de Vero el Robot.
    /// Mecánica principal de derrota: los errores reducen su vida y al llegar a 0 se activa GameOver.
    /// </summary>
    public class VeroHealthController : MonoBehaviour
    {
                private static VeroHealthController _instance;
        public static VeroHealthController Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<VeroHealthController>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuración de Vida")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float damagePerError = 15f;
        [SerializeField] private float healPerSuccess = 3f;
        [SerializeField] private bool enableHealOnSuccess = true;

        [Header("Estado de Vero")]
        [SerializeField] private bool isDefeated = false;

        // Eventos C# según especificación Sprint 3
        public event Action<float, float> OnHealthChanged; // current, max
        public event Action<float> OnVeroDamaged;          // damageAmount
        public event Action<float> OnVeroHealed;           // healAmount
        public event Action OnVeroDefeated;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => (maxHealth > 0f) ? (currentHealth / maxHealth) * 100f : 0f;
        public bool IsDefeated => isDefeated;
        public float DamagePerError => damagePerError;
        public float HealPerSuccess => healPerSuccess;

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
            ResetHealth();
        }

        /// <summary>
        /// Restablece la vida de Vero al 100%.
        /// </summary>
        public void ResetHealth()
        {
            isDefeated = false;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (CharacterControllerUI.Instance != null)
            {
                CharacterControllerUI.Instance.SetVeroState(VeroState.Idle);
            }
        }

        /// <summary>
        /// Aplica daño a Vero y verifica condición de derrota.
        /// </summary>
        public void SetErrorDamage(float damage)
        {
            damagePerError = damage;
        }

        public void TakeDamage(float amount = -1f)
        {
            if (isDefeated) return;

            float actualDamage = (amount > 0f) ? amount : damagePerError;
            currentHealth -= actualDamage;
            currentHealth = Mathf.Max(0f, currentHealth);

            Debug.Log($"[VeroHealthController] Vero recibió {actualDamage} de daño. Vida actual: {currentHealth:F0}/{maxHealth}");

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnVeroDamaged?.Invoke(actualDamage);

            if (currentHealth <= 0f)
            {
                TriggerDefeat();
            }
            else
            {
                if (CharacterControllerUI.Instance != null)
                {
                    CharacterControllerUI.Instance.OnPlayerError();
                }
            }
        }

        /// <summary>
        /// Recupera una cantidad de vida para Vero.
        /// </summary>
        public void Heal(float amount = -1f)
        {
            if (isDefeated || !enableHealOnSuccess) return;

            float actualHeal = (amount > 0f) ? amount : healPerSuccess;
            if (currentHealth >= maxHealth) return;

            currentHealth += actualHeal;
            currentHealth = Mathf.Min(maxHealth, currentHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnVeroHealed?.Invoke(actualHeal);
        }

        private void TriggerDefeat()
        {
            if (isDefeated) return;
            isDefeated = true;
            currentHealth = 0f;

            Debug.LogWarning("[VeroHealthController] ¡Vero ha sido derrotado! Activando GameOver.");

            OnHealthChanged?.Invoke(0f, maxHealth);
            OnVeroDefeated?.Invoke();

            if (CharacterControllerUI.Instance != null)
            {
                CharacterControllerUI.Instance.SetVeroState(VeroState.Defeated);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}