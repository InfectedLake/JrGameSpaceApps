using System.Collections.Generic;
using System.Linq;

namespace Crater.Core
{
    /// <summary>
    /// Calendario de eventos: ciclo 1 sin eventos, un evento por ciclo desde el 2.
    /// Los tres tipos aparecen al menos una vez; los extra se eligen con 40/35/25 %
    /// y el eclipse aparece como máximo dos veces.
    /// </summary>
    public static class EventScheduler
    {
        public static List<ScheduledEvent> Generate(BalanceConfig c, IRandom rng)
        {
            int eventCycles = System.Math.Max(0, c.cycles - 1);
            var types = new List<CraterEventType> { CraterEventType.SolarRadiation, CraterEventType.Micrometeorites, CraterEventType.EarthShadow };
            Shuffle(types, rng);
            if (types.Count > eventCycles) types = types.Take(eventCycles).ToList();

            int eclipses = types.Count(t => t == CraterEventType.EarthShadow);
            while (types.Count < eventCycles)
            {
                var t = PickExtra(c, rng, eclipses >= c.maxEclipses);
                if (t == CraterEventType.EarthShadow) eclipses++;
                types.Add(t);
            }
            Shuffle(types, rng);

            var list = new List<ScheduledEvent>();
            for (int i = 0; i < types.Count; i++)
            {
                int cycle = i + 2;
                int cycleStart = (cycle - 1) * c.turnsPerCycle + 1;
                // Entre el 2.º y el último día del ciclo, para que siempre haya tiempo de aviso.
                int offset = c.turnsPerCycle > 1 ? rng.Range(1, c.turnsPerCycle) : 0;
                list.Add(new ScheduledEvent { type = types[i], day = cycleStart + offset });
            }
            return list;
        }

        public static CraterEventType PickExtra(BalanceConfig c, IRandom rng, bool noEclipse)
        {
            float r = c.extraRadiationChance, m = c.extraMeteorChance, e = noEclipse ? 0f : c.extraEclipseChance;
            double roll = rng.NextDouble() * (r + m + e);
            if (roll < r) return CraterEventType.SolarRadiation;
            if (roll < r + m) return CraterEventType.Micrometeorites;
            return CraterEventType.EarthShadow;
        }

        /// <summary>Intensidad de juego sin datos reales: 50 % / 35 % / 15 %.</summary>
        public static int RandomIntensity(BalanceConfig c, IRandom rng)
        {
            double roll = rng.NextDouble();
            double acc = 0;
            for (int i = 0; i < c.intensityChances.Length; i++)
            {
                acc += c.intensityChances[i];
                if (roll < acc) return i + 1;
            }
            return c.intensityChances.Length;
        }

        public static int WarningLead(CraterEventType t, BalanceConfig c)
        {
            switch (t)
            {
                case CraterEventType.SolarRadiation: return c.radiationWarningLead;
                case CraterEventType.Micrometeorites: return c.meteorWarningLead;
                default: return c.eclipseWarningLead;
            }
        }

        public static IEnumerable<ScheduledEvent> Arrivals(GameState s)
            => s.schedule.Where(e => !e.resolved && e.day == s.day);

        public static IEnumerable<ScheduledEvent> DueWarnings(GameState s, BalanceConfig c)
            => s.schedule.Where(e => !e.resolved && !e.warningProcessed && e.day > s.day && s.day >= e.day - WarningLead(e.type, c))
                         .OrderBy(e => e.day);

        public static ScheduledEvent Next(GameState s)
            => s.schedule.Where(e => !e.resolved).OrderBy(e => e.day).FirstOrDefault();

        static void Shuffle<T>(IList<T> list, IRandom rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
            }
        }
    }
}
