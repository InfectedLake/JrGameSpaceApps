using System;
using System.Collections.Generic;

namespace Crater.Core
{
    /// <summary>
    /// Estado completo de la partida. Serializable con JsonUtility (solo campos públicos).
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Calendario
        public int day = 1;
        public int totalTurns = 24;
        public int turnsPerCycle = 4;

        // Recursos principales (0..100)
        public int energy, oxygen, food, health;
        public int astronauts = 3;

        // Sistemas auxiliares
        public int water;      // 0..100
        public int habitat;    // vivienda e integridad 0..100
        public int batteries;  // 0..4
        public EquipmentState panels = new EquipmentState();
        public EquipmentState rovers = new EquipmentState();
        public float radiationDose;   // mSv acumulados (fondo [REAL] + tormentas [JUEGO])
        public float stormDose;       // parte de la dosis atribuida a tormentas [JUEGO]

        // Edificios (índice = BuildingId)
        public bool[] buildingOn = new bool[4];

        // Efectos temporales y contadores
        public DayModifiers today = new DayModifiers();
        public int foodPenaltyTurns;
        public int energyOverheadTurns;
        public int roverBusyTurns;
        public RoverTask roverTask;
        public bool regolithReady;
        public bool nextEventUnwarned;
        public int habitatDownCycle = -1;   // último ciclo en que la vivienda estuvo apagada o sin energía
        public int eclipseCount;

        // Emergencia y resultado
        public bool emergency;
        public string emergencyReason = "";
        public Outcome outcome = Outcome.Playing;
        public string defeatCause = "";

        // Estadísticas
        public int totalHabitatDamage;
        public int breakdowns;

        // Aleatoriedad reproducible (para guardar/cargar)
        public int rngSeed;
        public int rngDraws;

        // Identidad de la misión
        public string baseName = "Base Shackleton";
        public List<string> astronautNames = new List<string>();

        // Registro
        public List<ScheduledEvent> schedule = new List<ScheduledEvent>();
        public List<ScheduledMinor> minorSchedule = new List<ScheduledMinor>();
        public List<EventRecord> eventsLog = new List<EventRecord>();
        public List<string> decisions = new List<string>();
        public List<DaySnapshot> history = new List<DaySnapshot>();

        public int Cycle => Math.Min((day - 1) / Math.Max(1, turnsPerCycle) + 1, TotalCycles);
        public int TotalCycles => Math.Max(1, totalTurns / Math.Max(1, turnsPerCycle));
        public int DayInCycle => (day - 1) % Math.Max(1, turnsPerCycle) + 1;
        public bool IsLastDayOfCycle => DayInCycle == turnsPerCycle;
        public bool IsOn(BuildingId id) => buildingOn[(int)id];
        public EquipmentState GetEquipment(Equipment e) => e == Equipment.Panels ? panels : rovers;

        /// <summary>
        /// Copia para simular. Los campos de valor y los objetos mutables de la simulación se copian;
        /// las listas de registro se comparten porque la simulación nunca las modifica.
        /// </summary>
        public GameState Clone()
        {
            var c = (GameState)MemberwiseClone();
            c.buildingOn = (bool[])buildingOn.Clone();
            c.panels = panels.Clone();
            c.rovers = rovers.Clone();
            c.today = today.Clone();
            return c;
        }

        public static GameState CreateNew(BalanceConfig c, int seed)
        {
            var s = new GameState
            {
                day = 1,
                totalTurns = c.TotalTurns,
                turnsPerCycle = c.turnsPerCycle,
                energy = c.startEnergy,
                oxygen = c.startOxygen,
                food = c.startFood,
                health = c.startHealth,
                astronauts = c.astronauts,
                water = c.startWater,
                habitat = c.startHabitat,
                batteries = c.startBatteries,
                rngSeed = seed,
            };
            for (int i = 0; i < 4; i++) s.buildingOn[i] = c.Get((BuildingId)i).startsOn;
            return s;
        }

        public void ClampAll(BalanceConfig c)
        {
            energy = Num.Clamp(energy, 0, c.resourceMax);
            oxygen = Num.Clamp(oxygen, 0, c.resourceMax);
            food = Num.Clamp(food, 0, c.resourceMax);
            health = Num.Clamp(health, 0, c.resourceMax);
            water = Num.Clamp(water, 0, c.resourceMax);
            habitat = Num.Clamp(habitat, 0, c.resourceMax);
            batteries = Num.Clamp(batteries, 0, c.maxBatteries);
        }

        public DaySnapshot Snapshot() => new DaySnapshot
        {
            day = day, energy = energy, oxygen = oxygen, food = food, health = health, water = water, habitat = habitat
        };
    }
}
