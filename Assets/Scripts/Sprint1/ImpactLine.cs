using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint2;

namespace ReCiclo.Sprint1
{
    /// <summary>
    /// Zona/Línea de impacto estilo Guitar Hero ubicada en la parte inferior de los carriles.
    /// Detecta y evalúa la precisión con la que el jugador toca o clasifica cada residuo.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class ImpactLine : MonoBehaviour
    {
        public static ImpactLine Instance { get; private set; }

        [Header("Configuración de Impacto")]
        [SerializeField] private float perfectThreshold = 45f;
        [SerializeField] private float goodThreshold = 100f;
        [SerializeField] private float missYPosition = -520f;
        [SerializeField] private RectTransform[] laneHitReceptors;

        [Header("Feedback Visual")]
        [SerializeField] private UnityEngine.UI.Image lineGlowImage;
        [SerializeField] private Color normalGlowColor = new Color(0.3f, 0.7f, 1f, 0.4f);
        [SerializeField] private Color hitGlowColor = new Color(0.3f, 1f, 0.5f, 0.9f);

        public event Action<FallingWaste, string> OnImpactHit; // waste, quality (PERFECT/GOOD)

        private RectTransform rectTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Intenta capturar/clasificar el residuo más cercano en el carril indicado.
        /// </summary>
        public bool TryHitLane(int laneIndex, WasteCategory expectedCategory)
        {
            FallingWaste targetWaste = FindClosestWasteInLane(laneIndex);
            if (targetWaste == null) return false;

            float distance = Mathf.Abs(targetWaste.Rect.anchoredPosition.y - rectTransform.anchoredPosition.y);

            if (distance <= goodThreshold)
            {
                bool isCorrectType = (targetWaste.Category == expectedCategory);
                if (isCorrectType)
                {
                    string quality = (distance <= perfectThreshold) ? "¡PERFECTO!" : "¡BIEN!";
                    float multiplier = (distance <= perfectThreshold) ? 1.5f : 1.0f;

                    Debug.Log($"[ImpactLine] {quality} en Carril {laneIndex} ({expectedCategory}) - Distancia: {distance:F1}px");
                    targetWaste.TriggerSuccess(multiplier);
                    OnImpactHit?.Invoke(targetWaste, quality);
                    TriggerGlow();
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[ImpactLine] Categoría incorrecta: esperaba {expectedCategory}, era {targetWaste.Category}");
                    targetWaste.TriggerMiss();
                    return false;
                }
            }
            return false;
        }

        private FallingWaste FindClosestWasteInLane(int laneIndex)
        {
            FallingWaste[] allActive = UnityEngine.Object.FindObjectsByType<FallingWaste>(FindObjectsSortMode.None);
            FallingWaste closest = null;
            float minDistance = float.MaxValue;
            float lineY = rectTransform.anchoredPosition.y;

            foreach (var waste in allActive)
            {
                if (waste.LaneIndex == laneIndex && !waste.IsClassified && !waste.IsMissed)
                {
                    float dist = Mathf.Abs(waste.Rect.anchoredPosition.y - lineY);
                    if (dist < minDistance && waste.Rect.anchoredPosition.y >= missYPosition)
                    {
                        minDistance = dist;
                        closest = waste;
                    }
                }
            }
            return closest;
        }

        private void TriggerGlow()
        {
            if (lineGlowImage != null)
            {
                StartCoroutine(GlowRoutine());
            }
        }

        private System.Collections.IEnumerator GlowRoutine()
        {
            if (lineGlowImage == null) yield break;
            lineGlowImage.color = hitGlowColor;
            yield return new WaitForSeconds(0.12f);
            lineGlowImage.color = normalGlowColor;
        }

        public float ImpactY => rectTransform != null ? rectTransform.anchoredPosition.y : 0f;
    }
}