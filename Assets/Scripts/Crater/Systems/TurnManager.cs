using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Control de turnos. El juego es completamente por turnos: nada avanza hasta que el jugador
    /// presiona TERMINAR DÍA. GameObject: "TurnManager" (hijo de "CRATER").
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;

        /// <summary>La interfaz lo activa durante animaciones que no permiten decidir.</summary>
        public bool Busy { get; set; }

        /// <summary>
        /// Proyección en vivo: exactamente el mismo cálculo que TERMINAR DÍA, sobre una copia.
        /// </summary>
        public TurnReport Projection()
        {
            if (G == null || G.State == null) return null;
            return TurnSimulator.Simulate(G.State, G.Config); // barato: se recalcula siempre, nunca queda desactualizado
        }

        public bool CanEndDay(out string reason)
        {
            reason = "";
            if (G == null || !G.IsPlaying) { reason = Loc.T("REASON_MISSION_OVER"); return false; }
            if (G.events.HasPending) { reason = Loc.T("REASON_ANSWER_EVENT"); return false; }
            if (Busy) { reason = Loc.T("REASON_WAIT"); return false; }
            if (G.Paused) { reason = Loc.T("REASON_PAUSED"); return false; }
            return true;
        }

        /// <summary>Ejecuta la simulación del día (pasos 1-11 del documento).</summary>
        public void EndDay()
        {
            if (!CanEndDay(out _)) return;

            var report = TurnSimulator.Simulate(G.State, G.Config);
            G.ReplaceState(report.after);
            G.State.history.Add(G.State.Snapshot());
            G.RaiseDayEnded(report);

            foreach (var n in report.notes) G.Notify(n, NoticeKind.Info);
            if (report.energy.Wasted > 0) G.Notify(Loc.T("NOTICE_ENERGY_WASTED", report.energy.Wasted), NoticeKind.Info);

            if (G.State.outcome != Outcome.Playing)
            {
                G.FinishGame();
                return;
            }
            BeginDay(false);
        }

        /// <summary>Inicio de un día: emergencias, ciclo, eventos programados y guardado.</summary>
        public void BeginDay(bool showTutorial)
        {
            var s = G.State;
            var c = G.Config;

            // Emergencia: se apagan las funciones no esenciales; la decisión de recuperación la presenta EventManager.
            if (s.emergency && s.energy <= 0) G.buildings.ApplyEmergencyShutdown();

            if (s.DayInCycle == 1 && s.Cycle == c.moraleCycle)
                G.Notify(Loc.T("NOTICE_CREW_TIRED"), NoticeKind.Warning);

            foreach (var (text, kind) in G.resources.DayWarnings(Projection())) G.Notify(text, kind);

            G.events.BeginDay();
            G.ui.OnDayBegan(showTutorial);
            G.saves.Save();
            G.NotifyChanged();
        }
    }
}
