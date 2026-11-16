namespace Crater.Core
{
    // Textos de la simulación, recursos, edificios y reglas (inglés / español).
    public static partial class Loc
    {
        static void AddGameplay()
        {
            // ---------------- Unidades y plurales
            Add("UNIT_DAY_ONE", "1 day", "1 día");
            Add("UNIT_DAYS", "{0} days", "{0} días");

            // ---------------- Recursos
            Add("RESOURCE_ENERGY", "Energy", "Energía");
            Add("RESOURCE_OXYGEN", "Oxygen", "Oxígeno");
            Add("RESOURCE_FOOD", "Food", "Comida");
            Add("RESOURCE_HEALTH", "Health", "Salud");
            Add("RESOURCE_WATER", "Water", "Agua");
            Add("RESOURCE_HABITAT", "Habitat", "Vivienda");
            Add("RESOURCE_BATTERIES", "Batteries", "Baterías");
            Add("RESOURCE_FULL", "full", "lleno");

            // ---------------- Edificios
            Add("BUILDING_SOLAR", "Solar Panels", "Panel solar");
            Add("BUILDING_GREENHOUSE", "Greenhouse", "Invernadero");
            Add("BUILDING_OXYGEN_RECYCLER", "Air Recycler", "Reciclador de aire");
            Add("BUILDING_HABITAT", "Habitat", "Vivienda");

            Add("BUILDING_SOLAR_DESC", "Turns sunlight into electricity for the whole base.", "Convierte la luz del Sol en electricidad para toda la base.");
            Add("BUILDING_SOLAR_HOW", "If damaged, it makes less. During an eclipse it makes nothing.", "Si se daña produce menos. Durante un eclipse no produce nada.");
            Add("BUILDING_GREENHOUSE_DESC", "Grows plants to feed the crew. It also makes a little oxygen.", "Cultiva plantas para alimentar a la tripulación. También da un poco de oxígeno.");
            Add("BUILDING_GREENHOUSE_HOW", "It needs water: with little water, the plants grow less.", "Necesita agua: con poca agua las plantas crecen menos.");
            Add("BUILDING_RECYCLER_DESC", "Makes the oxygen the astronauts breathe, using water and electricity.", "Fabrica el oxígeno que respiran los astronautas, usando agua y electricidad.");
            Add("BUILDING_RECYCLER_HOW", "Without power or water, it can't make oxygen.", "Sin energía o sin agua no fabrica oxígeno.");
            Add("BUILDING_HABITAT_DESC", "The crew's home: warm air, light and beds to rest.", "El hogar de la tripulación: aire cálido, luz y camas para descansar.");
            Add("BUILDING_HABITAT_HOW", "If you turn it off, the habitat wears down and the crew rests badly.", "Si la apagas, la vivienda se deteriora y la tripulación descansa mal.");

            Add("STATE_ON", "ON", "ENCENDIDO");
            Add("STATE_OFF", "OFF", "APAGADO");
            Add("STATE_NO_POWER", "NO POWER", "SIN ENERGÍA");
            Add("STATE_NO_WATER", "NO WATER", "SIN AGUA");
            Add("STATE_PAUSED", "PAUSED", "EN PAUSA");

            // ---------------- Desglose de recursos (simulación)
            Add("CHANGE_NONE", "No change", "Sin cambios");
            Add("SRC_SOLAR", "solar panels", "paneles solares");
            Add("SRC_RECYCLER", "recycler", "reciclador");
            Add("SRC_GREENHOUSE", "greenhouse", "invernadero");
            Add("SRC_ECLIPSE_OXYGEN", "eclipse without oxygen", "eclipse sin oxígeno");
            Add("SRC_ROVER_ICE", "rover ice", "hielo de los rovers");
            Add("SRC_CREW", "crew", "tripulación");
            Add("SRC_HABITAT_SYSTEMS", "habitat systems", "sistemas de la vivienda");
            Add("SRC_REGOLITH", "regolith insulation", "aislamiento de regolito");
            Add("SRC_FATIGUE", "tiredness and stress", "fatiga y estrés");
            Add("SRC_NO_OXYGEN", "no oxygen", "sin oxígeno");
            Add("SRC_NO_FOOD", "no food", "sin comida");
            Add("SRC_HABITAT_OFF", "habitat off", "vivienda apagada");
            Add("SRC_HABITAT_BAD", "habitat in bad shape", "vivienda en mal estado");
            Add("SRC_REST", "good rest and care", "buen descanso y cuidados");

            // ---------------- Motivos de estado de los edificios
            Add("REASON_ECLIPSE", "Earth is blocking the Sun: no light", "La Tierra tapa al Sol: no hay luz");
            Add("REASON_PANELS_STOWED", "Panels stored away from the micrometeorites", "Paneles guardados por los micrometeoritos");
            Add("REASON_REPAIRING", "Being repaired", "En reparación");
            Add("REASON_DAMAGED", "Damaged: working at {0}%", "Dañados: producen al {0}%");
            Add("REASON_BUNKER", "Production paused: the crew is in the shelter", "Producción pausada: la tripulación está en el búnker");
            Add("REASON_NO_WATER", "No water: it can't make oxygen", "Sin agua: no puede fabricar oxígeno");
            Add("REASON_NO_POWER", "Not enough energy (needs {0})", "No hay energía suficiente (necesita {0})");
            Add("REASON_OVERHEAD", "Unstable grid: uses +{0}%", "Red inestable: consume +{0}%");
            Add("REASON_NO_BATTERY_OXYGEN", "No batteries for oxygen", "Sin baterías para el oxígeno");
            Add("REASON_LOW_WATER", "little water −{0}%", "poca agua −{0}%");
            Add("REASON_CROPS_DARK", "crops without light −{0}%", "cultivos sin luz −{0}%");
            Add("REASON_WATER_SHIELD", "water used as a shield", "agua usada como escudo");

            // ---------------- Notas del día
            Add("NOTE_REGOLITH_DONE", "The rover finished the regolith shelter.", "El rover terminó el refugio de regolito.");
            Add("NOTE_CREW_TIRED", "The crew is tired: the habitat was off this cycle and they couldn't rest well.", "La tripulación está cansada: la vivienda estuvo apagada este ciclo y no pudieron descansar bien.");
            Add("NOTE_PANELS_REPAIRED", "The solar panels are working at 100% again.", "Los paneles solares vuelven a funcionar al 100%.");
            Add("NOTE_ROVERS_REPAIRED", "The rovers are working at 100% again.", "Los rovers vuelven a funcionar al 100%.");

            // ---------------- Emergencia y derrota (se guardan como claves en la partida)
            Add("EMERGENCY_ENERGY_HABITAT", "energy and habitat", "energía y vivienda");
            Add("EMERGENCY_ENERGY", "energy", "energía");
            Add("EMERGENCY_HABITAT", "habitat", "vivienda");
            Add("DEFEAT_OXYGEN", "The base ran out of oxygen.", "Se acabó el oxígeno de la base.");
            Add("DEFEAT_FOOD", "The crew ran out of food.", "Se acabó la comida de la tripulación.");
            Add("DEFEAT_HABITAT", "The habitat was left in very bad shape.", "La vivienda quedó en muy mal estado.");
            Add("DEFEAT_HEALTH", "The crew's health reached zero.", "La salud de la tripulación llegó a cero.");

            // ---------------- Requisitos y motivos de "no disponible"
            Add("NEED_WATER", "You need {0} water", "Necesitas {0} de agua");
            Add("NEED_ENERGY", "You need {0} energy", "Necesitas {0} de energía");
            Add("REASON_ROVER_BUSY", "The rover is already busy", "El rover ya está ocupado");
            Add("REASON_ROVER_REPAIRING", "The rover is being repaired", "El rover está en reparación");
            Add("REASON_NO_BATTERIES", "No batteries left", "No quedan baterías");
            Add("REASON_ONLY_LOW_ENERGY", "Only when energy is low ({0} or less)", "Solo con energía baja ({0} o menos)");
            Add("REASON_NO_REPAIR_NEEDED", "No repair needed", "No necesita reparación");
            Add("REASON_MISSION_OVER", "The mission is over", "La misión terminó");
            Add("REASON_ANSWER_EVENT", "Answer the event first", "Primero responde al evento");
            Add("REASON_WAIT", "Wait...", "Espera...");
            Add("REASON_PAUSED", "Game paused", "Juego en pausa");

            // ---------------- Radiación: bloqueo y resumen
            Add("WHY_NOT_ENOUGH_WATER", "Not enough water to fill the walls", "No hay suficiente agua para llenar las paredes");
            Add("WHY_WALLS_READY", "Walls prepared after the warning", "Paredes preparadas con aviso");
            Add("WHY_NOT_IN_TIME", "They weren't ready in time", "No se prepararon a tiempo");
            Add("WHY_NO_WARNING", "No early warning", "Sin aviso anticipado");
            Add("WHY_REGOLITH_DONE", "Regolith shelter finished", "Refugio de regolito terminado");
            Add("WHY_REGOLITH_THIN", "Regolith layer unfinished: secondary radiation", "Capa de regolito incompleta: radiación secundaria");
            Add("WHY_BUNKER", "Crew inside the shelter", "Tripulación dentro del búnker");
            Add("WHY_NONE", "No protection", "Sin protección");
            Add("SUMMARY_BLOCK", "Blocked {0}%", "Bloqueo {0}%");
            Add("SUMMARY_THIN_LAYER", "The thin layer RAISED the dose by {0}%", "La capa delgada AUMENTÓ la dosis un {0}%");
            Add("SUMMARY_HABITAT_LOSS", "Habitat −{0}.", "Vivienda −{0}.");
            Add("SUMMARY_PANELS_DAMAGED", "The radiation damaged the solar panels (50%)!", "¡La radiación dañó los paneles solares (50%)!");

            // ---------------- Micrometeoritos: resumen
            Add("METEOR_PANELS_SAFE", "Panels stored safely", "Paneles guardados a salvo");
            Add("METEOR_ROVERS_SAFE", "Rovers stored safely", "Rovers guardados a salvo");
            Add("METEOR_NOTHING_PROTECTED", "There was no time to protect anything", "No hubo tiempo de proteger nada");
            Add("METEOR_PANELS_HIT", "An impact damaged the solar panels! (50%)", "¡Un impacto dañó los paneles solares! (50%)");
            Add("METEOR_PANELS_HELD", "The panels held up", "Los paneles resistieron");
            Add("METEOR_ROVERS_HIT", "An impact damaged the rovers! (50%)", "¡Un impacto dañó los rovers! (50%)");
            Add("METEOR_ROVERS_HELD", "The rovers held up", "Los rovers resistieron");

            // ---------------- Eclipse: secciones y consecuencias
            Add("SECTION_OXYGEN", "Oxygen production", "Producción de oxígeno");
            Add("SECTION_FOOD", "Food production", "Producción de alimentos");
            Add("SECTION_ENERGY", "Energy center", "Centro de energía");
            Add("SECTION_MONITORING", "Monitoring and communications", "Monitoreo y comunicaciones");
            Add("CONSEQ_OXYGEN_ON", "Oxygen production keeps going.", "La producción de oxígeno continúa.");
            Add("CONSEQ_OXYGEN_OFF", "Oxygen made = 0 and Oxygen −{0}.", "Oxígeno producido = 0 y Oxígeno −{0}.");
            Add("CONSEQ_FOOD_ON", "The harvest is saved.", "Se conserva la cosecha.");
            Add("CONSEQ_FOOD_OFF", "Food −{0}% for {1}.", "Comida −{0}% durante {1}.");
            Add("CONSEQ_ENERGY_ON", "The power grid stays stable.", "La red eléctrica sigue estable.");
            Add("CONSEQ_ENERGY_OFF", "Buildings use +{0}% energy for {1}.", "Los edificios gastan +{0}% de energía durante {1}.");
            Add("CONSEQ_MONITOR_ON", "Alerts stay active.", "Las alertas siguen activas.");
            Add("CONSEQ_MONITOR_OFF", "No DONKI warning for the next event.", "Sin aviso DONKI para el próximo evento.");
            Add("ECLIPSE_POWERED", "POWERED", "CON ENERGÍA");
            Add("ECLIPSE_UNPOWERED", "NO POWER", "SIN ENERGÍA");
        }
    }
}
