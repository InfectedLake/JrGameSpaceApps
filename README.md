# CRATER

**English** · [Español](#español)

---

## English

### What is CRATER?

CRATER is an educational **lunar-base management game** for children aged **7 to 12**. Players command a small base in a crater near the Moon's south pole and keep the crew alive by deciding which buildings to power, how to share limited energy, and how to prepare for real space phenomena.

The goal is to teach **real engineering trade-offs** (energy vs. oxygen, food vs. water, protection vs. production) through short, turn-based decisions.

| | |
|---|---|
| Genre | Turn-based strategy / resource management, educational |
| Platforms | Web (browser) and Windows (desktop) |
| Engine | Unity **6000.3.28f1** (Unity 6.3 LTS), 2D (URP) |
| Languages | English (default) and Spanish |
| Audience | Kids 7–12, teachers and families |
| Event | **NASA Space Apps Challenge 2026** — challenge *Build a Junior Astronaut Mission Trainer* |

### Features (implemented)

- **Lunar base management** on a single screen built from the team's original pixel art.
- **Four buildings** that can be turned on and off, and **four main resources** with a projection of how each will change before you end the day.
- **Turn system**: a mission lasts 4, 6 or 8 *cycles* of 4 days (16, 24 or 32 days). You win if the crew survives every day.
- **Three major events**: each one is announced **2 days in advance**, has its own music and sound effect, and asks for a strategic decision.
- **Minor events**: everyday incidents with two options, and each option costs a different resource.
- **Emergencies**: if energy or the habitat reach zero, you get an emergency decision instead of an instant loss.
- **Real science screen**: 13 cards explaining the science behind the game. Every number is labeled [REAL] or [IN-GAME VALUE], with sources.
- **Live NASA data**: solar-storm intensity can come from **NASA DONKI** (live solar-flare data), or from archived real storms when there is no connection.
- **Music and sound effects** with an AudioMixer (master, music and effects volume).
- **Bilingual**: English / Español, switchable at any time from the main menu or Settings, and remembered between sessions.
- **Pause menu** (resume, settings, main menu, exit) with confirmations, plus a settings screen (volume, fullscreen, language, NASA data, tutorial, delete save).
- **Director's tutorial** for new players and automatic **save / continue** at the start of each day.

### Buildings and resources

| Building | What it does |
|---|---|
| **Solar Panels** | Make the energy for the whole base. They make nothing during an eclipse, and less when damaged. |
| **Greenhouse** | Grows food (and a little oxygen). Uses water and energy. |
| **Air Recycler** | Makes oxygen from water and electricity. |
| **Habitat** | The crew's home. If it is off or unpowered, the crew's health drops. |

**Main resources** (top bar): **Energy**, **Oxygen**, **Food**, **Health**.

**Auxiliary variables** (not main resources): **water** (collected as ice by the rovers and used by the greenhouse and recycler), **batteries** (used during Earth's Shadow and emergencies) and the **habitat condition**.

### Events

| Event | What happens | Decision |
|---|---|---|
| **Solar Radiation Storm** | Particles from the Sun hit the base. Intensity can come from real NASA DONKI data. | Water walls, regolith shelter, indoor shelter or no protection. |
| **Micrometeorite Shower** | Tiny space rocks hit the equipment outside. | Protect the solar panels or the rovers, then decide how to repair damage. |
| **Earth's Shadow** | Earth blocks the Sun and the panels stop making energy. | Choose which sections to power with the limited batteries. |

**Minor events** (dust on the panels, air or water leaks, crop trouble, a temperature drop, a clogged filter) ask you to choose which resource to sacrifice.

### Controls

CRATER is played with the **mouse** (or **touch** in the browser on touch screens). There are no keyboard shortcuts.

| Action | How |
|---|---|
| Select a building | Click it. The camera zooms in and its panel opens. |
| Turn a building on or off | **TURN ON / TURN OFF** button in the building panel. |
| End the day | **END DAY** button (bottom right). |
| Make a decision | Click an option in the event window. |
| Pause / menu | **☰** button (top right). |
| Change language | **ENGLISH / ESPAÑOL** in the main menu (bottom left) or in Settings. |

### Technologies

- **Unity 6000.3.28f1**, **C#**, Universal Render Pipeline (2D).
- **Unity UI (uGUI) + TextMeshPro**. The interface is built in code by `UIManager`.
- **Input System** (UI input module).
- **AudioMixer** (`Assets/Data/Crater/CraterMixer.mixer`: Master → Music, SFX).
- **Custom localization system** (`Crater.Core.Loc`, about 400 keys in English and Spanish). The Unity Localization package is *not* used.
- **NASA DONKI API** (`https://ccmc.gsfc.nasa.gov/DONKI-API/`, FLR endpoint, no key needed; CORS-enabled, so it also works in the browser) through `UnityWebRequest`.
- **PlayerPrefs** for settings, language and the saved game.
- **Unity Test Framework** for the simulation tests.

### Project structure

```
JrGameSpaceApps/
├── Assets/
│   ├── Audios/              Original music (Musica/) and sound effects (Efectos/)
│   ├── Data/Crater/         ScriptableObjects: art set, balance, buildings, events, science cards, AudioMixer
│   ├── Reference/           Reference screenshot (not included in builds)
│   ├── Resources/DonkiArchive/  Archived real NASA DONKI solar-flare data (offline fallback)
│   ├── Scenes/              Crater.unity (the game) and SampleScene.unity (empty Unity template)
│   ├── Scripts/Crater/
│   │   ├── Core/            Pure C# simulation: rules, turns, events, calendar, localization tables
│   │   ├── Data/            ScriptableObject types and default content
│   │   ├── Services/        NASA DONKI service
│   │   ├── Systems/         Game managers: GameManager, TurnManager, EventManager, AudioManager, SaveSystem…
│   │   ├── UI/              Interface built in code: UIManager (+ Screens, Modals, Pause, Language), base view, effects
│   │   └── Editor/          Editor tools: scene/data builder and build menu
│   ├── Settings/            URP and Input System settings
│   ├── Sprites/             Original pixel art by the team (and reference screenshots, not included in builds)
│   ├── Tests/EditMode/      Simulation tests
│   └── TextMesh Pro/        TextMeshPro resources and fonts
├── Docs/                    Technical design (Spanish), cleanup report and itch.io publishing guide
├── Packages/                manifest.json and packages-lock.json
├── ProjectSettings/
└── README.md
```

**Architecture in one paragraph:** `Core` holds the whole simulation in plain C# (no Unity), so the same `TurnSimulator.Simulate` produces both the on-screen projection and the real end of day, and it can be tested outside Unity. `Systems` are MonoBehaviours that apply decisions and talk to the UI through events. `UI` builds every screen in code from the original sprites.

### Running the project (developers)

1. Install **Unity Hub** and the **Unity 6000.3.28f1** editor. Add the modules you need: **Web Build Support** and/or **Windows Build Support** (Windows Mono comes with the Windows editor).
2. In Unity Hub choose **Add → Add project from disk** and select the repository folder. Unity restores the packages from `Packages/manifest.json` on first open (internet needed once).
3. Open **`Assets/Scenes/Crater.unity`** and press **Play**. The game starts in the main menu (**START → NEW GAME**).
4. **Tests:** *Window → General → Test Runner → EditMode → Run All* (`Assets/Tests/EditMode/CraterSimulationTests.cs`).
5. **Rebuilding the scene or data** (only if needed): menu **CRATER → Construir escena y datos (todo)** / **Asignar arte original**.

No external SDKs or keys are required.

### Builds and distribution

Use the menu **CRATER → Builds**:

- **Aplicar configuración de distribución** sets `Crater.unity` as the only build scene, sets the product name to *CRATER*, the Web size to 1280×720, and Web compression to **Gzip + Decompression Fallback**.
- **Web (itch.io HTML5)** outputs `Builds/Web/` (`index.html`, `Build/`, `TemplateData/`).
- **Windows 64-bit** outputs `Builds/Windows/` (`CRATER.exe`, `CRATER_Data/`, `UnityPlayer.dll`, …).

`Builds/` is ignored by git. The full step-by-step itch.io guide (web, download, desktop app and butler) is in **[Docs/ITCHIO_PUBLISHING.md](Docs/ITCHIO_PUBLISHING.md)**.

### Status and known limitations

- **Platforms:** Windows and Web builds have been generated and tested locally (Windows: the exe runs outside the Editor; Web: served locally, plays in the browser, language kept after reload). They have not been uploaded to itch.io yet. macOS and Linux are not prepared (they need the matching build modules and a machine to test on).
- **Font:** the "…" (ellipsis) character is missing from the TextMeshPro font. Long texts are cut without "…", but no visible text is cut in the screens we tested.
- **Credits:** NASA and the CSA do not endorse this game. Data sources are listed in the in-game Credits and Real Science screens.

### Team

CRATER was made for NASA Space Apps Challenge 2026 by the CRATER team (original pixel art, music and sound by the team).

---

## Español

### ¿Qué es CRATER?

CRATER es un juego educativo de **administración de una base lunar** para niñas y niños de **7 a 12 años**. El jugador dirige una pequeña base en un cráter cerca del polo sur de la Luna. Para mantener viva a la tripulación decide qué edificios encender, cómo repartir la energía limitada y cómo prepararse ante fenómenos espaciales reales.

El objetivo es enseñar **compromisos reales de ingeniería** (energía contra oxígeno, comida contra agua, protección contra producción) mediante decisiones breves por turnos.

| | |
|---|---|
| Género | Estrategia por turnos y gestión de recursos, educativo |
| Plataformas | Web (navegador) y Windows (escritorio) |
| Motor | Unity **6000.3.28f1** (Unity 6.3 LTS), 2D (URP) |
| Idiomas | Inglés (predeterminado) y español |
| Público | Niños de 7 a 12 años, docentes y familias |
| Evento | **NASA Space Apps Challenge 2026**, desafío *Build a Junior Astronaut Mission Trainer* |

### Características (implementadas)

- **Administración de una base lunar** en una sola pantalla, con el pixel art original del equipo.
- **Cuatro edificios** que se encienden y apagan, y **cuatro recursos principales**, con una proyección de cuánto cambiará cada uno antes de terminar el día.
- **Sistema de turnos:** una misión dura 4, 6 u 8 *ciclos* de 4 días (16, 24 o 32 días). Se gana si la tripulación sobrevive todos los días.
- **Tres acontecimientos principales:** cada uno se anuncia **con 2 días de anticipación**, tiene su música y su efecto de sonido, y pide una decisión estratégica.
- **Eventos menores:** incidentes cotidianos con dos opciones, y cada opción cuesta un recurso distinto.
- **Emergencias:** si la energía o la vivienda llegan a cero, aparece una decisión de emergencia en lugar de perder de inmediato.
- **Ciencia real:** 13 fichas con la ciencia detrás del juego. Cada cifra está marcada como [REAL] o [JUEGO], con sus fuentes.
- **Datos reales de NASA:** la intensidad de las tormentas solares puede venir de **NASA DONKI** (llamaradas en vivo) o de tormentas reales archivadas si no hay conexión.
- **Música y efectos de sonido** con AudioMixer (volumen general, música y efectos).
- **Bilingüe:** English / Español. Se cambia en cualquier momento desde el menú principal u Opciones y se recuerda entre sesiones.
- **Menú de pausa** (continuar, opciones, menú principal, salir) con confirmaciones, y pantalla de **opciones** (volumen, pantalla completa, idioma, datos de NASA, tutorial, borrar partida).
- **Tutorial del director** para nuevos jugadores y **guardado y continuación** automáticos al inicio de cada día.

### Edificios y recursos

| Edificio | Qué hace |
|---|---|
| **Panel solar** | Produce la energía de toda la base. No produce durante un eclipse, y produce menos si está dañado. |
| **Invernadero** | Cultiva comida (y un poco de oxígeno). Usa agua y energía. |
| **Reciclador de aire** | Fabrica oxígeno con agua y electricidad. |
| **Vivienda** | El hogar de la tripulación. Apagada o sin energía, baja la salud. |

**Recursos principales** (barra superior): **Energía**, **Oxígeno**, **Comida**, **Salud**.

**Variables auxiliares** (no son recursos principales): **agua** (los rovers la recogen como hielo; la usan el invernadero y el reciclador), **baterías** (para la Sombra de la Tierra y las emergencias) y el **estado de la vivienda**.

### Acontecimientos

| Acontecimiento | Qué pasa | Decisión |
|---|---|---|
| **Ráfaga de radiación solar** | Partículas del Sol golpean la base. La intensidad puede venir de datos reales de NASA DONKI. | Paredes de agua, refugio de regolito, refugio interior o sin protección. |
| **Lluvia de micrometeoritos** | Pequeñas rocas espaciales golpean el equipo exterior. | Proteger los paneles o los rovers, y después decidir cómo reparar los daños. |
| **Sombra de la Tierra** | La Tierra tapa al Sol y los paneles dejan de producir. | Elegir qué secciones alimentar con las pocas baterías. |

Los **eventos menores** (polvo en los paneles, fugas de aire o agua, problemas en los cultivos, frío, un filtro saturado) obligan a elegir qué recurso sacrificar.

### Controles

CRATER se juega con el **mouse** (o **tocando la pantalla** en el navegador con pantallas táctiles). No hay atajos de teclado.

| Acción | Cómo |
|---|---|
| Seleccionar un edificio | Hacer clic sobre él. La cámara se acerca y se abre su panel. |
| Encender o apagar un edificio | Botón **ENCENDER / APAGAR** del panel. |
| Terminar el día | Botón **TERMINAR DÍA** (abajo a la derecha). |
| Elegir una decisión | Hacer clic en una opción de la ventana del evento. |
| Pausa / menú | Botón **☰** (arriba a la derecha). |
| Cambiar idioma | **ENGLISH / ESPAÑOL** en el menú principal (abajo a la izquierda) o en Opciones. |

### Tecnologías

- **Unity 6000.3.28f1**, **C#**, Universal Render Pipeline (2D).
- **Unity UI (uGUI) + TextMeshPro**. La interfaz se construye por código en `UIManager`.
- **Input System** (módulo de entrada de la UI).
- **AudioMixer** (`Assets/Data/Crater/CraterMixer.mixer`: Master → Music, SFX).
- **Sistema de localización propio** (`Crater.Core.Loc`, unas 400 claves en inglés y español). *No* se usa el paquete Unity Localization.
- **API NASA DONKI** (`https://ccmc.gsfc.nasa.gov/DONKI-API/`, endpoint FLR, sin clave; permite CORS, así que funciona también en el navegador) mediante `UnityWebRequest`.
- **PlayerPrefs** para las opciones, el idioma y la partida guardada.
- **Unity Test Framework** para las pruebas de la simulación.

### Estructura del proyecto

El árbol de carpetas está en la versión en inglés de arriba. Resumen:

- **`Assets/Scripts/Crater/Core`:** toda la simulación en C# puro (reglas, turnos, eventos, calendario y tablas de idioma).
- **`Systems`:** los gestores del juego.
- **`UI`:** la interfaz construida por código.
- **`Data`:** los tipos de ScriptableObject.
- **`Services`:** el servicio de NASA DONKI.
- **`Editor`:** las herramientas de escena y de builds.
- **`Assets/Data/Crater`:** los datos editables (balance, edificios, eventos, ciencia, arte y mixer).
- **`Assets/Sprites` y `Assets/Audios`:** el arte y el audio originales del equipo.

**Arquitectura:** `Core` contiene la simulación sin Unity. La misma función `TurnSimulator.Simulate` calcula tanto la proyección que se ve en pantalla como el final real del día, y se puede probar fuera de Unity. `Systems` aplica las decisiones y avisa a la interfaz mediante eventos. `UI` construye todas las pantallas por código con los sprites originales.

### Ejecutar el proyecto (desarrolladores)

1. Instala **Unity Hub** y el editor **Unity 6000.3.28f1**, con los módulos que necesites: **Web Build Support** o **Windows Build Support** (Windows Mono viene con el editor de Windows).
2. En Unity Hub elige **Add → Add project from disk** y selecciona la carpeta del repositorio. Unity descarga los paquetes de `Packages/manifest.json` la primera vez (necesita internet una vez).
3. Abre **`Assets/Scenes/Crater.unity`** y pulsa **Play**. El juego empieza en el menú principal (**INICIO → NUEVA PARTIDA**).
4. **Pruebas:** *Window → General → Test Runner → EditMode → Run All* (`Assets/Tests/EditMode/CraterSimulationTests.cs`).
5. **Reconstruir la escena o los datos** (solo si hace falta): menú **CRATER → Construir escena y datos (todo)** o **Asignar arte original**.

No se necesitan SDK externos ni claves.

### Builds y distribución

Menú **CRATER → Builds**:

- **Aplicar configuración de distribución:** deja `Crater.unity` como única escena, el nombre del producto *CRATER*, la ventana Web en 1280×720 y la compresión Web en **Gzip + Decompression Fallback**.
- **Web (itch.io HTML5):** genera `Builds/Web/` (`index.html`, `Build/`, `TemplateData/`).
- **Windows 64-bit:** genera `Builds/Windows/` (`CRATER.exe`, `CRATER_Data/`, `UnityPlayer.dll`, …).

La carpeta `Builds/` está excluida de git. La guía completa para itch.io (web, descarga, aplicación de escritorio y butler) está en **[Docs/ITCHIO_PUBLISHING.md](Docs/ITCHIO_PUBLISHING.md)**.

### Estado y limitaciones conocidas

- **Plataformas:** los builds de Windows y Web se generaron y probaron en local (Windows: el exe abre fuera del Editor; Web: servido en local, se juega en el navegador y conserva el idioma al recargar). Todavía no se subieron a itch.io. macOS y Linux no están preparados (necesitan sus módulos de build y un equipo donde probarlos).
- **Fuente:** a la fuente de TextMeshPro le falta el carácter "…" (puntos suspensivos). Los textos largos se cortan sin "…", pero ningún texto visible se corta en las pantallas probadas.
- **Créditos:** NASA y la CSA no respaldan este juego. Las fuentes de datos están en las pantallas de Créditos y Ciencia real.

### Equipo

CRATER fue creado para el NASA Space Apps Challenge 2026 por el equipo CRATER (pixel art, música y sonidos originales del equipo).
