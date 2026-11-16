namespace Crater.Core
{
    // Interfaz: menús, HUD, ficha de edificio, tutorial, pausa, opciones, créditos, ciencia, final y avisos.
    public static partial class Loc
    {
        static void AddUI()
        {
            // ---------------- Comunes
            Add("BACK", "BACK", "VOLVER");
            Add("CLOSE", "CLOSE", "CERRAR");
            Add("CANCEL", "CANCEL", "CANCELAR");

            // ---------------- Menú principal
            Add("MAIN_MENU_START", "START", "INICIO");
            Add("MAIN_MENU_OPTIONS", "SETTINGS", "OPCIONES");
            Add("MAIN_MENU_CREDITS", "CREDITS", "CRÉDITOS");
            Add("MAIN_MENU_EXIT", "EXIT", "SALIR");
            Add("MAIN_MENU_CONTINUE", "CONTINUE", "CONTINUAR");
            Add("MAIN_MENU_NEW_GAME", "NEW GAME", "NUEVA PARTIDA");
            Add("NOTICE_NO_SAVE", "There's no valid saved game.", "No hay una partida guardada válida.");

            // ---------------- Configuración de la misión
            Add("SETUP_TITLE", "NEW MISSION", "NUEVA MISIÓN");
            Add("SETUP_BASE_NAME", "Base name", "Nombre de la base");
            Add("SETUP_BASE_PLACEHOLDER", "Type a name...", "Escribe un nombre...");
            Add("SETUP_CREW_COUNT", "Number of astronauts (each one uses 2 oxygen and 2 food per day)", "Número de astronautas (cada uno consume 2 de oxígeno y 2 de comida por día)");
            Add("SETUP_CREW_NORMAL", "{0} (normal)", "{0} (normal)");
            Add("SETUP_ASTRONAUT_NAMES", "Astronaut names", "Nombres de los astronautas");
            Add("SETUP_NAME_PLACEHOLDER", "Name", "Nombre");
            Add("SETUP_LENGTH", "Mission length (one cycle = 4 days of management)", "Duración de la misión (un ciclo = 4 días de gestión)");
            Add("SETUP_LENGTH_SHORT", "Short (easy): {0} cycles", "Corta (fácil): {0} ciclos");
            Add("SETUP_LENGTH_NORMAL", "Normal: {0} cycles", "Normal: {0} ciclos");
            Add("SETUP_LENGTH_LONG", "Long: {0} cycles", "Larga: {0} ciclos");
            Add("SETUP_TUTORIAL_ON", "Director's tutorial: YES (tap to skip it)", "Tutorial del director: SÍ (toca para omitirlo)");
            Add("SETUP_TUTORIAL_OFF", "Director's tutorial: NO (tap to turn it on)", "Tutorial del director: NO (toca para activarlo)");
            Add("SETUP_NOTE", "Starting values from the design document: Energy 100, Oxygen 70, Food 60, Health 100. Water 50 and Habitat 70 [IN-GAME VALUE].", "Valores iniciales del documento: Energía 100, Oxígeno 70, Comida 60, Salud 100. Agua 50 y Vivienda 70 [JUEGO].");
            Add("SETUP_START", "START MISSION!", "¡EMPEZAR MISIÓN!");

            // ---------------- HUD y calendario
            Add("HUD_DAY", "DAY", "DÍA");
            Add("HUD_DAY_N", "DAY {0}", "DÍA {0}");
            Add("HUD_CYCLE", "CYCLE {0} OF {1}", "CICLO {0} DE {1}");
            Add("HUD_END_DAY", "END\nDAY", "TERMINAR\nDÍA");

            // ---------------- Ficha de edificio
            Add("CARD_PER_DAY", "{0} per day", "{0} por día");
            Add("CARD_TURN_ON", "TURN ON", "ENCENDER");
            Add("CARD_TURN_OFF", "TURN OFF", "APAGAR");
            Add("STATE_WORKING", "WORKING", "FUNCIONANDO");
            Add("STATE_BROKEN", "BROKEN", "AVERIADO");
            Add("STATE_INACTIVE", "INACTIVE", "INACTIVO");

            // ---------------- Letras de recurso (icono de los eventos menores)
            Add("RES_LETTER_ENERGY", "E", "E");
            Add("RES_LETTER_OXYGEN", "O2", "O2");
            Add("RES_LETTER_FOOD", "F", "C");
            Add("RES_LETTER_HEALTH", "H", "S");

            Add("ECLIPSE_NOT_ENOUGH", "Not enough batteries. Turn off another section first.", "No alcanzan las baterías. Apaga otra sección primero.");

            // ---------------- Tutorial
            Add("TUTORIAL_DIRECTOR", "DIRECTOR", "DIRECTOR");
            Add("TUTORIAL_NEXT", "NEXT", "SIGUIENTE");
            Add("TUTORIAL_GO", "LET'S GO!", "¡A TRABAJAR!");
            Add("TUTORIAL_SKIP", "SKIP TUTORIAL", "OMITIR TUTORIAL");

            // ---------------- Pausa y confirmaciones
            Add("PAUSE_TITLE", "MENU", "MENÚ");
            Add("PAUSE_CONTINUE", "RESUME", "CONTINUAR");
            Add("PAUSE_OPTIONS", "SETTINGS", "OPCIONES");
            Add("PAUSE_MAIN_MENU", "MAIN MENU", "MENÚ PRINCIPAL");
            Add("PAUSE_EXIT", "EXIT GAME", "SALIR DEL JUEGO");
            Add("CONFIRM_MENU_TITLE", "GO BACK TO THE MAIN MENU?", "¿SALIR AL MENÚ PRINCIPAL?");
            Add("CONFIRM_MENU_TEXT", "You'll leave the current game. The start of this day will be saved so you can CONTINUE, but you'll lose what you did today.", "Vas a abandonar la partida actual. Se guardará el inicio de este día para que puedas CONTINUAR, pero perderás lo que hiciste hoy.");
            Add("CONFIRM_QUIT_TITLE", "ARE YOU SURE YOU WANT TO EXIT?", "¿DESEAS SALIR DEL JUEGO?");
            Add("CONFIRM_QUIT_TEXT", "The game is saved at the start of every day. You can CONTINUE next time.", "La partida se guarda al inicio de cada día. Podrás CONTINUAR la próxima vez.");
            Add("CONFIRM_QUIT_TEXT_WEB", "In the browser the game can't close the tab: it will stop and you can close it yourself.", "En el navegador el juego no puede cerrar la pestaña: se detendrá y podrás cerrarla tú.");
            Add("CONFIRM_EXIT", "EXIT", "SALIR");
            Add("NOTICE_CLOSE_TAB", "You can close the browser tab now.", "Puedes cerrar la pestaña del navegador.");

            // ---------------- Opciones
            Add("OPTIONS_TITLE", "SETTINGS", "OPCIONES");
            Add("OPTIONS_SOUND_DISPLAY", "SOUND & DISPLAY", "SONIDO Y PANTALLA");
            Add("OPTIONS_DATA_GAME", "DATA & GAME", "DATOS Y PARTIDA");
            Add("OPTIONS_MASTER", "Master volume", "Volumen general");
            Add("OPTIONS_MUSIC", "Music", "Música");
            Add("OPTIONS_SFX", "Sound effects", "Efectos de sonido");
            Add("OPTIONS_FULLSCREEN", "Fullscreen: {0}", "Pantalla completa: {0}");
            Add("OPTIONS_ON", "ON", "SÍ");
            Add("OPTIONS_OFF", "OFF", "NO");
            Add("OPTIONS_FULLSCREEN_UNAVAILABLE", "Fullscreen: not available on this platform", "Pantalla completa: no disponible en esta plataforma");
            Add("OPTIONS_LANGUAGE", "Language", "Idioma");
            Add("OPTIONS_LIVE_ON", "Live NASA data: ON", "Datos de NASA en vivo: ACTIVADOS");
            Add("OPTIONS_LIVE_OFF", "Live NASA data: OFF (use archive)", "Datos de NASA en vivo: DESACTIVADOS (usar archivo)");
            Add("OPTIONS_ENDPOINT", "Endpoint: {0}FLR (windows of up to 30 days, no key).", "Endpoint: {0}FLR (ventanas de hasta 30 días, sin clave).");
            Add("OPTIONS_RETRY_NASA", "Check NASA DONKI again", "Volver a consultar NASA DONKI");
            Add("OPTIONS_TUTORIAL_ON", "Tutorial at the start: YES", "Tutorial al empezar: SÍ");
            Add("OPTIONS_TUTORIAL_OFF", "Tutorial at the start: NO", "Tutorial al empezar: NO");
            Add("OPTIONS_DELETE_SAVE", "Delete the saved game", "Borrar la partida guardada");
            Add("NOTICE_SAVE_DELETED", "Saved game deleted.", "Partida guardada borrada.");

            // ---------------- Créditos
            Add("CREDITS_TITLE", "CREDITS", "CRÉDITOS");
            Add("CREDITS_TEXT",
                "<b>CRATER</b> - an educational game about running a lunar base.\n" +
                "A project for the NASA Space Apps Challenge 2026.\n\n" +
                "<b>Data and scientific sources</b>\n" +
                "NASA DONKI - Community Coordinated Modeling Center (CCMC): solar activity.\n" +
                "NASA RadLab: radiation measurements (LND on the lunar surface, CRaTER in orbit).\n" +
                "NASA studies on radiation and shielding (water, regolith, ARMOR concept).\n" +
                "NASA studies on habitability and astronaut health: the HERA analog and ISS missions.\n" +
                "NASA Lunar Reconnaissance Orbiter (March 2025 eclipse) and NASA Moon Trek (lunar terrain).\n" +
                "Canadian Space Agency: Deep Space Food Challenge and the CANGrow system.\n\n" +
                "NASA and the CSA do not endorse this game. More details in REAL SCIENCE.\n\n" +
                "<b>Art</b>: original sprites by the CRATER team.\n" +
                "<b>Technology</b>: Unity, TextMeshPro. Liberation Sans font (SIL OFL license).",
                "<b>CRATER</b> - juego educativo sobre la administración de una base lunar.\n" +
                "Proyecto para NASA Space Apps Challenge 2026.\n\n" +
                "<b>Datos y fuentes científicas</b>\n" +
                "NASA DONKI - Community Coordinated Modeling Center (CCMC): actividad solar.\n" +
                "NASA RadLab: mediciones de radiación (LND en la superficie lunar, CRaTER en órbita).\n" +
                "Estudios de NASA sobre radiación y blindaje (agua, regolito, concepto ARMOR).\n" +
                "Estudios de NASA sobre habitabilidad y salud de astronautas: análogo HERA y misiones de la ISS.\n" +
                "NASA Lunar Reconnaissance Orbiter (eclipse de marzo de 2025) y NASA Moon Trek (terreno lunar).\n" +
                "Canadian Space Agency: Deep Space Food Challenge y sistema CANGrow.\n\n" +
                "NASA y la CSA no respaldan este juego. Más detalles en CIENCIA REAL.\n\n" +
                "<b>Arte</b>: sprites originales del equipo CRATER.\n" +
                "<b>Tecnología</b>: Unity, TextMeshPro. Fuente Liberation Sans (licencia SIL OFL).");

            // ---------------- Ciencia real
            Add("SCIENCE_TITLE", "REAL SCIENCE", "CIENCIA REAL");
            Add("SCIENCE_GAME_VALUES", "Game values", "Valores del juego");
            Add("SCIENCE_GAME_VALUES_BUTTON", "Game values and labels", "Valores del juego y etiquetas");
            Add("SCIENCE_GAME_VALUES_INTRO", "To make the game fun, we made up some of the numbers ourselves. We always label them like this:", "Para que el juego sea divertido, algunas cifras las inventamos nosotros. Siempre las marcamos así:");
            Add("SCIENCE_TAG_REAL", "data backed by NASA, the CSA or a cited scientific publication.", "dato respaldado por NASA, la CSA o una publicación científica citada.");
            Add("SCIENCE_TAG_GAME", "a number created to balance the experience.", "cifra creada para equilibrar la experiencia.");
            Add("SCIENCE_TAG_PENDING", "data that still needs to be checked.", "dato que todavía requiere verificación.");
            Add("SCIENCE_NASA_STATUS", "NASA data status", "Estado de los datos de NASA");
            Add("SCIENCE_REAL_FACT", "REAL FACT", "DATO REAL");
            Add("SCIENCE_PENDING_FACT", "FACT PENDING VERIFICATION", "DATO PENDIENTE DE VERIFICACIÓN");
            Add("SCIENCE_SOURCE", "Source: {0}", "Fuente: {0}");
            Add("SCIENCE_GAME_VALUE", "GAME VALUE", "VALOR DEL JUEGO");

            // Resumen de valores del juego
            Add("GV_SOLAR", "{0}: +{1} energy per day", "{0}: +{1} energía por día");
            Add("GV_GREENHOUSE", "{0}: {1} energy, +{2} oxygen, +{3} food, {4} water", "{0}: {1} energía, +{2} oxígeno, +{3} comida, {4} agua");
            Add("GV_RECYCLER", "{0}: {1} energy, +{2} oxygen, {3} water", "{0}: {1} energía, +{2} oxígeno, {3} agua");
            Add("GV_HABITAT", "{0}: {1} energy; off or without power: Health −{2} per day", "{0}: {1} energía; apagada o sin energía: Salud −{2} por día");
            Add("GV_MINOR", "Minor events: about {0} per mission, each with two options that cost different resources", "Eventos menores: unos {0} por misión, cada uno con dos opciones que cuestan recursos distintos");
            Add("GV_ASTRONAUT", "Each astronaut: −{0} oxygen and −{1} food per day", "Cada astronauta: −{0} oxígeno y −{1} comida por día");
            Add("GV_STARVING", "No oxygen or no food: health −{0} per day", "Sin oxígeno o sin comida: salud −{0} por día");
            Add("GV_ROVER", "Rover: +{0} water per day (ice)", "Rover: +{0} agua por día (hielo)");
            Add("GV_RADIATION", "Radiation damage: Intensity × {0} × (1 − Blocking)", "Daño por radiación: Intensidad × {0} × (1 − Bloqueo)");
            Add("GV_DOSE", "Extra dose per storm: {0} mSv per intensity point without protection", "Dosis extra por tormenta: {0} mSv por punto de intensidad sin protección");
            Add("GV_METEOR", "Micrometeorite damage: {0}% for each exposed piece of equipment", "Avería por micrometeoritos: {0}% para cada equipo expuesto");
            Add("GV_EXTRA", "Extra events: radiation {0}%, micrometeorites {1}%, eclipse {2}%", "Eventos extra: radiación {0}%, micrometeoritos {1}%, eclipse {2}%");
            Add("GV_INTENSITY", "Intensity without data: low {0}%, medium {1}%, high {2}%", "Intensidad sin datos: baja {0}%, media {1}%, alta {2}%");
            Add("GV_DONKI", "DONKI flare class → intensity: X = 3, M = 2, others = 1", "Clase de llamarada DONKI → intensidad: X = 3, M = 2, otras = 1");

            // ---------------- Pantalla final
            Add("END_MISSION_COMPLETE", "MISSION COMPLETE!", "¡MISIÓN COMPLETADA!");
            Add("END_MISSION_FAILED", "MISSION FAILED", "MISIÓN FALLIDA");
            Add("END_DAYS_SURVIVED", "Days survived: {0} of {1}", "Días sobrevividos: {0} de {1}");
            Add("END_THE_CREW", "The crew", "La tripulación");
            Add("END_WIN_HEALTHY", "{0} came home healthy and happy.", "{0} volvieron sanos y felices.");
            Add("END_WIN_TIRED", "{0} came home tired, but safe.", "{0} volvieron cansados, pero a salvo.");
            Add("END_WIN_CLOSE", "{0} came home safe... just barely!", "{0} volvieron a salvo... ¡por muy poco!");
            Add("END_PLAY_AGAIN", "PLAY AGAIN", "JUGAR OTRA VEZ");
            Add("END_TRY_AGAIN", "TRY AGAIN", "INTENTAR DE NUEVO");

            // ---------------- Rovers y equipos
            Add("EQUIP_THE_PANELS", "the solar panels", "los paneles solares");
            Add("EQUIP_THE_ROVERS", "the rovers", "los rovers");

            // ---------------- Avisos temporales
            Add("NOTICE_PANELS_REPAIRED", "Solar panels repaired: back to 100%.", "Paneles solares reparados: vuelven al 100%.");
            Add("NOTICE_ROVERS_REPAIRED", "Rovers repaired: back to 100%.", "Rovers reparados: vuelven al 100%.");
            Add("NOTICE_PANELS_WORKSHOP", "The solar panels stay in the workshop for {0}.", "Los paneles solares quedan en el taller {0}.");
            Add("NOTICE_ROVERS_WORKSHOP", "The rovers stay in the workshop for {0}.", "Los rovers quedan en el taller {0}.");
            Add("NOTICE_BATTERY_CONNECTED", "Battery connected: +{0} energy.", "Batería conectada: +{0} de energía.");
            Add("NOTICE_ENERGY_WASTED", "The main battery was full: {0} energy was wasted.", "La batería principal se llenó: se desperdiciaron {0} de energía.");
            Add("NOTICE_CREW_TIRED", "The crew is tired of being shut in. Keep the HABITAT on this whole cycle so they can rest.", "La tripulación está cansada de estar encerrada. Mantén la VIVIENDA encendida todo este ciclo para que descansen.");
            Add("WARN_OXYGEN_OUT", "Oxygen will run out! Turn on the air recycler.", "¡Se acabará el oxígeno! Enciende el reciclador de aire.");
            Add("WARN_FOOD_OUT", "Food will run out! Turn on the greenhouse.", "¡Se acabará la comida! Enciende el invernadero.");
            Add("WARN_NO_POWER", "{0}: not enough energy.", "{0}: no hay energía suficiente.");
            Add("WARN_NO_WATER", "{0}: there's no water!", "{0}: ¡no hay agua!");
            Add("WARN_HABITAT_BAD", "The habitat is in bad shape: keep it on.", "La vivienda está en mal estado: mantenla encendida.");

            // ---------------- Datos de NASA
            Add("NASA_IDLE", "Not checked yet", "Sin consultar");
            Add("NASA_LOADING", "Checking NASA DONKI...", "Consultando NASA DONKI...");
            Add("NASA_LIVE", "LIVE: {0} flares recorded by DONKI between {1}.", "EN VIVO: {0} llamaradas registradas por DONKI entre el {1}.");
            Add("NASA_OFFLINE", "No NASA data: the game generates the intensity [IN-GAME VALUE].", "Sin datos de NASA: la intensidad la genera el juego [JUEGO].");
            Add("NASA_WINDOW", "{0}, {1} to {2}", "{0}, {1} a {2}");
            Add("NASA_SAMPLE_LIVE", "LIVE DATA from NASA DONKI", "DATOS EN VIVO de NASA DONKI");
            Add("NASA_SAMPLE_LIVE_DETAIL", "Based on a class {0} flare recorded on {1}. The day it reaches the base is a game simulation.", "Basado en una llamarada clase {0} registrada el {1}. El día de llegada a la base es una simulación del juego.");
            Add("NASA_SAMPLE_ARCHIVE_DETAIL", "Class {0} flare from {1} ({2}).", "Llamarada clase {0} del {1} ({2}).");
            Add("NASA_SAMPLE_GAME", "[IN-GAME VALUE] Intensity generated by the game", "[JUEGO] Intensidad generada por el juego");
            Add("NASA_SAMPLE_GAME_DETAIL", "No NASA data available: the game's chances were used (50% low, 35% medium, 15% high).", "No hay datos de NASA disponibles: se usaron las probabilidades del juego (50% baja, 35% media, 15% alta).");
        }
    }
}
