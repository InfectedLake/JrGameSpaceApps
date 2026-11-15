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
            return
                "Panel solar: +" + c.Get(BuildingId.Solar).energy + " energía por día\n" +
                "Invernadero: " + Num.Signed(c.Get(BuildingId.Greenhouse).energy) + " energía, +" + c.Get(BuildingId.Greenhouse).oxygen + " oxígeno, +" + c.Get(BuildingId.Greenhouse).food + " comida, " + Num.Signed(c.Get(BuildingId.Greenhouse).water) + " agua\n" +
                "Reciclador: " + Num.Signed(c.Get(BuildingId.Recycler).energy) + " energía, +" + c.Get(BuildingId.Recycler).oxygen + " oxígeno, " + Num.Signed(c.Get(BuildingId.Recycler).water) + " agua\n" +
                "Vivienda: " + Num.Signed(c.Get(BuildingId.Habitat).energy) + " energía; apagada o sin energía: Salud −" + c.habitatOffHealthLoss + " por día\n" +
                "Eventos menores: unos " + Num.Round(c.TotalTurns * c.minorEventsPerTurn) + " por misión, cada uno con dos opciones que cuestan recursos distintos\n" +
                "Cada astronauta: −" + c.oxygenPerAstronaut + " oxígeno y −" + c.foodPerAstronaut + " comida por día\n" +
                "Sin oxígeno o sin comida: salud −" + c.starvingHealthLoss + " por día\n" +
                "Rover: +" + c.roverIcePerTurn + " agua por día (hielo)\n" +
                "Daño por radiación: Intensidad × " + c.radiationDamagePerIntensity + " × (1 − Bloqueo)\n" +
                "Dosis extra por tormenta: " + c.stormDosePerIntensity.ToString("0") + " mSv por punto de intensidad sin protección\n" +
                "Avería por micrometeoritos: " + Num.Round(c.meteorBreakChance * 100) + "% para cada equipo expuesto\n" +
                "Eventos extra: radiación " + Num.Round(c.extraRadiationChance * 100) + "%, micrometeoritos " + Num.Round(c.extraMeteorChance * 100) + "%, eclipse " + Num.Round(c.extraEclipseChance * 100) + "%\n" +
                "Intensidad sin datos: baja " + Num.Round(c.intensityChances[0] * 100) + "%, media " + Num.Round(c.intensityChances[1] * 100) + "%, alta " + Num.Round(c.intensityChances[2] * 100) + "%\n" +
                "Clase de llamarada DONKI → intensidad: X = 3, M = 2, otras = 1";
        }
    }
}
