// CRATER - Tipos base de la simulación.
// Esta carpeta (Core) NO depende de UnityEngine: contiene solo reglas puras,
// lo que permite probarlas fuera de Unity y garantiza que la proyección y el
// cálculo definitivo usan exactamente el mismo código.
using System;

namespace Crater.Core
{
    public enum BuildingId { Solar = 0, Greenhouse = 1, Recycler = 2, Habitat = 3 }

    /// <summary>Estado visual calculado para un edificio en la proyección del día.</summary>
    public enum BuildingVisual
    {
        Off,        // apagado por el jugador
        Working,    // encendido y funcionando
        NoPower,    // encendido pero sin energía suficiente (rojo)
        NoWater,    // encendido pero sin agua (reciclador)
        Paused,     // producción pausada por un evento / decisión
        Broken,     // funcionando con avería (50%)
        Inactive    // encendido pero inactivo (eclipse, guardado, reparación lenta)
    }

    public enum CraterEventType { SolarRadiation = 0, Micrometeorites = 1, EarthShadow = 2 }

    /// <summary>
    /// Ciclo de vida de un acontecimiento principal (lo calcula EventScheduler.Phase a partir del estado guardado).
    /// Scheduled → Warning (día de la alerta) → Preparation (días hasta el evento) → Active (día del evento,
    /// hasta resolverlo) → Resolving (resto de ese día y mientras duren sus efectos de varios turnos) → Resolved.
    /// </summary>
    public enum EventPhase { Scheduled, Warning, Preparation, Active, Resolving, Resolved }

    public enum RadiationDefense { None = 0, WaterWalls = 1, RegolithIgloo = 2, Bunker = 3 }

    public enum MeteorProtect { None = -1, Panels = 0, Rovers = 1 }

    public enum EclipseSection { Oxygen = 0, Food = 1, EnergyCenter = 2, Monitoring = 3 }

    public enum Outcome { Playing, Victory, Defeat }

    /// <summary>Origen de los datos que generan un evento.</summary>
    public enum DataKind { Game = 0, Live = 1, Archive = 2 }

    /// <summary>Etiqueta de veracidad obligatoria para todo dato mostrado.</summary>
    public enum DataTag { Real, Juego, Pendiente }

    public enum RoverTask { None = 0, Regolith = 1 }

    public enum Equipment { Panels = 0, Rovers = 1 }

    [Serializable]
    public class EquipmentState
    {
        /// <summary>100 = normal, 50 = averiado.</summary>
        public int capacity = 100;
        /// <summary>Averiado y esperando una decisión de reparación.</summary>
        public bool broken;
        /// <summary>Reparación con tiempo: mientras sea &gt; 0 el equipo está inactivo (0%).</summary>
        public int repairTurnsLeft;

        public int Effective => repairTurnsLeft > 0 ? 0 : capacity;
        public bool Repairing => repairTurnsLeft > 0;

        public EquipmentState Clone() => (EquipmentState)MemberwiseClone();
    }

    /// <summary>Efectos temporales que solo duran el día actual. Se reinician al terminar el día.</summary>
    [Serializable]
    public class DayModifiers
    {
        public bool panelsStowed;           // micrometeoritos: paneles guardados
        public bool roversStowed;           // micrometeoritos: rovers guardados
        public bool foodPaused;             // paredes de agua: sin producción de comida
        public bool productionPaused;       // búnker: producción pausada
        public bool eclipse;                // sombra de la Tierra: paneles a 0
        public bool eclipseOxygenUnpowered; // eclipse: sección de oxígeno sin baterías
        public float oxygenFactor = 1f;     // paredes de agua: oxígeno al 50%
        // Solo para la presentación visual (no cambian reglas):
        public int stormDefense = -1;       // defensa usada hoy contra la tormenta (RadiationDefense)
        public bool eclipsePowerOxygen, eclipsePowerFood, eclipsePowerEnergy, eclipsePowerMonitoring;

        public DayModifiers Clone() => (DayModifiers)MemberwiseClone();
    }

    [Serializable]
    public class ScheduledEvent
    {
        public CraterEventType type;
        public int day;
        public int intensity;                 // 1..3 (solo radiación)
        public bool warningProcessed;
        public bool warned;                   // el jugador recibió aviso anticipado
        public int choice = -1;               // decisión tomada durante el aviso
        public int choiceDay = -1;
        public bool resolved;
        public int warningDay = -1;           // día en que se mostró la alerta anticipada
        public int resolvedDay = -1;          // día en que el jugador resolvió el acontecimiento
        public DataKind dataKind;
        public string dataLabel = "";
        public string dataDetail = "";

        /// <summary>Identificador único del acontecimiento (un evento por ciclo: tipo + día no se repiten).</summary>
        public string Uid => (int)type + "@" + day;
    }

    [Serializable]
    public class EventRecord
    {
        public int day;
        public CraterEventType type;
        public int intensity;
        public bool warned;
        public string decision = "";
        public string result = "";
        public int habitatDamage;
        public string dataLabel = "";
    }

    [Serializable]
    public class DaySnapshot
    {
        public int day, energy, oxygen, food, health, water, habitat;
    }

    public static class Num
    {
        /// <summary>Redondeo estándar del juego (0.5 se aleja de cero).</summary>
        public static int Round(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);
        public static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
        public static string Signed(int v) => v > 0 ? "+" + v : (v < 0 ? "−" + (-v) : "0");
    }
}
