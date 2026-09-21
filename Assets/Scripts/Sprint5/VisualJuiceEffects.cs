using UnityEngine;

namespace ReCiclo.Sprint5
{
    public class VisualJuiceEffects : MonoBehaviour
    {
        public static VisualJuiceEffects Instance { get; private set; }

        [Header("Sistemas de Partículas")]
        [SerializeField] private ParticleSystem successSparklesPrefab;
        [SerializeField] private ParticleSystem confettiBurstPrefab;
        [SerializeField] private TrailRenderer touchTrailPrefab;

        private TrailRenderer currentTrail;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        public void PlaySuccessSparkles(Vector3 worldPosition)
        {
            if (successSparklesPrefab != null)
            {
                ParticleSystem sparkles = Instantiate(successSparklesPrefab, worldPosition, Quaternion.identity);
                sparkles.Play();
                Destroy(sparkles.gameObject, 2.0f);
            }
        }

        public void PlayVictoryConfetti()
        {
            if (confettiBurstPrefab != null)
            {
                Vector3 centerPosition = mainCamera != null ? mainCamera.transform.position + Vector3.forward * 5f : Vector3.zero;
                ParticleSystem confetti = Instantiate(confettiBurstPrefab, centerPosition, Quaternion.identity);
                confetti.Play();
                Destroy(confetti.gameObject, 4.0f);
            }
        }

        public void StartTouchTrail(Vector3 startPosition)
        {
            if (touchTrailPrefab != null)
            {
                if (currentTrail != null) Destroy(currentTrail.gameObject);
                currentTrail = Instantiate(touchTrailPrefab, startPosition, Quaternion.identity);
            }
        }

        public void UpdateTouchTrail(Vector3 currentPosition)
        {
            if (currentTrail != null)
            {
                currentTrail.transform.position = currentPosition;
            }
        }

        public void StopTouchTrail()
        {
            if (currentTrail != null)
            {
                Destroy(currentTrail.gameObject, currentTrail.time);
                currentTrail = null;
            }
        }
    }
}
