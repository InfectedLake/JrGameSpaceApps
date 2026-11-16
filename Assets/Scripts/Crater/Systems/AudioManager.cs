using UnityEngine;
using UnityEngine.Audio;

namespace Crater.Systems
{
    /// <summary>Pista musical activa. Solo suena una a la vez (con fundido cruzado entre ellas).</summary>
    public enum MusicTrack { None = 0, Menu = 1, Game = 2, Event = 3 }

    /// <summary>
    /// Audio del juego (único sistema de audio). Tres grupos en el AudioMixer Assets/Data/Crater/CraterMixer.mixer:
    ///  - Master (parámetro expuesto "MasterVolume") → Music ("MusicVolume") y SFX ("SFXVolume").
    ///  - MUSIC: una AudioSource por pista (menú, gameplay, evento grande), salida al grupo Music. Solo una está
    ///           activa; los cambios se hacen con fundido sobre AudioSource.volume (0..1), así el fundido nunca
    ///           toca el volumen del jugador, que vive solo en el mixer. La pista de gameplay se PAUSA durante
    ///           un evento y continúa donde iba al terminarlo.
    ///  - SFX: una AudioSource para la interfaz y otra para los acontecimientos, salida al grupo SFX.
    /// Cada categoría tiene UNA sola fuente de control: las propiedades Master/Music/Effects, que convierten el
    /// valor normalizado (0..1) a decibelios (20·log10, mínimo −80 dB) y lo aplican con AudioMixer.SetFloat.
    /// Los volúmenes y la pantalla completa se guardan en PlayerPrefs (por defecto 100 / 60 / 80 %).
    /// Garantiza exactamente un AudioListener activo (sin él Unity no saca audio aunque las fuentes suenen).
    /// El audio es 2D y no depende de Time.timeScale: la música sigue sonando en pausa.
    /// GameObject: "AudioManager" (lo crea GameManager si no está en la escena).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        const string KeyMaster = "crater.vol.master", KeyMusic = "crater.vol.music", KeySfx = "crater.vol.sfx", KeyFull = "crater.fullscreen";
        public const string MasterParam = "MasterVolume", MusicParam = "MusicVolume", SfxParam = "SFXVolume";
        public const float DefaultMaster = 1f, DefaultMusic = 0.6f, DefaultSfx = 0.8f;
        public const float SilenceDb = -80f;
        const float FadeSeconds = 1.0f;          // cambios normales (menú ↔ partida)
        const float EventFadeIn = 1.5f, EventFadeOut = 3.0f;   // entrada y salida de la música de un acontecimiento
        float fadeSeconds = FadeSeconds;
        string eventMusicId;                     // acontecimiento dueño de la música de evento (null = ninguno)

        /// <summary>Activa los registros de diagnóstico (sliders, dB aplicados). Apagado en la versión final.</summary>
        public static bool LogDiagnostics = false;

        class Channel
        {
            public AudioSource src;
            public float fade;      // 0..1: solo el fundido. El volumen del jugador está en el mixer.
            public bool held;       // pausada (no detenida): retoma desde el mismo punto
        }

        readonly Channel[] channels = new Channel[4];
        MusicTrack current = MusicTrack.None;
        AudioSource uiSfx, eventSfx;
        AudioMixer mixer;
        float master = DefaultMaster, musicVol = DefaultMusic, sfxVol = DefaultSfx;
        AudioClip lastUiClip;
        float lastUiTime = -1f;

        public MusicTrack CurrentTrack => current;
        public AudioMixer Mixer => mixer;

        void Awake()
        {
            for (int i = 1; i < channels.Length; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.loop = true;
                s.playOnAwake = false;   // la pista la decide SetMusic según la pantalla
                s.spatialBlend = 0f;
                s.volume = 0f;           // empieza en silencio y entra con fundido (nunca un golpe a volumen máximo)
                channels[i] = new Channel { src = s };
            }
            uiSfx = NewSfxSource();
            eventSfx = NewSfxSource();
            master = LoadVolume(KeyMaster, DefaultMaster);
            musicVol = LoadVolume(KeyMusic, DefaultMusic);
            sfxVol = LoadVolume(KeySfx, DefaultSfx);
            if (PlayerPrefs.HasKey(KeyFull) && FullscreenSupported) Screen.fullScreen = PlayerPrefs.GetInt(KeyFull) == 1;
        }

        // AudioMixer.SetFloat no tiene efecto fiable durante Awake: los volúmenes se aplican en Start y en Configure.
        void Start()
        {
            EnsureSingleListener();
            Apply();
        }

        /// <summary>Lee un volumen guardado; si no existe o no es válido usa el predeterminado (nunca 0 por accidente).</summary>
        static float LoadVolume(string key, float fallback)
        {
            if (!PlayerPrefs.HasKey(key)) return fallback;
            float v = PlayerPrefs.GetFloat(key, fallback);
            return float.IsNaN(v) || float.IsInfinity(v) ? fallback : Mathf.Clamp01(v);
        }

        AudioSource NewSfxSource()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.spatialBlend = 0f;
            return s;
        }

        /// <summary>
        /// Conecta el mixer y los clips de cada pista (vienen de CraterArtSet). Un clip nulo deja esa pista en silencio.
        /// Sin mixer el juego sigue sonando: el volumen se aplica directamente en las AudioSource.
        /// </summary>
        public void Configure(AudioMixer audioMixer, AudioClip menu, AudioClip game, AudioClip eventMusic)
        {
            mixer = audioMixer;
            if (mixer == null) Debug.LogWarning("[CRATER] AudioManager sin AudioMixer: el volumen se aplica en las AudioSource.");
            var musicGroup = Group("Music");
            var sfxGroup = Group("SFX");
            for (int i = 1; i < channels.Length; i++) channels[i].src.outputAudioMixerGroup = musicGroup;
            uiSfx.outputAudioMixerGroup = sfxGroup;
            eventSfx.outputAudioMixerGroup = sfxGroup;
            channels[(int)MusicTrack.Menu].src.clip = menu;
            channels[(int)MusicTrack.Game].src.clip = game;
            channels[(int)MusicTrack.Event].src.clip = eventMusic;
            EnsureSingleListener();
            Apply();
        }

        AudioMixerGroup Group(string name)
        {
            if (mixer == null) return null;
            var g = System.Array.Find(mixer.FindMatchingGroups(name), x => x.name == name);
            if (g == null) Debug.LogError("[CRATER] El AudioMixer no tiene el grupo \"" + name + "\".");
            return g;
        }

        /// <summary>Sin AudioListener Unity no reproduce nada; con varios avisa y mezcla mal. Deja exactamente uno activo.</summary>
        void EnsureSingleListener()
        {
            var enabled = System.Array.FindAll(FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None), l => l.enabled);
            if (enabled.Length == 0)
            {
                var host = Camera.main != null ? Camera.main.gameObject : gameObject;
                host.AddComponent<AudioListener>();
                Debug.LogWarning("[CRATER] La escena no tenía AudioListener: se agregó uno a \"" + host.name + "\".");
            }
            for (int i = 1; i < enabled.Length; i++)
            {
                enabled[i].enabled = false;
                Debug.LogWarning("[CRATER] AudioListener duplicado desactivado en \"" + enabled[i].gameObject.name + "\".");
            }
        }

        /// <summary>Valor normalizado (0..1) → decibelios. 1 = 0 dB, 0.5 ≈ −6 dB, 0.25 ≈ −12 dB, 0 = −80 dB (silencio).</summary>
        public static float ToDecibels(float v) => v <= 0.0001f ? SilenceDb : Mathf.Max(SilenceDb, 20f * Mathf.Log10(v));

        void SetParam(string param, float normalized)
        {
            float db = ToDecibels(normalized);
            if (!mixer.SetFloat(param, db)) Debug.LogError("[CRATER] AudioMixer.SetFloat(\"" + param + "\") falló: el parámetro no está expuesto en " + mixer.name + ".");
            else if (LogDiagnostics) Debug.Log($"[AUDIO] {param}: {normalized:0.00} → {db:0.0} dB");
        }

        void Apply()
        {
            if (mixer != null)
            {
                SetParam(MasterParam, master);
                SetParam(MusicParam, musicVol);
                SetParam(SfxParam, sfxVol);
            }
            for (int i = 1; i < channels.Length; i++)
                if (channels[i] != null) channels[i].src.volume = SourceMusicVolume(channels[i]);
            uiSfx.volume = SourceSfxVolume;
            eventSfx.volume = SourceSfxVolume;
        }

        // Con mixer, la AudioSource lleva solo el fundido (o 1 para efectos) y el mixer lleva el volumen del jugador.
        // Sin mixer (respaldo) se multiplica aquí.
        float SourceMusicVolume(Channel c) => mixer != null ? c.fade : master * musicVol * c.fade;
        float SourceSfxVolume => mixer != null ? 1f : master * sfxVol;

        public float Master { get => master; set => SetVolume(ref master, value, KeyMaster); }
        public float Music { get => musicVol; set => SetVolume(ref musicVol, value, KeyMusic); }
        public float Effects { get => sfxVol; set => SetVolume(ref sfxVol, value, KeySfx); }

        /// <summary>Única vía para cambiar un volumen: guarda el valor normalizado y lo aplica. No toca la reproducción.</summary>
        void SetVolume(ref float field, float value, string key)
        {
            field = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(key, field);
            if (LogDiagnostics) Debug.Log($"[AUDIO] slider {key} = {field:0.00} (guardado)");
            Apply();
        }

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

        // ------------------------------------------------------------------ Música

        /// <summary>
        /// Cambia la pista activa. Pedir la pista que ya suena no hace nada (nunca reinicia la canción).
        /// La anterior se desvanece; la de gameplay queda en pausa si se pasa a un evento.
        /// </summary>
        public void SetMusic(MusicTrack track) => SetMusic(track, FadeSeconds);

        void SetMusic(MusicTrack track, float fade)
        {
            if (track == current) return;
            current = track;
            fadeSeconds = fade;
            if (track != MusicTrack.Event) eventMusicId = null;   // menú o partida sin evento: no queda ningún dueño
            if (track == MusicTrack.None) return;
            var c = channels[(int)track];
            if (c.src.clip == null) return;
            if (c.held) { c.src.UnPause(); c.held = false; }
            else if (!c.src.isPlaying) { c.fade = 0f; c.src.Play(); }
        }

        // ------------------------------------------------------------------ Música de acontecimientos

        /// <summary>Acontecimiento cuya música suena ahora (null si no hay ninguno).</summary>
        public string ActiveEventMusic => eventMusicId;

        /// <summary>
        /// Solicitud explícita: el acontecimiento "id" entró en alerta (o llegó sin aviso). Si su música ya suena no hace
        /// nada; si sonaba la de otro acontecimiento, la pista sigue sin reiniciarse y cambia de dueño.
        /// </summary>
        public void BeginEventMusic(string id)
        {
            if (string.IsNullOrEmpty(id) || (eventMusicId == id && current == MusicTrack.Event)) return;
            if (LogDiagnostics) Debug.Log("[AUDIO] música de evento: inicio (" + id + ")");
            SetMusic(MusicTrack.Event, EventFadeIn);
            eventMusicId = id;
        }

        /// <summary>
        /// Partida sin acontecimiento en curso: música de gameplay. Si sonaba la de un acontecimiento (que terminó),
        /// el cambio usa un fundido largo; desde el menú usa el fundido normal. Pedirla de nuevo no hace nada.
        /// </summary>
        public void EndEventMusic()
        {
            if (current == MusicTrack.Game) return;
            bool fromEvent = current == MusicTrack.Event;
            if (fromEvent && LogDiagnostics) Debug.Log("[AUDIO] música de evento: fin (" + eventMusicId + ")");
            eventMusicId = null;
            SetMusic(MusicTrack.Game, fromEvent ? EventFadeOut : FadeSeconds);
        }

        void Update()
        {
            float step = Time.unscaledDeltaTime / fadeSeconds;
            for (int i = 1; i < channels.Length; i++)
            {
                var c = channels[i];
                if (c.src.clip == null) continue;
                if (i == (int)current)
                {
                    c.fade = Mathf.MoveTowards(c.fade, 1f, step);
                }
                else
                {
                    c.fade = Mathf.MoveTowards(c.fade, 0f, step);
                    // Al terminar de desvanecerse ninguna pista inactiva queda sonando (ni siquiera a volumen 0).
                    if (c.fade <= 0f)
                    {
                        if (KeepHeld(i)) { if (c.src.isPlaying) { c.src.Pause(); c.held = true; } }
                        else if (c.src.isPlaying || c.held) { c.src.Stop(); c.held = false; }
                    }
                }
                c.src.volume = SourceMusicVolume(c);
            }
        }

        /// <summary>La música de gameplay se conserva en pausa mientras dura un evento; las demás se detienen.</summary>
        bool KeepHeld(int i) => i == (int)MusicTrack.Game && current == MusicTrack.Event;

        // ------------------------------------------------------------------ Efectos

        /// <summary>Efecto de interfaz (clics). Ignora el mismo clip repetido en menos de 60 ms (un solo sonido por clic).</summary>
        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            float now = Time.unscaledTime;
            if (clip == lastUiClip && now - lastUiTime < 0.06f) return;
            lastUiClip = clip;
            lastUiTime = now;
            uiSfx.PlayOneShot(clip, volume);
        }

        /// <summary>Efecto de un acontecimiento (tormenta, impactos, oscurecimiento). Uno a la vez.</summary>
        public void PlayEventSfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            eventSfx.Stop();
            eventSfx.clip = clip;
            eventSfx.volume = SourceSfxVolume * volume;
            eventSfx.Play();
        }

        public void StopEventSfx() => eventSfx.Stop();

        /// <summary>Con el menú de pausa abierto el efecto del acontecimiento se detiene y luego continúa donde iba.</summary>
        public void SetEventSfxPaused(bool paused)
        {
            if (paused) eventSfx.Pause();
            else eventSfx.UnPause();
        }

        public void Save() => PlayerPrefs.Save();
    }
}
