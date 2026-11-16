# Publishing CRATER on itch.io / Publicar CRATER en itch.io

**English** · [Español](#español)

> Placeholders: `USERNAME` = your itch.io user name, `crater` = the project slug (the last part of the page URL `https://USERNAME.itch.io/crater`). Never share your password or API key; butler opens the browser to log in.

---

## English

### 1. Introduction

CRATER is published on **one itch.io page with two separate distributions**:

| Distribution | What players do | File you upload |
|---|---|---|
| **Web (HTML5)** | Press **Run game** and play in the browser | `CRATER_Web.zip` (with `index.html` at the root) |
| **Windows download** | Download, unzip and run `CRATER.exe` (or install with the itch app) | `CRATER_Windows.zip` |

They are different builds with different settings. Do not mix their files.

### 2. Requirements

- **Unity 6000.3.28f1** (Unity 6.3 LTS) with these modules (Unity Hub → *Installs* → ⚙ → *Add modules*):
  - **Web Build Support**, for the browser version. In Unity 6 the platform is called **Web** (formerly *WebGL*).
  - **Windows Build Support (Mono)** is included with the Windows editor. **IL2CPP** is optional.
- An itch.io account (https://itch.io/register).
- Optional: the **itch app** (https://itch.io/app) and/or **butler** (https://itchio.itch.io/butler).

### 3. Web build

1. Open the project and run **CRATER → Builds → Aplicar configuración de distribución** once (then *File → Save Project*). It sets:
   - **Build scene:** `Assets/Scenes/Crater.unity` only.
   - **Product name:** `CRATER`.
   - **Web resolution:** 1280 × 720 (16:9, the game is designed at 1920 × 1080 and scales).
   - **Compression:** *Player Settings → Web → Publishing Settings → Compression Format* = **Gzip**, with **Decompression Fallback** enabled.
     - *Why Gzip:* it works in every browser over HTTP and HTTPS, and itch.io detects gzip content and sends `Content-Encoding: gzip`.
     - *Why the fallback:* if a server does not send that header, Unity's loader decompresses the files in JavaScript, so the game still loads. The cost is a slightly larger loader and no WebAssembly streaming.
2. Run **CRATER → Builds → Web (itch.io HTML5)**. The output goes to `Builds/Web/`:
   ```
   Builds/Web/
   ├── index.html
   ├── Build/           (Web.loader.js, Web.framework.js.unityweb, Web.wasm.unityweb, Web.data.unityweb)
   └── TemplateData/    (style.css, icons…)
   ```
   The build step also enables `autoSyncPersistentDataPath: true` in `index.html`, so the saved game (a file in `persistentDataPath`) is written to the browser's IndexedDB and survives a page reload.
3. **Test locally.** Opening `index.html` by double click does not work (browsers block `file://`). Serve the folder instead:
   - From Unity: *File → Build Profiles → Web → Build And Run* (Unity starts a local server).
   - Or any static server, for example `npx http-server Builds/Web -p 8080`, then open `http://localhost:8080`.
4. **Make the ZIP.** Open `Builds/Web/`, select **its contents** (`index.html`, `Build`, `TemplateData`) and compress them. `index.html` must be at the root of the ZIP, **not** inside an extra folder:
   ```
   CRATER_Web.zip
   ├── index.html
   ├── Build/
   └── TemplateData/
   ```
   Command (Windows 10/11, run inside `Builds/Web`): `tar -a -c -f ../CRATER_Web.zip index.html Build TemplateData`. **Do not use PowerShell `Compress-Archive`**: on Windows PowerShell 5.1 it writes paths with ``, and itch.io then cannot find `Build/` (index.html loads but every game file gives 404).

itch.io limits for HTML5 ZIPs (https://itch.io/docs/creators/html5): at most **1,000 files**, **500 MB** uncompressed in total, **200 MB** per file, and paths of at most **240 characters**. File names are case sensitive. CRATER's Web build is far below these limits.

### 4. Publishing the Web version from the website

1. Log in to itch.io and open the **Dashboard** (https://itch.io/dashboard), then click **Create new project**.
2. **Title:** `CRATER`. **Project URL:** `crater` (or another free slug).
3. **Short description:** for example *"Run a lunar base and keep your crew alive! An educational space strategy game for kids 7–12, in English and Spanish."*
4. **Classification:** *Games*. **Kind of project:** **HTML** (it must be HTML, not the default *Downloadable*, for the browser player to appear).
5. **Pricing:** *No payments* or *Donate* (on itch.io, HTML5 games can only take payments as donations).
6. **Uploads → Upload files:** choose `CRATER_Web.zip` and tick **"This file will be played in the browser"**.
7. **Embed options:**
   - **Embed in page**, viewport **1280 × 720** (or *Click to launch in fullscreen*).
   - Turn on **Fullscreen button**.
   - Turn off **Scrollbars**.
   - *Mobile friendly* is optional (the game uses the mouse or touch).
8. **Details:**
   - **Genre:** *Strategy* or *Educational*.
   - **Tags** (up to 10): `educational`, `space`, `strategy`, `management`, `nasa`, `kids`, `turn-based`, `pixel-art`, `bilingual`, `spanish`.
   - **Languages:** English, Spanish.
   - **Input:** Mouse, Touchscreen.
9. **Media:**
   - **Cover image:** 630 × 500 (aspect ratio 315:250).
   - **Screenshots:** 3–5 of the menu, base, events and Real Science.
   - Optional: a trailer or gameplay video link.
10. **Visibility:** leave it as **Draft / private**, click **Save**, open **View page** and test the game. When it works, change it to **Public** and save again.

### 5. Windows build

1. Run **CRATER → Builds → Windows 64-bit**. It applies the same distribution settings and builds **x86_64** (64-bit Windows, Mono backend) into `Builds/Windows/`:
   ```
   Builds/Windows/
   ├── CRATER.exe
   ├── CRATER_Data/
   ├── MonoBleedingEdge/
   ├── D3D12/
   ├── UnityPlayer.dll
   ├── UnityCrashHandler64.exe
   └── CRATER_BurstDebugInformation_DoNotShip/   ← do NOT include in the ZIP
   ```
2. **Test outside the Editor:**
   - Copy the folder to another location (for example the Desktop) and run `CRATER.exe`.
   - Play a mission, change the language, close and reopen the game. The language, the volume and the saved game must still be there.
   - The game starts in fullscreen; use **Settings → Fullscreen** to switch to windowed.
3. **Make the ZIP** with everything **except** `CRATER_BurstDebugInformation_DoNotShip`. Never upload only the `.exe`.
   ```powershell
   cd Builds/Windows; tar -a -c -f ../CRATER_Windows.zip --exclude "*_DoNotShip*" *
   ```

### 6. Publishing the Windows download from the website

1. Edit the same project (**Dashboard → CRATER → Edit**) and keep *Kind of project = HTML*. A project can have browser and download files at the same time.
2. **Upload files** → `CRATER_Windows.zip`. Do **not** tick *played in the browser*. Tick the **Windows** platform icon.
3. Optional: name the file (for example *CRATER for Windows (64-bit)*).
4. **Save**. On the page, players now see **Run game** (Web) and a **Download** button (Windows).

Windows may show *SmartScreen* ("Windows protected your PC") because the exe is not signed. Players click **More info → Run anyway**. Mention it on the page.

### 7. The itch app (players and creators)

**For players.** The itch app runs on **Windows, macOS and Linux** (https://itch.io/app):
1. Install it from https://itch.io/app and log in with the itch.io account.
2. Search for **CRATER** (or open the project page and choose *Install*).
3. Click **Install**. The app downloads and extracts the Windows build, and the button changes to **Launch**.
4. **Updates:** when a new build is pushed, the app downloads only the changes (when the build was uploaded with butler or with the app). If an upload was made as a plain ZIP on the website, players can still reinstall it.

**For creators.** The itch app **v26.12.0 or later** includes a graphical interface for pushing builds with butler, and it keeps its bundled butler up to date (https://itch.io/docs/butler/installing). If your installed version does not show that option, use butler from the command line (section 8). The steps are the same: choose the project, the channel and the folder, check the changes, then push.

### 8. Publishing with butler (optional)

butler uploads only the files that changed and keeps a version history per **channel**.

1. **Install:** download it from https://itchio.itch.io/butler, unzip it (for example to `C:\butler`), add it to `PATH`, and check it with `butler version`.
2. **Log in** (it opens the browser; credentials are saved locally):
   ```bash
   butler login
   ```
3. **Find the project ID:** `USERNAME/crater`, the same as in the page URL.
4. **Push the builds** (folders or ZIP files both work):
   ```bash
   butler push Builds/Web USERNAME/crater:html5 --userversion 1.0.0
   ```
   ```bash
   butler push Builds/Windows USERNAME/crater:windows --userversion 1.0.0 --ignore "*_DoNotShip"
   ```
5. **Channel names:**
   - You can choose them freely. A name containing `win`/`windows`, `linux` or `mac`/`osx` sets that platform tag.
   - The Web channel must be marked **"This file will be played in the browser"** (HTML5) on the project's **Edit** page after the first push.
   - Check that the `windows` channel shows the Windows icon.
6. **Preview a push without uploading:** `butler push --dry-run …` or `butler push-preview …`.

Do not paste API keys in chat or in the repository. For CI, use the `BUTLER_API_KEY` environment variable.

### 9. Updating versions

1. Increase the version: *Project Settings → Player → Version* (for example `1.0.1`).
2. Rebuild (**CRATER → Builds**), test, and upload again:
   - **Website:** upload the new ZIP and delete or replace the old file.
   - **butler:** push again to the same channel with a new `--userversion`.
3. **Web saves are not shared between versions.** Each upload is served from a new URL, and the browser keeps PlayerPrefs and saved games per URL. After a Web update, players start again with the default settings (English). The Windows version keeps its data between updates (registry and `%USERPROFILE%\AppData\LocalLow\<company>\CRATER`).

### 10. Checklist before publishing

**Web**
- [ ] `index.html` is at the root of `CRATER_Web.zip`.
- [ ] The page loads with no errors in the browser console (F12), and there are no decompression errors.
- [ ] The game fills the frame (1280 × 720) and the fullscreen button works.
- [ ] Music starts after the first click (browsers block audio until the user interacts).
- [ ] You can play a mission, and a major event shows its music, sound and window.
- [ ] The game starts in **English**, ESPAÑOL works, and after a reload it is still in Spanish.

**Windows**
- [ ] `CRATER.exe` opens from a different folder, with all files included and without `_DoNotShip`.
- [ ] Audio works, and a mission can be completed (win or lose screen).
- [ ] Language, volume and saved game persist after closing and reopening.

**Project**
- [ ] The project compiles, the console shows no *Missing Script*, and the EditMode tests pass.

### 11. Troubleshooting

| Problem | Fix |
|---|---|
| itch.io says it cannot find `index.html` | The ZIP has an extra parent folder. Zip the **contents** of `Builds/Web`, not the folder. |
| "Unable to parse Build/…" or a decompression error | Check that *Compression = Gzip* and *Decompression Fallback* is on (**CRATER → Builds → Aplicar configuración…**), then rebuild. Brotli without the fallback can fail on some hosts. |
| Black screen or the game does not start locally | Do not open `index.html` from disk. Use *Build And Run* or a local server. |
| No sound in the browser | Browsers need a click first. Click the game once. |
| The saved game is lost after reloading the page | Check that `index.html` contains `autoSyncPersistentDataPath: true` (the build menu adds it automatically). |
| Settings reset after a new upload | Expected on itch.io Web (new URL per upload). See section 9. |
| The Windows game does not open on another PC | The ZIP is missing `CRATER_Data/`, `UnityPlayer.dll` or `MonoBleedingEdge/`. Upload the whole folder. |
| SmartScreen warning | The exe is not signed. *More info → Run anyway*. |
| The NASA live data says "archive" | No connection, or the DONKI API did not answer. The game uses archived real storms automatically. |

**Official documentation**
- Unity, Web deployment and compression: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/building-distribution/deploying
- itch.io, HTML5 games: https://itch.io/docs/creators/html5
- itch.io, getting started: https://itch.io/docs/creators/getting-started
- butler, installing / login / pushing: https://itch.io/docs/butler/installing · https://itch.io/docs/butler/login · https://itch.io/docs/butler/pushing
- itch app: https://itch.io/app

---

## Español

### 1. Introducción

CRATER se publica en **una sola página de itch.io con dos distribuciones separadas**:

| Distribución | Qué hace el jugador | Archivo que se sube |
|---|---|---|
| **Web (HTML5)** | Pulsa **Run game** y juega en el navegador | `CRATER_Web.zip` (con `index.html` en la raíz) |
| **Descarga para Windows** | Descarga, descomprime y ejecuta `CRATER.exe` (o lo instala con la app de itch) | `CRATER_Windows.zip` |

Son builds distintos, con configuraciones distintas. No mezcles sus archivos.

### 2. Requisitos

- **Unity 6000.3.28f1** (Unity 6.3 LTS) con estos módulos (Unity Hub → *Installs* → ⚙ → *Add modules*):
  - **Web Build Support**, para el navegador. En Unity 6 la plataforma se llama **Web** (antes *WebGL*).
  - **Windows Build Support (Mono)** viene con el editor de Windows. **IL2CPP** es opcional.
- Una cuenta de itch.io (https://itch.io/register).
- Opcional: la **app de itch** (https://itch.io/app) y/o **butler** (https://itchio.itch.io/butler).

### 3. Build Web

1. Abre el proyecto y ejecuta una vez **CRATER → Builds → Aplicar configuración de distribución** (después *File → Save Project*). Deja:
   - **Escena de build:** solo `Assets/Scenes/Crater.unity`.
   - **Nombre del producto:** `CRATER`.
   - **Resolución Web:** 1280 × 720 (16:9; el juego está diseñado a 1920 × 1080 y se escala).
   - **Compresión:** *Player Settings → Web → Publishing Settings → Compression Format* = **Gzip**, con **Decompression Fallback** activado.
     - *Por qué Gzip:* funciona en todos los navegadores con HTTP y HTTPS, e itch.io detecta el contenido gzip y envía `Content-Encoding: gzip`.
     - *Por qué el fallback:* si un servidor no envía ese encabezado, el cargador de Unity descomprime en JavaScript y el juego carga igual. A cambio, el cargador es un poco más grande y no hay streaming de WebAssembly.
2. Ejecuta **CRATER → Builds → Web (itch.io HTML5)**. El resultado queda en `Builds/Web/` (`index.html`, `Build/`, `TemplateData/`). El build también activa `autoSyncPersistentDataPath: true` en `index.html`, para que la partida guardada llegue a IndexedDB y sobreviva a recargar la página.
3. **Prueba local.** Abrir `index.html` con doble clic no funciona, porque el navegador bloquea `file://`. Usa *File → Build Profiles → Web → Build And Run*, o un servidor estático como `npx http-server Builds/Web -p 8080`, y abre `http://localhost:8080`.
4. **Crea el ZIP** con el **contenido** de `Builds/Web` (no con la carpeta). `index.html` debe quedar en la raíz:
   ```
   CRATER_Web.zip
   ├── index.html
   ├── Build/
   └── TemplateData/
   ```
   Comando (Windows 10/11, dentro de `Builds/Web`): `tar -a -c -f ../CRATER_Web.zip index.html Build TemplateData`. **No uses `Compress-Archive` de PowerShell**: en Windows PowerShell 5.1 escribe las rutas con `` e itch.io no encuentra `Build/` (index.html carga, pero todos los archivos del juego dan 404).

Límites de itch.io para HTML5 (https://itch.io/docs/creators/html5): como máximo **1.000 archivos**, **500 MB** descomprimidos en total, **200 MB** por archivo y rutas de **240 caracteres**. Los nombres distinguen mayúsculas y minúsculas. El build Web de CRATER está muy por debajo de esos límites.

### 4. Publicar la versión Web desde el sitio

1. Entra a itch.io, abre el **Dashboard** (https://itch.io/dashboard) y pulsa **Create new project**.
2. **Title:** `CRATER`. **Project URL:** `crater` (u otro identificador libre).
3. **Descripción corta:** por ejemplo *"¡Dirige una base lunar y mantén viva a tu tripulación! Juego educativo espacial de estrategia para niños de 7 a 12 años, en inglés y español."*
4. **Classification:** *Games*. **Kind of project:** **HTML** (tiene que ser HTML y no *Downloadable*, que es la opción predeterminada, para que aparezca el reproductor en el navegador).
5. **Pricing:** *No payments* o *Donate* (en itch.io los juegos HTML5 solo aceptan donaciones).
6. **Uploads → Upload files:** sube `CRATER_Web.zip` y marca **"This file will be played in the browser"**.
7. **Embed options:**
   - **Embed in page**, con un área de **1280 × 720** (o *Click to launch in fullscreen*).
   - Activa **Fullscreen button**.
   - Desactiva **Scrollbars**.
   - *Mobile friendly* es opcional (el juego se usa con mouse o táctil).
8. **Details:**
   - **Género:** *Strategy* o *Educational*.
   - **Etiquetas** (hasta 10): `educational`, `space`, `strategy`, `management`, `nasa`, `kids`, `turn-based`, `pixel-art`, `bilingual`, `spanish`.
   - **Idiomas:** inglés y español.
   - **Entrada:** mouse y pantalla táctil.
9. **Imágenes:**
   - **Portada:** 630 × 500 (proporción 315:250).
   - **Capturas:** 3 a 5 del menú, la base, los eventos y Ciencia real.
   - Opcional: un tráiler o video de juego.
10. **Visibilidad:** déjala en **Draft / privada**, pulsa **Save**, abre **View page** y prueba el juego. Cuando funcione, cámbiala a **Public** y guarda.

### 5. Build de Windows

1. Ejecuta **CRATER → Builds → Windows 64-bit**. Aplica la misma configuración y genera **x86_64** (Windows de 64 bits, backend Mono) en `Builds/Windows/`: `CRATER.exe`, `CRATER_Data/`, `MonoBleedingEdge/`, `D3D12/`, `UnityPlayer.dll`, `UnityCrashHandler64.exe` y `CRATER_BurstDebugInformation_DoNotShip/`. Esta última **no** se incluye en el ZIP.
2. **Prueba fuera del Editor:**
   - Copia la carpeta a otro lugar y ejecuta `CRATER.exe`.
   - Juega una misión, cambia el idioma, cierra y vuelve a abrir. El idioma, el volumen y la partida deben conservarse.
   - El juego empieza en pantalla completa; se cambia en **Opciones → Pantalla completa**.
3. **Crea el ZIP** con todo **menos** la carpeta `_DoNotShip`. Nunca subas solo el `.exe`.
   ```powershell
   cd Builds/Windows; tar -a -c -f ../CRATER_Windows.zip --exclude "*_DoNotShip*" *
   ```

### 6. Publicar la descarga de Windows desde el sitio

1. Edita el mismo proyecto (**Dashboard → CRATER → Edit**) y deja *Kind of project = HTML*. Un proyecto puede tener a la vez archivos para el navegador y para descargar.
2. **Upload files** → `CRATER_Windows.zip`. **No** marques *played in the browser*. Marca el ícono de **Windows**.
3. Opcional: ponle nombre al archivo (por ejemplo *CRATER para Windows (64 bits)*).
4. **Save.** La página mostrará **Run game** (Web) y un botón **Download** (Windows).

Windows puede mostrar *SmartScreen* ("Windows protegió tu PC") porque el exe no está firmado. El jugador elige **Más información → Ejecutar de todas formas**. Conviene avisarlo en la página.

### 7. La aplicación de itch (jugadores y creadores)

**Jugadores.** La app de itch funciona en **Windows, macOS y Linux** (https://itch.io/app):
1. Instálala desde https://itch.io/app e inicia sesión.
2. Busca **CRATER** (o abre la página del proyecto y elige *Install*).
3. Pulsa **Install**. La app descarga y descomprime el build de Windows, y el botón cambia a **Launch**.
4. **Actualizaciones:** cuando hay un build nuevo subido con butler o con la app, solo se descargan los cambios. Si el archivo se subió como ZIP desde el sitio, se puede reinstalar.

**Creadores.** La app **v26.12.0 o posterior** incluye una interfaz gráfica para publicar builds con butler y mantiene su butler actualizado (https://itch.io/docs/butler/installing). Si tu versión no muestra esa opción, usa butler desde la línea de comandos (sección 8). Los pasos son los mismos: elegir el proyecto, el canal y la carpeta, revisar los cambios y publicar.

### 8. Publicar con butler (opcional)

1. **Instala** butler desde https://itchio.itch.io/butler, descomprímelo (por ejemplo en `C:\butler`), agrégalo al `PATH` y verifica con `butler version`.
2. **Inicia sesión** (se abre el navegador; las credenciales se guardan en tu equipo):
   ```bash
   butler login
   ```
3. **Identificador del proyecto:** `USERNAME/crater`, el mismo de la URL.
4. **Publica:**
   ```bash
   butler push Builds/Web USERNAME/crater:html5 --userversion 1.0.0
   ```
   ```bash
   butler push Builds/Windows USERNAME/crater:windows --userversion 1.0.0 --ignore "*_DoNotShip"
   ```
5. **Canales:**
   - Los nombres son libres. Si contienen `win`/`windows`, `linux` o `mac`/`osx`, toman esa plataforma.
   - El canal Web hay que marcarlo como **"This file will be played in the browser"** (HTML5) en la página **Edit** del proyecto, después del primer push.
   - Verifica que el canal `windows` tenga el ícono de Windows.
6. **Revisar sin subir:** `butler push --dry-run …` o `butler push-preview …`.

No pegues claves de API en el chat ni en el repositorio. Para integración continua usa la variable `BUTLER_API_KEY`.

### 9. Actualizar versiones

1. Sube la versión en *Project Settings → Player → Version* (por ejemplo `1.0.1`).
2. Vuelve a generar los builds (**CRATER → Builds**), pruébalos y súbelos:
   - **Sitio:** sube el ZIP nuevo y reemplaza o borra el anterior.
   - **butler:** `push` al mismo canal con otra `--userversion`.
3. **En Web, los datos guardados no pasan de una versión a otra.** Cada subida usa una URL nueva, y el navegador guarda PlayerPrefs y partidas por URL. Tras actualizar, el jugador empieza con las opciones por defecto (inglés). En Windows los datos se conservan (registro y `%USERPROFILE%\AppData\LocalLow\<empresa>\CRATER`).

### 10. Lista de verificación antes de publicar

**Web**
- [ ] `index.html` está en la raíz de `CRATER_Web.zip`.
- [ ] La página carga sin errores en la consola del navegador (F12) y sin errores de descompresión.
- [ ] El juego ocupa bien el área (1280 × 720) y funciona el botón de pantalla completa.
- [ ] La música empieza después del primer clic (los navegadores bloquean el audio hasta que el usuario interactúa).
- [ ] Se puede jugar una misión, y un evento principal muestra su música, su sonido y su ventana.
- [ ] Empieza en **inglés**, ESPAÑOL funciona, y tras recargar sigue en español.

**Windows**
- [ ] `CRATER.exe` abre desde otra carpeta, con todos los archivos incluidos y sin `_DoNotShip`.
- [ ] Funciona el audio y se puede terminar una misión (pantalla de victoria o derrota).
- [ ] El idioma, el volumen y la partida se conservan al cerrar y volver a abrir.

**Proyecto**
- [ ] Compila, la consola no muestra *Missing Script* y pasan las pruebas EditMode.

### 11. Problemas frecuentes

| Problema | Solución |
|---|---|
| itch.io no encuentra `index.html` | El ZIP tiene una carpeta padre extra. Comprime el **contenido** de `Builds/Web`. |
| "Unable to parse Build/…" o error de descompresión | Revisa que *Compression = Gzip* y que *Decompression Fallback* esté activado (**CRATER → Builds → Aplicar configuración…**), y vuelve a generar el build. Brotli sin fallback puede fallar en algunos servidores. |
| Pantalla negra en local | No abras `index.html` desde el disco. Usa *Build And Run* o un servidor local. |
| No hay sonido en el navegador | El navegador necesita un clic antes. Haz clic una vez en el juego. |
| Se pierde la partida al recargar | Verifica que `index.html` tenga `autoSyncPersistentDataPath: true` (el menú de build lo agrega solo). |
| Las opciones se reinician tras subir una versión nueva | Es normal en la versión Web de itch.io (URL nueva en cada subida). Ver sección 9. |
| El juego de Windows no abre en otra PC | Faltan `CRATER_Data/`, `UnityPlayer.dll` o `MonoBleedingEdge/`. Sube la carpeta completa. |
| Aviso de SmartScreen | El exe no está firmado. *Más información → Ejecutar de todas formas*. |
| Los datos de NASA dicen "archivo" | No hay conexión o DONKI no respondió. El juego usa tormentas reales archivadas automáticamente. |

**Documentación oficial:** ver los enlaces al final de la versión en inglés (Unity Web, itch.io HTML5, getting started, butler y app de itch).
