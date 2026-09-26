using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    /// <summary>
    /// Administra rachas de aciertos consecutivos y multiplicadores de puntuación estilo Guitar Hero.
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
                private static ComboSystem _instance;
        public static ComboSystem Instance
        {
            get
            {
                if (_instance == null) _instance = UnityEngine.Object.FindAnyObjectByType<ComboSystem>();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Configuración de Combo")]
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private int currentMultiplier = 1;
        [SerializeField] private UnityEngine.UI.Text comboText;

        public event Action<int, int> OnComboChanged; // combo, multiplier

        public int CurrentCombo => currentCombo;
        public int CurrentMultiplier => currentMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddCombo()
        {
            currentCombo++;
            CalculateMultiplier();
            UpdateUI();
            OnComboChanged?.Invoke(currentCombo, currentMultiplier);
        }

        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                Debug.Log($"[ComboSystem] Combo reseteado (Racha anterior: {currentCombo}).");
            }
            currentCombo = 0;
            currentMultiplier = 1;
            UpdateUI();
            OnComboChanged?.Invoke(currentCombo, currentMultiplier);
        }

        private void CalculateMultiplier()
        {
            if (currentCombo >= 20) currentMultiplier = 4;
            else if (currentCombo >= 10) currentMultiplier = 3;
            else if (currentCombo >= 5) currentMultiplier = 2;
            else currentMultiplier = 1;
        }

        private void UpdateUI()
        {
            if (comboText == null) return;

            if (currentCombo >= 2)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"🔥 COMBO x{currentCombo} (x{currentMultiplier})";
                comboText.color = GetMultiplierColor(currentMultiplier);
                transform.localScale = Vector3.one * 1.15f;
            }
            else
            {
                comboText.gameObject.SetActive(false);
                transform.localScale = Vector3.one;
            }
        }

        private Color GetMultiplierColor(int mult)
        {
            switch (mult)
            {
                case 2: return new Color(0.2f, 0.8f, 1f); // Celeste
                case 3: return new Color(1f, 0.85f, 0.2f); // Dorado
                case 4: return new Color(1f, 0.3f, 0.3f); // Rojo Fuego
                default: return Color.white;
            }
        }
    }
}