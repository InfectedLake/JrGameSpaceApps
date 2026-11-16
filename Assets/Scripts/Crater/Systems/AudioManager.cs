using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Audio del juego (único sistema de audio; el proyecto no tenía AudioMixer).
    /// Volumen general → AudioListener.volume; música y efectos → una AudioSource cada uno.
    /// Los volúmenes y la pantalla completa se guardan en PlayerPrefs (se conservan entre sesiones).
    /// Los clips vienen de Assets/Audios a través de CraterArtSet; si falta alguno no se produce error.
    /// GameObject: "AudioManager" (lo crea GameManager si no está en la escena).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        const string KeyMaster = "crater.vol.master", KeyMusic = "crater.vol.music", KeySfx = "crater.vol.sfx", KeyFull = "crater.fullscreen";

        AudioSource music, sfx;
        float master = 1f, musicVol = 0.6f, sfxVol = 0.8f;

        void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            master = PlayerPrefs.GetFloat(KeyMaster, 1f);
            musicVol = PlayerPrefs.GetFloat(KeyMusic, 0.6f);
            sfxVol = PlayerPrefs.GetFloat(KeySfx, 0.8f);
            if (PlayerPrefs.HasKey(KeyFull) && FullscreenSupported) Screen.fullScreen = PlayerPrefs.GetInt(KeyFull) == 1;
            Apply();
        }

        void Apply()
        {
            AudioListener.volume = master;
            music.volume = musicVol;
            sfx.volume = sfxVol;
        }

        public float Master { get => master; set { master = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KeyMaster, master); Apply(); } }
        public float Music { get => musicVol; set { musicVol = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KeyMusic, musicVol); Apply(); } }
        public float Effects { get => sfxVol; set { sfxVol = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KeySfx, sfxVol); Apply(); } }

        /// <summary>En WebGL y móviles la pantalla completa depende del navegador o del sistema.</summary>
        public static bool FullscreenSupported => Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer;

        public bool Fullscreen
        {
            get => Screen.fullScreen;
            set
            {
                if (!FullscreenSupported) return;
                Screen.fullScreen = value;
                PlayerPrefs.SetInt(KeyFull, value ? 1 : 0);
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) { music.Stop(); music.clip = null; return; }
            if (music.clip == clip && music.isPlaying) return;
            music.clip = clip;
            music.Play();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip != null) sfx.PlayOneShot(clip, volume);
        }

        public void Save() => PlayerPrefs.Save();
    }
}
