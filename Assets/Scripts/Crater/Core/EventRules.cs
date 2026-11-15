using System.Collections.Generic;

namespace Crater.Core
{
    // ======================================================================
    // EVENTO 1: RÁFAGA DE RADIACIÓN SOLAR
    // Daño = Intensidad × 12 × (1 − Bloqueo)   (fórmula del documento de diseño)
    // ======================================================================
    public class RadiationOutcome
    {
        public RadiationDefense defense;
        public float block;
        public int damage;
        public float dose;
        public bool electronicsDamaged;
        public string summary = "";
    }

    public static class RadiationRules
    {
        public static int Damage(int intensity, float block, BalanceConfig c)
            => System.Math.Max(0, Num.Round(intensity * c.radiationDamagePerIntensity * (1.0 - block)));

        public static float Dose(int intensity, float block, BalanceConfig c)
            => (float)System.Math.Max(0, intensity * c.stormDosePerIntensity * (1.0 - block));

        /// <summary>
        /// Las paredes de agua bloquean 80% solo si hubo aviso y se prepararon antes de la tormenta.
        /// </summary>
        public static bool WaterPrepared(GameState s, ScheduledEvent ev)
            => ev.warned && ev.choice == (int)RadiationDefense.WaterWalls && ev.choiceDay >= 0
               && ev.choiceDay < ev.day;

        /// <summary>Bloqueo que se aplicaría si la tormenta llegara ahora con esta defensa.</summary>
        public static float Block(RadiationDefense d, GameState s, ScheduledEvent ev, BalanceConfig c, out string why)
        {
            switch (d)
            {
                case RadiationDefense.WaterWalls:
                    if (s.water < c.waterWallWater) { why = "No hay suficiente agua para llenar las paredes"; return 0f; }
                    if (WaterPrepared(s, ev)) { why = "Paredes preparadas con aviso"; return c.waterBlockWarned; }
                    why = ev.warned ? "No se prepararon a tiempo" : "Sin aviso anticipado";
                    return c.waterBlockUnwarned;
                case RadiationDefense.RegolithIgloo:
                    if (s.regolithReady) { why = "Refugio de regolito terminado"; return c.regolithBlock; }
                    why = "Capa de regolito incompleta: radiación secundaria";
                    return -c.regolithThinPenalty;
                case RadiationDefense.Bunker:
                    why = "Tripulación dentro del búnker";
                    return c.bunkerBlock;
                default:
                    why = "Sin protección";
                    return 0f;
            }
        }

        public static bool CanChoose(RadiationDefense d, GameState s, BalanceConfig c, bool atWarning, out string reason)
        {
            reason = "";
            switch (d)
            {
                case RadiationDefense.WaterWalls:
                    if (!atWarning && s.water < c.waterWallWater) { reason = "Necesitas " + c.waterWallWater + " de agua"; return false; }
                    return true;
                case RadiationDefense.RegolithIgloo:
                    if (s.regolithReady) return true;
                    if (s.roverBusyTurns > 0) { reason = "El rover ya está ocupado"; return false; }
                    if (s.rovers.Effective <= 0) { reason = "El rover está en reparación"; return false; }
                    if (s.energy < c.regolithEnergy) { reason = "Necesitas " + c.regolithEnergy + " de energía"; return false; }
                    return true;
                default:
                    return true;
            }
        }

        /// <summary>Costos que se pagan en el momento de elegir (antes de la tormenta).</summary>
        public static void ApplyChoiceCosts(RadiationDefense d, GameState s, BalanceConfig c)
        {
            if (d == RadiationDefense.RegolithIgloo && !s.regolithReady && s.roverBusyTurns == 0)
                BaseActions.StartRegolith(s, c);
        }

        /// <summary>Resuelve la llegada de la tormenta. Modifica el estado.</summary>
        public static RadiationOutcome Resolve(GameState s, ScheduledEvent ev, RadiationDefense d, BalanceConfig c)
        {
            var o = new RadiationOutcome { defense = d };
            o.block = Block(d, s, ev, c, out string why);

            switch (d)
            {
                case RadiationDefense.WaterWalls:
                    if (s.water >= c.waterWallWater)
                    {
                        s.water -= c.waterWallWater;
                        s.today.foodPaused = true;
                        s.today.oxygenFactor = c.waterWallOxygenFactor;
                    }
                    break;
                case RadiationDefense.RegolithIgloo:
                    if (s.regolithReady) s.regolithReady = false;       // [JUEGO] la cubierta se usa en una tormenta
                    else if (s.roverBusyTurns == 0 && s.rovers.Effective > 0 && s.energy >= c.regolithEnergy)
                        BaseActions.StartRegolith(s, c);                 // empieza tarde: protegerá el aislamiento después
                    break;
                case RadiationDefense.Bunker:
                    s.habitat -= c.bunkerHabitat;
                    s.oxygen -= c.bunkerOxygen;
                    s.water -= c.bunkerWater;
                    s.food -= c.bunkerFood;
                    s.today.productionPaused = true;
                    break;
                case RadiationDefense.None:
                    if (s.panels.capacity >= 100 && !s.panels.Repairing)
                    {
                        // [JUEGO] La electrónica expuesta se daña: los paneles quedan al 50%.
                        s.panels.capacity = c.brokenCapacity; s.panels.broken = true;
                        s.breakdowns++;
                        o.electronicsDamaged = true;
                    }
                    break;
            }

            o.damage = Damage(ev.intensity, o.block, c);
            o.dose = Dose(ev.intensity, o.block, c);
            s.habitat -= o.damage;
            s.totalHabitatDamage += o.damage;
            s.radiationDose += o.dose;
            s.stormDose += o.dose;
            s.ClampAll(c);

            int pct = Num.Round(o.block * 100);
            o.summary = (pct >= 0 ? "Bloqueo " + pct + "%" : "La capa delgada AUMENTÓ la dosis un " + (-pct) + "%")
                        + " (" + why + "). Vivienda −" + o.damage + ".";
            if (o.electronicsDamaged) o.summary += " ¡La radiación dañó los paneles solares (50%)!";
            return o;
        }
    }

    // ======================================================================
    // EVENTO 2: LLUVIA DE MICROMETEORITOS
    // ======================================================================
    public class MeteorOutcome
    {
        public MeteorProtect protect;
        public bool panelsBroke, roversBroke;
        public string summary = "";
    }

    public static class MeteorRules
    {
        public static bool CanProtect(MeteorProtect p, GameState s, BalanceConfig c, out string reason)
        {
            reason = "";
            if (p == MeteorProtect.Panels && s.energy < c.stowPanelsEnergy)
            {
                reason = "Necesitas " + c.stowPanelsEnergy + " de energía";
                return false;
            }
            return true;
        }

        public static MeteorOutcome Resolve(GameState s, ScheduledEvent ev, MeteorProtect p, IRandom rng, BalanceConfig c)
        {
            var o = new MeteorOutcome { protect = p };
            bool panelsExposed = true, roversExposed = true;

            if (p == MeteorProtect.Panels)
            {
                s.energy -= c.stowPanelsEnergy;
                s.today.panelsStowed = true;
                panelsExposed = false;
            }
            else if (p == MeteorProtect.Rovers)
            {
                s.water -= c.stowRoversWater;
                s.oxygen -= c.stowRoversOxygen;
                s.today.roversStowed = true;
                roversExposed = false;
            }

            if (panelsExposed && rng.NextDouble() < c.meteorBreakChance) { Break(s.panels, c); o.panelsBroke = true; s.breakdowns++; }
            if (roversExposed && rng.NextDouble() < c.meteorBreakChance) { Break(s.rovers, c); o.roversBroke = true; s.breakdowns++; }
            s.ClampAll(c);

            var parts = new List<string>();
            if (p == MeteorProtect.Panels) parts.Add("Paneles guardados a salvo");
            if (p == MeteorProtect.Rovers) parts.Add("Rovers guardados a salvo");
            if (p == MeteorProtect.None) parts.Add("No hubo tiempo de proteger nada");
            if (o.panelsBroke) parts.Add("¡Un impacto dañó los paneles solares! (50%)");
            else if (panelsExposed) parts.Add("Los paneles resistieron");
            if (o.roversBroke) parts.Add("¡Un impacto dañó los rovers! (50%)");
            else if (roversExposed) parts.Add("Los rovers resistieron");
            o.summary = string.Join(". ", parts) + ".";
            return o;
        }

        static void Break(EquipmentState e, BalanceConfig c)
        {
            if (e.Repairing) return; // ya está guardado en el taller
            e.capacity = System.Math.Min(e.capacity, c.brokenCapacity);
            e.broken = true;
        }
    }

    // ======================================================================
    // EVENTO 3: SOMBRA DE LA TIERRA
    // 4 baterías, 5 necesarias: siempre hay que sacrificar algo.
    // ======================================================================
    public static class EclipseRules
    {
        public static int Cost(bool[] powered, BalanceConfig c)
        {
            int t = 0;
            for (int i = 0; i < 4; i++) if (powered[i]) t += c.eclipseSectionCost[i];
            return t;
        }

        public static int TotalCost(BalanceConfig c)
        {
            int t = 0; foreach (var v in c.eclipseSectionCost) t += v; return t;
        }

        public static bool Affordable(bool[] powered, int batteries, BalanceConfig c) => Cost(powered, c) <= batteries;

        public static string SectionName(EclipseSection sec)
        {
            switch (sec)
            {
                case EclipseSection.Oxygen: return "Producción de oxígeno";
                case EclipseSection.Food: return "Producción de alimentos";
                case EclipseSection.EnergyCenter: return "Centro de energía";
                default: return "Monitoreo y comunicaciones";
            }
        }

        public static string Consequence(EclipseSection sec, bool powered, BalanceConfig c)
        {
            switch (sec)
            {
                case EclipseSection.Oxygen:
                    return powered ? "La producción de oxígeno continúa." : "Oxígeno producido = 0 y Oxígeno −" + c.eclipseOxygenLoss + ".";
                case EclipseSection.Food:
                    return powered ? "Se conserva la cosecha." : "Comida −" + Num.Round(c.eclipseFoodPenalty * 100) + "% durante " + c.eclipseFoodPenaltyTurns + " días.";
                case EclipseSection.EnergyCenter:
                    return powered ? "La red eléctrica sigue estable." : "Los edificios gastan +" + Num.Round(c.eclipseEnergyOverhead * 100) + "% de energía durante " + c.eclipseEnergyOverheadTurns + " días.";
                default:
                    return powered ? "Las alertas siguen activas." : "Sin aviso DONKI para el próximo evento.";
            }
        }

        public static List<string> Apply(GameState s, bool[] powered, BalanceConfig c)
        {
            var notes = new List<string>();
            s.batteries -= Cost(powered, c);
            s.today.eclipse = true;
            s.eclipseCount++;
            if (!powered[(int)EclipseSection.Oxygen]) s.today.eclipseOxygenUnpowered = true;
            if (!powered[(int)EclipseSection.Food]) s.foodPenaltyTurns = System.Math.Max(s.foodPenaltyTurns, c.eclipseFoodPenaltyTurns);
            if (!powered[(int)EclipseSection.EnergyCenter]) s.energyOverheadTurns = System.Math.Max(s.energyOverheadTurns, c.eclipseEnergyOverheadTurns);
            if (!powered[(int)EclipseSection.Monitoring]) s.nextEventUnwarned = true;
            for (int i = 0; i < 4; i++)
                notes.Add((powered[i] ? "CON ENERGÍA - " : "SIN ENERGÍA - ") + SectionName((EclipseSection)i) + ": " + Consequence((EclipseSection)i, powered[i], c));
            s.ClampAll(c);
            return notes;
        }
    }
}
