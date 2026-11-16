using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Recursos principales (Energía, Oxígeno, Comida, Salud) y agua auxiliar: lectura,
    /// validación de rangos y aplicación de costos inmediatos. GameObject: "ResourceManager".
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;
        GameState S => G.State;

        public int Energy => S.energy;
        public int Oxygen => S.oxygen;
        public int Food => S.food;
        public int Health => S.health;
        public int Water => S.water;
        public int Max => G.Config.resourceMax;

        public bool IsLow(int value) => value <= G.Config.alertThreshold;

        /// <summary>Aplica un costo o beneficio inmediato y mantiene todo entre 0 y 100.</summary>
        public void Apply(int energy = 0, int oxygen = 0, int food = 0, int water = 0, int habitat = 0)
        {
            S.energy += energy; S.oxygen += oxygen; S.food += food; S.water += water; S.habitat += habitat;
            S.ClampAll(G.Config);
            G.NotifyChanged();
        }

        /// <summary>
        /// Avisos breves al empezar el día (se muestran unos segundos, no como panel permanente).
        /// Los valores auxiliares solo se mencionan cuando afectan una decisión.
        /// </summary>
        public List<(string text, NoticeKind kind)> DayWarnings(TurnReport p)
        {
            var list = new List<(string, NoticeKind)>();
            if (S == null || p == null) return list;
            if (p.after.oxygen <= 0) list.Add((Loc.T("WARN_OXYGEN_OUT"), NoticeKind.Danger));
            else if (p.after.food <= 0) list.Add((Loc.T("WARN_FOOD_OUT"), NoticeKind.Danger));
            foreach (var b in p.buildings)
            {
                if (b.visual == BuildingVisual.NoPower) list.Add((Loc.T("WARN_NO_POWER", Loc.Building(b.id)), NoticeKind.Warning));
                if (b.visual == BuildingVisual.NoWater) list.Add((Loc.T("WARN_NO_WATER", Loc.Building(b.id)), NoticeKind.Warning));
            }
            if (S.habitat <= G.Config.lowHabitatThreshold) list.Add((Loc.T("WARN_HABITAT_BAD"), NoticeKind.Warning));
            return list;
        }
    }
}
