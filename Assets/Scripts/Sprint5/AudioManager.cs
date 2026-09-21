using UnityEngine;

namespace ReCiclo.Sprint5
{
    public enum SoundType
    {
        GrabItem,
        DropSuccess,
        DropError,
        ComboUp,
        PowerUp,
        Victory,
        GameOver,
        BossRoar
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        [Header("Clips de Audio - BGM")]
        [SerializeField] private AudioClip mainThemeBGM;
        [SerializeField] private AudioClip bossThemeBGM;

        [Header("Clips de Audio - SFX")]
        [SerializeField] private AudioClip grabItemSFX;
        [SerializeField] private AudioClip dropSuccessSFX;
        [SerializeField] private AudioClip dropErrorSFX;
        [SerializeField] private AudioClip comboUpSFX;
        [SerializeField] private AudioClip powerUpSFX;
        [SerializeField] private AudioClip victorySFX;
        [SerializeField] private AudioClip gameOverSFX;
        [SerializeField] private AudioClip bossRoarSFX;

        [Header("Configuración de Volumen")]
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmAudioSource == null) bgmAudioSource = gameObject.AddComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();

            bgmAudioSource.loop = true;
        }

        private void Start()
        {
            PlayBGM(false);
        }

        public void PlayBGM(bool isBossBattle)
        {
            AudioClip targetClip = isBossBattle ? bossThemeBGM : mainThemeBGM;
            if (targetClip != null && bgmAudioSource.clip != targetClip)
            {
                bgmAudioSource.clip = targetClip;
                bgmAudioSource.volume = bgmVolume;
                bgmAudioSource.Play();
            }
        }

        public void PlaySFX(SoundType sound)
        {
            AudioClip clipToPlay = null;

            switch (sound)
            {
                case SoundType.GrabItem: clipToPlay = grabItemSFX; break;
                case SoundType.DropSuccess: clipToPlay = dropSuccessSFX; break;
                case SoundType.DropError: clipToPlay = dropErrorSFX; break;
                case SoundType.ComboUp: clipToPlay = comboUpSFX; break;
                case SoundType.PowerUp: clipToPlay = powerUpSFX; break;
                case SoundType.Victory: clipToPlay = victorySFX; break;
                case SoundType.GameOver: clipToPlay = gameOverSFX; break;
                case SoundType.BossRoar: clipToPlay = bossRoarSFX; break;
            }

            if (clipToPlay != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(clipToPlay, sfxVolume);
            }
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmAudioSource != null) bgmAudioSource.volume = bgmVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
    }
}
