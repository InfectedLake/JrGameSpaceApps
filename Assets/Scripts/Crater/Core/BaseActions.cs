namespace Crater.Core
{
    /// <summary>
    /// Acciones del comandante fuera de los edificios. Todas tienen un costo [JUEGO]:
    /// ninguna decisión es completamente gratuita.
    /// </summary>
    public static class BaseActions
    {
        // ---------- Batería de emergencia ----------
        public static bool CanUseBattery(GameState s, BalanceConfig c, out string reason)
        {
            reason = "";
            if (s.batteries <= 0) { reason = "No quedan baterías"; return false; }
            if (!s.emergency && s.energy > c.alertThreshold) { reason = "Solo con energía baja (" + c.alertThreshold + " o menos)"; return false; }
            return true;
        }

        public static void UseBattery(GameState s, BalanceConfig c)
        {
            s.batteries--;
            s.energy += c.emergencyBatteryEnergy;
            s.ClampAll(c);
        }

        // ---------- Reparación de equipos ----------
        public static bool CanRepairNow(EquipmentState e, GameState s, BalanceConfig c, out string reason)
        {
            reason = "";
            if (!e.broken || e.Repairing) { reason = "No necesita reparación"; return false; }
            if (s.energy < c.repairNowEnergy) { reason = "Necesitas " + c.repairNowEnergy + " de energía"; return false; }
            if (s.water < c.repairNowWater) { reason = "Necesitas " + c.repairNowWater + " de agua"; return false; }
            return true;
        }

        public static void RepairNow(EquipmentState e, GameState s, BalanceConfig c)
        {
            s.energy -= c.repairNowEnergy;
            s.water -= c.repairNowWater;
            e.capacity = 100; e.broken = false; e.repairTurnsLeft = 0;
            s.ClampAll(c);
        }

        public static bool CanRepairSlow(EquipmentState e, out string reason)
        {
            reason = "";
            if (!e.broken || e.Repairing) { reason = "No necesita reparación"; return false; }
            return true;
        }

        /// <summary>Sin costo de recursos: el equipo queda inactivo (0%) durante 2 días y luego vuelve al 100%.</summary>
        public static void RepairSlow(EquipmentState e, BalanceConfig c) => e.repairTurnsLeft = c.slowRepairTurns;

        // ---------- Rover: refugio de regolito ----------
        public static void StartRegolith(GameState s, BalanceConfig c)
        {
            s.energy -= c.regolithEnergy;
            s.roverBusyTurns = c.regolithTurns;
            s.roverTask = RoverTask.Regolith;
            s.ClampAll(c);
        }
    }
}
