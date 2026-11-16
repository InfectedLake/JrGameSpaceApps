using System;
using System.Collections.Generic;
using System.Linq;

namespace Crater.Core
{
    /// <summary>Recursos principales que puede costar una decisión.</summary>
    public enum ResourceKind { Energy = 0, Oxygen = 1, Food = 2, Health = 3 }

    /// <summary>Una alternativa de un evento menor: un texto breve y un costo en UN recurso.</summary>
    [Serializable]
    public class MinorOption
    {
        public string text = "";
        public ResourceKind resource;
        public int cost;

        public MinorOption() { }
        public MinorOption(string text, ResourceKind resource, int cost) { this.text = text; this.resource = resource; this.cost = cost; }
    }

    /// <summary>
    /// Evento menor [JUEGO]: incidente cotidiano ficticio con exactamente dos opciones.
    /// Editable desde GameBalance.asset sin tocar código.
    /// </summary>
    [Serializable]
    public class MinorEventDef
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public MinorOption optionA = new MinorOption();
        public MinorOption optionB = new MinorOption();

        public MinorEventDef() { }
        public MinorEventDef(string id, string name, string description, MinorOption a, MinorOption b)
        { this.id = id; this.name = name; this.description = description; optionA = a; optionB = b; }

        public MinorOption Option(int i) => i == 0 ? optionA : optionB;
    }

    [Serializable]
    public class ScheduledMinor
    {
        public int day;
        public string id = "";
        public bool resolved;
        public bool skipped;
    }

    public static class MinorEventRules
    {
        public static List<MinorEventDef> Defaults() => new List<MinorEventDef>
        {
            new MinorEventDef("polvo", "Polvo lunar en los paneles",
                "El polvo lunar está reduciendo la eficiencia de nuestros paneles solares. Necesitamos actuar.",
                new MinorOption("Limpiar los paneles", ResourceKind.Energy, 8),
                new MinorOption("Bajar las luces del invernadero", ResourceKind.Food, 7)),
            new MinorEventDef("filtro", "Filtro de aire saturado",
                "El filtro de aire necesita mantenimiento para seguir funcionando correctamente.",
                new MinorOption("Ciclo extra de mantenimiento", ResourceKind.Energy, 8),
                new MinorOption("Usar reservas de oxígeno", ResourceKind.Oxygen, 7)),
            new MinorEventDef("frio", "Descenso de temperatura",
                "La temperatura de la vivienda está disminuyendo.",
                new MinorOption("Mantener la calefacción", ResourceKind.Energy, 8),
                new MinorOption("Bajar la calefacción", ResourceKind.Health, 5)),
            new MinorEventDef("cultivos", "Problema en los cultivos",
                "Una parte de nuestros cultivos está teniendo problemas. Debemos decidir cómo actuar.",
                new MinorOption("Descartar parte de la cosecha", ResourceKind.Food, 8),
                new MinorOption("Usar más electricidad para salvarla", ResourceKind.Energy, 9)),
            new MinorEventDef("fuga_aire", "Pequeña fuga de aire",
                "Se detectó una fuga menor en el sistema de aire.",
                new MinorOption("Compensar con las reservas", ResourceKind.Oxygen, 8),
                new MinorOption("Forzar el reciclador", ResourceKind.Energy, 10)),
            new MinorEventDef("fuga_agua", "Fuga en el depósito de agua",
                "El sistema de agua presenta una pequeña fuga.",
                new MinorOption("Activar la recuperación de agua", ResourceKind.Energy, 8),
                new MinorOption("Racionar el agua del invernadero", ResourceKind.Food, 7)),
        };

        public static int Get(GameState s, ResourceKind r)
        {
            switch (r)
            {
                case ResourceKind.Energy: return s.energy;
                case ResourceKind.Oxygen: return s.oxygen;
                case ResourceKind.Food: return s.food;
                default: return s.health;
            }
        }

        /// <summary>Una opción solo se puede pagar si alcanza el recurso (nunca es gratis ni deja valores negativos).</summary>
        public static bool CanPay(GameState s, MinorOption o) => o != null && o.cost > 0 && Get(s, o.resource) >= o.cost;

        public static bool CanResolve(GameState s, MinorEventDef d) => CanPay(s, d.optionA) || CanPay(s, d.optionB);

        /// <summary>Aplica una sola vez el costo de la opción elegida.</summary>
        public static void Apply(GameState s, MinorOption o, BalanceConfig c)
        {
            switch (o.resource)
            {
                case ResourceKind.Energy: s.energy -= o.cost; break;
                case ResourceKind.Oxygen: s.oxygen -= o.cost; break;
                case ResourceKind.Food: s.food -= o.cost; break;
                default: s.health -= o.cost; break;
            }
            s.ClampAll(c);
        }

        public static string ResourceName(ResourceKind r) => Loc.Resource(r);

        // Textos visibles de un evento menor: tabla de localización por id; si el id no está (evento creado en el
        // asset), se usa el texto escrito en el asset.
        public static string Name(MinorEventDef d) => Loc.TOr("MINOR_" + d.id.ToUpperInvariant() + "_NAME", d.name);
        public static string Description(MinorEventDef d) => Loc.TOr("MINOR_" + d.id.ToUpperInvariant() + "_DESC", d.description);
        public static string OptionText(MinorEventDef d, int i) => Loc.TOr("MINOR_" + d.id.ToUpperInvariant() + (i == 0 ? "_A" : "_B"), d.Option(i).text);

        public static MinorEventDef Find(BalanceConfig c, string id) => c.minorEvents?.FirstOrDefault(e => e.id == id);

        /// <summary>
        /// Días de eventos menores: ninguno en el ciclo 1 (tutorial), nunca el mismo día que un evento principal
        /// ni durante su aviso, con días tranquilos entre ellos. No se repite un evento hasta agotar el conjunto.
        /// </summary>
        public static List<ScheduledMinor> Generate(BalanceConfig c, IRandom rng, List<ScheduledEvent> mains)
        {
            var result = new List<ScheduledMinor>();
            var defs = c.minorEvents;
            if (defs == null || defs.Count == 0) return result;

            int total = c.TotalTurns;
            var blocked = new HashSet<int>();
            foreach (var m in mains) { blocked.Add(m.day); blocked.Add(m.day - EventScheduler.WarningLead(m.type, c)); }

            var candidates = new List<int>();
            for (int d = c.turnsPerCycle + 1; d <= total; d++) if (!blocked.Contains(d)) candidates.Add(d);
            Shuffle(candidates, rng);

            int target = Num.Round(total * c.minorEventsPerTurn);
            var days = new List<int>();
            foreach (var d in candidates)
            {
                if (days.Count >= target) break;
                if (days.Any(x => Math.Abs(x - d) < c.minorEventMinGap)) continue;
                days.Add(d);
            }
            days.Sort();

            var bag = new List<int>();
            foreach (var d in days)
            {
                if (bag.Count == 0) { bag = Enumerable.Range(0, defs.Count).ToList(); Shuffle(bag, rng); }
                int idx = bag[0]; bag.RemoveAt(0);
                result.Add(new ScheduledMinor { day = d, id = defs[idx].id });
            }
            return result;
        }

        /// <summary>Evento menor que toca hoy (o uno pospuesto).</summary>
        public static ScheduledMinor Due(GameState s) => s.minorSchedule.FirstOrDefault(m => !m.resolved && m.day <= s.day);

        /// <summary>Lo pasa al día siguiente; si ya no hay días, se descarta.</summary>
        public static void Postpone(GameState s, ScheduledMinor m)
        {
            m.day = s.day + 1;
            if (m.day > s.totalTurns) { m.resolved = true; m.skipped = true; }
        }

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
