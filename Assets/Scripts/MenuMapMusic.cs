using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReCiclo.UI
{
    /// <summary>
    /// Keeps the menu theme playing seamlessly between Inicio and MapaMundial.
    /// The music is removed when the player enters any gameplay scene.
    /// </summary>
    public sealed class MenuMapMusic : MonoBehaviour
    {
        private const string MusicResource = "Audio/Morning_in_the_Meadow";
        private static MenuMapMusic instance;

        public static void EnsurePlaying()
        {
            if (instance != null) return;

            var musicObject = new GameObject("MenuMapMusic");
            instance = musicObject.AddComponent<MenuMapMusic>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            AudioClip clip = Resources.Load<AudioClip>(MusicResource);
            if (clip == null)
            {
                Debug.LogError($"[ReCiclo] No se encontro la musica en Resources/{MusicResource}.");
                Destroy(gameObject);
                return;
            }

            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0.45f;
            source.spatialBlend = 0f;
            source.Play();

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            if (instance == this) instance = null;
        }

        private void OnActiveSceneChanged(Scene previous, Scene current)
        {
            if (current.name == "Inicio" || current.name == "MapaMundial") return;
            Destroy(gameObject);
        }
    }
}
