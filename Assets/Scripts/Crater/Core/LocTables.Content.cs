namespace Crater.Core
{
    // Contenido: eventos menores, textos de los tres acontecimientos principales, tutorial y etiquetas de datos.
    public static partial class Loc
    {
        static void AddMinorEvents()
        {
            Add("MINOR_POLVO_NAME", "Moon dust on the panels", "Polvo lunar en los paneles");
            Add("MINOR_POLVO_DESC", "Moon dust is making our solar panels work worse. We need to act.", "El polvo lunar está reduciendo la eficiencia de nuestros paneles solares. Necesitamos actuar.");
            Add("MINOR_POLVO_A", "Clean the panels", "Limpiar los paneles");
            Add("MINOR_POLVO_B", "Dim the greenhouse lights", "Bajar las luces del invernadero");

            Add("MINOR_FILTRO_NAME", "Clogged air filter", "Filtro de aire saturado");
            Add("MINOR_FILTRO_DESC", "The air filter needs maintenance to keep working properly.", "El filtro de aire necesita mantenimiento para seguir funcionando correctamente.");
            Add("MINOR_FILTRO_A", "Extra maintenance cycle", "Ciclo extra de mantenimiento");
            Add("MINOR_FILTRO_B", "Use oxygen reserves", "Usar reservas de oxígeno");

            Add("MINOR_FRIO_NAME", "Temperature drop", "Descenso de temperatura");
            Add("MINOR_FRIO_DESC", "The habitat is getting colder.", "La temperatura de la vivienda está disminuyendo.");
            Add("MINOR_FRIO_A", "Keep the heating on", "Mantener la calefacción");
            Add("MINOR_FRIO_B", "Turn the heating down", "Bajar la calefacción");

            Add("MINOR_CULTIVOS_NAME", "Crop trouble", "Problema en los cultivos");
            Add("MINOR_CULTIVOS_DESC", "Some of our crops are having trouble. We must decide what to do.", "Una parte de nuestros cultivos está teniendo problemas. Debemos decidir cómo actuar.");
            Add("MINOR_CULTIVOS_A", "Throw away part of the harvest", "Descartar parte de la cosecha");
            Add("MINOR_CULTIVOS_B", "Use more electricity to save it", "Usar más electricidad para salvarla");

            Add("MINOR_FUGA_AIRE_NAME", "Small air leak", "Pequeña fuga de aire");
            Add("MINOR_FUGA_AIRE_DESC", "A small leak was found in the air system.", "Se detectó una fuga menor en el sistema de aire.");
            Add("MINOR_FUGA_AIRE_A", "Make up for it with reserves", "Compensar con las reservas");
            Add("MINOR_FUGA_AIRE_B", "Push the recycler harder", "Forzar el reciclador");

            Add("MINOR_FUGA_AGUA_NAME", "Water tank leak", "Fuga en el depósito de agua");
            Add("MINOR_FUGA_AGUA_DESC", "The water system has a small leak.", "El sistema de agua presenta una pequeña fuga.");
            Add("MINOR_FUGA_AGUA_A", "Turn on water recovery", "Activar la recuperación de agua");
            Add("MINOR_FUGA_AGUA_B", "Ration the greenhouse water", "Racionar el agua del invernadero");

            Add("MINOR_NOT_ENOUGH", "not enough {0}", "no hay suficiente {0}");
            Add("MINOR_RESOLVED", "{0} solved. {1}.", "{0} resuelto. {1}.");
            Add("MINOR_UNAVAILABLE", "Not available: {0}", "No disponible: {0}");
        }

        static void AddContent()
        {
            // ---------------- Acontecimientos principales (textos de EventData)
            Add("EVENT_SOLAR_RADIATION", "Solar Radiation Storm", "Ráfaga de radiación solar");
            Add("EVENT_MICROMETEORITES", "Micrometeorite Shower", "Lluvia de micrometeoritos");
            Add("EVENT_EARTH_SHADOW", "Earth's Shadow", "Sombra de la Tierra");

            Add("EVENT_SOLAR_TITLE", "Solar alert!", "¡Alerta solar!");
            Add("EVENT_SOLAR_DESC", "The solar storm has reached the base!", "¡La tormenta solar llegó a la base!");
            Add("EVENT_SOLAR_WARN", "A storm is coming. How will you protect your crew?", "Una tormenta se aproxima. ¿Cómo protegerás a tu tripulación?");
            Add("EVENT_SOLAR_EDU", "Water and moon soil help stop radiation.", "El agua y la tierra lunar ayudan a frenar la radiación.");
            Add("EVENT_SOLAR_SOURCE", "NASA DONKI (CCMC); NASA RadLab; shielding studies", "NASA DONKI (CCMC); NASA RadLab; estudios de blindaje");
            Add("EVENT_SOLAR_OPT0_TITLE", "Water walls", "Paredes de agua");
            Add("EVENT_SOLAR_OPT0_DESC", "We fill the walls with water.", "Llenamos las paredes con agua.");
            Add("EVENT_SOLAR_OPT1_TITLE", "Regolith shelter", "Refugio de regolito");
            Add("EVENT_SOLAR_OPT1_DESC", "The rover covers the habitat with moon soil.", "El rover cubre la vivienda con tierra lunar.");
            Add("EVENT_SOLAR_OPT2_TITLE", "Indoor shelter", "Refugio interior");
            Add("EVENT_SOLAR_OPT2_DESC", "The crew stays locked inside the habitat.", "La tripulación se encierra en la vivienda.");
            Add("EVENT_SOLAR_OPT3_TITLE", "No protection", "Sin protección");
            Add("EVENT_SOLAR_OPT3_DESC", "We spend nothing, but we stay exposed.", "No gastamos nada, pero quedamos expuestos.");

            Add("EVENT_METEORS_TITLE", "Micrometeorites!", "¡Micrometeoritos!");
            Add("EVENT_METEORS_DESC", "Tiny space rocks are hitting the outside of the base.", "Pequeñas rocas espaciales golpean el exterior de la base.");
            Add("EVENT_METEORS_WARN", "The outside equipment is in danger. What will you protect?", "El equipo exterior está en peligro. ¿Qué protegerás?");
            Add("EVENT_METEORS_EDU", "Inside the base you are safe. The equipment outside is not.", "Dentro de la base estás protegido. Los equipos de afuera, no.");
            Add("EVENT_METEORS_SOURCE", "NASA impact model (CRATER research document)", "Modelo de impactos de NASA (documento de investigación de CRATER)");
            Add("EVENT_METEORS_OPT0_TITLE", "Protect the solar panels", "Proteger los paneles solares");
            Add("EVENT_METEORS_OPT0_DESC", "The rovers stay outside.", "Los rovers quedan afuera.");
            Add("EVENT_METEORS_OPT1_TITLE", "Protect the rovers", "Proteger los rovers");
            Add("EVENT_METEORS_OPT1_DESC", "The panels stay outside.", "Los paneles quedan afuera.");

            Add("EVENT_SHADOW_TITLE", "Earth's Shadow!", "¡Sombra de la Tierra!");
            Add("EVENT_SHADOW_DESC", "We can't make energy. We must save our batteries.", "No podemos generar energía. Debemos ahorrar nuestras baterías.");
            Add("EVENT_SHADOW_WARN", "Soon Earth will block the Sun and the panels will stop making energy.", "Pronto la Tierra tapará al Sol y los paneles dejarán de producir energía.");
            Add("EVENT_SHADOW_EDU", "When Earth blocks the Sun, you have to decide what stays on.", "Cuando la Tierra tapa al Sol, hay que decidir qué mantener encendido.");
            Add("EVENT_SHADOW_SOURCE", "NASA - Lunar Reconnaissance Orbiter (March 2025 eclipse)", "NASA - Lunar Reconnaissance Orbiter (eclipse de marzo de 2025)");

            // ---------------- Tutorial del director
            Add("TUTORIAL_0", "Welcome, commander! I'm the director of the space agency. Your mission: keep the crew alive until the end.", "¡Bienvenido, comandante! Soy el director de la agencia espacial. Tu misión: mantener viva a la tripulación hasta el final.");
            Add("TUTORIAL_1", "Up top are your 4 resources: ENERGY, OXYGEN, FOOD and HEALTH. The small colored number shows how much they will change today.", "Arriba están tus 4 recursos: ENERGÍA, OXÍGENO, COMIDA y SALUD. El numerito de color dice cuánto cambiarán hoy.");
            Add("TUTORIAL_2", "Tap a building to see what it does and turn it on or off. Buildings use energy.", "Toca un edificio para ver qué hace y encenderlo o apagarlo. Los edificios gastan energía.");
            Add("TUTORIAL_3", "You can't keep everything on all the time! If energy runs short, a building turns red and stops working.", "¡No puedes tener todo encendido siempre! Si falta energía, un edificio se pone rojo y no funciona.");
            Add("TUTORIAL_4", "When you're ready, press END DAY. Sometimes surprises will come from space: think carefully about how to respond!", "Cuando estés listo, presiona TERMINAR DÍA. A veces llegarán sorpresas del espacio: ¡piensa bien cómo responder!");

            // ---------------- Etiquetas de datos (Ciencia real)
            Add("TAG_REAL", "[REAL]", "[REAL]");
            Add("TAG_GAME", "[IN-GAME VALUE]", "[JUEGO]");
            Add("TAG_PENDING", "[PENDING]", "[PENDIENTE]");
            Add("ARCHIVE_LABEL", "ARCHIVED DATA: these records come from a real solar storm in the past", "DATOS ARCHIVADOS: estos registros corresponden a una tormenta solar real del pasado");
        }
    }
}
