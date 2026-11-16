# CRATER — Project cleanup report / Informe de limpieza

**English** · [Español](#español)

---

## English

Date: 2026-11-16 · Branch: `crater-localizacion` (nothing committed by this cleanup).

### Method

All 58 C# files under `Assets/` (excluding the TextMesh Pro package folder) were checked. For each one I looked at:

- **Declared types** and their use in every other script (by type name, with comments removed).
- **Serialized references**: the script's GUID (from its `.meta`) searched in every `.unity`, `.prefab`, `.asset`, `.controller`, `.anim` and `.mixer` file under `Assets/` and `ProjectSettings/`.
- **Indirect use**: reflection, `Resources.Load`, `SendMessage`/`Invoke` by name, string references, `[MenuItem]` attributes, extension methods, test runner discovery.
- **Assembly definitions**: the game code lives in the `Crater` assembly (`Crater.asmdef`), editor tools in `Crater.Editor` and tests in `Crater.Tests`. None of them references the default `Assembly-CSharp`.

No prefabs, Prefab Variants, Animation Events or Addressables exist in the project. All UI is built in code by `UIManager`.

### Classification

| Category | Files |
|---|---|
| **A. In use** | 51 files in `Assets/Scripts/Crater/` and `Assets/Tests/` (all of them; see the notes below) |
| **B. Looks unused, but used indirectly (kept)** | `Data/LocalizedContent.cs` and `UI/LocText.cs` (extension methods, so the class name never appears at the call site), `Editor/CraterSceneBuilder.cs` and `Editor/CraterBuild.cs` (`[MenuItem]` editor tools), `Tests/EditMode/CraterSimulationTests.cs` (found by the Test Runner), `DonkiSep` in `NASADataService.cs` (filled by `JsonUtility` when reading the archive JSON files) |
| **C. Obsolete, no verifiable references (deleted)** | The first prototype, see below |
| **D. Test or prototype code** | The same first prototype |
| **E. Third party** | TextMesh Pro (`Assets/TextMesh Pro/`) and Unity packages (`Packages/`). Not touched. |

### Deleted

**1. The first prototype (Nov 14).** It was a self-contained set, used only by itself:

| File | Referenced only by |
|---|---|
| `Assets/Scripts/Logica/EdificioData.cs` | `Assets/Data/{Invernadero,PanelSolar,Reciclador,Vivienda}.asset` |
| `Assets/Scripts/Logica/EstadoJuego.cs` | prototype `TurnManager` only |
| `Assets/Scripts/Logica/TurnManager.cs` (global namespace, different from `Crater.Systems.TurnManager`) | `MainScene.unity`, `TestMainScene.unity` |
| `Assets/Scripts/UI/EdificioUI.cs`, `FilaRecursos.cs`, `HudController.cs` | `MainScene.unity` |
| `Assets/Data/Invernadero.asset`, `PanelSolar.asset`, `Reciclador.asset`, `Vivienda.asset` | `MainScene.unity`, `TestMainScene.unity` |
| `Assets/Scenes/MainScene.unity`, `TestMainScene.unity` | nothing (not in Build Settings, not loaded by code) |

**Evidence:**
- These classes are in `Assembly-CSharp`, and the `Crater` assemblies cannot reference `Assembly-CSharp`, so the final game cannot depend on them.
- Their GUIDs appear only inside this group.
- No string or reflection use exists.
- The whole group was removed together, each file with its `.meta`, so no scene is left with a *Missing Script*.
- Everything is still in git history (commits `1f655cf` … `91417d7`).

**2. Dead members inside the current code** (left over from earlier UI iterations, 0 call sites):
- `DefaultContent.ArchiveLabel`, replaced by the localization key `ARCHIVE_LABEL`.
- `RadiationSystem.IntensityName`.
- `RoverManager.StatusText`, `RoverManager.EquipmentStatus` and the properties they used (`IsCollecting`, `IsBuilding`, `ExpectedIce`). They belonged to the old rover status panel.
- The 10 localization keys that only those members used (`ROVER_*`, `EQUIP_REPAIRING`, `EQUIP_BROKEN`, `INTENSITY_*`).

After the cleanup the project compiles (0 errors), the 30 simulation tests pass, and the Windows and Web builds succeed.

### Kept on purpose

| Item | Why it was kept |
|---|---|
| `Assets/Scenes/SampleScene.unity` | Empty Unity template scene (camera and 2D light, no scripts). `ProjectSettings` uses it as the *template default scene*. It is removed from the build list by **CRATER → Builds → Aplicar configuración de distribución**. |
| `Assets/Reference/Screenshot 2026-11-15 011911.png` and the *"ejemplo de como…"* screenshots in `Assets/Sprites/` | The team uses them as layout references (`Assets/Sprites/README.txt`). Nothing references them, so Unity does **not** include them in builds. |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity`, `Lit2DSceneTemplate.scenetemplate` | URP 2D template files, not included in builds. |
| `DefaultContent` Spanish texts and `TutorialSteps` | Fallback content used when an asset has no localization key, and by the data builder. |
| `Assets/Audios/**` | Original audio. All 7 clips are referenced by `ArtSet.asset`. |

### Packages (review manually, not removed)

The game code does **not** use these packages: `2d.animation`, `2d.aseprite`, `2d.psdimporter`, `2d.spriteshape`, `2d.tilemap`, `2d.tilemap.extras`, `2d.tooling`, `collab-proxy`, `multiplayer.center`, `timeline`, `visualscripting`.

They came with the URP 2D project template. I did not remove them because:
- Some of them are dependencies of others.
- Removing packages while the editor is open, and right before the deadline, is a risk.
- The prompt asked not to change packages in this phase.

Removing `visualscripting`, `timeline` and the unused 2D importers would make the builds a little smaller and faster to compile. Do it in a separate branch, and run the tests and both builds afterwards.

### Assets and size

- **Windows build:** 172 MB. The biggest part is `sharedassets0` (63 MB): the sprites are imported **without compression** on purpose, so the pixel art stays exact. `Managed` (33 MB) is the scripting runtime plus the package assemblies.
- **Not changed:** texture compression, because it would alter the pixel art.
- **Audio:** music streams (`Streaming`) and effects are decompressed on load and preloaded. All clips are Vorbis. The AudioMixer is used by every source.

### .gitignore

The existing file already ignores `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`, `Recordings/`, IDE projects (`*.csproj`, `*.sln`, `*.slnx`) and `.vs/`. It keeps `Assets/`, `Packages/`, `ProjectSettings/` and every `.meta` file. **No change was needed.**

### Manual review checklist

- [ ] Open the project and check that the console shows no *Missing Script* and no compile errors.
- [ ] Run **CRATER → Builds → Aplicar configuración de distribución** once, then save the project, so the build scene and Web settings are stored in `ProjectSettings/`.
- [ ] Decide about the unused packages listed above (optional).

---

## Español

Fecha: 16-nov-2026 · Rama: `crater-localizacion` (esta limpieza no hizo ningún commit).

### Método

Se revisaron los 58 archivos C# de `Assets/` (sin contar el paquete TextMesh Pro). Para cada uno se revisó:

- **Tipos declarados** y su uso en todos los demás scripts (por nombre de tipo, sin comentarios).
- **Referencias serializadas:** el GUID del script (de su `.meta`) buscado en todos los `.unity`, `.prefab`, `.asset`, `.controller`, `.anim` y `.mixer` de `Assets/` y `ProjectSettings/`.
- **Usos indirectos:** reflexión, `Resources.Load`, `SendMessage`/`Invoke` por nombre, referencias por texto, atributos `[MenuItem]`, métodos de extensión y descubrimiento del Test Runner.
- **Assembly definitions:** el juego está en el ensamblado `Crater`, las herramientas de editor en `Crater.Editor` y las pruebas en `Crater.Tests`. Ninguno referencia `Assembly-CSharp`.

El proyecto no tiene prefabs, Prefab Variants, Animation Events ni Addressables. Toda la interfaz la construye `UIManager` por código.

### Clasificación

| Categoría | Archivos |
|---|---|
| **A. En uso** | Los 51 archivos de `Assets/Scripts/Crater/` y `Assets/Tests/` (todos; ver notas) |
| **B. Parecen sin uso, pero se usan de forma indirecta (se conservan)** | `LocalizedContent.cs` y `LocText.cs` (métodos de extensión: el nombre de la clase no aparece donde se usan), `CraterSceneBuilder.cs` y `CraterBuild.cs` (menús del editor `[MenuItem]`), `CraterSimulationTests.cs` (lo encuentra el Test Runner), `DonkiSep` (lo llena `JsonUtility` al leer los JSON archivados) |
| **C. Obsoletos sin referencias verificables (eliminados)** | El primer prototipo, ver abajo |
| **D. Pruebas o prototipos** | El mismo primer prototipo |
| **E. Terceros** | TextMesh Pro y los paquetes de Unity. No se tocaron. |

### Eliminado

**1. El primer prototipo (14 de noviembre):** un conjunto cerrado que solo se usaba a sí mismo.
- **Scripts:** `Assets/Scripts/Logica/` (`EdificioData`, `EstadoJuego`, `TurnManager` global) y `Assets/Scripts/UI/` (`EdificioUI`, `FilaRecursos`, `HudController`).
- **Datos:** los 4 assets de `Assets/Data/` (`Invernadero`, `PanelSolar`, `Reciclador`, `Vivienda`).
- **Escenas:** `MainScene.unity` y `TestMainScene.unity`.

**Pruebas de que no se usaba:**
- Esas clases están en `Assembly-CSharp`, que los ensamblados `Crater` no pueden referenciar, así que el juego final no puede depender de ellas.
- Sus GUID solo aparecen dentro de ese grupo.
- No hay usos por texto ni por reflexión.
- Se eliminó todo junto, cada archivo con su `.meta`, así que ninguna escena queda con *Missing Script*.
- Sigue disponible en el historial de git (commits `1f655cf` … `91417d7`).

**2. Miembros muertos del código actual** (0 usos, restos de iteraciones anteriores):
- `DefaultContent.ArchiveLabel`.
- `RadiationSystem.IntensityName`.
- `RoverManager.StatusText`, `RoverManager.EquipmentStatus` y sus propiedades auxiliares. Eran del antiguo panel de estado de los rovers.
- Las 10 claves de idioma que solo usaban esos miembros.

Después de la limpieza el proyecto compila sin errores, pasan las 30 pruebas y los builds de Windows y Web se generan correctamente.

### Conservado a propósito

- **`SampleScene.unity`:** escena vacía de la plantilla, que ProjectSettings usa como *template default scene*. Sale de la lista de build con **CRATER → Builds → Aplicar configuración de distribución**.
- **Capturas de referencia** (`Assets/Reference/` y los *"ejemplo de como…"* de `Assets/Sprites/`): el equipo las usa como guía. Como nada las referencia, **no entran en los builds**.
- **Plantillas de URP 2D**, los **textos de respaldo** en español de `DefaultContent` y **todo el audio original** (los 7 clips están referenciados en `ArtSet.asset`).

### Paquetes (revisión manual, no se eliminaron)

El código del juego no usa estos paquetes: `2d.animation`, `2d.aseprite`, `2d.psdimporter`, `2d.spriteshape`, `2d.tilemap`, `2d.tilemap.extras`, `2d.tooling`, `collab-proxy`, `multiplayer.center`, `timeline` y `visualscripting`.

Vienen de la plantilla URP 2D. No se quitaron porque:
- Algunos son dependencias de otros.
- Quitar paquetes con el editor abierto y cerca de la entrega es un riesgo.
- El prompt pedía no cambiar paquetes en esta fase.

Quitarlos en otra rama reduciría un poco el tamaño de los builds; después habría que repetir las pruebas y los dos builds.

### Assets y tamaño

- **Build de Windows:** 172 MB. Lo más grande son los sprites, importados **sin compresión** a propósito para no alterar el pixel art (63 MB), y el runtime de scripts con los ensamblados de los paquetes (33 MB).
- **No se cambió:** la compresión de texturas, porque alteraría el pixel art.
- **Audio:** sin cambios. La música se reproduce en streaming, los efectos se precargan y todo pasa por el AudioMixer.

### .gitignore

Ya excluía `Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/` y los proyectos de IDE, y conserva `Assets/`, `Packages/`, `ProjectSettings/` y los `.meta`. **No hizo falta cambiarlo.**

### Revisión manual pendiente

- [ ] Abrir el proyecto y confirmar que la consola no muestra *Missing Script* ni errores de compilación.
- [ ] Ejecutar una vez **CRATER → Builds → Aplicar configuración de distribución** y guardar el proyecto, para que la escena de build y la configuración Web queden en `ProjectSettings/`.
- [ ] Decidir si se quitan los paquetes sin uso (opcional).
