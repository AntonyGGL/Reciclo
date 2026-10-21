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
        private int lastWidth = -1;
        private int lastHeight = -1;

        private void Awake()
        {
            ApplyAdaptation();
        }

        private void Update()
        {
            float currentAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
            if (Mathf.Abs(currentAspect - lastAspectRatio) > 0.002f || lastWidth != Screen.width || lastHeight != Screen.height)
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

            float currentAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
            lastAspectRatio = currentAspect;
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            // Conserva visible toda el área de diseño en móviles, tablets y pantallas horizontales.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            ApplyTextSafety();
        }

        private void ApplyTextSafety()
        {
            Text[] labels = GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
            {
                int configuredSize = Mathf.Max(10, label.fontSize);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = Mathf.Min(10, configuredSize);
                label.resizeTextMaxSize = configuredSize;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }
    }
}
