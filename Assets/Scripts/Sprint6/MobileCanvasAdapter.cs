using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint6
{
    /// <summary>
    /// Adaptador automático de resolución y escala para móviles (Sprint 7).
    /// Garantiza que en cualquier relación de aspecto (9:16, 9:19.5, 9:20, tablets o Editor Free Aspect)
    /// todos los elementos del HUD, carriles, jefe y contenedores sean 100% visibles sin solapamiento ni recorte.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasScaler))]
    public class MobileCanvasAdapter : MonoBehaviour
    {
        private CanvasScaler scaler;
        private float lastAspectRatio = -1f;

        private void Awake()
        {
            ApplyAdaptation();
        }

        private void Update()
        {
            float currentAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
            if (Mathf.Abs(currentAspect - lastAspectRatio) > 0.002f)
            {
                ApplyAdaptation();
            }
        }

        public void ApplyAdaptation()
        {
            if (scaler == null) scaler = GetComponent<CanvasScaler>();
            if (scaler == null) return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            float targetAspect = 1080f / 1920f; // 0.5625f (9:16 Portrait)
            float currentAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
            lastAspectRatio = currentAspect;

            if (currentAspect <= targetAspect)
            {
                // Móvil vertical estándar o alto (9:16, 9:19, 9:20)
                // Igualar ancho (0) para asegurar los 1080px horizontales
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;
            }
            else
            {
                // Pantalla más ancha (Editor en Free Aspect, tablet 4:3, laptop)
                // Igualar alto (1) para asegurar que los 1920px verticales no se corten
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;
            }
        }
    }
}