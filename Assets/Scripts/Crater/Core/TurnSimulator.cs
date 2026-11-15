using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crater.Core
{
    /// <summary>Cambio de un recurso durante el día, con su cálculo paso a paso.</summary>
    public class ResourceChange
    {
        public string name;
        public int before;
        public int gross;      // valor antes de limitar a 0..max
        public int after;      // valor final limitado
        public readonly List<int> parts = new List<int>();
        public readonly List<string> labels = new List<string>();

        public ResourceChange(string name, int before) { this.name = name; this.before = before; gross = before; }

        /// <summary>Cambio neto calculado (antes del límite).</summary>
        public int GrossDelta => gross - before;
        /// <summary>Cambio real que verá el jugador.</summary>
        public int Delta => after - before;
        public int Wasted => gross > after ? gross - after : 0;

        public void Add(int v, string label)
        {
            if (v == 0) return;
            parts.Add(v); labels.Add(label); gross += v;
        }

        public string Formula()
        {
            var sb = new StringBuilder(before.ToString());
            foreach (var p in parts) sb.Append(p >= 0 ? " + " + p : " − " + (-p));
            return sb.ToString();
        }

        public string Breakdown()
        {
            if (parts.Count == 0) return "Sin cambios";
            var sb = new StringBuilder();
            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0) sb.Append("   ");
                sb.Append(Num.Signed(parts[i])).Append(' ').Append(labels[i]);
            }
            return sb.ToString();
        }
    }

    public class BuildingResult
    {
        public BuildingId id;
        public bool on;
        public BuildingVisual visual;
        public int energyUsed, energyMade, oxygenMade, foodMade, waterUsed;
        public string reason = "";
        public bool IsWorking => visual == BuildingVisual.Working || visual == BuildingVisual.Broken;
    }

    public class TurnReport
    {
        public GameState before;
        public GameState after;
        public readonly BuildingResult[] buildings = new BuildingResult[4];
        public ResourceChange energy, oxygen, food, health, water, habitat;
        public int solarProduced;
        public int iceCollected;
        public bool batteryRecharged;
        public bool regolithCompleted;
        public bool moraleHit;
        public readonly List<string> notes = new List<string>();

        public BuildingResult Get(BuildingId id) => buildings[(int)id];
        public IEnumerable<ResourceChange> Main => new[] { energy, oxygen, food, health };
    }

    /// <summary>
    /// Simulación de un día. Es la ÚNICA fuente de verdad: la proyección en vivo y el botón
    /// TERMINAR DÍA llaman a este mismo método, por eso nunca pueden dar resultados distintos.
    /// No modifica el estado recibido.
    /// </summary>
    public static class TurnSimulator
    {
        public static TurnReport Simulate(GameState s0, BalanceConfig c)
        {
            var s = s0.Clone();
            var m = s.today;
            var r = new TurnReport { before = s0 };
            var E = r.energy = new ResourceChange("Energía", s.energy);
            var O = r.oxygen = new ResourceChange("Oxígeno", s.oxygen);
            var F = r.food = new ResourceChange("Comida", s.food);
            var H = r.health = new ResourceChange("Salud", s.health);
            var W = r.water = new ResourceChange("Agua", s.water);
            var V = r.habitat = new ResourceChange("Vivienda", s.habitat);
            int waterAtStart = s.water;

            // 1. Paneles solares generan energía según su capacidad.
            var solarStats = c.Get(BuildingId.Solar);
            var solar = r.buildings[0] = new BuildingResult { id = BuildingId.Solar, on = s.IsOn(BuildingId.Solar) };
            if (!solar.on) solar.visual = BuildingVisual.Off;
            else if (m.eclipse) { solar.visual = BuildingVisual.Inactive; solar.reason = "La Tierra tapa al Sol: no hay luz"; }
            else if (m.panelsStowed) { solar.visual = BuildingVisual.Inactive; solar.reason = "Paneles guardados por los micrometeoritos"; }
            else if (s.panels.Effective <= 0) { solar.visual = BuildingVisual.Inactive; solar.reason = "En reparación"; }
            else
            {
                solar.energyMade = Num.Round(solarStats.energy * s.panels.Effective / 100.0);
                solar.visual = s.panels.Effective < 100 ? BuildingVisual.Broken : BuildingVisual.Working;
                if (s.panels.Effective < 100) solar.reason = "Dañados: producen al " + s.panels.Effective + "%";
            }
            r.solarProduced = solar.energyMade;
            E.Add(solar.energyMade, "paneles solares");

            // 2-3. Edificios encendidos con energía suficiente, en orden de prioridad.
            var order = new[] { BuildingId.Recycler, BuildingId.Greenhouse, BuildingId.Habitat }
                .OrderBy(id => c.Get(id).priority).ToArray();
            float overhead = s.energyOverheadTurns > 0 ? 1f + c.eclipseEnergyOverhead : 1f;

            foreach (var id in order)
            {
                var st = c.Get(id);
                var br = r.buildings[(int)id] = new BuildingResult { id = id, on = s.IsOn(id) };
                if (!br.on) { br.visual = BuildingVisual.Off; continue; }

                if (m.productionPaused && id != BuildingId.Habitat)
                {
                    br.visual = BuildingVisual.Paused;
                    br.reason = "Producción pausada: la tripulación está en el búnker";
                    continue;
                }

                int cost = Num.Round(-st.energy * overhead);
                int waterNeed = -st.water;

                if (id == BuildingId.Recycler && waterNeed > 0 && W.gross < waterNeed)
                {
                    br.visual = BuildingVisual.NoWater;
                    br.reason = "Sin agua: no puede fabricar oxígeno";
                    continue;
                }
                if (E.gross < cost)
                {
                    br.visual = BuildingVisual.NoPower;
                    br.reason = "No hay energía suficiente (necesita " + cost + ")";
                    continue;
                }

                E.Add(-cost, Lower(st.name));
                br.energyUsed = cost;
                br.visual = BuildingVisual.Working;
                if (overhead > 1f) br.reason = "Red inestable: consume +" + Num.Round(c.eclipseEnergyOverhead * 100) + "%";

                if (id == BuildingId.Recycler)
                {
                    W.Add(-waterNeed, "reciclador");
                    br.waterUsed = waterNeed;
                    br.oxygenMade = m.eclipseOxygenUnpowered ? 0 : Num.Round(st.oxygen * m.oxygenFactor);
                    O.Add(br.oxygenMade, "reciclador");
                    if (m.eclipseOxygenUnpowered) { br.visual = BuildingVisual.Inactive; br.reason = "Sin baterías para el oxígeno"; }
                }
                else if (id == BuildingId.Greenhouse)
                {
                    int use = System.Math.Min(waterNeed, System.Math.Max(0, W.gross));
                    W.Add(-use, "invernadero");
                    br.waterUsed = use;

                    double mult = 1.0;
                    var why = new List<string>();
                    if (waterAtStart < c.lowWaterThreshold) { mult *= 1.0 - c.lowWaterFoodPenalty; why.Add("poca agua −" + Num.Round(c.lowWaterFoodPenalty * 100) + "%"); }
                    if (s.foodPenaltyTurns > 0) { mult *= 1.0 - c.eclipseFoodPenalty; why.Add("cultivos sin luz −" + Num.Round(c.eclipseFoodPenalty * 100) + "%"); }
                    bool paused = m.foodPaused;
                    br.foodMade = paused ? 0 : Num.Round(st.food * mult);
                    br.oxygenMade = m.eclipseOxygenUnpowered ? 0 : Num.Round(st.oxygen * m.oxygenFactor);
                    F.Add(br.foodMade, "invernadero");
                    O.Add(br.oxygenMade, "invernadero");
                    if (m.foodPaused) why.Add("agua usada como escudo");
                    if (why.Count > 0) br.reason = string.Join(", ", why);
                }
            }
            bool habitatWorking = r.buildings[(int)BuildingId.Habitat].visual == BuildingVisual.Working;

            if (m.eclipseOxygenUnpowered) O.Add(-c.eclipseOxygenLoss, "eclipse sin oxígeno");

            // 4. Agua: los rovers recolectan hielo según su capacidad.
            if (!m.roversStowed && s.roverBusyTurns == 0 && s.rovers.Effective > 0)
                r.iceCollected = Num.Round(c.roverIcePerTurn * s.rovers.Effective / 100.0);
            W.Add(r.iceCollected, "hielo de los rovers");

            // 5-6. Consumo de la tripulación.
            O.Add(-s.astronauts * c.oxygenPerAstronaut, "tripulación");
            F.Add(-s.astronauts * c.foodPerAstronaut, "tripulación");

            // Vivienda: descanso, refugio de regolito y ánimo.
            // Vivienda (edificio): encendida mantiene y mejora el hábitat; apagada o sin energía lo deteriora.
            if (habitatWorking) V.Add(c.habitatUpkeepGain, "sistemas de la vivienda");
            else { V.Add(-c.habitatOffLoss, "vivienda sin sistemas"); s.habitatDownCycle = s.Cycle; }
            if (s.roverBusyTurns > 0)
            {
                s.roverBusyTurns--;
                if (s.roverBusyTurns == 0 && s.roverTask == RoverTask.Regolith)
                {
                    s.roverTask = RoverTask.None;
                    s.regolithReady = true;
                    r.regolithCompleted = true;
                    V.Add(c.regolithHabitatBonus, "aislamiento de regolito");
                    r.notes.Add("El rover terminó el refugio de regolito.");
                }
            }
            if (s.Cycle == c.moraleCycle && s.IsLastDayOfCycle && s.habitatDownCycle == c.moraleCycle)
            {
                V.Add(-c.moraleHabitatLoss, "fatiga y estrés");
                r.moraleHit = true;
                r.notes.Add("La tripulación está cansada: la vivienda estuvo apagada este ciclo y no pudieron descansar bien.");
            }

            // 7. Salud.
            bool noOxygen = O.gross <= 0, noFood = F.gross <= 0;
            if (noOxygen || noFood)
                H.Add(-c.starvingHealthLoss, noOxygen ? "sin oxígeno" : "sin comida");
            int habitatNow = Num.Clamp(V.gross, 0, c.resourceMax);
            if (habitatNow < c.lowHabitatThreshold)
                H.Add(-c.lowHabitatHealthLoss, "vivienda en mal estado");
            else if (!noOxygen && !noFood && habitatNow >= c.healthRecoveryHabitatMin && s.health < c.resourceMax)
                H.Add(c.healthRecovery, "buen descanso y cuidados");

            // 8. Límites.
            int max = c.resourceMax;
            s.energy = E.after = Num.Clamp(E.gross, 0, max);
            s.oxygen = O.after = Num.Clamp(O.gross, 0, max);
            s.food = F.after = Num.Clamp(F.gross, 0, max);
            s.health = H.after = Num.Clamp(H.gross, 0, max);
            s.water = W.after = Num.Clamp(W.gross, 0, max);
            s.habitat = V.after = Num.Clamp(V.gross, 0, max);

            // Contadores de fin de día.
            TickRepair(s.panels, "Los paneles solares", r);
            TickRepair(s.rovers, "Los rovers", r);
            if (s.foodPenaltyTurns > 0) s.foodPenaltyTurns--;
            if (s.energyOverheadTurns > 0) s.energyOverheadTurns--;
            if (r.solarProduced > 0 && s.batteries < c.maxBatteries)
            {
                s.batteries = System.Math.Min(c.maxBatteries, s.batteries + c.batteryRechargePerTurn);
                r.batteryRecharged = true;
            }
            s.radiationDose += c.backgroundDosePerTurn;
            s.today = new DayModifiers();

            // 10. Avanza el día.
            s.day++;

            // 11. Emergencia, victoria o derrota.
            if (s.energy <= 0 || s.habitat <= 0)
            {
                s.emergency = true;
                s.emergencyReason = s.energy <= 0 && s.habitat <= 0 ? "energía y vivienda"
                                  : s.energy <= 0 ? "energía" : "vivienda";
            }
            else { s.emergency = false; s.emergencyReason = ""; }

            if (s.health <= 0)
            {
                s.outcome = Outcome.Defeat;
                s.defeatCause = noOxygen ? "Se acabó el oxígeno de la base." : noFood ? "Se acabó la comida de la tripulación." : "La vivienda quedó en muy mal estado.";
            }
            else if (s.day > s.totalTurns) s.outcome = Outcome.Victory;

            r.after = s;
            return r;
        }

        static void TickRepair(EquipmentState e, string name, TurnReport r)
        {
            if (e.repairTurnsLeft <= 0) return;
            e.repairTurnsLeft--;
            if (e.repairTurnsLeft == 0)
            {
                e.capacity = 100; e.broken = false;
                r.notes.Add(name + " vuelven a funcionar al 100%.");
            }
        }

        static string Lower(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
    }
}
