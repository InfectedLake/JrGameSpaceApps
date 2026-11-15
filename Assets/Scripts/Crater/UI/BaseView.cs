using Crater.Core;
using Crater.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Crater.UI
{
    /// <summary>
    /// Escena principal de la base, compuesta con los sprites ORIGINALES de Assets/Sprites/gamplay principal
    /// en las posiciones medidas sobre la captura de referencia (ver CraterLayout).
    ///
    /// Jerarquía (el orden de hermanos de uGUI es el orden de capas, de atrás hacia adelante):
    ///   Fondo → Edificio_Solar → Edificio_Greenhouse (incluye explanada y tanque) → Rover
    ///   → Nucleo → Edificio_Recycler → Edificio_Habitat
    /// Los eventos no reemplazan la escena: cambian el sprite del edificio afectado o activan su capa.
    /// </summary>
    public class BaseView : MonoBehaviour
    {
        public readonly BuildingController[] buildings = new BuildingController[4];
        public RoverView rover;
        public RectTransform World { get; private set; }
        bool[] eclipsePreview;

        public static BaseView Create(RectTransform world, CraterArtSet art)
        {
            var bv = world.gameObject.AddComponent<BaseView>();
            bv.World = world;
            bv.Build(art);
            return bv;
        }

        void Build(CraterArtSet art)
        {
            var bg = UIFactory.Image(World, "Fondo", Color.white, art.background);
            bg.rectTransform.TopLeft(Vector2.zero, CraterLayout.WorldSize);

            buildings[(int)BuildingId.Solar] = BuildingController.Create(World, BuildingId.Solar, art.energy, art.energyLit, art.energyBroken, art.waterEnergy, art.dustEnergy);
            buildings[(int)BuildingId.Greenhouse] = BuildingController.Create(World, BuildingId.Greenhouse, art.food, art.foodLit, null, art.waterFood, art.dustFood);
            rover = RoverView.Create(World, art.rover, art.roverBroken);

            var central = UIFactory.Image(World, "Nucleo", Color.white, art.central);
            if (art.central != null) central.rectTransform.TopLeft(CraterLayout.Central, art.central.rect.size);

            buildings[(int)BuildingId.Recycler] = BuildingController.Create(World, BuildingId.Recycler, art.oxygen, art.oxygenLit, null, art.waterOxygen, art.dustOxygen);
            buildings[(int)BuildingId.Habitat] = BuildingController.Create(World, BuildingId.Habitat, art.habitat, art.habitatLit, null, art.waterHabitat, art.dustHabitat);
        }

        /// <summary>
        /// Vista previa de la distribución de baterías mientras el jugador decide (eclipse).
        /// null = usar el estado real del día.
        /// </summary>
        public void PreviewEclipse(bool[] powered) => eclipsePreview = powered;

        public void SetSelectable(bool on)
        {
            foreach (var b in buildings) b.SetSelectable(on);
        }

        public void SetState(GameState s, TurnReport p)
        {
            var m = s.today;
            bool water = m.stormDefense == (int)RadiationDefense.WaterWalls;
            bool dust = m.stormDefense == (int)RadiationDefense.RegolithIgloo || s.regolithReady;
            bool eclipse = m.eclipse || eclipsePreview != null;

            // Eclipse: brilla el edificio de cada sección alimentada con baterías. El núcleo nunca se enciende.
            // Monitoreo y comunicaciones → vivienda (único edificio restante con variante encendida).
            bool[] lit = new bool[4];
            if (eclipse)
            {
                var pw = eclipsePreview ?? new[] { m.eclipsePowerOxygen, m.eclipsePowerFood, m.eclipsePowerEnergy, m.eclipsePowerMonitoring };
                lit[(int)BuildingId.Recycler] = pw[(int)EclipseSection.Oxygen];
                lit[(int)BuildingId.Greenhouse] = pw[(int)EclipseSection.Food];
                lit[(int)BuildingId.Solar] = pw[(int)EclipseSection.EnergyCenter];
                lit[(int)BuildingId.Habitat] = pw[(int)EclipseSection.Monitoring];
            }
            bool panelsBroken = s.panels.broken || s.panels.Repairing;
            for (int i = 0; i < 4; i++)
                buildings[i].Refresh(p?.buildings[i], lit[i], i == (int)BuildingId.Solar && panelsBroken, water, dust);
            rover.SetState(s);
        }
    }
}
