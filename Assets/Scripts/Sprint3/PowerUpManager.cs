using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint1;
using ReCiclo.Sprint2;
using ReCiclo.Sprint5;

namespace ReCiclo.Sprint3
{
    public enum PowerUpType
    {
        GoldenClock,
        DoubleScore,
        RainbowBin
    }

    [Serializable]
    public class PowerUpConfig
    {
        public PowerUpType type;
        public string displayName;
        public Color iconColor;
        public float duration = 10f;
        [Range(0f, 1f)] public float spawnChance = 0.08f;
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Configuracion de Power-Ups")]
        [SerializeField] private List<PowerUpConfig> powerUpConfigs = new List<PowerUpConfig>();
        [SerializeField] private float powerUpSpawnInterval = 25f;
        [SerializeField] private float powerUpSpawnIntervalVariance = 8f;

        [Header("Efectos Activos")]
        [SerializeField] private bool isDoubleScoreActive = false;
        [SerializeField] private bool isRainbowBinActive = false;
        [SerializeField] private float doubleScoreTimeRemaining = 0f;
        [SerializeField] private float rainbowBinTimeRemaining = 0f;

        [Header("HUD de Power-Ups")]
        [SerializeField] private GameObject powerUpHUDPanel;
        [SerializeField] private Text activeEffectText;
        [SerializeField] private Image activeEffectIcon;
        [SerializeField] private Text countdownText;

        [Header("Notificacion Central")]
        [SerializeField] private Text centerNotificationText;
        [SerializeField] private float notificationDuration = 2.0f;

        public event Action<PowerUpType> OnPowerUpActivated;
        public event Action<PowerUpType> OnPowerUpExpired;
        public event Action<PowerUpType> OnPowerUpSpawned;

        private Coroutine doubleScoreRoutine;
        private Coroutine rainbowBinRoutine;
        private Coroutine spawnRoutine;
        private Coroutine notificationRoutine;

        public bool IsDoubleScoreActive => isDoubleScoreActive;
        public bool IsRainbowBinActive => isRainbowBinActive;
        public float DoubleScoreTimeRemaining => doubleScoreTimeRemaining;
        public float RainbowBinTimeRemaining => rainbowBinTimeRemaining;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultConfigs();
        }

        private void Start()
        {
            if (powerUpHUDPanel != null) powerUpHUDPanel.SetActive(false);
            if (centerNotificationText != null) centerNotificationText.gameObject.SetActive(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += HandleGameStart;
                GameManager.Instance.OnVictory += HandleGameEnd;
                GameManager.Instance.OnGameOver += HandleGameEnd;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= HandleGameStart;
                GameManager.Instance.OnVictory -= HandleGameEnd;
                GameManager.Instance.OnGameOver -= HandleGameEnd;
            }
        }

        private void InitializeDefaultConfigs()
        {
            if (powerUpConfigs != null && powerUpConfigs.Count > 0) return;
            powerUpConfigs = new List<PowerUpConfig>
            {
                new PowerUpConfig { type = PowerUpType.GoldenClock, displayName = "Reloj Dorado", iconColor = new Color(1f, 0.84f, 0f), duration = 0f, spawnChance = 0.10f },
                new PowerUpConfig { type = PowerUpType.DoubleScore, displayName = "Estrella 2X", iconColor = new Color(1f, 0.6f, 0f), duration = 10f, spawnChance = 0.08f },
                new PowerUpConfig { type = PowerUpType.RainbowBin, displayName = "Contenedor Arcoiris", iconColor = new Color(0.6f, 0.2f, 1f), duration = 10f, spawnChance = 0.06f }
            };
        }

        public void ActivatePowerUp(PowerUpType type)
        {
            PowerUpConfig config = GetConfig(type);
            if (config == null) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(SoundType.PowerUp);
            switch (type)
            {
                case PowerUpType.GoldenClock: ActivateGoldenClock(); break;
                case PowerUpType.DoubleScore: ActivateDoubleScore(config.duration); break;
                case PowerUpType.RainbowBin: ActivateRainbowBin(config.duration); break;
            }
            OnPowerUpActivated?.Invoke(type);
            ShowCenterNotification(config.displayName + " ACTIVADO!", config.iconColor);
            Debug.Log("[PowerUpManager] Power-Up activado: " + config.displayName);
        }

        private void ActivateGoldenClock()
        {
            if (LevelTimer.Instance != null) LevelTimer.Instance.AddTime(15f);
            ShowHUDEffect("+ 15s", new Color(1f, 0.84f, 0f), 2.0f);
        }

        private void ActivateDoubleScore(float duration)
        {
            if (doubleScoreRoutine != null) StopCoroutine(doubleScoreRoutine);
            doubleScoreRoutine = StartCoroutine(DoubleScoreRoutine(duration));
        }

        private void ActivateRainbowBin(float duration)
        {
            if (rainbowBinRoutine != null) StopCoroutine(rainbowBinRoutine);
            rainbowBinRoutine = StartCoroutine(RainbowBinRoutine(duration));
        }

        private IEnumerator DoubleScoreRoutine(float duration)
        {
            isDoubleScoreActive = true;
            doubleScoreTimeRemaining = duration;
            ShowHUDEffect("2X PUNTAJE", new Color(1f, 0.6f, 0f), duration);
            while (doubleScoreTimeRemaining > 0f)
            {
                doubleScoreTimeRemaining -= Time.deltaTime;
                UpdateCountdownDisplay(doubleScoreTimeRemaining);
                yield return null;
            }
            isDoubleScoreActive = false;
            doubleScoreTimeRemaining = 0f;
            HideHUDEffect();
            OnPowerUpExpired?.Invoke(PowerUpType.DoubleScore);
        }

        private IEnumerator RainbowBinRoutine(float duration)
        {
            isRainbowBinActive = true;
            rainbowBinTimeRemaining = duration;
            RecycleBin[] bins = UnityEngine.Object.FindObjectsByType<RecycleBin>(FindObjectsSortMode.None);
            foreach (var bin in bins) bin.SetWildcardMode(true);
            ShowHUDEffect("COMODIN", new Color(0.6f, 0.2f, 1f), duration);
            while (rainbowBinTimeRemaining > 0f)
            {
                rainbowBinTimeRemaining -= Time.deltaTime;
                UpdateCountdownDisplay(rainbowBinTimeRemaining);
                yield return null;
            }
            foreach (var bin in bins) { if (bin != null) bin.SetWildcardMode(false); }
            isRainbowBinActive = false;
            rainbowBinTimeRemaining = 0f;
            HideHUDEffect();
            OnPowerUpExpired?.Invoke(PowerUpType.RainbowBin);
        }

        public void StartPowerUpSpawning()
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            spawnRoutine = StartCoroutine(PowerUpSpawnLoop());
        }

        public void StopPowerUpSpawning()
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        }

        private IEnumerator PowerUpSpawnLoop()
        {
            yield return new WaitForSeconds(powerUpSpawnInterval * 0.5f);
            while (true)
            {
                float variance = UnityEngine.Random.Range(-powerUpSpawnIntervalVariance, powerUpSpawnIntervalVariance);
                float nextSpawnTime = Mathf.Max(8f, powerUpSpawnInterval + variance);
                yield return new WaitForSeconds(nextSpawnTime);
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                    TrySpawnPowerUp();
            }
        }

        private void TrySpawnPowerUp()
        {
            float totalChance = 0f;
            foreach (var config in powerUpConfigs) totalChance += config.spawnChance;
            float roll = UnityEngine.Random.Range(0f, totalChance);
            float accumulated = 0f;
            PowerUpType selectedType = PowerUpType.GoldenClock;
            foreach (var config in powerUpConfigs)
            {
                accumulated += config.spawnChance;
                if (roll <= accumulated) { selectedType = config.type; break; }
            }
            ActivatePowerUp(selectedType);
            OnPowerUpSpawned?.Invoke(selectedType);
        }

        private Coroutine hudRoutine;

        private void ShowHUDEffect(string effectName, Color color, float duration)
        {
            if (powerUpHUDPanel != null) powerUpHUDPanel.SetActive(true);
            if (activeEffectText != null) { activeEffectText.text = effectName; activeEffectText.color = color; }
            if (activeEffectIcon != null) activeEffectIcon.color = color;
            if (duration > 0f && hudRoutine != null) StopCoroutine(hudRoutine);
            if (duration > 0f) hudRoutine = StartCoroutine(AutoHideHUD(duration));
        }

        private void UpdateCountdownDisplay(float timeLeft)
        {
            if (countdownText != null) countdownText.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)) + "s";
        }

        private void HideHUDEffect()
        {
            if (powerUpHUDPanel != null) powerUpHUDPanel.SetActive(false);
        }

        private IEnumerator AutoHideHUD(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideHUDEffect();
        }

        private void ShowCenterNotification(string message, Color color)
        {
            if (centerNotificationText == null) return;
            if (notificationRoutine != null) StopCoroutine(notificationRoutine);
            notificationRoutine = StartCoroutine(CenterNotificationRoutine(message, color));
        }

        private IEnumerator CenterNotificationRoutine(string message, Color color)
        {
            centerNotificationText.gameObject.SetActive(true);
            centerNotificationText.text = message;
            centerNotificationText.color = color;
            RectTransform rt = centerNotificationText.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.zero;
                float elapsed = 0f;
                while (elapsed < 0.3f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 0.3f;
                    float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                    rt.localScale = Vector3.one * Mathf.Lerp(0f, overshoot, t);
                    yield return null;
                }
                rt.localScale = Vector3.one;
            }
            yield return new WaitForSeconds(notificationDuration);
            float fadeElapsed = 0f;
            Color startColor = centerNotificationText.color;
            while (fadeElapsed < 0.4f)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = 1f - (fadeElapsed / 0.4f);
                centerNotificationText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
            centerNotificationText.gameObject.SetActive(false);
            centerNotificationText.color = startColor;
        }

        private void HandleGameStart()
        {
            isDoubleScoreActive = false;
            isRainbowBinActive = false;
            doubleScoreTimeRemaining = 0f;
            rainbowBinTimeRemaining = 0f;
            if (doubleScoreRoutine != null) StopCoroutine(doubleScoreRoutine);
            if (rainbowBinRoutine != null) StopCoroutine(rainbowBinRoutine);
            HideHUDEffect();
            StartPowerUpSpawning();
        }

        private void HandleGameEnd()
        {
            StopPowerUpSpawning();
            if (doubleScoreRoutine != null) StopCoroutine(doubleScoreRoutine);
            if (rainbowBinRoutine != null) StopCoroutine(rainbowBinRoutine);
            isDoubleScoreActive = false;
            isRainbowBinActive = false;
            HideHUDEffect();
        }

        public PowerUpConfig GetConfig(PowerUpType type)
        {
            return powerUpConfigs?.Find(c => c.type == type);
        }

        public int GetScoreMultiplier()
        {
            return isDoubleScoreActive ? 2 : 1;
        }
    }
}