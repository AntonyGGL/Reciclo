using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint1;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint4
{
    public enum BossState
    {
        Hidden,
        Intro,
        Idle,
        Attacking,
        Hurt,
        Defeated
    }

    /// <summary>
    /// Controlador principal del villano final: Sr. Basura.
    /// Administra estados, animaciones de ataque/flotación, oleadas de residuos y smog tóxico.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        private static BossController _instance;
        public static BossController Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<BossController>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Estadísticas de Sr. Basura")]
        [SerializeField] private string bossName = "Sr. Basura";
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float currentHealth = 500f;
        [SerializeField] private BossState currentState = BossState.Hidden;

        [Header("Referencias Visuales")]
        [SerializeField] private RectTransform bossRectTransform;
        [SerializeField] private UnityEngine.UI.Image bossImage;
        [SerializeField] private UnityEngine.UI.Text bossSpeechText;
        [SerializeField] private Sprite bossIdleSprite;
        [SerializeField] private Sprite bossAttackSprite;
        [SerializeField] private Sprite bossHurtSprite;
        [SerializeField] private Sprite bossDefeatedSprite;

        [Header("Configuración de Oleadas de Ataque")]
        [SerializeField] private int wastesPerWave = 2;
        [SerializeField] private float timeBetweenWaves = 2.5f;
        [SerializeField] private float waveFallSpeed = 400f;
        [SerializeField] private bool enableSmogAttack = true;
        [SerializeField] private float smogTriggerChance = 0.4f;

        [Header("Animación de Flotación (Idle)")]
        [SerializeField] private float bobAmplitude = 12f;
        [SerializeField] private float bobSpeed = 2.5f;

        // Eventos C# según especificación Sprint 4
        public event Action<BossState> OnBossStateChanged;
        public event Action<float, float> OnBossHealthChanged;
        public event Action<float> OnBossDamaged;
        public event Action OnBossDefeated;

        public BossState CurrentState => currentState;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => (maxHealth > 0f) ? (currentHealth / maxHealth) * 100f : 0f;

        private Coroutine stateRoutine;
        private Coroutine attackLoopRoutine;
        private Coroutine bobbingRoutine;
        private Vector2 initialAnchoredPos;
        private bool isBattleActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (bossRectTransform == null) bossRectTransform = GetComponent<RectTransform>();
            if (bossRectTransform != null) initialAnchoredPos = bossRectTransform.anchoredPosition;
        }

        private void Start()
        {
            if (currentState == BossState.Hidden)
            {
                SetBossState(BossState.Idle);
            }
        }

        /// <summary>
        /// Inicia la batalla contra Sr. Basura en el nivel final.
        /// </summary>
        public void InitializeBoss(float hp = -1f)
        {
            if (hp > 0f)
            {
                maxHealth = hp;
                currentHealth = hp;
            }
            StartBossBattle();
        }

        public void StartBossBattle()
        {
            currentHealth = maxHealth;
            isBattleActive = true;
            gameObject.SetActive(true);

            if (BossHealthBar.Instance != null)
            {
                BossHealthBar.Instance.InitializeBar(maxHealth);
            }

            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            SetBossState(BossState.Intro);
            ShowSpeech("¡JAJAJA! ¡La ciudad será devorada por la BASURA!", 2.5f);

            // Descenso épico desde arriba
            if (bossRectTransform != null)
            {
                Vector2 startPos = initialAnchoredPos + new Vector2(0, 300);
                bossRectTransform.anchoredPosition = startPos;
                float elapsed = 0f;
                float duration = 1.5f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    bossRectTransform.anchoredPosition = Vector2.Lerp(startPos, initialAnchoredPos, elapsed / duration);
                    yield return null;
                }
                bossRectTransform.anchoredPosition = initialAnchoredPos;
            }

            yield return new WaitForSeconds(1.0f);

            SetBossState(BossState.Idle);
            StartAttackLoop();
        }

        public void SetBossState(BossState newState)
        {
            currentState = newState;
            OnBossStateChanged?.Invoke(newState);

            // Actualizar Sprite según estado
            if (bossImage != null)
            {
                switch (newState)
                {
                    case BossState.Idle:
                        if (bossIdleSprite != null) bossImage.sprite = bossIdleSprite;
                        bossImage.color = Color.white;
                        break;
                    case BossState.Attacking:
                        if (bossAttackSprite != null) bossImage.sprite = bossAttackSprite;
                        break;
                    case BossState.Hurt:
                        if (bossHurtSprite != null) bossImage.sprite = bossHurtSprite;
                        bossImage.color = new Color(1f, 0.4f, 0.4f);
                        break;
                    case BossState.Defeated:
                        if (bossDefeatedSprite != null) bossImage.sprite = bossDefeatedSprite;
                        bossImage.color = new Color(0.6f, 0.6f, 0.6f);
                        break;
                    case BossState.Hidden:
                        bossImage.color = new Color(1f, 1f, 1f, 0f);
                        break;
                }
            }

            // Iniciar animación de flotación si está en Idle
            if (newState == BossState.Idle)
            {
                if (bobbingRoutine == null) bobbingRoutine = StartCoroutine(BobbingRoutine());
            }
            else
            {
                if (bobbingRoutine != null)
                {
                    StopCoroutine(bobbingRoutine);
                    bobbingRoutine = null;
                }
            }
        }

        private IEnumerator BobbingRoutine()
        {
            while (currentState == BossState.Idle)
            {
                if (bossRectTransform != null)
                {
                    float offsetY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
                    bossRectTransform.anchoredPosition = initialAnchoredPos + new Vector2(0, offsetY);
                }
                yield return null;
            }
        }

        private void StartAttackLoop()
        {
            if (attackLoopRoutine != null) StopCoroutine(attackLoopRoutine);
            attackLoopRoutine = StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (isBattleActive && currentState != BossState.Defeated)
            {
                // Dificultad progresiva según la vida restante
                float hpPercent = HealthPercent;
                if (hpPercent > 66f)
                {
                    wastesPerWave = 2;
                    timeBetweenWaves = 2.8f;
                    waveFallSpeed = 380f;
                }
                else if (hpPercent > 33f)
                {
                    wastesPerWave = 3;
                    timeBetweenWaves = 1.9f;
                    waveFallSpeed = 460f;
                }
                else
                {
                    // Fase Frenesí Final
                    wastesPerWave = 4;
                    timeBetweenWaves = 1.2f;
                    waveFallSpeed = 540f;
                }

                yield return new WaitForSeconds(timeBetweenWaves);

                if (isBattleActive && currentState != BossState.Defeated && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    yield return LaunchWaveRoutine();
                }
            }
        }

        private IEnumerator LaunchWaveRoutine()
        {
            SetBossState(BossState.Attacking);
            ShowSpeech("¡TOMA ESTA BASURA!", 1.2f);

            // Animación de ataque: Squash & Stretch punch
            if (bossRectTransform != null)
            {
                Vector3 originalScale = Vector3.one;
                bossRectTransform.localScale = new Vector3(1.3f, 0.8f, 1f);
                yield return new WaitForSeconds(0.15f);
                bossRectTransform.localScale = originalScale;
            }

            // Lanzar residuos en carriles aleatorios
            if (LaneSpawner.Instance != null)
            {
                int totalLanes = LaneSpawner.Instance.LaneCount;
                List<int> availableLanes = new List<int>();
                for (int i = 0; i < totalLanes; i++) availableLanes.Add(i);

                int countToSpawn = Mathf.Min(wastesPerWave, totalLanes);
                for (int i = 0; i < countToSpawn; i++)
                {
                    if (availableLanes.Count == 0) break;
                    int randomIndex = UnityEngine.Random.Range(0, availableLanes.Count);
                    int chosenLane = availableLanes[randomIndex];
                    availableLanes.RemoveAt(randomIndex);

                    var waste = LaneSpawner.Instance.SpawnNextWaste(chosenLane);
                    if (waste != null)
                    {
                        waste.FallSpeed = waveFallSpeed;
                    }
                }
            }

            // Posibilidad de activar Smog
            if (enableSmogAttack && (HealthPercent <= 66f) && UnityEngine.Random.value < smogTriggerChance)
            {
                TriggerSmogWave();
            }

            yield return new WaitForSeconds(0.3f);
            if (currentState != BossState.Defeated)
            {
                SetBossState(BossState.Idle);
            }
        }

        public void TriggerSmogWave()
        {
            if (SmogController.Instance != null)
            {
                float smogDuration = (HealthPercent <= 33f) ? 6.0f : 4.5f;
                SmogController.Instance.ActivateSmog(smogDuration, 0.7f);
                ShowSpeech("¡Siente la asfixia del SMOG TÓXICO!", 2.0f);
            }
        }

        /// <summary>
        /// Aplica daño a Sr. Basura cuando el jugador clasifica correctamente.
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (currentState == BossState.Defeated) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(0f, currentHealth);

            Debug.Log($"[BossController] Sr. Basura recibió {damageAmount} de daño. Vida: {currentHealth:F0}/{maxHealth}");

            if (BossHealthBar.Instance != null)
            {
                BossHealthBar.Instance.SetHealth(currentHealth, true);
            }

            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
            OnBossDamaged?.Invoke(damageAmount);

            if (CharacterControllerUI.Instance != null)
            {
                CharacterControllerUI.Instance.OnBossDamaged();
            }

            if (currentHealth <= 0f)
            {
                DefeatBoss();
            }
            else
            {
                StartCoroutine(HurtFlashRoutine());
            }
        }

        /// <summary>
        /// Llamado cuando el jugador clasifica un residuo para calcular daño al jefe por combos y rachas perfectas.
        /// </summary>
        public void OnPlayerClassified(bool isPerfect, int comboStreak)
        {
            if (currentState == BossState.Defeated) return;

            float damage = 15f;

            // Bonificación por combo (cada 5 aciertos causa daño extra)
            if (comboStreak >= 5)
            {
                damage += 10f;
            }
            if (comboStreak >= 10)
            {
                damage += 15f;
            }

            // Bonificación por clasificación perfecta
            if (isPerfect)
            {
                damage += 10f;
                if (SmogController.Instance != null)
                {
                    SmogController.Instance.OnCleanHit();
                }
            }

            TakeDamage(damage);
        }

        private IEnumerator HurtFlashRoutine()
        {
            SetBossState(BossState.Hurt);

            if (bossRectTransform != null)
            {
                bossRectTransform.localScale = Vector3.one * 0.9f;
            }

            yield return new WaitForSeconds(0.2f);

            if (bossRectTransform != null)
            {
                bossRectTransform.localScale = Vector3.one;
            }

            if (currentState != BossState.Defeated)
            {
                SetBossState(BossState.Idle);
            }
        }

        private void DefeatBoss()
        {
            if (currentState == BossState.Defeated) return;
            isBattleActive = false;
            SetBossState(BossState.Defeated);

            if (attackLoopRoutine != null) StopCoroutine(attackLoopRoutine);
            if (bobbingRoutine != null) StopCoroutine(bobbingRoutine);

            if (SmogController.Instance != null)
            {
                SmogController.Instance.ClearSmog(true);
            }

            if (LaneSpawner.Instance != null)
            {
                LaneSpawner.Instance.StopSpawning();
            }

            ShowSpeech("¡NOOO! ¡MI AMADA BASURA! ¡ME LAS PAGARÁS VERO...!", 3.5f);
            Debug.LogWarning("[BossController] ¡SR. BASURA HA SIDO DERROTADO! ¡Ciudad Limpia restaurada!");

            OnBossDefeated?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }

            StartCoroutine(DefeatAnimationRoutine());
        }

        private IEnumerator DefeatAnimationRoutine()
        {
            float elapsed = 0f;
            float duration = 2.0f;
            Vector3 startScale = (bossRectTransform != null) ? bossRectTransform.localScale : Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                if (bossRectTransform != null)
                {
                    bossRectTransform.Rotate(0, 0, 360f * Time.deltaTime);
                    bossRectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                }

                if (bossImage != null)
                {
                    bossImage.color = new Color(0.6f, 0.6f, 0.6f, 1f - progress);
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }

        private void ShowSpeech(string text, float duration = 2f)
        {
            if (bossSpeechText != null)
            {
                if (stateRoutine != null) StopCoroutine(stateRoutine);
                stateRoutine = StartCoroutine(SpeechRoutine(text, duration));
            }
        }

        private IEnumerator SpeechRoutine(string text, float duration)
        {
            bossSpeechText.gameObject.SetActive(true);
            bossSpeechText.text = text;
            yield return new WaitForSeconds(duration);
            bossSpeechText.gameObject.SetActive(false);
        }
    }
}
