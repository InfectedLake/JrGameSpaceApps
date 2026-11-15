using System.Collections.Generic;
using Crater.Core;
using UnityEngine;

namespace Crater.Data
{
    /// <summary>
    /// Contenido por defecto (textos, fuentes y valores). Se usa cuando no hay assets asignados
    /// en el Inspector y también lo usa el menú CRATER &gt; Crear assets de datos para generarlos.
    /// Todo dato científico lleva la atribución indicada en el documento de investigación del proyecto.
    /// </summary>
    public static class DefaultContent
    {
        public const string ArchiveLabel = "DATOS ARCHIVADOS: estos registros corresponden a una tormenta solar real del pasado";

        public static readonly Color SolarColor = new Color(0.35f, 0.62f, 0.95f);
        public static readonly Color GreenhouseColor = new Color(0.38f, 0.80f, 0.45f);
        public static readonly Color RecyclerColor = new Color(0.45f, 0.82f, 0.90f);
        public static readonly Color HabitatColor = new Color(0.95f, 0.80f, 0.35f);

        // ------------------------------------------------------------------ Edificios
        public static BuildingData[] Buildings()
        {
            var defaults = BalanceConfig.DefaultBuildings();
            return new[]
            {
                Building(defaults[0], SolarColor, "solar",
                    "Convierte la luz del Sol en electricidad para toda la base.",
                    "Si se daña produce menos. Durante un eclipse no produce nada."),
                Building(defaults[1], GreenhouseColor, "cultivos",
                    "Cultiva plantas para alimentar a la tripulación. También da un poco de oxígeno.",
                    "Necesita agua: con poca agua las plantas crecen menos."),
                Building(defaults[2], RecyclerColor, "oxigeno",
                    "Fabrica el oxígeno que respiran los astronautas, usando agua y electricidad.",
                    "Sin energía o sin agua no fabrica oxígeno."),
                Building(defaults[3], HabitatColor, "animo",
                    "El hogar de la tripulación: aire cálido, luz y camas para descansar.",
                    "Si la apagas, la vivienda se deteriora y la tripulación descansa mal."),
            };
        }

        static BuildingData Building(BuildingStats stats, Color color, string scienceId, string desc, string how)
        {
            var b = ScriptableObject.CreateInstance<BuildingData>();
            b.name = stats.name;
            b.stats = stats;
            b.color = color;
            b.scienceId = scienceId;
            b.description = desc;
            b.howItWorks = how;
            return b;
        }

        // ------------------------------------------------------------------ Eventos
        public static EventData[] Events() => new[]
        {
            Event(CraterEventType.SolarRadiation, "¡Alerta solar!", new Color(1f, 0.70f, 0.20f), "radiacion",
                "¡La tormenta solar llegó a la base!",
                "Una tormenta se aproxima. ¿Cómo protegerás a tu tripulación?",
                "El agua y la tierra lunar ayudan a frenar la radiación.",
                "NASA DONKI (CCMC); NASA RadLab; estudios de blindaje",
                new[]
                {
                    Opt("Paredes de agua", "Llenamos las paredes con agua."),
                    Opt("Refugio de regolito", "El rover cubre la vivienda con tierra lunar."),
                    Opt("Refugio interior", "La tripulación se encierra en la vivienda."),
                    Opt("Sin protección", "No gastamos nada, pero quedamos expuestos."),
                }),
            Event(CraterEventType.Micrometeorites, "¡Micrometeoritos!", new Color(0.95f, 0.50f, 0.30f), "micrometeoritos",
                "Pequeñas rocas espaciales golpean el exterior de la base.",
                "El equipo exterior está en peligro. ¿Qué protegerás?",
                "Dentro de la base estás protegido. Los equipos de afuera, no.",
                "Modelo de impactos de NASA (documento de investigación de CRATER)",
                new[]
                {
                    Opt("Proteger los paneles solares", "Los rovers quedan afuera."),
                    Opt("Proteger los rovers", "Los paneles quedan afuera."),
                }),
            Event(CraterEventType.EarthShadow, "¡Sombra de la Tierra!", new Color(0.40f, 0.45f, 0.85f), "eclipse",
                "No podemos generar energía. Debemos ahorrar nuestras baterías.",
                "Pronto la Tierra tapará al Sol y los paneles dejarán de producir energía.",
                "Cuando la Tierra tapa al Sol, hay que decidir qué mantener encendido.",
                "NASA - Lunar Reconnaissance Orbiter (eclipse de marzo de 2025)",
                new EventOptionText[0]),
        };

        static EventOptionText Opt(string t, string d) => new EventOptionText { title = t, description = d, tag = DataTag.Juego };

        static EventData Event(CraterEventType type, string title, Color color, string sci, string desc, string warn, string edu, string source, EventOptionText[] opts)
        {
            var e = ScriptableObject.CreateInstance<EventData>();
            e.name = title;
            e.type = type; e.title = title; e.color = color; e.scienceId = sci;
            e.description = desc; e.warningText = warn; e.educationalMessage = edu; e.source = source;
            e.options = new List<EventOptionText>(opts);
            return e;
        }

        // ------------------------------------------------------------------ Ciencia real
        public static ScienceData[] Science() => new[]
        {
            Sci("radiacion", "¿Qué es la radiación solar?",
                "El Sol puede lanzar partículas muy rápidas que viajan por el espacio. Cuando llegan a la Luna pueden ser peligrosas para los astronautas y para los aparatos electrónicos. La Luna no tiene un aire grueso ni un campo magnético como la Tierra que la proteja.",
                "La superficie lunar recibe más radiación que el interior de la Estación Espacial Internacional: el instrumento LND midió en promedio unos 1,369 µSv por día en la superficie lunar, frente a 731 µSv por día en la ISS durante el periodo citado.",
                DataTag.Real, "NASA RadLab; mediciones del instrumento LND en la superficie lunar y CRaTER en órbita lunar",
                "El daño que recibe la vivienda se calcula con una fórmula simplificada: Intensidad × 12 × (1 − Bloqueo). Los niveles de intensidad 1, 2 y 3 son una abstracción del juego, no una escala científica oficial."),
            Sci("dosis", "Dosis acumulada",
                "La radiación es un riesgo que se va sumando con el tiempo: cada día en la Luna suma un poquito. Por eso los científicos llevan la cuenta de la dosis total que recibe cada astronauta.",
                "El valor de fondo que suma el juego cada día (1.369 mSv) corresponde al promedio diario medido por LND en la superficie lunar.",
                DataTag.Real, "Mediciones del instrumento LND (NASA RadLab)",
                "La dosis extra de cada tormenta y la idea de que el daño ocurre de golpe son simplificaciones del juego. El juego no fija ningún límite médico."),
            Sci("agua_escudo", "El agua como escudo",
                "Los materiales con mucho hidrógeno, como el agua y algunos plásticos, ayudan a frenar las partículas del Sol. Por eso una pared llena de agua puede proteger a los astronautas.",
                "Los materiales ricos en hidrógeno, como el agua y ciertos plásticos, pueden ayudar a reducir la exposición a partículas solares.",
                DataTag.Real, "Estudios de blindaje contra radiación (documento de investigación de CRATER)",
                "Los porcentajes de bloqueo (80% con aviso, 20% sin aviso) y el costo de 30 de agua son valores del juego."),
            Sci("regolito", "¿Qué es el regolito?",
                "Es el polvo y las piedritas sueltas que cubren la superficie lunar. Se puede amontonar sobre una base para protegerla de la radiación y de los cambios de temperatura.",
                "El concepto ARMOR contempla espesores cercanos a tres metros de regolito para ciertos escenarios de blindaje. Una capa insuficiente puede producir radiación secundaria.",
                DataTag.Real, "Estudios de blindaje con regolito, concepto ARMOR (documento de investigación de CRATER)",
                "El bloqueo del 70%, los 2 días de construcción y el aumento de hasta 15% de la dosis con una capa delgada son valores del juego, no una regla universal."),
            Sci("donki", "NASA DONKI",
                "DONKI es una base de datos de NASA que registra el \"clima espacial\": llamaradas solares, eyecciones de masa coronal, partículas solares energéticas y tormentas geomagnéticas. Gracias al monitoreo, una tormenta puede detectarse antes de que llegue.",
                "DONKI registra llamaradas (FLR), eyecciones de masa coronal (CME), partículas energéticas (SEP) y tormentas geomagnéticas (GST).",
                DataTag.Real, "NASA DONKI - Community Coordinated Modeling Center (CCMC)",
                "El juego convierte la clase de una llamarada real en intensidad: clase X = 3, clase M = 2, otras = 1. Esa conversión es del juego. Las fechas de los registros no predicen cuándo llegaría una tormenta a la base."),
            Sci("micrometeoritos", "Micrometeoritos",
                "Son granitos de roca del espacio, más pequeños que un grano de arena, que viajan rapidísimo. En la Luna no hay aire que los frene, así que golpean directamente el suelo y los equipos.",
                "Un modelo de NASA estimó hasta unos 23,000 impactos al año para una base del tamaño de la ISS en el ecuador lunar (cara oculta). Una base blindada tendría entre 0.024 y 0.037 impactos al año, con mínimos en las regiones polares.",
                DataTag.Real, "Modelo de NASA de impactos de micrometeoritos (documento de investigación de CRATER)",
                "No existe una API que anticipe estos impactos: en el juego el evento es aleatorio. El aviso de un día y la probabilidad de avería del 30% son abstracciones del juego."),
            Sci("eclipse", "La sombra de la Tierra",
                "A veces la Tierra se pone entre el Sol y la Luna. Desde la Luna eso se ve como un eclipse de Sol. Los paneles solares se quedan sin luz y hay que usar baterías.",
                "Durante el eclipse de marzo de 2025, el Lunar Reconnaissance Orbiter pasó casi seis horas en oscuridad y su equipo apagó algunos instrumentos para ahorrar energía.",
                DataTag.Real, "NASA - Lunar Reconnaissance Orbiter",
                "En el juego el eclipse dura un día. Los costos en baterías (2, 1, 1 y 1) están diseñados para que siempre haya que sacrificar algo."),
            Sci("noche_lunar", "La noche lunar",
                "Un día completo en la Luna dura casi un mes terrestre. Por eso la noche lunar es muy larga.",
                "En el ecuador lunar la noche dura alrededor de 14 días. En las regiones polares puede haber periodos de oscuridad intermitente de hasta unas 100 horas.",
                DataTag.Real, "NASA (documento de investigación de CRATER)",
                "La noche lunar completa está reservada para un posible modo avanzado."),
            Sci("hielo", "Hielo en los polos lunares",
                "En los polos de la Luna hay cráteres tan profundos que el Sol casi nunca ilumina su fondo. Allí el agua puede quedar congelada como hielo. Los rovers lo recogen para la base.",
                "Las regiones polares lunares tienen depósitos de hielo que una base podría aprovechar para obtener agua.",
                DataTag.Real, "NASA Moon Trek (información del terreno lunar)",
                "La ubicación exacta del cráter de la base es configurable. Los 6 de agua que recoge el rover cada día son un valor del juego."),
            Sci("oxigeno", "Oxígeno a partir de agua",
                "El agua está hecha de hidrógeno y oxígeno. Con electricidad se pueden separar para obtener oxígeno que los astronautas respiran.",
                "NASA trabaja en una planta piloto para producir oxígeno a partir de agua.",
                DataTag.Real, "NASA (documento de investigación de CRATER)",
                "La cantidad de agua que gasta el reciclador (4 por día) es un parámetro de balance del juego."),
            Sci("cultivos", "Cultivos en el espacio",
                "Las plantas pueden crecer sin tierra, con sus raíces en agua con nutrientes. Esto se llama hidroponía. Las luces especiales reemplazan al Sol.",
                "La Agencia Espacial Canadiense organizó el Deep Space Food Challenge, donde se presentó el sistema modular de cultivo CANGrow.",
                DataTag.Real, "Canadian Space Agency - Deep Space Food Challenge",
                "La reducción del 30% con poca agua representa de forma simplificada el estrés hídrico de los cultivos."),
            Sci("animo", "Cansancio y estrés",
                "Vivir mucho tiempo en un lugar pequeño y lejos de casa cansa. Dormir bien y tomar descansos ayuda a la tripulación a sentirse mejor.",
                "NASA estudia el comportamiento y la salud de las tripulaciones en el análogo HERA y en las misiones de larga duración de la ISS.",
                DataTag.Real, "NASA - HERA y estudios de misiones de la ISS",
                "La caída de 10 puntos de Vivienda en el ciclo 4 es una simplificación del juego."),
            Sci("moontrek", "Explora el terreno lunar",
                "Los científicos usan mapas muy detallados para elegir dónde aterrizar y construir. El escenario del juego se inspira en una región polar con cráteres.",
                "NASA Moon Trek ofrece mapas y datos del terreno lunar.",
                DataTag.Real, "NASA Moon Trek",
                "La forma del cráter del juego es una ilustración, no un mapa real."),
        };

        static ScienceData Sci(string id, string title, string expl, string fact, DataTag tag, string source, string gameNote)
        {
            var s = ScriptableObject.CreateInstance<ScienceData>();
            s.name = id;
            s.id = id; s.title = title; s.explanation = expl; s.fact = fact; s.factTag = tag; s.source = source; s.gameNote = gameNote;
            return s;
        }

        // ------------------------------------------------------------------ Tutorial
        public static readonly string[] TutorialSteps =
        {
            "¡Bienvenido, comandante! Soy el director de la agencia espacial. Tu misión: mantener viva a la tripulación hasta el final.",
            "Arriba están tus 4 recursos: ENERGÍA, OXÍGENO, COMIDA y SALUD. El numerito de color dice cuánto cambiarán hoy.",
            "Toca un edificio para ver qué hace y encenderlo o apagarlo. Los edificios gastan energía.",
            "¡No puedes tener todo encendido siempre! Si falta energía, un edificio se pone rojo y no funciona.",
            "Cuando estés listo, presiona TERMINAR DÍA. A veces llegarán sorpresas del espacio: ¡piensa bien cómo responder!",
        };

        public static readonly string[] DefaultAstronauts = { "Ada", "Luis", "Mei", "Tomás" };

        public static string TagLabel(DataTag t)
        {
            switch (t)
            {
                case DataTag.Real: return "[REAL]";
                case DataTag.Juego: return "[JUEGO]";
                default: return "[PENDIENTE]";
            }
        }

        public static Color TagColor(DataTag t)
        {
            switch (t)
            {
                case DataTag.Real: return new Color(0.25f, 0.65f, 0.40f);
                case DataTag.Juego: return new Color(0.30f, 0.50f, 0.90f);
                default: return new Color(0.85f, 0.55f, 0.15f);
            }
        }
    }
}
