using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    public class LevelTimer : MonoBehaviour
    {
        public static LevelTimer Instance { get; private set; }

        [Header("ConfiguraciÃ³n del Reloj")]
        [SerializeField] private float timeRemaining = 90f;
        [SerializeField] private bool isTimerRunning = false;
        [SerializeField] private Text timerText;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.red;
        [SerializeField] private float warningThresholdSeconds = 15f;

        public event Action OnTimerExpired;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartTimer(float duration)
        {
            timeRemaining = duration;
            isTimerRunning = true;
            UpdateUI();
        }

        public void AddTime(float seconds)
        {
            timeRemaining += seconds;
            UpdateUI();
        }

        public void StopTimer()
        {
            isTimerRunning = false;
        }

        private void Update()
        {
            if (!isTimerRunning) return;

            if (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                timeRemaining = 0f;
                isTimerRunning = false;
                UpdateUI();
                OnTimerExpired?.Invoke();

                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.OnTimeExpired();
                }
            }
        }

        private void UpdateUI()
        {
            if (timerText == null) return;

            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, timeRemaining));
            timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

            if (timeRemaining <= warningThresholdSeconds)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }
}
