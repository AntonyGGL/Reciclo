using UnityEngine;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint2
{
    /// <summary>
    /// Envoltura de compatibilidad hacia atrás para PollutionBar que delega en VeroHealthController.
    /// </summary>
    public class PollutionBar : MonoBehaviour
    {
        public static PollutionBar Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ResetBar()
        {
            if (VeroHealthController.Instance != null) VeroHealthController.Instance.ResetHealth();
        }

        public void AddPollution(float amount)
        {
            if (VeroHealthController.Instance != null) VeroHealthController.Instance.TakeDamage(amount);
        }

        public void ReducePollution(float amount)
        {
            if (VeroHealthController.Instance != null) VeroHealthController.Instance.Heal(amount);
        }
    }
}