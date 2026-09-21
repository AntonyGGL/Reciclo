using System.Collections;
using UnityEngine;
using ReCiclo.Sprint2;
using ReCiclo.Sprint1;

namespace ReCiclo.Sprint3
{
    public enum PowerUpType
    {
        GoldenClock,       // +15 Segundos
        DoubleScore,       // 2X Puntos por 10s
        RainbowBin         // Contenedor Comodín por 10s
    }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Efectos Activos")]
        [SerializeField] private bool isDoubleScoreActive = false;
        [SerializeField] private bool isRainbowBinActive = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ActivatePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.GoldenClock:
                    if (LevelTimer.Instance != null)
                    {
                        LevelTimer.Instance.AddTime(15f);
                        Debug.Log("[PowerUpManager] ¡Reloj Dorado activado! +15 Segundos.");
                    }
                    break;
                case PowerUpType.DoubleScore:
                    StartCoroutine(DoubleScoreRoutine(10f));
                    break;
                case PowerUpType.RainbowBin:
                    StartCoroutine(RainbowBinRoutine(10f));
                    break;
            }
        }

        private IEnumerator DoubleScoreRoutine(float duration)
        {
            isDoubleScoreActive = true;
            Debug.Log("[PowerUpManager] ¡Estrella 2X activada!");
            yield return new WaitForSeconds(duration);
            isDoubleScoreActive = false;
            Debug.Log("[PowerUpManager] Estrella 2X ha finalizado.");
        }

        private IEnumerator RainbowBinRoutine(float duration)
        {
            isRainbowBinActive = true;
            RecycleBin[] bins = Object.FindObjectsByType<RecycleBin>(FindObjectsSortMode.None);
            foreach (var bin in bins)
            {
                bin.SetWildcardMode(true);
            }

            Debug.Log("[PowerUpManager] ¡Contenedor Arcoíris Comodín activado!");
            yield return new WaitForSeconds(duration);

            foreach (var bin in bins)
            {
                bin.SetWildcardMode(false);
            }
            isRainbowBinActive = false;
            Debug.Log("[PowerUpManager] Contenedor Arcoíris ha finalizado.");
        }

        public bool IsDoubleScoreActive => isDoubleScoreActive;
        public bool IsRainbowBinActive => isRainbowBinActive;
    }
}
