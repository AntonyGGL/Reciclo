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

        [Header("Secuencia de Jefes del Nivel 1")]
        [SerializeField] private float canBasuraHealth = 300f;
        [SerializeField] private float señorBasuraHealth = 500f;
        [SerializeField] private float canBasuraFramesPerSecond = 8f;

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
        private bool twoBossSequence;
        private int activeBossPhase = 1; // 0: Can Basura, 1: Señor Basura
        private Coroutine spriteAnimationRoutine;
        private Sprite[] canIdleFrames;
        private Sprite[] canAttackFrames;
        private Sprite[] canHurtFrames;
        private Sprite[] canDefeatedFrames;

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
            LoadCanBasuraSprites();
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
            if (isBattleActive) return;

            LevelData level = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            twoBossSequence = level != null && level.levelIndex == 1;
            activeBossPhase = twoBossSequence ? 0 : 1;
            bossName = activeBossPhase == 0 ? "Can Basura" : "Señor Basura";
            maxHealth = activeBossPhase == 0 ? canBasuraHealth : señorBasuraHealth;
            currentHealth = maxHealth;
            isBattleActive = true;
            gameObject.SetActive(true);

            if (bossRectTransform != null)
            {
                bossRectTransform.localScale = Vector3.one;
                bossRectTransform.localRotation = Quaternion.identity;
            }

            if (BossHealthBar.Instance != null)
            {
                BossHealthBar.Instance.InitializeBar(maxHealth);
            }

            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            SetBossState(BossState.Intro);
            ShowSpeech(activeBossPhase == 0
                ? "¡GRRR! ¡NADIE LIMPIARÁ MI TERRITORIO!"
                : "¡JAJAJA! ¡Ahora te enfrentarás al SEÑOR BASURA!", 2.5f);

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

            if (activeBossPhase == 0)
            {
                ApplyCanBasuraVisual(newState);
            }

            // Actualizar Sprite según estado
            if (bossImage != null && activeBossPhase == 1)
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

            Debug.Log($"[BossController] {bossName} recibió {damageAmount} de daño. Vida: {currentHealth:F0}/{maxHealth}");

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

            if (twoBossSequence && activeBossPhase == 0)
            {
                StartCoroutine(TransitionToSecondBossRoutine());
                return;
            }

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
            Debug.LogWarning($"[BossController] ¡{bossName.ToUpperInvariant()} HA SIDO DERROTADO! ¡Ciudad Limpia restaurada!");

            OnBossDefeated?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }

            StartCoroutine(DefeatAnimationRoutine());
        }

        private IEnumerator TransitionToSecondBossRoutine()
        {
            isBattleActive = false;
            if (attackLoopRoutine != null) StopCoroutine(attackLoopRoutine);
            if (bobbingRoutine != null) StopCoroutine(bobbingRoutine);
            SetBossState(BossState.Defeated);
            ShowSpeech("¡AUUU! ¡SEÑOR BASURA, AYÚDAME!", 1.5f);
            Debug.LogWarning("[BossController] Can Basura derrotado. Comienza la segunda fase.");

            yield return new WaitForSeconds(1.6f);

            activeBossPhase = 1;
            bossName = "Señor Basura";
            maxHealth = señorBasuraHealth;
            currentHealth = maxHealth;
            if (spriteAnimationRoutine != null)
            {
                StopCoroutine(spriteAnimationRoutine);
                spriteAnimationRoutine = null;
            }
            if (bossRectTransform != null)
            {
                bossRectTransform.localScale = Vector3.one;
                bossRectTransform.localRotation = Quaternion.identity;
                bossRectTransform.anchoredPosition = initialAnchoredPos;
            }
            if (bossImage != null) bossImage.color = Color.white;
            if (BossHealthBar.Instance != null) BossHealthBar.Instance.InitializeBar(maxHealth);

            isBattleActive = true;
            StartCoroutine(IntroSequenceRoutine());
        }

        private void LoadCanBasuraSprites()
        {
            Texture2D sheet = Resources.Load<Texture2D>("Bosses/CanBasura_Spritesheet");
            if (sheet == null)
            {
                Debug.LogError("[BossController] No se encontró Resources/Bosses/CanBasura_Spritesheet.png");
                return;
            }

            sheet.filterMode = FilterMode.Point;
            sheet.wrapMode = TextureWrapMode.Clamp;
            canIdleFrames = CreateSpriteRow(sheet, 0);
            canAttackFrames = CreateSpriteRow(sheet, 3);
            canHurtFrames = CreateSpriteRow(sheet, 4);
            canDefeatedFrames = CreateSpriteRow(sheet, 5);
        }

        private static Sprite[] CreateSpriteRow(Texture2D sheet, int rowFromTop)
        {
            const int columns = 6;
            const int rows = 6;
            int cellWidth = sheet.width / columns;
            int cellHeight = sheet.height / rows;
            var frames = new Sprite[columns];
            int y = sheet.height - ((rowFromTop + 1) * cellHeight);
            for (int column = 0; column < columns; column++)
            {
                var rect = new Rect(column * cellWidth, y, cellWidth, cellHeight);
                frames[column] = Sprite.Create(sheet, rect, new Vector2(.5f, .5f), 100f);
                frames[column].name = $"CanBasura_R{rowFromTop}_F{column}";
            }
            return frames;
        }

        private void ApplyCanBasuraVisual(BossState state)
        {
            if (bossImage == null) return;
            if (spriteAnimationRoutine != null) StopCoroutine(spriteAnimationRoutine);

            Sprite[] frames = state switch
            {
                BossState.Attacking => canAttackFrames,
                BossState.Hurt => canHurtFrames,
                BossState.Defeated => canDefeatedFrames,
                _ => canIdleFrames
            };
            bossImage.color = state == BossState.Hidden ? new Color(1, 1, 1, 0) : Color.white;
            if (frames != null && frames.Length > 0)
                spriteAnimationRoutine = StartCoroutine(AnimateCanBasuraRoutine(frames, state == BossState.Idle));
        }

        private IEnumerator AnimateCanBasuraRoutine(Sprite[] frames, bool loop)
        {
            do
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    if (bossImage != null) bossImage.sprite = frames[i];
                    yield return new WaitForSeconds(1f / Mathf.Max(1f, canBasuraFramesPerSecond));
                }
            } while (loop && activeBossPhase == 0 && currentState == BossState.Idle);
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
