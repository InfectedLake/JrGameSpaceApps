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
            QueueMinorEvent();
            ShowNext();
        }

        // ------------------------------------------------------------------ Eventos menores

        /// <summary>
        /// Un evento menor solo aparece en un día sin ninguna otra ventana (ni principal, ni aviso, ni emergencia)
        /// y solo si al menos una opción se puede pagar. Si no, se pospone al día siguiente.
        /// </summary>
        void QueueMinorEvent()
        {
            var due = MinorEventRules.Due(S);
            if (due == null) return;
            var def = MinorEventRules.Find(C, due.id);
            if (def == null) { due.resolved = true; due.skipped = true; return; }
            if (pending.Count > 0 || !MinorEventRules.CanResolve(S, def)) { MinorEventRules.Postpone(S, due); return; }
            pending.Add(MinorPrompt(due, def));
        }

        // Atajos: texto de una clave (con parámetros fijos) evaluado al mostrarse.
        static Func<string> L(string key) => () => Loc.T(key);
        static Func<string> L(string key, params object[] args) => () => Loc.T(key, args);

        EventPrompt MinorPrompt(ScheduledMinor due, MinorEventDef def)
        {
            var p = new EventPrompt
            {
                kind = PromptKind.Choice,
                title = () => MinorEventRules.Name(def),
                body = () => MinorEventRules.Description(def),
                color = new Color(0.32f, 0.45f, 0.62f),
            };
            for (int i = 0; i < 2; i++)
            {
                var o = def.Option(i);
                int index = i;
                var opt = new PromptOption { label = () => MinorEventRules.OptionText(def, index), resource = o.resource, cost = o.cost };
                opt.enabled = MinorEventRules.CanPay(S, o);
                if (!opt.enabled) opt.disabledReason = () => Loc.T("MINOR_NOT_ENOUGH", MinorEventRules.ResourceName(o.resource).ToLowerInvariant());
                opt.onChoose = () =>
                {
                    MinorEventRules.Apply(S, o, C);
                    due.resolved = true;
                    string loss = MinorEventRules.ResourceName(o.resource) + " −" + o.cost;
                    G.Decision(MinorEventRules.Name(def) + ": " + MinorEventRules.OptionText(def, index) + " (" + loss + ")");
                    G.Notify(Loc.T("MINOR_RESOLVED", MinorEventRules.Name(def), loss), NoticeKind.Info);
                };
                p.options.Add(opt);
            }
            return p;
        }

        /// <summary>Descarta las ventanas pendientes (al salir al menú). No aplica ninguna decisión.</summary>
        public void Clear()
        {
            pending.Clear();
            if (Current == null) return;
            Current = null;
            PromptChanged?.Invoke(null);
        }

        public void Choose(PromptOption option)
        {
            if (Current == null || option == null || !option.enabled || G.Paused) return;
            Current = null;
            option.onChoose?.Invoke();
            Finish();
        }

        public void ConfirmEclipse(bool[] powered)
        {
            if (Current == null || Current.kind != PromptKind.EclipseAllocation || G.Paused) return;
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
            bool noEnergy = S.energy <= 0;
            var p = new EventPrompt
            {
                kind = PromptKind.Choice,
                title = L("EMERGENCY_TITLE"),
                subtitle = () => Loc.T("EMERGENCY_FAILED", Loc.Stored(S.emergencyReason)),
                body = noEnergy ? L("EMERGENCY_BODY_ENERGY") : L("EMERGENCY_BODY_HABITAT"),
                color = new Color(0.90f, 0.30f, 0.30f),
            };
            var battery = new PromptOption
            {
                label = L("EMERGENCY_BATTERY"),
                costs = L("EMERGENCY_BATTERY_COST", C.emergencyBatteryEnergy),
                onChoose = () => G.batteries.UseEmergency(),
            };
            battery.enabled = BaseActions.CanUseBattery(S, C, out _);
            battery.disabledReason = () => { BaseActions.CanUseBattery(S, C, out var r); return r; };
            p.options.Add(battery);
            p.options.Add(new PromptOption
            {
                label = L("EMERGENCY_CONTINUE"),
                costs = L("EMERGENCY_CONTINUE_COST"),
                onChoose = () => G.Decision(Loc.T("DECISION_EMERGENCY_NO_BATTERY")),
            });
            return p;
        }

        // ------------------------------------------------------------------ Avisos

        void ProcessWarning(ScheduledEvent ev)
        {
            ev.warningProcessed = true;
            ev.warningDay = S.day;
            if (S.nextEventUnwarned)
            {
                S.nextEventUnwarned = false;
                ev.warned = false;
                G.Notify(Loc.T("NOTICE_MONITORING_OFF"), NoticeKind.Warning);
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

            EventPrompt p = null;
            switch (ev.type)
            {
                case CraterEventType.SolarRadiation:
                    p = ev.warned && ev.choice >= 0 ? StormArrived(ev) : RadiationPrompt(ev, false);
                    break;
                case CraterEventType.Micrometeorites:
                    p = MeteorArrivalPrompt(ev);
                    break;
                case CraterEventType.EarthShadow:
                    p = EclipsePrompt(ev);
                    break;
            }
            if (p == null) return;
            p.arrival = true;   // la interfaz presenta antes la notificación y el fenómeno visual (con su música y efecto)
            pending.Add(p);
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
                intensity = ev.intensity,
                title = () => d.LTitle(),
                color = d.color,
                image = d.image,
            };
            // Solo la radiación usa datos de NASA. Una línea breve para no confundir archivo con datos en vivo.
            if (ev.type == CraterEventType.SolarRadiation && ev.dataKind != DataKind.Game)
            {
                p.dataKind = ev.dataKind;
                p.dataLabel = ev.dataKind == DataKind.Live ? L("DATA_LIVE_STORM") : L("DATA_ARCHIVE_STORM");
            }
            return p;
        }

        EventPrompt Result(ScheduledEvent ev, Func<string> subtitle, Func<string> body)
        {
            var p = Base(ev, PromptKind.Info);
            p.subtitle = subtitle;
            p.body = body;
            p.dataLabel = null;
            p.options.Add(new PromptOption { label = L("BACK_TO_BASE") });
            return p;
        }

        void Record(ScheduledEvent ev, string decision, string result, int damage)
        {
            ev.resolved = true;
            ev.resolvedDay = S.day;   // el acontecimiento sigue en fase "Resolving" el resto del día
            S.eventsLog.Add(new EventRecord
            {
                day = S.day, type = ev.type, intensity = ev.intensity, warned = ev.warned,
                decision = decision, result = result, habitatDamage = damage, dataLabel = ev.dataLabel
            });
            EffectRequested?.Invoke(ev.type, ev.intensity);
        }

        static string Strength(int intensity) => Loc.T(intensity >= 3 ? "STRENGTH_VERY_STRONG" : intensity == 2 ? "STRENGTH_STRONG" : "STRENGTH_WEAK");

        /// <summary>Texto de la alerta anticipada, p. ej. "WARNING! A strong solar storm will arrive in 2 days."</summary>
        string Alert(ScheduledEvent ev)
        {
            int days = ev.day - S.day;
            string when = days <= 1 ? Loc.T("ALERT_TOMORROW") : Loc.T("ALERT_IN_DAYS", Loc.Days(days));
            switch (ev.type)
            {
                case CraterEventType.SolarRadiation: return Loc.T("ALERT_SOLAR", Strength(ev.intensity), when);
                case CraterEventType.Micrometeorites: return Loc.T("ALERT_METEORS", when);
                default: return Loc.T("ALERT_SHADOW", when);
            }
        }

        // ------------------------------------------------------------------ Radiación

        EventPrompt RadiationPrompt(ScheduledEvent ev, bool atWarning)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Choice);
            int days = ev.day - S.day;
            if (atWarning) p.subtitle = () => Alert(ev);
            else p.subtitle = () => Loc.T(ev.warned ? "STORM_ARRIVED" : "STORM_ARRIVED_UNWARNED", Strength(ev.intensity));
            p.body = atWarning ? (Func<string>)(() => d.LWarning()) : () => d.LDescription() + " " + Loc.T("STORM_HOW_PROTECT");

            foreach (RadiationDefense def in new[] { RadiationDefense.WaterWalls, RadiationDefense.RegolithIgloo, RadiationDefense.Bunker, RadiationDefense.None })
            {
                var chosen = def;
                var opt = new PromptOption
                {
                    label = () => RadiationSystem.DefenseName(chosen),
                    details = () => OptionText(d, chosen),
                    costs = () => RadiationCosts(chosen, ev, atWarning),
                };
                opt.enabled = RadiationRules.CanChoose(def, S, C, atWarning, out _);
                opt.disabledReason = () => { RadiationRules.CanChoose(chosen, S, C, atWarning, out var r); return r; };
                if (atWarning)
                    opt.onChoose = () =>
                    {
                        ev.choice = (int)chosen; ev.choiceDay = S.day;
                        RadiationRules.ApplyChoiceCosts(chosen, S, C);
                        G.Decision(Loc.T("DECISION_PREPARED", RadiationSystem.DefenseName(chosen), ev.day));
                        if (chosen == RadiationDefense.RegolithIgloo && S.roverTask == RoverTask.Regolith)
                            G.Notify(Loc.T("NOTICE_ROVER_REGOLITH"), NoticeKind.Info);
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
            return d.LOptionDesc(i);
        }

        /// <summary>Costos y consecuencias principales, sin fórmulas.</summary>
        string RadiationCosts(RadiationDefense def, ScheduledEvent ev, bool atWarning)
        {
            switch (def)
            {
                case RadiationDefense.WaterWalls:
                    return Loc.T("COST_WATER_WALLS", C.waterWallWater) + " " + Loc.T(atWarning || ev.warned ? "COST_WATER_WALLS_GOOD" : "COST_WATER_WALLS_WEAK");
                case RadiationDefense.RegolithIgloo:
                    if (S.regolithReady) return Loc.T("COST_REGOLITH_READY");
                    if (atWarning) return Loc.T("COST_REGOLITH", C.regolithEnergy, Loc.Days(C.regolithTurns));
                    return Loc.T("COST_REGOLITH_LATE");
                case RadiationDefense.Bunker:
                    return Loc.T("COST_BUNKER", C.bunkerOxygen);
                default:
                    return Loc.T("COST_NO_PROTECTION");
            }
        }

        EventPrompt StormArrived(ScheduledEvent ev)
        {
            var p = Base(ev, PromptKind.Info);
            p.subtitle = () => Loc.T("STORM_ARRIVED", Strength(ev.intensity));
            p.body = () => Loc.T("STORM_YOUR_PROTECTION", RadiationSystem.DefenseName((RadiationDefense)ev.choice));
            p.options.Add(new PromptOption { label = L("SEE_WHAT_HAPPENED"), onChoose = () => ResolveRadiation(ev, (RadiationDefense)ev.choice) });
            return p;
        }

        void ResolveRadiation(ScheduledEvent ev, RadiationDefense def)
        {
            var o = G.radiation.Resolve(ev, def);
            string decision = RadiationSystem.DefenseName(def) + " " + Loc.T(ev.warned ? "DECISION_WITH_WARNING" : "DECISION_WITHOUT_WARNING");
            Record(ev, decision, o.summary, o.damage);
            G.Decision(Loc.T("DECISION_STORM", decision, o.damage));
            Func<string> quality = L(o.block >= 0.7f ? "RESULT_VERY_PROTECTED" : o.block >= 0.5f ? "RESULT_PROTECTED" : o.block > 0f ? "RESULT_WEAK" : "RESULT_EXPOSED");
            Func<string> body = () =>
            {
                string b = Loc.T("RESULT_HABITAT_LOST", o.damage);
                if (def == RadiationDefense.WaterWalls) b += " " + Loc.T("RESULT_WATER_WALLS_DAY");
                if (def == RadiationDefense.Bunker) b += " " + Loc.T("RESULT_BUNKER_DAY");
                if (o.block < 0f) b += " " + Loc.T("RESULT_THIN_LAYER");
                if (o.electronicsDamaged) b += " " + Loc.T("RESULT_PANELS_DAMAGED");
                return b;
            };
            PushFront(Result(ev, quality, body));
        }

        // ------------------------------------------------------------------ Micrometeoritos

        EventPrompt MeteorWarningPrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Choice);
            p.subtitle = () => Alert(ev);
            p.body = () => d.LWarning();
            var panels = new PromptOption
            {
                label = () => d.LOptionTitle(0),
                costs = L("COST_STOW_PANELS", C.stowPanelsEnergy),
            };
            panels.enabled = MeteorRules.CanProtect(MeteorProtect.Panels, S, C, out _);
            panels.disabledReason = () => { MeteorRules.CanProtect(MeteorProtect.Panels, S, C, out var r); return r; };
            panels.onChoose = () => { ev.choice = (int)MeteorProtect.Panels; ev.choiceDay = S.day; G.Decision(Loc.T("DECISION_WILL_PROTECT_PANELS")); };
            var rovers = new PromptOption
            {
                label = () => d.LOptionTitle(1),
                costs = L("COST_STOW_ROVERS", C.stowRoversWater, C.stowRoversOxygen),
                onChoose = () => { ev.choice = (int)MeteorProtect.Rovers; ev.choiceDay = S.day; G.Decision(Loc.T("DECISION_WILL_PROTECT_ROVERS")); },
            };
            p.options.Add(panels);
            p.options.Add(rovers);
            return p;
        }

        EventPrompt MeteorArrivalPrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Info);
            var protect = ev.warned && ev.choice >= 0 ? (MeteorProtect)ev.choice : MeteorProtect.None;
            if (protect == MeteorProtect.Panels && !MeteorRules.CanProtect(protect, S, C, out _)) protect = MeteorProtect.None;
            p.subtitle = L(ev.warned ? "IMPACTS" : "IMPACTS_UNWARNED");
            p.body = protect == MeteorProtect.None ? L("IMPACTS_NOTHING_STORED") : (Func<string>)(() => d.LDescription());
            var chosen = protect;
            p.options.Add(new PromptOption { label = L("SEE_WHAT_HAPPENED"), onChoose = () => ResolveMeteors(ev, chosen) });
            return p;
        }

        void ResolveMeteors(ScheduledEvent ev, MeteorProtect protect)
        {
            var o = MeteorRules.Resolve(S, ev, protect, G.Rng, C);
            string decision = Loc.T(protect == MeteorProtect.Panels ? "DECISION_PROTECTED_PANELS" : protect == MeteorProtect.Rovers ? "DECISION_PROTECTED_ROVERS" : "WHY_NONE");
            Record(ev, decision, o.summary, 0);
            Func<string> body = () =>
            {
                var parts = new List<string>();
                if (protect == MeteorProtect.Panels) parts.Add(Loc.T("RESULT_PANELS_SAFE"));
                if (protect == MeteorProtect.Rovers) parts.Add(Loc.T("RESULT_ROVERS_SAFE"));
                if (o.panelsBroke) parts.Add(Loc.T("RESULT_PANELS_BROKE"));
                if (o.roversBroke) parts.Add(Loc.T("RESULT_ROVERS_BROKE"));
                if (!o.panelsBroke && !o.roversBroke) parts.Add(Loc.T("RESULT_NOTHING_BROKE"));
                return string.Join(" ", parts);
            };
            var follow = new List<EventPrompt> { Result(ev, L(o.panelsBroke || o.roversBroke ? "RESULT_DAMAGE" : "RESULT_NO_DAMAGE"), body) };
            if (o.panelsBroke) follow.Add(RepairPrompt(Equipment.Panels));
            if (o.roversBroke) follow.Add(RepairPrompt(Equipment.Rovers));
            PushFront(follow.ToArray());
        }

        EventPrompt RepairPrompt(Equipment eq)
        {
            bool panels = eq == Equipment.Panels;
            var p = new EventPrompt
            {
                kind = PromptKind.Choice,
                eventType = CraterEventType.Micrometeorites,
                title = L(panels ? "REPAIR_TITLE_PANELS" : "REPAIR_TITLE_ROVERS"),
                subtitle = L(panels ? "REPAIR_SUBTITLE_PANELS" : "REPAIR_SUBTITLE_ROVERS"),
                body = L("REPAIR_BODY"),
                color = new Color(0.95f, 0.6f, 0.3f),
            };
            var now = new PromptOption
            {
                label = L("REPAIR_NOW"),
                costs = L("REPAIR_NOW_COST", C.repairNowEnergy, C.repairNowWater),
                onChoose = () => G.rovers.RepairNow(eq),
            };
            now.enabled = G.rovers.CanRepairNow(eq, out _);
            now.disabledReason = () => { G.rovers.CanRepairNow(eq, out var r); return r; };
            p.options.Add(now);
            p.options.Add(new PromptOption
            {
                label = L("REPAIR_SLOW"),
                costs = () => Loc.T("REPAIR_SLOW_COST", Loc.Days(C.slowRepairTurns)),
                onChoose = () => G.rovers.RepairSlow(eq),
            });
            return p;
        }

        // ------------------------------------------------------------------ Sombra de la Tierra

        EventPrompt EclipseWarningPrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.Info);
            p.subtitle = () => Alert(ev);
            int batteries = S.batteries;
            p.body = () => d.LWarning() + " " + Loc.T("SHADOW_WARNING_BATTERIES", batteries);
            p.options.Add(new PromptOption { label = L("GOT_IT") });
            return p;
        }

        EventPrompt EclipsePrompt(ScheduledEvent ev)
        {
            var d = G.GetEvent(ev.type);
            var p = Base(ev, PromptKind.EclipseAllocation);
            p.subtitle = L(ev.warned ? "SHADOW_ARRIVED" : "SHADOW_ARRIVED_UNWARNED");
            p.body = () => d.LDescription() + " " + Loc.T("SHADOW_CHOOSE");
            p.onEclipseConfirm = powered =>
            {
                var notes = EclipseRules.Apply(S, powered, C);
                var off = new List<string>();
                for (int i = 0; i < 4; i++) if (!powered[i]) off.Add(EclipseRules.SectionName((EclipseSection)i));
                string decision = off.Count == 0 ? Loc.T("DECISION_ALL_POWERED") : Loc.T("DECISION_UNPOWERED", string.Join(", ", off));
                Record(ev, decision, string.Join(" ", notes), 0);
                G.Decision(Loc.T("DECISION_SHADOW", decision));
                var choice = (bool[])powered.Clone();
                Func<string> lines = () =>
                {
                    var l = new List<string>();
                    for (int i = 0; i < 4; i++)
                        if (!choice[i]) l.Add(EclipseRules.SectionName((EclipseSection)i) + ": " + EclipseRules.Consequence((EclipseSection)i, false, C));
                    return string.Join("\n", l);
                };
                PushFront(Result(ev, L(off.Count == 0 ? "RESULT_ALL_ON" : "RESULT_SACRIFICED"), lines));
            };
            return p;
        }
    }
}
