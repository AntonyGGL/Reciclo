using System;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint2
{
    public class ComboSystem : MonoBehaviour
    {
        public static ComboSystem Instance { get; private set; }

        [Header("Configuración de Combo")]
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private int currentMultiplier = 1;
        [SerializeField] private Text comboText;

        public event Action<int, int> OnComboChanged; // combo, multiplier

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
            currentCombo = 0;
            currentMultiplier = 1;
            UpdateUI();
            OnComboChanged?.Invoke(currentCombo, currentMultiplier);
        }

        private void CalculateMultiplier()
        {
            if (currentCombo >= 15) currentMultiplier = 4;
            else if (currentCombo >= 10) currentMultiplier = 3;
            else if (currentCombo >= 5) currentMultiplier = 2;
            else currentMultiplier = 1;
        }

        private void UpdateUI()
        {
            if (comboText == null) return;

            if (currentCombo > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"COMBO x{currentCombo} (x{currentMultiplier})";
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }

        public int CurrentMultiplier => currentMultiplier;
        public int CurrentCombo => currentCombo;
    }
}
