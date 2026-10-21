using System;
using System.Collections;
using UnityEngine;
using ReCiclo.Sprint4;

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
        BossRoar,
        BossHurt,
        SmogActivate,
        UIClick,
        LevelStart,
        TimerWarning
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
        [SerializeField] private AudioClip firstBossThemeBGM;
        [SerializeField] private AudioClip secondLevelThemeBGM;
        [SerializeField] private AudioClip finalLevelThemeBGM;
        [SerializeField] private AudioClip menuThemeBGM;
        [SerializeField] private AudioClip victoryThemeBGM;

        [Header("Clips de Audio - SFX")]
        [SerializeField] private AudioClip grabItemSFX;
        [SerializeField] private AudioClip dropSuccessSFX;
        [SerializeField] private AudioClip dropErrorSFX;
        [SerializeField] private AudioClip comboUpSFX;
        [SerializeField] private AudioClip powerUpSFX;
        [SerializeField] private AudioClip victorySFX;
        [SerializeField] private AudioClip gameOverSFX;
        [SerializeField] private AudioClip bossRoarSFX;
        [SerializeField] private AudioClip bossHurtSFX;
        [SerializeField] private AudioClip smogActivateSFX;
        [SerializeField] private AudioClip uiClickSFX;
        [SerializeField] private AudioClip levelStartSFX;
        [SerializeField] private AudioClip timerWarningSFX;

        [Header("Configuracion de Volumen")]
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float fadeDuration = 1.0f;

        private Coroutine fadeRoutine;
        private bool isMuted = false;

        public event Action<float> OnBGMVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;

        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;
        public bool IsMuted => isMuted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // No convertir en persistente el contenedor completo del nivel cuando
            // AudioManager comparte GameObject con GameManager y otros sistemas.
            // Un AudioManager independiente sí puede sobrevivir entre escenas.
            if (GetComponents<Component>().Length <= 2)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (bgmAudioSource == null) bgmAudioSource = gameObject.AddComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();

            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.spatialBlend = 0f;
            sfxAudioSource.playOnAwake = false;

            if (firstBossThemeBGM == null)
            {
                firstBossThemeBGM = Resources.Load<AudioClip>("Audio/Sorting_the_Chaos");
            }
            if (secondLevelThemeBGM == null)
            {
                secondLevelThemeBGM = Resources.Load<AudioClip>("Audio/Plaza_Morning_Sort");
            }
            if (finalLevelThemeBGM == null)
            {
                finalLevelThemeBGM = Resources.Load<AudioClip>("Audio/Final_Reclamation");
            }

            LoadVolumeSettings();
        }

        private void Start()
        {
            PlayBGM(false);
        }

        public void PlayBGM(bool isBossBattle)
        {
            AudioClip targetClip = ResolveBGMClip(isBossBattle);
            if (targetClip != null && bgmAudioSource.clip != targetClip)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(CrossfadeBGM(targetClip));
            }
            else if (targetClip != null && !bgmAudioSource.isPlaying)
            {
                bgmAudioSource.clip = targetClip;
                bgmAudioSource.volume = bgmVolume;
                bgmAudioSource.Play();
            }
        }

        private AudioClip ResolveBGMClip(bool isBossBattle)
        {
            LevelData currentLevel = WorldMapManager.Instance != null
                ? WorldMapManager.Instance.GetCurrentLevel()
                : null;

            // Plaza Morning Sort pertenece exclusivamente al nivel 2: Mercado.
            if (currentLevel != null && currentLevel.levelIndex == 2 && secondLevelThemeBGM != null)
            {
                return secondLevelThemeBGM;
            }

            if (!isBossBattle) return mainThemeBGM;

            // Final Reclamation pertenece exclusivamente al nivel 3: Basurero Final.
            if (currentLevel != null && currentLevel.levelIndex == 3 && finalLevelThemeBGM != null)
            {
                return finalLevelThemeBGM;
            }

            // Sorting the Chaos pertenece exclusivamente a Can Basura, jefe del nivel 1.
            if (currentLevel != null && currentLevel.levelIndex == 1 && firstBossThemeBGM != null)
            {
                return firstBossThemeBGM;
            }

            return bossThemeBGM;
        }

        public void PlayMenuBGM()
        {
            if (menuThemeBGM != null)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(CrossfadeBGM(menuThemeBGM));
            }
        }

        public void PlayVictoryBGM()
        {
            if (victoryThemeBGM != null)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(CrossfadeBGM(victoryThemeBGM));
            }
        }

        public void StopBGM()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutBGM());
        }

        private IEnumerator CrossfadeBGM(AudioClip newClip)
        {
            float startVol = bgmAudioSource.volume;
            float elapsed = 0f;
            float halfDuration = fadeDuration * 0.5f;

            // Fade out
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
                yield return null;
            }

            bgmAudioSource.Stop();
            bgmAudioSource.clip = newClip;
            bgmAudioSource.Play();

            // Fade in
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmAudioSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / halfDuration);
                yield return null;
            }
            bgmAudioSource.volume = bgmVolume;
        }

        private IEnumerator FadeOutBGM()
        {
            float startVol = bgmAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
                yield return null;
            }
            bgmAudioSource.Stop();
        }

        public void PlaySFX(SoundType sound)
        {
            if (isMuted) return;
            AudioClip clipToPlay = GetSFXClip(sound);
            if (clipToPlay != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(clipToPlay, sfxVolume);
            }
        }

        public void PlaySFXWithPitch(SoundType sound, float pitchVariation = 0.1f)
        {
            if (isMuted) return;
            AudioClip clipToPlay = GetSFXClip(sound);
            if (clipToPlay != null && sfxAudioSource != null)
            {
                float originalPitch = sfxAudioSource.pitch;
                sfxAudioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
                sfxAudioSource.PlayOneShot(clipToPlay, sfxVolume);
                sfxAudioSource.pitch = originalPitch;
            }
        }

        private AudioClip GetSFXClip(SoundType sound)
        {
            switch (sound)
            {
                case SoundType.GrabItem: return grabItemSFX;
                case SoundType.DropSuccess: return dropSuccessSFX;
                case SoundType.DropError: return dropErrorSFX;
                case SoundType.ComboUp: return comboUpSFX;
                case SoundType.PowerUp: return powerUpSFX;
                case SoundType.Victory: return victorySFX;
                case SoundType.GameOver: return gameOverSFX;
                case SoundType.BossRoar: return bossRoarSFX;
                case SoundType.BossHurt: return bossHurtSFX;
                case SoundType.SmogActivate: return smogActivateSFX;
                case SoundType.UIClick: return uiClickSFX;
                case SoundType.LevelStart: return levelStartSFX;
                case SoundType.TimerWarning: return timerWarningSFX;
                default: return null;
            }
        }

        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmAudioSource != null) bgmAudioSource.volume = bgmVolume;
            SaveVolumeSettings();
            OnBGMVolumeChanged?.Invoke(bgmVolume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveVolumeSettings();
            OnSFXVolumeChanged?.Invoke(sfxVolume);
        }

        public void ToggleMute()
        {
            isMuted = !isMuted;
            bgmAudioSource.mute = isMuted;
        }

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("ReCiclo_BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("ReCiclo_SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            if (PlayerPrefs.HasKey("ReCiclo_BGMVolume"))
                bgmVolume = PlayerPrefs.GetFloat("ReCiclo_BGMVolume");
            if (PlayerPrefs.HasKey("ReCiclo_SFXVolume"))
                sfxVolume = PlayerPrefs.GetFloat("ReCiclo_SFXVolume");

            if (bgmAudioSource != null) bgmAudioSource.volume = bgmVolume;
        }
    }
}
