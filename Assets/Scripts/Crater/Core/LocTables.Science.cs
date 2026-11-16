namespace Crater.Core
{
    // Ciencia real: fichas educativas. Las cifras, fuentes y nombres oficiales (NASA, CSA, DONKI, RadLab, CRaTER, LND,
    // Moon Trek, CANGrow, HERA, ARMOR) son idénticos en los dos idiomas; solo se traducen las explicaciones.
    public static partial class Loc
    {
        static void Sci(string id, string titleEn, string titleEs, string explEn, string explEs, string factEn, string factEs,
                        string sourceEn, string sourceEs, string noteEn, string noteEs)
        {
            string k = "SCI_" + id.ToUpperInvariant();
            Add(k + "_TITLE", titleEn, titleEs);
            Add(k + "_EXPL", explEn, explEs);
            Add(k + "_FACT", factEn, factEs);
            Add(k + "_SOURCE", sourceEn, sourceEs);
            Add(k + "_NOTE", noteEn, noteEs);
        }

        static void AddScience()
        {
            Sci("radiacion", "What is solar radiation?", "¿Qué es la radiación solar?",
                "The Sun can throw out very fast particles that travel through space. When they reach the Moon they can be dangerous for astronauts and for electronic devices. The Moon has no thick air or magnetic field like Earth's to protect it.",
                "El Sol puede lanzar partículas muy rápidas que viajan por el espacio. Cuando llegan a la Luna pueden ser peligrosas para los astronautas y para los aparatos electrónicos. La Luna no tiene un aire grueso ni un campo magnético como la Tierra que la proteja.",
                "The lunar surface gets more radiation than the inside of the International Space Station: the LND instrument measured an average of about 1,369 µSv per day on the lunar surface, compared with 731 µSv per day on the ISS during the period cited.",
                "La superficie lunar recibe más radiación que el interior de la Estación Espacial Internacional: el instrumento LND midió en promedio unos 1,369 µSv por día en la superficie lunar, frente a 731 µSv por día en la ISS durante el periodo citado.",
                "NASA RadLab; measurements from the LND instrument on the lunar surface and CRaTER in lunar orbit",
                "NASA RadLab; mediciones del instrumento LND en la superficie lunar y CRaTER en órbita lunar",
                "The damage to the habitat uses a simplified formula: Intensity × 12 × (1 − Blocking). Intensity levels 1, 2 and 3 are a game simplification, not an official scientific scale.",
                "El daño que recibe la vivienda se calcula con una fórmula simplificada: Intensidad × 12 × (1 − Bloqueo). Los niveles de intensidad 1, 2 y 3 son una abstracción del juego, no una escala científica oficial.");

            Sci("dosis", "Total dose", "Dosis acumulada",
                "Radiation is a risk that adds up over time: every day on the Moon adds a little more. That's why scientists keep track of the total dose each astronaut receives.",
                "La radiación es un riesgo que se va sumando con el tiempo: cada día en la Luna suma un poquito. Por eso los científicos llevan la cuenta de la dosis total que recibe cada astronauta.",
                "The background value the game adds each day (1.369 mSv) matches the daily average measured by LND on the lunar surface.",
                "El valor de fondo que suma el juego cada día (1.369 mSv) corresponde al promedio diario medido por LND en la superficie lunar.",
                "Measurements from the LND instrument (NASA RadLab)",
                "Mediciones del instrumento LND (NASA RadLab)",
                "The extra dose from each storm and the idea that the damage happens all at once are game simplifications. The game does not set any medical limit.",
                "La dosis extra de cada tormenta y la idea de que el daño ocurre de golpe son simplificaciones del juego. El juego no fija ningún límite médico.");

            Sci("agua_escudo", "Water as a shield", "El agua como escudo",
                "Materials with lots of hydrogen, like water and some plastics, help slow down particles from the Sun. That's why a wall filled with water can protect astronauts.",
                "Los materiales con mucho hidrógeno, como el agua y algunos plásticos, ayudan a frenar las partículas del Sol. Por eso una pared llena de agua puede proteger a los astronautas.",
                "Hydrogen-rich materials, such as water and certain plastics, can help reduce exposure to solar particles.",
                "Los materiales ricos en hidrógeno, como el agua y ciertos plásticos, pueden ayudar a reducir la exposición a partículas solares.",
                "Radiation shielding studies (CRATER research document)",
                "Estudios de blindaje contra radiación (documento de investigación de CRATER)",
                "The blocking percentages (80% with a warning, 20% without one) and the cost of 30 water are game values.",
                "Los porcentajes de bloqueo (80% con aviso, 20% sin aviso) y el costo de 30 de agua son valores del juego.");

            Sci("regolito", "What is regolith?", "¿Qué es el regolito?",
                "It's the dust and small loose rocks that cover the Moon's surface. It can be piled on top of a base to protect it from radiation and from temperature changes.",
                "Es el polvo y las piedritas sueltas que cubren la superficie lunar. Se puede amontonar sobre una base para protegerla de la radiación y de los cambios de temperatura.",
                "The ARMOR concept considers regolith about three meters thick for some shielding scenarios. A layer that is too thin can produce secondary radiation.",
                "El concepto ARMOR contempla espesores cercanos a tres metros de regolito para ciertos escenarios de blindaje. Una capa insuficiente puede producir radiación secundaria.",
                "Regolith shielding studies, ARMOR concept (CRATER research document)",
                "Estudios de blindaje con regolito, concepto ARMOR (documento de investigación de CRATER)",
                "The 70% blocking, the 2 days of building and the dose increase of up to 15% with a thin layer are game values, not a universal rule.",
                "El bloqueo del 70%, los 2 días de construcción y el aumento de hasta 15% de la dosis con una capa delgada son valores del juego, no una regla universal.");

            Sci("donki", "NASA DONKI", "NASA DONKI",
                "DONKI is a NASA database that records \"space weather\": solar flares, coronal mass ejections, solar energetic particles and geomagnetic storms. Thanks to this monitoring, a storm can be spotted before it arrives.",
                "DONKI es una base de datos de NASA que registra el \"clima espacial\": llamaradas solares, eyecciones de masa coronal, partículas solares energéticas y tormentas geomagnéticas. Gracias al monitoreo, una tormenta puede detectarse antes de que llegue.",
                "DONKI records flares (FLR), coronal mass ejections (CME), energetic particles (SEP) and geomagnetic storms (GST).",
                "DONKI registra llamaradas (FLR), eyecciones de masa coronal (CME), partículas energéticas (SEP) y tormentas geomagnéticas (GST).",
                "NASA DONKI - Community Coordinated Modeling Center (CCMC)",
                "NASA DONKI - Community Coordinated Modeling Center (CCMC)",
                "The game turns the class of a real flare into an intensity: class X = 3, class M = 2, others = 1. That conversion belongs to the game. The record dates do not predict when a storm would reach the base.",
                "El juego convierte la clase de una llamarada real en intensidad: clase X = 3, clase M = 2, otras = 1. Esa conversión es del juego. Las fechas de los registros no predicen cuándo llegaría una tormenta a la base.");

            Sci("micrometeoritos", "Micrometeorites", "Micrometeoritos",
                "They are tiny bits of space rock, smaller than a grain of sand, that travel super fast. The Moon has no air to slow them down, so they hit the ground and the equipment directly.",
                "Son granitos de roca del espacio, más pequeños que un grano de arena, que viajan rapidísimo. En la Luna no hay aire que los frene, así que golpean directamente el suelo y los equipos.",
                "A NASA model estimated up to about 23,000 impacts per year for an ISS-sized base at the lunar equator (far side). A shielded base would get between 0.024 and 0.037 impacts per year, with the lowest values in the polar regions.",
                "Un modelo de NASA estimó hasta unos 23,000 impactos al año para una base del tamaño de la ISS en el ecuador lunar (cara oculta). Una base blindada tendría entre 0.024 y 0.037 impactos al año, con mínimos en las regiones polares.",
                "NASA micrometeorite impact model (CRATER research document)",
                "Modelo de NASA de impactos de micrometeoritos (documento de investigación de CRATER)",
                "There is no API that predicts these impacts: in the game the event is random. The early warning and the 30% chance of damage are game simplifications.",
                "No existe una API que anticipe estos impactos: en el juego el evento es aleatorio. El aviso anticipado y la probabilidad de avería del 30% son abstracciones del juego.");

            Sci("eclipse", "Earth's shadow", "La sombra de la Tierra",
                "Sometimes Earth moves between the Sun and the Moon. From the Moon, that looks like a solar eclipse. The solar panels are left without light and batteries must be used.",
                "A veces la Tierra se pone entre el Sol y la Luna. Desde la Luna eso se ve como un eclipse de Sol. Los paneles solares se quedan sin luz y hay que usar baterías.",
                "During the March 2025 eclipse, the Lunar Reconnaissance Orbiter spent almost six hours in darkness and its team turned off some instruments to save energy.",
                "Durante el eclipse de marzo de 2025, el Lunar Reconnaissance Orbiter pasó casi seis horas en oscuridad y su equipo apagó algunos instrumentos para ahorrar energía.",
                "NASA - Lunar Reconnaissance Orbiter",
                "NASA - Lunar Reconnaissance Orbiter",
                "In the game the eclipse lasts one day. The battery costs (2, 1, 1 and 1) are designed so you always have to give something up.",
                "En el juego el eclipse dura un día. Los costos en baterías (2, 1, 1 y 1) están diseñados para que siempre haya que sacrificar algo.");

            Sci("noche_lunar", "The lunar night", "La noche lunar",
                "A full day on the Moon lasts almost a month on Earth. That's why the lunar night is very long.",
                "Un día completo en la Luna dura casi un mes terrestre. Por eso la noche lunar es muy larga.",
                "At the lunar equator the night lasts about 14 days. In the polar regions there can be periods of on-and-off darkness of up to about 100 hours.",
                "En el ecuador lunar la noche dura alrededor de 14 días. En las regiones polares puede haber periodos de oscuridad intermitente de hasta unas 100 horas.",
                "NASA (CRATER research document)",
                "NASA (documento de investigación de CRATER)",
                "The full lunar night is saved for a possible advanced mode.",
                "La noche lunar completa está reservada para un posible modo avanzado.");

            Sci("hielo", "Ice at the lunar poles", "Hielo en los polos lunares",
                "At the Moon's poles there are craters so deep that the Sun almost never lights their floor. There, water can stay frozen as ice. The rovers collect it for the base.",
                "En los polos de la Luna hay cráteres tan profundos que el Sol casi nunca ilumina su fondo. Allí el agua puede quedar congelada como hielo. Los rovers lo recogen para la base.",
                "The lunar polar regions have ice deposits that a base could use to get water.",
                "Las regiones polares lunares tienen depósitos de hielo que una base podría aprovechar para obtener agua.",
                "NASA Moon Trek (lunar terrain information)",
                "NASA Moon Trek (información del terreno lunar)",
                "The exact location of the base's crater can be configured. The 6 water the rover collects each day is a game value.",
                "La ubicación exacta del cráter de la base es configurable. Los 6 de agua que recoge el rover cada día son un valor del juego.");

            Sci("oxigeno", "Oxygen from water", "Oxígeno a partir de agua",
                "Water is made of hydrogen and oxygen. With electricity they can be split apart to get oxygen for the astronauts to breathe.",
                "El agua está hecha de hidrógeno y oxígeno. Con electricidad se pueden separar para obtener oxígeno que los astronautas respiran.",
                "NASA is working on a pilot plant to produce oxygen from water.",
                "NASA trabaja en una planta piloto para producir oxígeno a partir de agua.",
                "NASA (CRATER research document)",
                "NASA (documento de investigación de CRATER)",
                "The amount of water the recycler uses (4 per day) is a game balance setting.",
                "La cantidad de agua que gasta el reciclador (4 por día) es un parámetro de balance del juego.");

            Sci("cultivos", "Growing crops in space", "Cultivos en el espacio",
                "Plants can grow without soil, with their roots in water with nutrients. This is called hydroponics. Special lights take the place of the Sun.",
                "Las plantas pueden crecer sin tierra, con sus raíces en agua con nutrientes. Esto se llama hidroponía. Las luces especiales reemplazan al Sol.",
                "The Canadian Space Agency ran the Deep Space Food Challenge, where the CANGrow modular growing system was presented.",
                "La Agencia Espacial Canadiense organizó el Deep Space Food Challenge, donde se presentó el sistema modular de cultivo CANGrow.",
                "Canadian Space Agency - Deep Space Food Challenge",
                "Canadian Space Agency - Deep Space Food Challenge",
                "The 30% drop with little water is a simplified way to show water stress in crops.",
                "La reducción del 30% con poca agua representa de forma simplificada el estrés hídrico de los cultivos.");

            Sci("animo", "Tiredness and stress", "Cansancio y estrés",
                "Living for a long time in a small place far from home is tiring. Sleeping well and taking breaks helps the crew feel better.",
                "Vivir mucho tiempo en un lugar pequeño y lejos de casa cansa. Dormir bien y tomar descansos ayuda a la tripulación a sentirse mejor.",
                "NASA studies crew behavior and health in the HERA analog and in long ISS missions.",
                "NASA estudia el comportamiento y la salud de las tripulaciones en el análogo HERA y en las misiones de larga duración de la ISS.",
                "NASA - HERA and ISS mission studies",
                "NASA - HERA y estudios de misiones de la ISS",
                "The 10-point Habitat drop in cycle 4 is a game simplification.",
                "La caída de 10 puntos de Vivienda en el ciclo 4 es una simplificación del juego.");

            Sci("moontrek", "Explore the lunar terrain", "Explora el terreno lunar",
                "Scientists use very detailed maps to choose where to land and build. The game's setting is inspired by a polar region full of craters.",
                "Los científicos usan mapas muy detallados para elegir dónde aterrizar y construir. El escenario del juego se inspira en una región polar con cráteres.",
                "NASA Moon Trek offers maps and data about the lunar terrain.",
                "NASA Moon Trek ofrece mapas y datos del terreno lunar.",
                "NASA Moon Trek",
                "NASA Moon Trek",
                "The shape of the game's crater is an illustration, not a real map.",
                "La forma del cráter del juego es una ilustración, no un mapa real.");
        }
    }
}
