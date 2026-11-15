using System;
using System.Collections.Generic;
using System.Linq;
using Crater.Core;
using Crater.Data;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Eventos especiales: calendario, avisos, llegada, decisiones y aplicación de efectos.
    /// Genera "EventPrompt" breves que la interfaz muestra; el juego no avanza hasta que el jugador responde.
    /// También presenta la decisión de emergencia cuando la energía o la vivienda llegan a cero.
    /// GameObject: "EventManager".
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;
        GameState S => G.State;
        BalanceConfig C => G.Config;

        readonly List<EventPrompt> pending = new List<EventPrompt>();
        public EventPrompt Current { get; private set; }
        public bool HasPending => Current != null;

        public event Action<EventPrompt> PromptChanged;
        /// <summary>Pide a la interfaz la animación de un evento (tipo, intensidad).</summary>
        public event Action<CraterEventType, int> EffectRequested;

        // ------------------------------------------------------------------ Flujo

        public void BeginDay()
        {
            pending.Clear();
            Current = null;
            if (S.emergency) pending.Add(EmergencyPrompt());
            foreach (var ev in EventScheduler.Arrivals(S).ToList()) EnqueueArrival(ev);
            foreach (var ev in EventScheduler.DueWarnings(S, C).ToList()) ProcessWarning(ev);
            ShowNext();
        }

        public void Choose(PromptOption option)
        {
            if (Current == null || option == null || !option.enabled) return;
            Current = null;
            option.onChoose?.Invoke();
            Finish();
        }

        public void ConfirmEclipse(bool[] powered)
        {
            if (Current == null || Current.kind != PromptKind.EclipseAllocation) return;
            if (!EclipseRules.Affordable(powered, S.batteries, C)) return;
            var cb = Current.onEclipseConfirm;
            Current = null;
            cb?.Invoke(powered);
            Finish();
        }

        void Finish()
        {
            G.saves.Save();
            G.NotifyChanged();
            ShowNext();
        }

        void ShowNext()
        {
            if (pending.Count > 0) { Current = pending[0]; pending.RemoveAt(0); }
            else Current = null;
            PromptChanged?.Invoke(Current);
        }

        void PushFront(params EventPrompt[] prompts) => pending.InsertRange(0, prompts.Where(p => p != null));

        // ------------------------------------------------------------------ Emergencia

        EventPrompt EmergencyPrompt()
        {
            var p = new EventPrompt
            {
                kind = PromptKind.Choice,
                title = "¡Emergencia!",
                subtitle = "Falló: " + S.emergencyReason,
                body = S.energy <= 0
                    ? "La base se quedó sin energía. Apagamos el invernadero para ahorrar."
                    : "La vivienda está muy dañada. Mantén la vivienda encendida para repararla poco a poco.",
                color = new Color(0.90f, 0.30f, 0.30f),
            };
            var battery = new PromptOption
            {
                label = "Conectar una batería",
                costs = "+" + C.emergencyBatteryEnergy + " energía. Quedará una batería menos para un eclipse.",
                onChoose = () => G.batteries.UseEmergency(),
            };
            battery.enabled = BaseActions.CanUseBattery(S, C, out battery.disabledReason);
            p.options.Add(battery);
            p.options.Add(new PromptOption
            {
                label = "Seguir sin batería",
                costs = "Apaga lo que no necesites. Los paneles producirán energía mañana.",
                onChoose = () => G.Decision("Emergencia: continuó sin usar batería"),
            });
            return p;
        }

        // ------------------------------------------------------------------ Avisos

        void ProcessWarning(ScheduledEvent ev)
        {
            ev.warningProcessed = true;
            if (S.nextEventUnwarned)
            {
                S.nextEventUnwarned = false;
                ev.warned = false;
                G.Notify("El monitoreo estuvo apagado: no sabremos qué viene.", NoticeKind.Warning);
                return;
            }
            ev.warned = true;
            EnsureIntensity(ev);

            switch (ev.type)
            {
                case CraterEventType.SolarRadiation: pending.Add(RadiationPrompt(ev, true)); break;
                case CraterEventType.Micrometeorites: pending.Add(MeteorWarningPrompt(ev)); break;
                case CraterEventType.EarthShadow: pending.Add(EclipseWarningPrompt(ev)); break;
            }
        }

        // ------------------------------------------------------------------ Llegadas

        void EnqueueArrival(ScheduledEvent ev)
        {
            if (!ev.warningProcessed)
            {
                ev.warningProcessed = true;
                ev.warned = false;
                if (S.nextEventUnwarned) S.nextEventUnwarned = false;
            }
            EnsureIntensity(ev);

            switch (ev.type)
            {
                case CraterEventType.SolarRadiation:
                    if (ev.warned && ev.choice >= 0) pending.Add(StormArrived(ev));
                    else pending.Add(RadiationPrompt(ev, false));
                    break;
                case CraterEventType.Micrometeorites:
                    pending.Add(MeteorArrivalPrompt(ev));
                    break;
                case CraterEventType.EarthShadow:
                    pending.Add(EclipsePrompt(ev));
                    break;
            }
        }

        void EnsureIntensity(ScheduledEvent ev)
        {
            if (ev.type != CraterEventType.SolarRadiation || ev.intensity > 0) return;
            var sample = G.nasa.Sample(G.Rng, C);
            ev.intensity = sample.intensity;
            ev.dataKind = sample.kind;
            ev.dataLabel = sample.label;
            ev.dataDetail = sample.detail;
        }

        EventPrompt Base(ScheduledEvent ev, PromptKind kind)
        {
            var d = G.GetEvent(ev.type);
            var p = new EventPrompt
            {
                kind = kind,
                eventType = ev.type,
                title = d.title,
                color = d.color,
                image = d.image,
            };
            // Solo la radiación usa datos de NASA. Una línea breve para no confundir archivo con datos en vivo.
            if (ev.type == CraterEventType.SolarRadiation && ev.dataKind != DataKind.Game)
            {
                p.dataKind = ev.dataKind;
                p.dataLabel = ev.dataKind == DataKind.Live
                    ? "Basada en una llamarada real detectada por NASA (datos en vivo)."
                    : "Basada en una tormenta solar real del pasado (datos archivados de NASA).";
            }
            return p;
        }

        EventPrompt Result(ScheduledEvent ev, string subtitle, string body)
        {
            var p = Base(ev, PromptKind.Info);
            p.subtitle = subtitle;
            p.body = body;
            p.dataLabel = "";
            p.options.Add(new PromptOption { label = "Volver a la base" });
            return p;
        }

        void Record(ScheduledEvent ev, string decision, string result, int damage)
        {
            ev.resolved = true;
            S.eventsLog.Add(new EventRecord
            {
                day = S.day, type = ev.type, intensity = ev.intensity, warned = ev.warned,
                decision = decision, result = result, habitatDamage = damage, dataLabel = ev.dataLabel
            });
            EffectRequested?.Invoke(ev.type, ev.intensity);
        }

        static string Strength(int intensity) => intensity >= 3 ? "muy fuerte" : (intensity == 2 ? "fuerte" : "débil");

        // ------------------------------------------------------------------ Radiación

        EventPrompt RadiationPrompt(ScheduledEvent ev, bool atWarning)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Choice);
            int days = ev.day - S.day;
            p.subtitle = atWarning
                ? "Tormenta " + Strength(ev.intensity) + ": llega en " + days + " día(s)"
                : (ev.warned ? "¡La tormenta llegó!" : "¡La tormenta llegó sin aviso!") + " Es " + Strength(ev.intensity) + ".";
            p.body = atWarning ? d.warningText : d.description + " ¿Cómo protegerás a tu tripulación?";

            foreach (RadiationDefense def in new[] { RadiationDefense.WaterWalls, RadiationDefense.RegolithIgloo, RadiationDefense.Bunker, RadiationDefense.None })
            {
                var opt = new PromptOption
                {
                    label = RadiationSystem.DefenseName(def),
                    details = OptionText(d, def),
                    costs = RadiationCosts(def, ev, atWarning),
                };
                opt.enabled = RadiationRules.CanChoose(def, S, C, atWarning, out opt.disabledReason);
                var chosen = def;
                if (atWarning)
                    opt.onChoose = () =>
                    {
                        ev.choice = (int)chosen; ev.choiceDay = S.day;
                        RadiationRules.ApplyChoiceCosts(chosen, S, C);
                        G.Decision("Preparó \"" + RadiationSystem.DefenseName(chosen) + "\" para la tormenta del día " + ev.day);
                        if (chosen == RadiationDefense.RegolithIgloo && S.roverTask == RoverTask.Regolith)
                            G.Notify("El rover empezó a cubrir la vivienda con regolito.", NoticeKind.Info);
                    };
                else
                    opt.onChoose = () => ResolveRadiation(ev, chosen);
                p.options.Add(opt);
            }
            return p;
        }

        static string OptionText(EventData d, RadiationDefense def)
        {
            int i = def == RadiationDefense.WaterWalls ? 0 : def == RadiationDefense.RegolithIgloo ? 1 : def == RadiationDefense.Bunker ? 2 : 3;
            return i < d.options.Count ? d.options[i].description : "";
        }

        /// <summary>Costos y consecuencias principales, sin fórmulas.</summary>
        string RadiationCosts(RadiationDefense def, ScheduledEvent ev, bool atWarning)
        {
            switch (def)
            {
                case RadiationDefense.WaterWalls:
                    return "Agua −" + C.waterWallWater + ". Ese día: sin comida nueva y la mitad de oxígeno." +
                           (atWarning || ev.warned ? " Protección: muy alta." : " Sin aviso protege poco.");
                case RadiationDefense.RegolithIgloo:
                    if (S.regolithReady) return "El refugio ya está listo. Protección: alta.";
                    if (atWarning) return "Energía −" + C.regolithEnergy + ". El rover trabaja " + C.regolithTurns + " días (no trae hielo). Protección: alta.";
                    return "¡No da tiempo de terminarlo! Una capa delgada puede empeorar la radiación.";
                case RadiationDefense.Bunker:
                    return "Oxígeno, comida y agua −" + C.bunkerOxygen + ". La tripulación descansa mal. Un día sin producir. Protección: media.";
                default:
                    return "No gasta nada. Protección: ninguna. La vivienda recibe todo el daño y los paneles se dañan.";
            }
        }

        EventPrompt StormArrived(ScheduledEvent ev)
        {
            var p = Base(ev, PromptKind.Info);
            p.subtitle = "¡La tormenta llegó! Es " + Strength(ev.intensity) + ".";
            p.body = "Tu protección: " + RadiationSystem.DefenseName((RadiationDefense)ev.choice) + ".";
            p.options.Add(new PromptOption { label = "Ver qué pasó", onChoose = () => ResolveRadiation(ev, (RadiationDefense)ev.choice) });
            return p;
        }

        void ResolveRadiation(ScheduledEvent ev, RadiationDefense def)
        {
            var o = G.radiation.Resolve(ev, def);
            string decision = RadiationSystem.DefenseName(def) + (ev.warned ? " (con aviso)" : " (sin aviso)");
            Record(ev, decision, o.summary, o.damage);
            G.Decision("Tormenta solar: " + decision + " → Vivienda −" + o.damage);
            string quality = o.block >= 0.7f ? "¡Muy bien protegidos!" : o.block >= 0.5f ? "Bastante protegidos." : o.block > 0f ? "Protección débil." : "Quedamos expuestos.";
            string body = "La vivienda perdió " + o.damage + " puntos.";
            if (def == RadiationDefense.WaterWalls) body += " Hoy no habrá comida nueva y el oxígeno será la mitad.";
            if (def == RadiationDefense.Bunker) body += " Hoy no se produce nada y la tripulación descansa mal.";
            if (o.block < 0f) body += " La capa de regolito era muy delgada y empeoró la radiación.";
            if (o.electronicsDamaged) body += " ¡La radiación dañó los paneles solares!";
            PushFront(Result(ev, quality, body));
        }

        // ------------------------------------------------------------------ Micrometeoritos

        EventPrompt MeteorWarningPrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Choice);
            p.subtitle = "Llegan en " + (ev.day - S.day) + " día(s)";
            p.body = d.warningText;
            var panels = new PromptOption
            {
                label = "Proteger los paneles solares",
                costs = "Ese día: Energía −" + C.stowPanelsEnergy + " y los paneles no producen. Los rovers pueden averiarse.",
            };
            panels.enabled = MeteorRules.CanProtect(MeteorProtect.Panels, S, C, out panels.disabledReason);
            panels.onChoose = () => { ev.choice = (int)MeteorProtect.Panels; ev.choiceDay = S.day; G.Decision("Protegerá los paneles de los micrometeoritos"); };
            var rovers = new PromptOption
            {
                label = "Proteger los rovers",
                costs = "Ese día: no traen hielo, Agua −" + C.stowRoversWater + " y Oxígeno −" + C.stowRoversOxygen + ". Los paneles pueden averiarse.",
                onChoose = () => { ev.choice = (int)MeteorProtect.Rovers; ev.choiceDay = S.day; G.Decision("Protegerá los rovers de los micrometeoritos"); },
            };
            p.options.Add(panels);
            p.options.Add(rovers);
            return p;
        }

        EventPrompt MeteorArrivalPrompt(ScheduledEvent ev)
        {
            var p = Base(ev, PromptKind.Info);
            var protect = ev.warned && ev.choice >= 0 ? (MeteorProtect)ev.choice : MeteorProtect.None;
            if (protect == MeteorProtect.Panels && !MeteorRules.CanProtect(protect, S, C, out _)) protect = MeteorProtect.None;
            p.subtitle = ev.warned ? "¡Impactos!" : "¡Impactos sin aviso!";
            p.body = protect == MeteorProtect.None ? "No hubo tiempo de guardar nada. Todo el equipo exterior está expuesto." : G.GetEvent(ev.type).description;
            var chosen = protect;
            p.options.Add(new PromptOption { label = "Ver qué pasó", onChoose = () => ResolveMeteors(ev, chosen) });
            return p;
        }

        void ResolveMeteors(ScheduledEvent ev, MeteorProtect protect)
        {
            var o = MeteorRules.Resolve(S, ev, protect, G.Rng, C);
            string decision = protect == MeteorProtect.Panels ? "Protegió los paneles" : protect == MeteorProtect.Rovers ? "Protegió los rovers" : "Sin protección";
            Record(ev, decision, o.summary, 0);
            var parts = new List<string>();
            if (protect == MeteorProtect.Panels) parts.Add("Los paneles están a salvo.");
            if (protect == MeteorProtect.Rovers) parts.Add("Los rovers están a salvo.");
            if (o.panelsBroke) parts.Add("¡Los paneles solares se averiaron! Ahora producen la mitad.");
            if (o.roversBroke) parts.Add("¡Los rovers se averiaron! Ahora traen la mitad de hielo.");
            if (!o.panelsBroke && !o.roversBroke) parts.Add("¡Nada se rompió! Tuvimos suerte.");
            var follow = new List<EventPrompt> { Result(ev, o.panelsBroke || o.roversBroke ? "Hubo daños" : "Sin daños", string.Join(" ", parts)) };
            if (o.panelsBroke) follow.Add(RepairPrompt(Equipment.Panels));
            if (o.roversBroke) follow.Add(RepairPrompt(Equipment.Rovers));
            PushFront(follow.ToArray());
        }

        EventPrompt RepairPrompt(Equipment eq)
        {
            var p = new EventPrompt
            {
                kind = PromptKind.Choice,
                eventType = CraterEventType.Micrometeorites,
                title = "¿Cómo reparamos " + RoverManager.Name(eq) + "?",
                subtitle = RoverManager.Name(eq, true) + " funcionan a la mitad",
                body = "Las averías no son permanentes.",
                color = new Color(0.95f, 0.6f, 0.3f),
            };
            var now = new PromptOption
            {
                label = "Reparar ahora",
                costs = "Energía −" + C.repairNowEnergy + " y Agua −" + C.repairNowWater + ". Funcionan al 100% hoy mismo.",
                onChoose = () => G.rovers.RepairNow(eq),
            };
            now.enabled = G.rovers.CanRepairNow(eq, out now.disabledReason);
            p.options.Add(now);
            p.options.Add(new PromptOption
            {
                label = "Reparar con tiempo",
                costs = "Gratis. No funcionan durante " + C.slowRepairTurns + " días y luego vuelven al 100%.",
                onChoose = () => G.rovers.RepairSlow(eq),
            });
            return p;
        }

        // ------------------------------------------------------------------ Sombra de la Tierra

        EventPrompt EclipseWarningPrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Info);
            p.subtitle = "Ocurre en " + (ev.day - S.day) + " día(s)";
            p.body = d.warningText + " Tenemos " + S.batteries + " baterías. Consejo: guarda energía.";
            p.options.Add(new PromptOption { label = "Entendido" });
            return p;
        }

        EventPrompt EclipsePrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.EclipseAllocation);
            p.subtitle = ev.warned ? "La Tierra tapa al Sol" : "La Tierra tapa al Sol ¡y no hubo aviso!";
            p.body = d.description + " Elige qué alimentar con las baterías.";
            p.onEclipseConfirm = powered =>
            {
                var notes = EclipseRules.Apply(S, powered, C);
                var off = new List<string>();
                for (int i = 0; i < 4; i++) if (!powered[i]) off.Add(EclipseRules.SectionName((EclipseSection)i));
                string decision = off.Count == 0 ? "Alimentó todas las secciones" : "Sin energía: " + string.Join(", ", off);
                Record(ev, decision, string.Join(" ", notes), 0);
                G.Decision("Sombra de la Tierra: " + decision);
                var lines = new List<string>();
                for (int i = 0; i < 4; i++)
                    if (!powered[i]) lines.Add(EclipseRules.SectionName((EclipseSection)i) + ": " + EclipseRules.Consequence((EclipseSection)i, false, C));
                PushFront(Result(ev, off.Count == 0 ? "¡Todo encendido!" : "Lo que sacrificamos", string.Join("\n", lines)));
            };
            return p;
        }
    }
}
