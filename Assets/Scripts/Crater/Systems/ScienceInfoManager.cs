using System.Collections.Generic;
using Crater.Core;
using Crater.Data;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Explicaciones educativas, fuentes y etiquetas REAL / JUEGO / PENDIENTE.
    /// GameObject: "ScienceInfoManager". Si la lista está vacía se usa el contenido por defecto.
    /// </summary>
    public class ScienceInfoManager : MonoBehaviour
    {
        public List<ScienceData> entries = new List<ScienceData>();
        readonly Dictionary<string, ScienceData> byId = new Dictionary<string, ScienceData>();

        void Awake()
        {
            if (entries == null || entries.Count == 0) entries = new List<ScienceData>(DefaultContent.Science());
            foreach (var e in entries) if (e != null && !string.IsNullOrEmpty(e.id)) byId[e.id] = e;
        }

        public IReadOnlyList<ScienceData> All => entries;

        public ScienceData Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            byId.TryGetValue(id, out var d);
            return d;
        }

        /// <summary>Resumen de los valores de juego más importantes para la pantalla de ciencia.</summary>
        public static string GameValuesSummary(BalanceConfig c)
        {
            var g = c.Get(BuildingId.Greenhouse);
            var r = c.Get(BuildingId.Recycler);
            return string.Join("\n", new[]
            {
                Loc.T("GV_SOLAR", Loc.Building(BuildingId.Solar), c.Get(BuildingId.Solar).energy),
                Loc.T("GV_GREENHOUSE", Loc.Building(BuildingId.Greenhouse), Num.Signed(g.energy), g.oxygen, g.food, Num.Signed(g.water)),
                Loc.T("GV_RECYCLER", Loc.Building(BuildingId.Recycler), Num.Signed(r.energy), r.oxygen, Num.Signed(r.water)),
                Loc.T("GV_HABITAT", Loc.Building(BuildingId.Habitat), Num.Signed(c.Get(BuildingId.Habitat).energy), c.habitatOffHealthLoss),
                Loc.T("GV_MINOR", Num.Round(c.TotalTurns * c.minorEventsPerTurn)),
                Loc.T("GV_ASTRONAUT", c.oxygenPerAstronaut, c.foodPerAstronaut),
                Loc.T("GV_STARVING", c.starvingHealthLoss),
                Loc.T("GV_ROVER", c.roverIcePerTurn),
                Loc.T("GV_RADIATION", c.radiationDamagePerIntensity),
                Loc.T("GV_DOSE", c.stormDosePerIntensity.ToString("0")),
                Loc.T("GV_METEOR", Num.Round(c.meteorBreakChance * 100)),
                Loc.T("GV_EXTRA", Num.Round(c.extraRadiationChance * 100), Num.Round(c.extraMeteorChance * 100), Num.Round(c.extraEclipseChance * 100)),
                Loc.T("GV_INTENSITY", Num.Round(c.intensityChances[0] * 100), Num.Round(c.intensityChances[1] * 100), Num.Round(c.intensityChances[2] * 100)),
                Loc.T("GV_DONKI"),
            });
        }
    }
}
