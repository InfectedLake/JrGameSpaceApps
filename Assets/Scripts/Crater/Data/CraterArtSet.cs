using UnityEngine;

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

        [Header("Rover (PENDIENTE: la carpeta Sprites no tiene arte de rover)")]
        public Sprite rover, roverBroken;

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
    }
}
