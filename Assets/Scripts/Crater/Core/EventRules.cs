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
                    if (s.water < c.waterWallWater) { why = Loc.T("WHY_NOT_ENOUGH_WATER"); return 0f; }
                    if (WaterPrepared(s, ev)) { why = Loc.T("WHY_WALLS_READY"); return c.waterBlockWarned; }
                    why = ev.warned ? Loc.T("WHY_NOT_IN_TIME") : Loc.T("WHY_NO_WARNING");
                    return c.waterBlockUnwarned;
                case RadiationDefense.RegolithIgloo:
                    if (s.regolithReady) { why = Loc.T("WHY_REGOLITH_DONE"); return c.regolithBlock; }
                    why = Loc.T("WHY_REGOLITH_THIN");
                    return -c.regolithThinPenalty;
                case RadiationDefense.Bunker:
                    why = Loc.T("WHY_BUNKER");
                    return c.bunkerBlock;
                default:
                    why = Loc.T("WHY_NONE");
                    return 0f;
            }
        }

        public static bool CanChoose(RadiationDefense d, GameState s, BalanceConfig c, bool atWarning, out string reason)
        {
            reason = "";
            switch (d)
            {
                case RadiationDefense.WaterWalls:
                    if (!atWarning && s.water < c.waterWallWater) { reason = Loc.T("NEED_WATER", c.waterWallWater); return false; }
                    return true;
                case RadiationDefense.RegolithIgloo:
                    if (s.regolithReady) return true;
                    if (s.roverBusyTurns > 0) { reason = Loc.T("REASON_ROVER_BUSY"); return false; }
                    if (s.rovers.Effective <= 0) { reason = Loc.T("REASON_ROVER_REPAIRING"); return false; }
                    if (s.energy < c.regolithEnergy) { reason = Loc.T("NEED_ENERGY", c.regolithEnergy); return false; }
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
            s.today.stormDefense = (int)d;
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
            o.summary = (pct >= 0 ? Loc.T("SUMMARY_BLOCK", pct) : Loc.T("SUMMARY_THIN_LAYER", -pct))
                        + " (" + why + "). " + Loc.T("SUMMARY_HABITAT_LOSS", o.damage);
            if (o.electronicsDamaged) o.summary += " " + Loc.T("SUMMARY_PANELS_DAMAGED");
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
                reason = Loc.T("NEED_ENERGY", c.stowPanelsEnergy);
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
            if (p == MeteorProtect.Panels) parts.Add(Loc.T("METEOR_PANELS_SAFE"));
            if (p == MeteorProtect.Rovers) parts.Add(Loc.T("METEOR_ROVERS_SAFE"));
            if (p == MeteorProtect.None) parts.Add(Loc.T("METEOR_NOTHING_PROTECTED"));
            if (o.panelsBroke) parts.Add(Loc.T("METEOR_PANELS_HIT"));
            else if (panelsExposed) parts.Add(Loc.T("METEOR_PANELS_HELD"));
            if (o.roversBroke) parts.Add(Loc.T("METEOR_ROVERS_HIT"));
            else if (roversExposed) parts.Add(Loc.T("METEOR_ROVERS_HELD"));
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
                case EclipseSection.Oxygen: return Loc.T("SECTION_OXYGEN");
                case EclipseSection.Food: return Loc.T("SECTION_FOOD");
                case EclipseSection.EnergyCenter: return Loc.T("SECTION_ENERGY");
                default: return Loc.T("SECTION_MONITORING");
            }
        }

        public static string Consequence(EclipseSection sec, bool powered, BalanceConfig c)
        {
            switch (sec)
            {
                case EclipseSection.Oxygen:
                    return powered ? Loc.T("CONSEQ_OXYGEN_ON") : Loc.T("CONSEQ_OXYGEN_OFF", c.eclipseOxygenLoss);
                case EclipseSection.Food:
                    return powered ? Loc.T("CONSEQ_FOOD_ON") : Loc.T("CONSEQ_FOOD_OFF", Num.Round(c.eclipseFoodPenalty * 100), Loc.Days(c.eclipseFoodPenaltyTurns));
                case EclipseSection.EnergyCenter:
                    return powered ? Loc.T("CONSEQ_ENERGY_ON") : Loc.T("CONSEQ_ENERGY_OFF", Num.Round(c.eclipseEnergyOverhead * 100), Loc.Days(c.eclipseEnergyOverheadTurns));
                default:
                    return powered ? Loc.T("CONSEQ_MONITOR_ON") : Loc.T("CONSEQ_MONITOR_OFF");
            }
        }

        public static List<string> Apply(GameState s, bool[] powered, BalanceConfig c)
        {
            var notes = new List<string>();
            s.batteries -= Cost(powered, c);
            s.today.eclipse = true;
            s.today.eclipsePowerOxygen = powered[(int)EclipseSection.Oxygen];
            s.today.eclipsePowerFood = powered[(int)EclipseSection.Food];
            s.today.eclipsePowerEnergy = powered[(int)EclipseSection.EnergyCenter];
            s.today.eclipsePowerMonitoring = powered[(int)EclipseSection.Monitoring];
            s.eclipseCount++;
            if (!powered[(int)EclipseSection.Oxygen]) s.today.eclipseOxygenUnpowered = true;
            if (!powered[(int)EclipseSection.Food]) s.foodPenaltyTurns = System.Math.Max(s.foodPenaltyTurns, c.eclipseFoodPenaltyTurns);
            if (!powered[(int)EclipseSection.EnergyCenter]) s.energyOverheadTurns = System.Math.Max(s.energyOverheadTurns, c.eclipseEnergyOverheadTurns);
            if (!powered[(int)EclipseSection.Monitoring]) s.nextEventUnwarned = true;
            for (int i = 0; i < 4; i++)
                notes.Add((powered[i] ? Loc.T("ECLIPSE_POWERED") : Loc.T("ECLIPSE_UNPOWERED")) + " - " + SectionName((EclipseSection)i) + ": " + Consequence((EclipseSection)i, powered[i], c));
            s.ClampAll(c);
            return notes;
        }
    }
}
