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

        [Header("Jefes por nivel")]
        [SerializeField] private float canBasuraHealth = 300f;
        [SerializeField] private float mercadoBossHealth = 400f;
        [SerializeField] private float señorBasuraHealth = 500f;
        [SerializeField] private float canBasuraFramesPerSecond = 8f;
        [SerializeField] private float senorBasuraFramesPerSecond = 8f;
        [SerializeField] private float finalBossHealPerError = 25f;

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
        private int activeBossPhase = -1; // -1: sin jefe, 0: Can Basura, 1: Señor Basura (Mercado), 2: jefe final
        private Coroutine spriteAnimationRoutine;
        private Sprite[] canIdleFrames;
        private Sprite[] canAttackFrames;
        private Sprite[] canHurtFrames;
        private Sprite[] canDefeatedFrames;
        private Sprite[] senorIdleFrames;
        private Sprite[] senorAttackFrames;
        private Sprite[] senorHurtFrames;
        private Sprite[] senorDefeatedFrames;
        private UnityEngine.UI.Text bossTitleText;
        private Coroutine healFeedbackRoutine;

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
            bossTitleText = FindChildText("CrownLabel");
            ConfigureSpeechText();
            LoadCanBasuraSprites();
            LoadSenorBasuraSprites();
        }

        private void Start()
        {
            if (!ConfigureBossForCurrentLevel()) return;
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

            if (!ConfigureBossForCurrentLevel()) return;
            maxHealth = activeBossPhase switch
            {
                0 => canBasuraHealth,
                1 => mercadoBossHealth,
                _ => señorBasuraHealth
            };
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
                BossHealthBar.Instance.SetBossName(bossName);
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

            if (activeBossPhase == 0) ApplyCanBasuraVisual(newState);
            else if (activeBossPhase == 1) ApplyMercadoBossVisual(newState);
            else if (activeBossPhase == 2) ApplySenorBasuraVisual(newState);

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
        /// El jefe final recupera vida cuando el jugador clasifica mal o deja caer un residuo.
        /// No afecta a Can Basura ni a niveles sin una batalla activa.
        /// </summary>
        public void RecoverHealthFromPlayerError()
        {
            if (activeBossPhase != 2 || !isBattleActive || currentState == BossState.Defeated) return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0f, finalBossHealPerError));
            float recoveredHealth = currentHealth - previousHealth;
            if (recoveredHealth <= 0f) return;

            if (BossHealthBar.Instance != null)
            {
                BossHealthBar.Instance.SetHealth(currentHealth, false);
            }

            OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
            ShowSpeech($"¡TU ERROR ME DEVUELVE {Mathf.CeilToInt(recoveredHealth)} DE VIDA!", 1.4f);

            if (healFeedbackRoutine != null) StopCoroutine(healFeedbackRoutine);
            healFeedbackRoutine = StartCoroutine(HealFeedbackRoutine());

            Debug.Log($"[BossController] {bossName} recuperó {recoveredHealth:F0} de vida por un error. Vida: {currentHealth:F0}/{maxHealth}");
        }

        private IEnumerator HealFeedbackRoutine()
        {
            if (bossImage == null) yield break;

            Color originalColor = bossImage.color;
            Vector3 originalScale = bossRectTransform != null ? bossRectTransform.localScale : Vector3.one;
            bossImage.color = new Color(0.45f, 1f, 0.45f, 1f);
            if (bossRectTransform != null) bossRectTransform.localScale = originalScale * 1.08f;

            yield return new WaitForSeconds(0.3f);

            if (bossImage != null) bossImage.color = originalColor;
            if (bossRectTransform != null) bossRectTransform.localScale = originalScale;
            healFeedbackRoutine = null;
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
            Debug.LogWarning($"[BossController] ¡{bossName.ToUpperInvariant()} HA SIDO DERROTADO! ¡Ciudad Limpia restaurada!");

            OnBossDefeated?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }

            StartCoroutine(DefeatAnimationRoutine());
        }

        private bool ConfigureBossForCurrentLevel()
        {
            LevelData level = WorldMapManager.Instance != null ? WorldMapManager.Instance.GetCurrentLevel() : null;
            if (level == null || level.levelIndex < 1 || level.levelIndex > 3)
            {
                DisableBossForCurrentLevel();
                return false;
            }

            activeBossPhase = level.levelIndex - 1;
            // Nivel 1: Can Basura. Nivel 2: Señor Basura original. Nivel 3: jefe final nuevo.
            bossName = activeBossPhase switch
            {
                0 => "Can Basura",
                1 => "Señor Basura",
                _ => "Señor Basura Final"
            };
            gameObject.name = activeBossPhase switch
            {
                0 => "Boss_CanBasura_Plaza",
                1 => "Boss_SeñorBasura_Mercado",
                _ => "Boss_SeñorBasura_Final"
            };
            ConfigureBossFrame();
            UpdateBossLabels();
            if (activeBossPhase == 0 && canIdleFrames != null) ApplyCanBasuraVisual(currentState);
            if (activeBossPhase == 1) ApplyMercadoBossVisual(currentState);
            if (activeBossPhase == 2 && senorIdleFrames != null) ApplySenorBasuraVisual(currentState);
            return true;
        }

        private void DisableBossForCurrentLevel()
        {
            activeBossPhase = -1;
            isBattleActive = false;
            currentState = BossState.Hidden;

            if (attackLoopRoutine != null) StopCoroutine(attackLoopRoutine);
            if (bobbingRoutine != null) StopCoroutine(bobbingRoutine);
            if (spriteAnimationRoutine != null) StopCoroutine(spriteAnimationRoutine);

            if (bossImage != null) bossImage.color = new Color(1f, 1f, 1f, 0f);
            if (BossHealthBar.Instance != null) BossHealthBar.Instance.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        private void ConfigureBossFrame()
        {
            if (bossRectTransform == null) return;
            bossRectTransform.sizeDelta = activeBossPhase switch
            {
                0 => new Vector2(460f, 380f),
                1 => new Vector2(400f, 340f),
                _ => new Vector2(480f, 400f)
            };

            if (bossImage != null) bossImage.preserveAspect = true;
        }

        private void UpdateBossLabels()
        {
            if (bossTitleText != null)
            {
                bossTitleText.text = activeBossPhase switch
                {
                    0 => "<b>CAN BASURA</b>\n<size=14>Guardián de la Plaza</size>",
                    1 => "<b>SEÑOR BASURA</b>\n<size=14>Amo del Mercado</size>",
                    _ => "<b>SEÑOR BASURA FINAL</b>\n<size=14>Tirano del Basurero</size>"
                };
                bossTitleText.resizeTextForBestFit = true;
                bossTitleText.resizeTextMinSize = 10;
                bossTitleText.resizeTextMaxSize = Mathf.Max(10, bossTitleText.fontSize);
                RectTransform titleRect = bossTitleText.rectTransform;
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(.5f, 0f);
                titleRect.anchoredPosition = new Vector2(0f, 8f);
                titleRect.sizeDelta = new Vector2(0f, 64f);
            }
            if (BossHealthBar.Instance != null) BossHealthBar.Instance.SetBossName(bossName);
        }

        private UnityEngine.UI.Text FindChildText(string childName)
        {
            foreach (UnityEngine.UI.Text label in GetComponentsInChildren<UnityEngine.UI.Text>(true))
                if (label.name == childName) return label;
            return null;
        }

        private void ConfigureSpeechText()
        {
            if (bossSpeechText == null) return;
            bossSpeechText.resizeTextForBestFit = true;
            bossSpeechText.resizeTextMinSize = 10;
            bossSpeechText.resizeTextMaxSize = Mathf.Max(10, bossSpeechText.fontSize);
            bossSpeechText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bossSpeechText.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform bubble = bossSpeechText.transform.parent as RectTransform;
            if (bubble != null)
            {
                bubble.sizeDelta = new Vector2(440f, 110f);
                bubble.anchoredPosition = new Vector2(0f, -155f);
            }
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
            canIdleFrames = CreateSpriteRow(sheet, 0, "CanBasura");
            canAttackFrames = CreateSpriteRow(sheet, 3, "CanBasura");
            canHurtFrames = CreateSpriteRow(sheet, 4, "CanBasura");
            canDefeatedFrames = CreateSpriteRow(sheet, 5, "CanBasura");
        }

        private void LoadSenorBasuraSprites()
        {
            Texture2D sheet = Resources.Load<Texture2D>("Bosses/SenorBasura_Spritesheet");
            if (sheet == null)
            {
                Debug.LogError("[BossController] No se encontró Resources/Bosses/SenorBasura_Spritesheet.png");
                return;
            }

            sheet.filterMode = FilterMode.Bilinear;
            sheet.wrapMode = TextureWrapMode.Clamp;
            senorIdleFrames = CreateSpriteRow(sheet, 0, "SenorBasura");
            senorAttackFrames = CreateSpriteRow(sheet, 3, "SenorBasura");
            senorHurtFrames = CreateSpriteRow(sheet, 4, "SenorBasura");
            senorDefeatedFrames = CreateSpriteRow(sheet, 5, "SenorBasura");
        }

        private static Sprite[] CreateSpriteRow(Texture2D sheet, int rowFromTop, string spriteName)
        {
            const int columns = 6;
            const int rows = 6;
            var frames = new Sprite[columns];
            int yMin = Mathf.RoundToInt((rows - rowFromTop - 1) * sheet.height / (float)rows);
            int yMax = Mathf.RoundToInt((rows - rowFromTop) * sheet.height / (float)rows);
            for (int column = 0; column < columns; column++)
            {
                int xMin = Mathf.RoundToInt(column * sheet.width / (float)columns);
                int xMax = Mathf.RoundToInt((column + 1) * sheet.width / (float)columns);
                var rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                frames[column] = Sprite.Create(sheet, rect, new Vector2(.5f, .5f), 100f);
                frames[column].name = $"{spriteName}_R{rowFromTop}_F{column}";
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

        private void ApplyMercadoBossVisual(BossState state)
        {
            if (bossImage == null) return;
            if (spriteAnimationRoutine != null)
            {
                StopCoroutine(spriteAnimationRoutine);
                spriteAnimationRoutine = null;
            }

            Sprite sprite = state switch
            {
                BossState.Attacking => bossAttackSprite,
                BossState.Hurt => bossHurtSprite,
                BossState.Defeated => bossDefeatedSprite,
                _ => bossIdleSprite
            };
            if (sprite != null) bossImage.sprite = sprite;

            bossImage.color = state switch
            {
                BossState.Hidden => new Color(1f, 1f, 1f, 0f),
                BossState.Hurt => new Color(1f, 0.4f, 0.4f, 1f),
                BossState.Defeated => new Color(0.6f, 0.6f, 0.6f, 1f),
                _ => Color.white
            };
        }

        private void ApplySenorBasuraVisual(BossState state)
        {
            if (bossImage == null) return;
            if (spriteAnimationRoutine != null) StopCoroutine(spriteAnimationRoutine);

            Sprite[] frames = state switch
            {
                BossState.Attacking => senorAttackFrames,
                BossState.Hurt => senorHurtFrames,
                BossState.Defeated => senorDefeatedFrames,
                _ => senorIdleFrames
            };

            bossImage.color = state switch
            {
                BossState.Hidden => new Color(1f, 1f, 1f, 0f),
                BossState.Hurt => new Color(1f, 0.65f, 0.65f, 1f),
                BossState.Defeated => new Color(0.7f, 0.7f, 0.7f, 1f),
                _ => Color.white
            };

            if (frames != null && frames.Length > 0)
            {
                bool loop = state == BossState.Idle || state == BossState.Intro;
                spriteAnimationRoutine = StartCoroutine(AnimateSenorBasuraRoutine(frames, loop));
            }
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

        private IEnumerator AnimateSenorBasuraRoutine(Sprite[] frames, bool loop)
        {
            do
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    if (bossImage != null) bossImage.sprite = frames[i];
                    yield return new WaitForSeconds(1f / Mathf.Max(1f, senorBasuraFramesPerSecond));
                }
            } while (loop && activeBossPhase == 2 &&
                     (currentState == BossState.Idle || currentState == BossState.Intro));
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
