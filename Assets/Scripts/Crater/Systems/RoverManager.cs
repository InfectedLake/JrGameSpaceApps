using Crater.Core;
using UnityEngine;

namespace Crater.Systems
{
    /// <summary>
    /// Rovers: capacidad, recolección de hielo, construcción del refugio de regolito,
    /// averías y reparaciones (también de los paneles, que son equipo exterior). GameObject: "RoverManager".
    /// </summary>
    public class RoverManager : MonoBehaviour
    {
        GameManager G => GameManager.Instance;
        GameState S => G.State;

        public bool CanRepairNow(Equipment eq, out string reason) => BaseActions.CanRepairNow(S.GetEquipment(eq), S, G.Config, out reason);
        public bool CanRepairSlow(Equipment eq, out string reason) => BaseActions.CanRepairSlow(S.GetEquipment(eq), out reason);

        public void RepairNow(Equipment eq)
        {
            if (!CanRepairNow(eq, out _)) return;
            BaseActions.RepairNow(S.GetEquipment(eq), S, G.Config);
            G.Decision(Loc.T("DECISION_REPAIR_NOW", Name(eq), G.Config.repairNowEnergy, G.Config.repairNowWater));
            G.Notify(Loc.T(eq == Equipment.Panels ? "NOTICE_PANELS_REPAIRED" : "NOTICE_ROVERS_REPAIRED"), NoticeKind.Good);
            G.NotifyChanged();
        }

        public void RepairSlow(Equipment eq)
        {
            if (!CanRepairSlow(eq, out _)) return;
            BaseActions.RepairSlow(S.GetEquipment(eq), G.Config);
            G.Decision(Loc.T("DECISION_REPAIR_SLOW", Name(eq), Loc.Days(G.Config.slowRepairTurns)));
            G.Notify(Loc.T(eq == Equipment.Panels ? "NOTICE_PANELS_WORKSHOP" : "NOTICE_ROVERS_WORKSHOP", Loc.Days(G.Config.slowRepairTurns)), NoticeKind.Info);
            G.NotifyChanged();
        }

        public static string Name(Equipment eq, bool capital = false)
        {
            string n = Loc.T(eq == Equipment.Panels ? "EQUIP_THE_PANELS" : "EQUIP_THE_ROVERS");
            return capital ? char.ToUpper(n[0]) + n.Substring(1) : n;
        }
    }
}
