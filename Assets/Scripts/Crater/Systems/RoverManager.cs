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

        public bool IsCollecting => S != null && !S.today.roversStowed && S.roverBusyTurns == 0 && S.rovers.Effective > 0;
        public bool IsBuilding => S != null && S.roverBusyTurns > 0 && S.roverTask == RoverTask.Regolith;
        public int ExpectedIce => IsCollecting ? Num.Round(G.Config.roverIcePerTurn * S.rovers.Effective / 100.0) : 0;

        public string StatusText()
        {
            if (S == null) return "";
            if (S.today.roversStowed) return "Guardados en el hangar (hoy no recogen hielo)";
            if (S.rovers.Repairing) return "En reparación: " + S.rovers.repairTurnsLeft + " día(s)";
            if (IsBuilding) return "Construyendo el refugio de regolito: " + S.roverBusyTurns + " día(s)";
            if (S.rovers.broken) return "Averiados: trabajan al 50% (+" + ExpectedIce + " agua/día)";
            return "Recolectando hielo (+" + ExpectedIce + " agua/día)";
        }

        public static string EquipmentStatus(EquipmentState e, string normal)
        {
            if (e.Repairing) return "En reparación (" + e.repairTurnsLeft + " día(s)) - 0%";
            if (e.broken) return "Averiado - " + e.capacity + "%";
            return normal + " - " + e.capacity + "%";
        }

        public bool CanRepairNow(Equipment eq, out string reason) => BaseActions.CanRepairNow(S.GetEquipment(eq), S, G.Config, out reason);
        public bool CanRepairSlow(Equipment eq, out string reason) => BaseActions.CanRepairSlow(S.GetEquipment(eq), out reason);

        public void RepairNow(Equipment eq)
        {
            if (!CanRepairNow(eq, out _)) return;
            BaseActions.RepairNow(S.GetEquipment(eq), S, G.Config);
            G.Decision("Reparación inmediata de " + Name(eq) + " (Energía −" + G.Config.repairNowEnergy + ", Agua −" + G.Config.repairNowWater + ")");
            G.Notify(Name(eq, true) + " reparados: vuelven al 100%.", NoticeKind.Good);
            G.NotifyChanged();
        }

        public void RepairSlow(Equipment eq)
        {
            if (!CanRepairSlow(eq, out _)) return;
            BaseActions.RepairSlow(S.GetEquipment(eq), G.Config);
            G.Decision("Reparación con tiempo de " + Name(eq) + " (inactivos " + G.Config.slowRepairTurns + " días)");
            G.Notify(Name(eq, true) + " quedan en el taller " + G.Config.slowRepairTurns + " días.", NoticeKind.Info);
            G.NotifyChanged();
        }

        public static string Name(Equipment eq, bool capital = false)
        {
            string n = eq == Equipment.Panels ? "los paneles solares" : "los rovers";
            return capital ? char.ToUpper(n[0]) + n.Substring(1) : n;
        }
    }
}
