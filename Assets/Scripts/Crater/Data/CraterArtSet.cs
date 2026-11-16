using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Crater.Data
{
    /// <summary>
    /// Referencias al arte original del equipo (carpeta Assets/Sprites). Los archivos fuente no se modifican:
    /// solo se configuran en el importador (Point, sin compresión, sin mipmaps).
    /// Asset: Assets/Data/Crater/ArtSet.asset (se reasigna con CRATER &gt; Asignar arte original).
    /// </summary>
    [CreateAssetMenu(fileName = "ArtSet", menuName = "CRATER/Arte original")]
    public class CraterArtSet : ScriptableObject
    {
        [Header("Gameplay principal")]
        public Sprite background;
        public Sprite energy, food, central, oxygen, habitat;

        [Header("Variantes de edificios")]
        public Sprite energyBroken;                                   // micrometeoritos
        public Sprite energyLit, foodLit, oxygenLit, habitatLit;      // eclipse: sección con baterías

        [Header("Tormenta solar")]
        public Sprite waterEnergy, waterFood, waterOxygen, waterHabitat;
        public Sprite dustEnergy, dustFood, dustOxygen, dustHabitat;

        [Header("Eclipse y micrometeoritos")]
        public Sprite eclipseFilter, eclipseSun, eclipseEarth;
        public Sprite meteorsOverlay;

        [Header("Interfaz de juego")]
        public Sprite topBar;
        public Sprite slotFood, slotEnergy, slotOxygen, slotHealth;
        public Sprite iconFood, iconEnergy, iconOxygen, iconHealth;
        public Sprite menuBackground, menuIcon, daysBackground, nextTurn;
        [Tooltip("Panel naranja (fondo menu) usado como ventana y botón con 9-slice.")]
        public Sprite panel;
        [Tooltip("PENDIENTE: el archivo original está vacío (todos sus píxeles son transparentes).")]
        public Sprite infoPanel;

        [Header("Pantalla de inicio")]
        public Sprite titleBackground, titleBanner;
        public Sprite buttonStart, buttonOptions, buttonCredits, buttonExit;
        [Tooltip("PENDIENTE: el archivo original está vacío (todos sus píxeles son transparentes).")]
        public Sprite buttonSelected;

        [Header("Mezclador (Assets/Data/Crater/CraterMixer.mixer): Master > Music, SFX")]
        public AudioMixer mixer;

        [Header("Música (Assets/Audios/Musica)")]
        public AudioClip musicMenu;        // menu.mp3: menú principal (loop)
        public AudioClip musicGame;        // main.mp3: gameplay (loop)
        [FormerlySerializedAs("musicTension")]
        public AudioClip musicEvent;       // MusicaEventoGrande.mp3: los tres acontecimientos principales

        [Header("Efectos (Assets/Audios/Efectos)")]
        public AudioClip sfxClick;         // botones.mp3: interfaz
        [FormerlySerializedAs("musicEclipse")]
        public AudioClip sfxEclipse;       // eclipse.mp3: la base se oscurece
        public AudioClip sfxRadiation;     // Radiacion.mp3: llega la tormenta solar
        public AudioClip sfxMeteors;       // real meteorits.mp3: impactos
    }
}
