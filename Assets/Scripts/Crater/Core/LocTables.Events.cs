namespace Crater.Core
{
    // Ventanas de acontecimientos: alertas, decisiones, costos, resultados, reparaciones y emergencia.
    public static partial class Loc
    {
        static void AddEvents()
        {
            // ---------------- Alerta anticipada
            Add("STRENGTH_WEAK", "weak", "débil");
            Add("STRENGTH_STRONG", "strong", "fuerte");
            Add("STRENGTH_VERY_STRONG", "very strong", "muy fuerte");
            Add("ALERT_TOMORROW", "will arrive tomorrow.", "llegará mañana.");
            Add("ALERT_IN_DAYS", "will arrive in {0}.", "llegará en {0}.");
            Add("ALERT_SOLAR", "WARNING! A {0} solar storm {1}", "¡ALERTA! Una tormenta solar {0} {1}");
            Add("ALERT_METEORS", "WARNING! A micrometeorite shower is approaching: it {0}", "¡ALERTA! Se aproxima una lluvia de micrometeoritos: {0}");
            Add("ALERT_SHADOW", "WARNING! Earth's Shadow {0}", "¡ALERTA! La Sombra de la Tierra {0}");
            Add("NOTICE_MONITORING_OFF", "Monitoring was off: we won't know what's coming.", "El monitoreo estuvo apagado: no sabremos qué viene.");
            Add("DATA_LIVE_STORM", "Based on a real flare detected by NASA (live data).", "Basada en una llamarada real detectada por NASA (datos en vivo).");
            Add("DATA_ARCHIVE_STORM", "Based on a real solar storm from the past (archived NASA data).", "Basada en una tormenta solar real del pasado (datos archivados de NASA).");

            // ---------------- Botones comunes
            Add("BACK_TO_BASE", "Back to the base", "Volver a la base");
            Add("SEE_WHAT_HAPPENED", "See what happened", "Ver qué pasó");
            Add("GOT_IT", "Got it", "Entendido");

            // ---------------- Radiación solar
            Add("STORM_ARRIVED", "The storm is here! It's {0}.", "¡La tormenta llegó! Es {0}.");
            Add("STORM_ARRIVED_UNWARNED", "The storm arrived with no warning! It's {0}.", "¡La tormenta llegó sin aviso! Es {0}.");
            Add("STORM_HOW_PROTECT", "How will you protect your crew?", "¿Cómo protegerás a tu tripulación?");
            Add("STORM_YOUR_PROTECTION", "Your protection: {0}.", "Tu protección: {0}.");
            Add("COST_WATER_WALLS", "Water −{0}. That day: no new food and half the oxygen.", "Agua −{0}. Ese día: sin comida nueva y la mitad de oxígeno.");
            Add("COST_WATER_WALLS_GOOD", "Protection: very high.", "Protección: muy alta.");
            Add("COST_WATER_WALLS_WEAK", "Without a warning it protects little.", "Sin aviso protege poco.");
            Add("COST_REGOLITH_READY", "The shelter is already ready. Protection: high.", "El refugio ya está listo. Protección: alta.");
            Add("COST_REGOLITH", "Energy −{0}. The rover works for {1} (no ice). Protection: high.", "Energía −{0}. El rover trabaja {1} (no trae hielo). Protección: alta.");
            Add("COST_REGOLITH_LATE", "There's no time to finish it! A thin layer can make the radiation worse.", "¡No da tiempo de terminarlo! Una capa delgada puede empeorar la radiación.");
            Add("COST_BUNKER", "Oxygen, food and water −{0}. The crew rests badly. One day without production. Protection: medium.", "Oxígeno, comida y agua −{0}. La tripulación descansa mal. Un día sin producir. Protección: media.");
            Add("COST_NO_PROTECTION", "Costs nothing. Protection: none. The habitat takes all the damage and the panels get damaged.", "No gasta nada. Protección: ninguna. La vivienda recibe todo el daño y los paneles se dañan.");
            Add("RESULT_VERY_PROTECTED", "Very well protected!", "¡Muy bien protegidos!");
            Add("RESULT_PROTECTED", "Fairly well protected.", "Bastante protegidos.");
            Add("RESULT_WEAK", "Weak protection.", "Protección débil.");
            Add("RESULT_EXPOSED", "We were exposed.", "Quedamos expuestos.");
            Add("RESULT_HABITAT_LOST", "The habitat lost {0} points.", "La vivienda perdió {0} puntos.");
            Add("RESULT_WATER_WALLS_DAY", "Today there will be no new food and half the oxygen.", "Hoy no habrá comida nueva y el oxígeno será la mitad.");
            Add("RESULT_BUNKER_DAY", "Today nothing is produced and the crew rests badly.", "Hoy no se produce nada y la tripulación descansa mal.");
            Add("RESULT_THIN_LAYER", "The regolith layer was too thin and made the radiation worse.", "La capa de regolito era muy delgada y empeoró la radiación.");
            Add("RESULT_PANELS_DAMAGED", "The radiation damaged the solar panels!", "¡La radiación dañó los paneles solares!");
            Add("NOTICE_ROVER_REGOLITH", "The rover started covering the habitat with regolith.", "El rover empezó a cubrir la vivienda con regolito.");

            // ---------------- Micrometeoritos
            Add("COST_STOW_PANELS", "That day: Energy −{0} and the panels make nothing. The rovers may break.", "Ese día: Energía −{0} y los paneles no producen. Los rovers pueden averiarse.");
            Add("COST_STOW_ROVERS", "That day: no ice, Water −{0} and Oxygen −{1}. The panels may break.", "Ese día: no traen hielo, Agua −{0} y Oxígeno −{1}. Los paneles pueden averiarse.");
            Add("IMPACTS", "Impacts!", "¡Impactos!");
            Add("IMPACTS_UNWARNED", "Impacts with no warning!", "¡Impactos sin aviso!");
            Add("IMPACTS_NOTHING_STORED", "There was no time to store anything. All the outside equipment is exposed.", "No hubo tiempo de guardar nada. Todo el equipo exterior está expuesto.");
            Add("RESULT_PANELS_SAFE", "The panels are safe.", "Los paneles están a salvo.");
            Add("RESULT_ROVERS_SAFE", "The rovers are safe.", "Los rovers están a salvo.");
            Add("RESULT_PANELS_BROKE", "The solar panels broke! Now they make half as much.", "¡Los paneles solares se averiaron! Ahora producen la mitad.");
            Add("RESULT_ROVERS_BROKE", "The rovers broke! Now they bring half as much ice.", "¡Los rovers se averiaron! Ahora traen la mitad de hielo.");
            Add("RESULT_NOTHING_BROKE", "Nothing broke! We were lucky.", "¡Nada se rompió! Tuvimos suerte.");
            Add("RESULT_DAMAGE", "There was damage", "Hubo daños");
            Add("RESULT_NO_DAMAGE", "No damage", "Sin daños");

            // ---------------- Reparaciones
            Add("REPAIR_TITLE_PANELS", "How do we repair the solar panels?", "¿Cómo reparamos los paneles solares?");
            Add("REPAIR_TITLE_ROVERS", "How do we repair the rovers?", "¿Cómo reparamos los rovers?");
            Add("REPAIR_SUBTITLE_PANELS", "The solar panels are working at half power", "Los paneles solares funcionan a la mitad");
            Add("REPAIR_SUBTITLE_ROVERS", "The rovers are working at half power", "Los rovers funcionan a la mitad");
            Add("REPAIR_BODY", "The damage isn't permanent.", "Las averías no son permanentes.");
            Add("REPAIR_NOW", "Repair now", "Reparar ahora");
            Add("REPAIR_NOW_COST", "Energy −{0} and Water −{1}. Back to 100% today.", "Energía −{0} y Agua −{1}. Funcionan al 100% hoy mismo.");
            Add("REPAIR_SLOW", "Repair slowly", "Reparar con tiempo");
            Add("REPAIR_SLOW_COST", "Free. They don't work for {0}, then they're back to 100%.", "Gratis. No funcionan durante {0} y luego vuelven al 100%.");

            // ---------------- Sombra de la Tierra
            Add("SHADOW_WARNING_BATTERIES", "We have {0} batteries. Tip: save energy.", "Tenemos {0} baterías. Consejo: guarda energía.");
            Add("SHADOW_ARRIVED", "Earth is blocking the Sun", "La Tierra tapa al Sol");
            Add("SHADOW_ARRIVED_UNWARNED", "Earth is blocking the Sun, and there was no warning!", "La Tierra tapa al Sol ¡y no hubo aviso!");
            Add("SHADOW_CHOOSE", "Choose what to power with the batteries.", "Elige qué alimentar con las baterías.");
            Add("RESULT_ALL_ON", "Everything stayed on!", "¡Todo encendido!");
            Add("RESULT_SACRIFICED", "What we gave up", "Lo que sacrificamos");
            Add("ECLIPSE_BATTERIES_STATUS", "Batteries available: {0}   {1}   Used: {2} of {0}", "Baterías disponibles: {0}   {1}   Usadas: {2} de {0}");
            Add("ECLIPSE_COST", "Cost: {0}", "Costo: {0}");
            Add("ECLIPSE_BATTERY_ONE", "1 battery", "1 batería");
            Add("ECLIPSE_BATTERIES", "{0} batteries", "{0} baterías");
            Add("CONFIRM", "CONFIRM", "CONFIRMAR");

            // ---------------- Emergencia
            Add("EMERGENCY_TITLE", "Emergency!", "¡Emergencia!");
            Add("EMERGENCY_FAILED", "Failure: {0}", "Falló: {0}");
            Add("EMERGENCY_BODY_ENERGY", "The base ran out of energy. We turned off the greenhouse to save power.", "La base se quedó sin energía. Apagamos el invernadero para ahorrar.");
            Add("EMERGENCY_BODY_HABITAT", "The habitat is badly damaged. Keep the habitat on to repair it little by little.", "La vivienda está muy dañada. Mantén la vivienda encendida para repararla poco a poco.");
            Add("EMERGENCY_BATTERY", "Connect a battery", "Conectar una batería");
            Add("EMERGENCY_BATTERY_COST", "+{0} energy. There will be one battery less for an eclipse.", "+{0} energía. Quedará una batería menos para un eclipse.");
            Add("EMERGENCY_CONTINUE", "Keep going without a battery", "Seguir sin batería");
            Add("EMERGENCY_CONTINUE_COST", "Turn off what you don't need. The panels will make energy tomorrow.", "Apaga lo que no necesites. Los paneles producirán energía mañana.");

            // ---------------- Registro de decisiones (texto que se guarda en el momento)
            Add("DECISION_EMERGENCY_NO_BATTERY", "Emergency: kept going without a battery", "Emergencia: continuó sin usar batería");
            Add("DECISION_PREPARED", "Prepared \"{0}\" for the storm on day {1}", "Preparó \"{0}\" para la tormenta del día {1}");
            Add("DECISION_WITH_WARNING", "(with warning)", "(con aviso)");
            Add("DECISION_WITHOUT_WARNING", "(without warning)", "(sin aviso)");
            Add("DECISION_STORM", "Solar storm: {0} → Habitat −{1}", "Tormenta solar: {0} → Vivienda −{1}");
            Add("DECISION_WILL_PROTECT_PANELS", "Will protect the panels from the micrometeorites", "Protegerá los paneles de los micrometeoritos");
            Add("DECISION_WILL_PROTECT_ROVERS", "Will protect the rovers from the micrometeorites", "Protegerá los rovers de los micrometeoritos");
            Add("DECISION_PROTECTED_PANELS", "Protected the panels", "Protegió los paneles");
            Add("DECISION_PROTECTED_ROVERS", "Protected the rovers", "Protegió los rovers");
            Add("DECISION_ALL_POWERED", "Powered every section", "Alimentó todas las secciones");
            Add("DECISION_UNPOWERED", "No power: {0}", "Sin energía: {0}");
            Add("DECISION_SHADOW", "Earth's Shadow: {0}", "Sombra de la Tierra: {0}");
            Add("DECISION_DAY", "Day {0}: {1}", "Día {0}: {1}");
            Add("DECISION_EMERGENCY_BATTERY", "Used an emergency battery (+{0} energy)", "Usó una batería de emergencia (+{0} energía)");
            Add("DECISION_REPAIR_NOW", "Repaired {0} right away (Energy −{1}, Water −{2})", "Reparación inmediata de {0} (Energía −{1}, Agua −{2})");
            Add("DECISION_REPAIR_SLOW", "Repaired {0} slowly (out of service for {1})", "Reparación con tiempo de {0} (inactivos {1})");
        }
    }
}
