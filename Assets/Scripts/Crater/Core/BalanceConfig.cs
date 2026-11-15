using System;

namespace Crater.Core
{
    /// <summary>
    /// Producción y consumo por día de un edificio [JUEGO].
    /// Positivo = produce, negativo = consume.
    /// </summary>
    [Serializable]
    public class BuildingStats
    {
        public BuildingId id;
        public string name = "";
        public int energy;
        public int oxygen;
        public int food;
        public int water;          // consumo de agua (negativo)
        public int priority;       // menor = recibe energía primero
        public bool startsOn;
        public bool essential = true;

        public BuildingStats() { }

        public BuildingStats(BuildingId id, string name, int energy, int oxygen, int food, int water, int priority, bool startsOn, bool essential)
        {
            this.id = id; this.name = name; this.energy = energy; this.oxygen = oxygen; this.food = food;
            this.water = water; this.priority = priority; this.startsOn = startsOn; this.essential = essential;
        }

        public BuildingStats Clone() => (BuildingStats)MemberwiseClone();
    }

    /// <summary>
    /// Todos los parámetros de balance. Ningún valor aquí es una cifra científica,
    /// salvo los marcados explícitamente como [REAL].
    /// </summary>
    [Serializable]
    public class BalanceConfig
    {
        // --- Valores iniciales (documento de diseño) ---
        public int startEnergy = 100;
        public int startOxygen = 70;
        public int startFood = 60;
        public int startHealth = 100;
        public int astronauts = 3;
        public int startWater = 50;        // [JUEGO]
        public int startHabitat = 70;      // [JUEGO]
        public int startBatteries = 4;
        public int maxBatteries = 4;

        public int resourceMax = 100;
        public int alertThreshold = 25;

        // --- Tripulación ---
        public int oxygenPerAstronaut = 1;
        public int foodPerAstronaut = 1;
        public int starvingHealthLoss = 20;

        // --- Edificios (orden = BuildingId) ---
        public BuildingStats[] buildings = DefaultBuildings();

        // --- Agua [JUEGO] ---
        public int lowWaterThreshold = 20;
        public float lowWaterFoodPenalty = 0.30f;
        public int roverIcePerTurn = 6;

        // --- Salud y vivienda [JUEGO] ---
        public int lowHabitatThreshold = 25;
        public int lowHabitatHealthLoss = 5;
        public int healthRecovery = 5;
        public int healthRecoveryHabitatMin = 50;

        // --- Calendario ---
        public int cycles = 6;
        public int turnsPerCycle = 4;
        public int moraleCycle = 4;
        public int moraleHabitatLoss = 10;

        // --- Vivienda (edificio) [JUEGO] ---
        public int habitatUpkeepGain = 2;      // encendida: la integridad mejora un poco cada día
        public int habitatOffLoss = 5;         // apagada o sin energía: la integridad baja

        // --- Baterías [JUEGO] ---
        public int emergencyBatteryEnergy = 20;
        public int batteryRechargePerTurn = 1;

        // --- Radiación ---
        public int radiationDamagePerIntensity = 12;
        public float waterBlockWarned = 0.80f;
        public float waterBlockUnwarned = 0.20f;
        public float regolithBlock = 0.70f;
        public float regolithThinPenalty = 0.15f;   // [JUEGO] radiación secundaria simplificada
        public float bunkerBlock = 0.50f;
        public int waterWallWater = 30;
        public float waterWallOxygenFactor = 0.5f;
        public int regolithEnergy = 25;
        public int regolithTurns = 2;
        public int regolithHabitatBonus = 10;
        public int bunkerHabitat = 15;
        public int bunkerOxygen = 8;
        public int bunkerWater = 8;
        public int bunkerFood = 8;
        public int radiationWarningLead = 2;
        public float[] intensityChances = { 0.50f, 0.35f, 0.15f };
        /// <summary>[REAL] Promedio medido por LND en la superficie lunar: ~1369 µSv/día = 1.369 mSv.</summary>
        public float backgroundDosePerTurn = 1.369f;
        /// <summary>[JUEGO] Dosis extra (mSv) por punto de intensidad sin protección.</summary>
        public float stormDosePerIntensity = 10f;

        // --- Micrometeoritos [JUEGO] ---
        public float meteorBreakChance = 0.30f;
        public int meteorWarningLead = 1;
        public int stowPanelsEnergy = 20;
        public int stowRoversWater = 15;
        public int stowRoversOxygen = 10;
        public int repairNowEnergy = 20;
        public int repairNowWater = 10;
        public int slowRepairTurns = 2;
        public int brokenCapacity = 50;

        // --- Sombra de la Tierra ---
        public int eclipseWarningLead = 1;
        public int[] eclipseSectionCost = { 2, 1, 1, 1 };
        public int eclipseOxygenLoss = 25;
        public float eclipseFoodPenalty = 0.30f;
        public int eclipseFoodPenaltyTurns = 2;
        public float eclipseEnergyOverhead = 0.20f;
        public int eclipseEnergyOverheadTurns = 2;
        public int maxEclipses = 2;

        // --- Eventos extra ---
        public float extraRadiationChance = 0.40f;
        public float extraMeteorChance = 0.35f;
        public float extraEclipseChance = 0.25f;

        public int TotalTurns => cycles * turnsPerCycle;

        public BuildingStats Get(BuildingId id)
        {
            foreach (var b in buildings) if (b != null && b.id == id) return b;
            return DefaultBuildings()[(int)id];
        }

        public static BuildingStats[] DefaultBuildings() => new[]
        {
            new BuildingStats(BuildingId.Solar,      "Planta de energía",  25,  0, 0,  0, 0, true,  true),
            new BuildingStats(BuildingId.Greenhouse, "Invernadero",       -10,  4, 8, -2, 2, true,  false),
            new BuildingStats(BuildingId.Recycler,   "Planta de oxígeno", -20, 10, 0, -4, 1, true,  true),
            new BuildingStats(BuildingId.Habitat,    "Vivienda",           -5,  0, 0,  0, 3, true,  true),
        };

        public BalanceConfig Clone()
        {
            var c = (BalanceConfig)MemberwiseClone();
            c.buildings = new BuildingStats[buildings.Length];
            for (int i = 0; i < buildings.Length; i++) c.buildings[i] = buildings[i].Clone();
            c.intensityChances = (float[])intensityChances.Clone();
            c.eclipseSectionCost = (int[])eclipseSectionCost.Clone();
            return c;
        }
    }
}
