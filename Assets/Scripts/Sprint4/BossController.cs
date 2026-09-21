using System;
using System.Collections;
using UnityEngine;

namespace ReCiclo.Sprint4
{
    public enum BossPhase
    {
        Intro,
        Phase1_Normal,      // Oleadas moderadas
        Phase2_Surge,       // Aceleración de camión recolector y ráfagas
        Phase3_Frenzy,      // Ráfagas desesperadas y residuos peligrosos
        Defeated
    }

    public class BossController : MonoBehaviour
    {
        public static BossController Instance { get; private set; }

        [Header("Estadísticas del Jefe")]
        [SerializeField] private string bossName = "Sr. Basura";
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float currentHealth;

        [Header("Referencias Visuales y UI")]
        [SerializeField] private BossHealthBar bossHealthBar;
        [SerializeField] private SpriteRenderer bossSpriteRenderer;
        [SerializeField] private Transform garbageTruckTransform;
        [SerializeField] private ParticleSystem exhaustSmokeParticles;

        [Header("Fases y Dificultad")]
        [SerializeField] private BossPhase currentPhase = BossPhase.Intro;
        [SerializeField] private float phase2HealthThreshold = 300f; // 60% HP
        [SerializeField] private float phase3HealthThreshold = 150f; // 30% HP

        [Header("Configuración de Ataque")]
        [SerializeField] private float phase1SpawnInterval = 2.0f;
        [SerializeField] private float phase2SpawnInterval = 1.2f;
        [SerializeField] private float phase3SpawnInterval = 0.7f;

        public event Action<BossPhase> OnPhaseChanged;
        public event Action OnBossDefeated;
        public event Action<float, float> OnBossHealthChanged; // current, max

        private Coroutine attackRoutine;
        private Vector3 truckOriginalPos;

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
            currentHealth = maxHealth;
            if (garbageTruckTransform != null)
            {
                truckOriginalPos = garbageTruckTransform.localPosition;
            }

            if (bossHealthBar != null)
            {
                bossHealthBar.InitializeBar(maxHealth);
            }
        }

        public void StartBossBattle()
        {
            currentHealth = maxHealth;
            SetPhase(BossPhase.Phase1_Normal);

            if (bossHealthBar != null)
            {
                bossHealthBar.gameObject.SetActive(true);
                bossHealthBar.InitializeBar(maxHealth);
            }

            Debug.Log($"[BossController] ¡Comienza la batalla contra {bossName}!");
        }

        public void TakeDamage(float damageAmount)
        {
            if (currentPhase == BossPhase.Defeated) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(0f, currentHealth);

            if (bossHealthBar != null)
            {
                bossHealthBar.SetHealth(currentHealth, true);
            }

            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

            StartCoroutine(FlashBossRedRoutine());

            // Verificar cambio de fase
            CheckPhaseTransition();

            // Derrota del jefe
            if (currentHealth <= 0f)
            {
                DefeatBoss();
            }
        }

        private void CheckPhaseTransition()
        {
            if (currentPhase == BossPhase.Phase1_Normal && currentHealth <= phase2HealthThreshold)
            {
                SetPhase(BossPhase.Phase2_Surge);
            }
            else if (currentPhase == BossPhase.Phase2_Surge && currentHealth <= phase3HealthThreshold)
            {
                SetPhase(BossPhase.Phase3_Frenzy);
            }
        }

        private void SetPhase(BossPhase newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);

            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            switch (newPhase)
            {
                case BossPhase.Phase1_Normal:
                    attackRoutine = StartCoroutine(AttackLoopRoutine(phase1SpawnInterval));
                    break;
                case BossPhase.Phase2_Surge:
                    if (exhaustSmokeParticles != null) exhaustSmokeParticles.Play();
                    attackRoutine = StartCoroutine(AttackLoopRoutine(phase2SpawnInterval));
                    Debug.Log($"[BossController] {bossName} entra en FASE 2: RÁFAGA DE RESIDUOS");
                    break;
                case BossPhase.Phase3_Frenzy:
                    if (exhaustSmokeParticles != null)
                    {
                        var main = exhaustSmokeParticles.main;
                        main.simulationSpeed = 2f;
                    }
                    attackRoutine = StartCoroutine(AttackLoopRoutine(phase3SpawnInterval));
                    Debug.Log($"[BossController] {bossName} entra en FASE 3: FRENESÍ FINAL");
                    break;
            }
        }

        private IEnumerator AttackLoopRoutine(float interval)
        {
            while (currentPhase != BossPhase.Defeated)
            {
                yield return new WaitForSeconds(interval);
                LaunchGarbageWave();
            }
        }

        private void LaunchGarbageWave()
        {
            // Simula sacudida del camión recolector al lanzar basura
            if (garbageTruckTransform != null)
            {
                StartCoroutine(ShakeTruckRoutine());
            }

            Debug.Log($"[BossController] Camión Recolector dispara oleada de basura en fase: {currentPhase}");
        }

        private IEnumerator ShakeTruckRoutine()
        {
            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                float offsetX = UnityEngine.Random.Range(-0.1f, 0.1f);
                if (garbageTruckTransform != null)
                {
                    garbageTruckTransform.localPosition = truckOriginalPos + new Vector3(offsetX, 0f, 0f);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (garbageTruckTransform != null)
            {
                garbageTruckTransform.localPosition = truckOriginalPos;
            }
        }

        private IEnumerator FlashBossRedRoutine()
        {
            if (bossSpriteRenderer != null)
            {
                bossSpriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.12f);
                bossSpriteRenderer.color = Color.white;
            }
        }

        private void DefeatBoss()
        {
            SetPhase(BossPhase.Defeated);
            if (attackRoutine != null) StopCoroutine(attackRoutine);

            if (exhaustSmokeParticles != null) exhaustSmokeParticles.Stop();

            Debug.Log($"[BossController] ¡{bossName} ha sido derrotado! La ciudad ha sido totalmente limpiada.");

            OnBossDefeated?.Invoke();

            if (EnvironmentController.Instance != null)
            {
                EnvironmentController.Instance.SetPollutionLevel(0f, false);
            }
        }

        public float GetHealthPercent()
        {
            return (currentHealth / maxHealth) * 100f;
        }
    }
}
