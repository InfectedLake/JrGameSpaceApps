# CRATER — Diseño técnico e implementación

## Cómo abrirlo en Unity (una sola vez)

1. Abre el proyecto en Unity 6000.3 y espera a que compile.
2. Menú **CRATER > Construir escena y datos (todo)**.
   - Crea los ScriptableObjects en `Assets/Data/Crater/` (balance, 4 edificios, 3 eventos, 13 fichas de ciencia).
   - Crea `Assets/Scenes/Crater.unity` con todos los sistemas conectados y la pone primera en Build Settings.
3. Presiona **Play**.

Pruebas: **Window > General > Test Runner > EditMode** (17 pruebas, incluidas las 9 de la sección 20).

## Arquitectura

```
Assets/Scripts/Crater/          (asmdef "Crater")
  Core/      C# puro, SIN UnityEngine: reglas y simulación (probables fuera de Unity)
    BalanceConfig      todos los parámetros [JUEGO] / [REAL]
    GameState          estado serializable (recursos, auxiliares, eventos, historial)
    TurnSimulator      ÚNICA fuente de verdad del día: proyección = cálculo definitivo
    EventRules         radiación, micrometeoritos, sombra de la Tierra
    EventScheduler     calendario, probabilidades, avisos
    BaseActions        descanso, reparar vivienda, batería, reparaciones, regolito
  Data/      ScriptableObjects (BuildingData, EventData, GameBalanceData, ScienceData) + DefaultContent
  Systems/   MonoBehaviours delgados sobre Core
  Services/  NASADataService (DONKI en vivo + archivo)
  UI/        UIManager (construye la interfaz por código), BaseView, BuildingController, RoverView, EffectsLayer...
  Editor/    CraterSceneBuilder (menú CRATER)
Assets/Resources/DonkiArchive/  respuestas reales de DONKI (sep-2017, may-2024, oct-2024)
Assets/Tests/EditMode/          pruebas NUnit
```

### Escena `Crater.unity`

| GameObject | Componente | Inspector |
|---|---|---|
| Main Camera | Camera | — |
| EventSystem | EventSystem + InputSystemUIInputModule | — |
| CRATER/GameManager | GameManager | `balance`, `buildingData[4]`, `eventData`, y referencias a los demás sistemas (el constructor las conecta; si faltan se buscan solas) |
| CRATER/TurnManager | TurnManager | — |
| CRATER/ResourceManager | ResourceManager | — |
| CRATER/BuildingManager | BuildingManager | — |
| CRATER/EventManager | EventManager | — |
| CRATER/RadiationSystem | RadiationSystem | — |
| CRATER/RoverManager | RoverManager | — |
| CRATER/BatteryManager | BatteryManager | — |
| CRATER/NASADataService | NASADataService | `baseUrl`, `windowDays` (≤30), `timeoutSeconds` |
| CRATER/ScienceInfoManager | ScienceInfoManager | `entries` (fichas de ciencia) |
| CRATER/SaveSystem | SaveSystem | `fileName` |
| CRATER/UIManager | UIManager | `baseScale` |

Si un asset queda vacío se usan los valores por defecto del documento (`DefaultContent`). Para reemplazar el arte procedural, asigna `icon` en cada `BuildingData`.

### Flujo de un día

`TERMINAR DÍA` → `TurnManager.EndDay()` → `TurnSimulator.Simulate()` (pasos 1–8, contadores, día +1, emergencia, victoria/derrota) → `BeginDay()` → `EventManager.BeginDay()` (llegadas y avisos) → guardado automático.

Encender/apagar un edificio solo cambia `GameState.buildingOn` y avisa a la UI; la UI pide `TurnManager.Projection()`, que ejecuta el mismo `Simulate()` sobre una copia. Por eso la proyección y el resultado nunca difieren (Prueba 9).

**Prioridad de energía [JUEGO]:** Reciclador (1) → Invernadero (2) → Escudo (3). La energía producida por los paneles se suma antes de comprobar si alcanza; el límite 0–100 se aplica al final del día (100 + 25 − 20 − 10 = 95).

## NASA DONKI — verificación del endpoint (15-nov-2026)

| URL | Resultado |
|---|---|
| `https://ccmc.gsfc.nasa.gov/DONKIAPI/get/FLR?...` (la del documento) | **404** |
| `https://ccmc.gsfc.nasa.gov/DONKI-API/get/FLR/?...` (con barra final) | 404 |
| `https://ccmc.gsfc.nasa.gov/DONKI-API/get/FLR?startDate=...&endDate=...` | **200, JSON, sin clave** |
| `https://api.nasa.gov/DONKI/FLR?...&api_key=DEMO_KEY` | 404 (redirige internamente a `/DONKI-API/get/FLR/`) |

El juego usa la tercera. Ventana: últimos 30 días. Si falla la red, la respuesta está vacía o el formato es inesperado → modo archivo con la etiqueta obligatoria *"DATOS ARCHIVADOS: estos registros corresponden a una tormenta solar real del pasado"*. Sin archivo → probabilidades 50/35/15 % etiquetadas [JUEGO].

Conversión [JUEGO]: clase de llamarada X → intensidad 3, M → 2, otras → 1. La fecha del registro nunca se presenta como predicción de impacto.

## Decisiones nuevas de diseño [JUEGO] (no definidas en el documento)

| Decisión | Valor |
|---|---|
| Consumo de agua del reciclador / invernadero | 4 / 2 por día |
| Hielo que recolecta el rover | 6 de agua por día × capacidad |
| Edificio Vivienda (reemplaza al escudo) | Energía −5/día, empieza encendida. Encendida: integridad +2/día y permite recuperar salud. Apagada o sin energía: Salud −3/día. |
| Paredes de agua | 80 % si hubo aviso y se eligieron antes de la tormenta; si no, 20 %. |
| Aviso de radiación | 2 días antes (permite terminar el iglú de 2 días). Micrometeoritos y eclipse: 1 día. |
| Iglú de regolito | Protege una tormenta y luego hay que rehacerlo. Si se elige sin tiempo: bloqueo −15 % (dosis +15 %). |
| Sin defensa contra radiación | La electrónica expuesta se daña: paneles al 50 %. |
| Dosis | Fondo 1.369 mSv/día [REAL, LND] + tormenta 10 mSv × intensidad × (1 − bloqueo) [JUEGO]. Sin límites médicos. |
| Averías | Se elige en el momento: reparar ya (Energía −20, Agua −10) o con tiempo (2 días inactivo). |
| Eclipse | Los edificios siguen gastando de la Energía principal; las baterías solo deciden las consecuencias de cada sección. |
| Baterías | Se recargan 1 por día si los paneles producen. Uso de emergencia (+20 energía): se ofrece como decisión contextual cuando hay emergencia. |
| Salud | −5/día si Vivienda < 25; +5/día si hay oxígeno, comida y Vivienda ≥ 50. |
| Emergencia | Energía o Vivienda en 0: si falta energía se apaga el invernadero (no esencial) y aparece una decisión: conectar una batería o seguir. No es derrota. |
| Ánimo del ciclo 4 | Vivienda −10 al final del ciclo 4 si la vivienda estuvo apagada o sin energía algún día de ese ciclo. |
| Eventos | Uno por ciclo desde el ciclo 2, entre el 2.º y el 4.º día del ciclo. |
| Duraciones | Corta 4 ciclos, normal 6, larga 8. Tripulación 2–4. |

## Revisión de balance (Fase 7)

500 misiones simuladas por estrategia con las reglas reales del núcleo:

- Jugador cuidadoso (apaga lo que sobra, se defiende, descansa): 500/500 victorias, base en buen estado.
- Jugador pasivo (deja todo igual, sin defensas): 500/500 victorias, pero termina con energía casi en 0 (1–4) y vivienda entre 16 y 56.

La misión es indulgente, como pide el público de 7–12 años. Para endurecerla, edita `GameBalance.asset` (p. ej. `healthRecovery`, `roverIcePerTurn`, `radiationDamagePerIntensity`).

## Pendiente

- Arte final: hoy todo se dibuja por código.
- Audio (las carpetas `Assets/Audios` están vacías).
- Las referencias bibliográficas completas (ARMOR, modelo de micrometeoritos, planta de oxígeno) se atribuyen tal como las da el documento de investigación.
- Modo avanzado de noche lunar completa.
- Los scripts antiguos del prototipo (`Assets/Scripts/Logica`, `Assets/Scripts/UI`, escenas `MainScene` / `TestMainScene`) siguen intactos y no se usan en `Crater.unity`.

## Refinamiento (15-nov-2026)

- **Distribución:** `BaseView` coloca todo con las coordenadas en píxeles de `Assets/Reference/Screenshot 2026-11-15 011911.png` (1425 × 738): Energía arriba a la izquierda, Comida arriba a la derecha, Oxígeno abajo a la izquierda, Vivienda abajo a la derecha, Agua en el extremo derecho y el núcleo con antena al centro, con los mismos pasillos. Los dibujos son placeholders 2.5D.
- **Arte definitivo:** cada edificio → `BuildingData.icon` (reemplaza su placeholder y conserva la zona clicable). Terreno + núcleo + agua → `UIManager.baseArt`. Ilustración de cada evento → `EventData.image`.
- **Interfaz de juego:** solo 4 recursos, `DÍA 04 / 24`, la base, TERMINAR DÍA y MENÚ. Al tocar un edificio aparece una ficha (nombre, estado, función, consumo/producción, ENCENDER/APAGAR) que se cierra con X, al tocar otro edificio o al terminar el día.
- **Eliminado del gameplay:** panel de suministros, panel de acciones (descanso, reparar vivienda, batería manual), cálculo del día, alertas permanentes, sección científica y fuentes en los eventos.
- **Eventos:** alerta con ilustración, una o dos frases, opciones con costo y consecuencia principal, sin fórmulas. La radiación muestra una sola línea que indica si la tormenta se basa en datos de NASA en vivo o archivados.
- **Pantalla final:** resultado, días sobrevividos, estado de la tripulación o causa de la derrota, y dos botones.
- **Ciencia:** sigue en CIENCIA REAL y CRÉDITOS del menú principal.
- **Partidas guardadas:** las de la versión anterior (con escudo) ya no se cargan.

## Rebalanceo y eventos menores (15-nov-2026)

Reemplaza los valores anteriores de esta guía. Todos son [JUEGO] y se editan en `Assets/Data/Crater/GameBalance.asset` (eventos menores, tripulación, salud) y en `Assets/Data/Crater/Edificios/*.asset` (producción y consumo).

### Valores finales

| Edificio | Energía | Oxígeno | Comida | Agua |
|---|---:|---:|---:|---:|
| Planta de energía | +23 | 0 | 0 | 0 |
| Invernadero | −12 | +2 | +8 | −2 |
| Planta de oxígeno | −22 | +10 | 0 | −4 |
| Vivienda | −5 | 0 | 0 | 0 |

- Cada astronauta: −2 oxígeno y −2 comida por día (3 astronautas: −6 y −6).
- Todo encendido: Energía −16, Oxígeno +6, Comida +2. Con la planta de oxígeno apagada: Energía +6, Oxígeno −4, Comida +2.
- Vivienda apagada o sin energía: Salud −3 por día. La salud solo se recupera (+2/día) con la vivienda encendida.
- Límites: el cambio de cada recurso se suma completo y el límite 0–100 se aplica una sola vez al final del día. La proyección usa el mismo `TurnSimulator.Simulate`, así que siempre coincide.

### Eventos menores

| Evento | Opción A | Opción B |
|---|---|---|
| Polvo lunar en los paneles | Energía −8 | Comida −7 |
| Filtro de aire saturado | Energía −8 | Oxígeno −7 |
| Descenso de temperatura | Energía −8 | Salud −5 |
| Problema en los cultivos | Comida −8 | Energía −9 |
| Pequeña fuga de aire | Oxígeno −8 | Energía −10 |
| Fuga en el depósito de agua | Energía −8 | Comida −7 |

- Frecuencia: `minorEventsPerTurn = 0.30` (unos 7 en 24 días, 4–5 en 16) y al menos un día de separación (`minorEventMinGap = 2`).
- Nunca en el ciclo 1 (tutorial), ni el día de un evento principal, ni el día de su aviso. Si ese día aparece otra ventana (emergencia, aviso), el menor se pospone al día siguiente.
- No se repite un evento hasta agotar los seis.
- Una opción que no se puede pagar se deshabilita y dice por qué. Si ninguna se puede pagar, el evento se pospone. Nunca hay valores negativos y el costo se cobra una vez.
- Se muestran con la misma ventana de eventos: nombre, una frase y dos botones con el icono, el nombre y la cantidad del recurso que se pierde.

### Simulación de balance (1000 partidas por estrategia, mismas reglas que el juego)

Misión normal de 24 días:

| Estrategia | Victorias | Salud final media al ganar |
|---|---:|---:|
| Todo encendido siempre | 28% | 40 |
| Todo apagado (solo paneles) | 0% | — |
| Priorizar solo oxígeno (invernadero apagado) | 0% | — |
| Planta de oxígeno apagada | 0% | — |
| Vivienda apagada | 11% | 14 |
| Adaptativa (alterna según los recursos) | 99.5% | 95 |
| Adaptativa con 25% de días al azar | 99.1% | 91 |
| Adaptativa con dos rachas de malas decisiones | 99.6% | 95 |
| Adaptativa eligiendo siempre la opción A / siempre la B | 99.7% / 99.5% | 95 / 95 |

Conclusiones:
- Mantener todo encendido o ignorar un sistema falla con frecuencia.
- Alternar sistemas es viable, y se puede recuperar de errores.
- Ninguna alternativa de los eventos menores es siempre mejor.

**Misión corta (16 días):** las reservas iniciales alcanzan para casi todo (todo encendido gana el 99.6%), así que en la configuración se presenta como "Corta (fácil)". No se cambiaron las duraciones.

## Integración del arte original (15-nov-2026)

Reemplaza los dibujos provisionales del juego. Los archivos de `Assets/Sprites` **no se modificaron**; solo se cambió su configuración de importación (`.meta`).

### Importación (49 sprites, sin incluir las 8 capturas "ejemplo…", que solo son referencia)

Filter Mode Point, sin compresión, sin mipmaps, Sprite Mode Single, Mesh Type Full Rect, Wrap Clamp, Max Size 4096 (el fondo mide 2146 px y el filtro del eclipse 3003 px, así que con 2048 Unity los reducía) y Read/Write activado (para que solo los píxeles opacos de los edificios reciban clics). `fondo menu.png` tiene un borde 9-slice de 6/6/6/29 px.

### Composición

Las posiciones se midieron comparando píxel a píxel cada sprite con las capturas de referencia; están en `UI/CraterLayout.cs`. La captura de gameplay es la composición a escala 0.5, y la vista del juego es la ventana de 1920 × 1080 del fondo que empieza en (184, 636): a 1920 × 1080, 1 píxel de arte equivale a 1 píxel de pantalla. La reconstrucción con estas coordenadas difiere de la referencia en 17/255 de media, sobre todo por el reescalado.

- **Orden de capas** (hermanos de uGUI, verificado contra la referencia): fondo → energía → agua y comida (incluye explanada y tanque) → rover → núcleo → oxígeno → vivienda. Las capas de cada evento son hijas de su edificio.
- **Variantes:** un solo objeto por edificio. Cambia el sprite de la misma Image (encendida en el eclipse, energía rota por micrometeoritos) o activa su capa (agua / polvo). La zona clicable y la lógica no cambian.
- **Tormenta solar:** con paredes de agua se activan las capas de agua; con regolito (o con el refugio listo), las de polvo. El núcleo y el tanque quedan sin capa, como en la referencia.
- **Eclipse:** filtro de oscurecimiento, Sol y Tierra arriba al centro, durante el aviso y el día del eclipse. Brilla el edificio de cada sección con baterías, con vista previa en vivo mientras se reparten. El núcleo nunca se enciende.
- **Micrometeoritos:** "meteoritos de fondo" durante la alerta y ese día. La energía usa "edificio energia roto" mientras los paneles estén averiados.
- **Cámara:** al tocar un edificio, la cámara se acerca ×2 en 0.45 s (con suavizado). Recién después aparece el panel, en la posición de la captura de selección, y al cerrarlo vuelve a la vista general. No se puede seleccionar otro edificio mientras dura el movimiento ni con el panel abierto. La cámara nunca sale de la zona pintada del fondo.
- **HUD:** barra superior original con las casillas Comida (agua y comida), Energía, Oxígeno y Salud (icono vivienda), el menú (hamburguesa), el recuadro de días y el botón ▶▶ de terminar el día.
- **Menú principal:** fondo, letrero del título y los botones INICIO, OPCIONES, CRÉDITOS y SALIR. CIENCIA REAL está dentro de CRÉDITOS. Todos los paneles y botones de los demás menús y ventanas usan "fondo menu" con 9-slice.

### Pendiente de arte (no se inventó ningún sustituto)

| Falta | Situación actual |
|---|---|
| **Rover** (normal y roto): la carpeta no tiene ningún sprite de rover | Marcador temporal en la posición indicada por el README, junto al tanque de agua. Basta asignar `rover` / `roverBroken` en `ArtSet.asset`. |
| `panel de informacion de edificio.png` | El archivo está vacío (todos los píxeles transparentes). Se usa "fondo menu", que tiene el mismo naranja que el panel de la captura. |
| `boton seleccionado universal.png` | También está vacío. El botón seleccionado se aclara. |
| Cráteres sobre la explanada del agua (captura de micrometeoritos) | No hay sprite para ese efecto. |
| Estados apagado / sin energía fuera del eclipse | No hay variantes: se aplica un tinte a la Image (gris si está apagado, rojo intermitente si no tiene energía). Como el tanque está dentro del sprite de agua y comida, también se tiñe. |

**Decisiones a revisar con el equipo:**
- La casilla de recurso "vivienda" muestra la **Salud** de la tripulación (el juego no tiene un recurso "vivienda").
- En el eclipse, la sección "Monitoreo y comunicaciones" ilumina la **vivienda**, porque es el único edificio restante con variante encendida.

Si se cambian o renombran sprites: **CRATER > Asignar arte original** vuelve a enlazar `ArtSet.asset` por ruta y lo asigna al UIManager.

## Corrección de interfaz y menú de pausa (15-nov-2026)

### Sombra de la Tierra (bloqueaba la partida)
**Causa:** las 4 tarjetas medían 4 × 270 px + espacios = 1116 px, pero el contenido solo tenía 1100 px. La cuarta pasaba a una segunda fila fuera del espacio reservado (300 px), quedaba encima de CONFIRMAR e interceptaba sus clics.

**Solución:**
- La ventana tiene una estructura vertical fija: encabezado con icono, título y subtítulo, luego el cuerpo, luego las decisiones.
- Las tarjetas van en una cuadrícula 2 × 2 de igual tamaño, calculada con el ancho disponible.
- El pie, con el indicador de baterías y CONFIRMAR, tiene un alto reservado.
- CONFIRMAR solo se habilita con una distribución válida (costo ≤ baterías), se desactiva al primer clic (no hay doble aplicación) y llama a `EventManager.ConfirmEclipse`. Las reglas no cambiaron: 4 baterías, costos 2/1/1/1.

### Texto y márgenes
- **Ventanas de eventos:** el alto se calcula con el texto real, con un máximo de 1000 px, y los márgenes laterales son de ~6%. Los textos de las opciones usan tamaño automático acotado y puntos suspensivos.
- **Borde del sprite:** el sprite "fondo menu" tiene 23 px transparentes arriba (`UIFactory.TopInset`); todos los textos y etiquetas de botón se bajan esa distancia para no quedar encima del borde.
- **Casillas de recurso:** nombre arriba, cantidad y cambio abajo; antes el nombre se superponía con la cantidad.
- **Recuadro de días:** se movió 61 px hacia dentro para que no se recorte (en la referencia salía 41 px del borde). Muestra el día y el ciclo.
- **Otras pantallas:** el panel del edificio, el tutorial, la configuración, las opciones, los créditos, la ciencia y el final tienen márgenes y alturas fijas, con tamaño automático acotado.

### Menú de pausa (botón de tres líneas)
- **Contenido:** CONTINUAR, OPCIONES, MENÚ PRINCIPAL y SALIR DEL JUEGO; los dos últimos piden confirmación.
- **Pausa:** `GameManager.SetPaused` (no existe un segundo PauseManager) pone `Time.timeScale = 0`. Además, TurnManager, BuildingManager, EventManager y la selección de edificios rechazan acciones mientras está en pausa, porque los turnos no dependen del tiempo. El fondo del menú bloquea todos los clics, y cerrarlo no cierra el evento pendiente.
- **Menú principal:** respeta el guardado automático existente (inicio del día), limpia las ventanas pendientes y vuelve a la pantalla de menú. El menú principal es una pantalla de la misma escena, no una escena aparte.
- **Salir:** usa `Application.Quit()`. En el Editor detiene el modo Play; en WebGL vuelve al menú y pide cerrar la pestaña.

### Audio y opciones
- **AudioManager:** es el único sistema de audio (no había AudioMixer). Volumen general → `AudioListener.volume`; música y efectos → una AudioSource cada uno. Se guarda en PlayerPrefs.
- **Pantalla completa:** `Screen.fullScreen`; desactivada en móviles.
- **Dónde están las opciones:** en el menú de pausa y en OPCIONES del menú principal, con los mismos controles y sincronizados.
- **Música** (Assets/Audios): `menu` en los menús, `main` en la partida, `eclipse` durante el eclipse y `ataque(suspenso)` en la tormenta y los micrometeoritos.
- **Efectos:** `botones` en cada clic, `real meteorits` en los impactos y `turbina` al terminar el día.

## Ajustes de interfaz y sprites nuevos (15-nov-2026)

### Contador de días
"fondo dias pasados" (201 × 216) tiene el interior en x 6–167, un recuadro claro en (26, 27)–(155, 98) y después una franja transparente con borde que en la referencia queda fuera de pantalla. Al moverlo hacia dentro, esa franja quedó visible y el texto, centrado en todo el sprite, no coincidía con el recuadro.

Ahora la jerarquía es `DayCounter → Background, DayLabel, DayNumber, CycleLabel`. "DÍA" y el número van en el recuadro claro, y el ciclo en la parte inferior, en una sola línea. El contador se coloca en x 1753, para que la parte diseñada termine en el borde derecho. El arte no se estira.

### Opciones (menú principal)
- **Tarjeta:** anclada a la pantalla (márgenes de 7% y 6%), así que no sale de la vista en ninguna resolución.
- **Columnas:** dos de igual ancho con Layout Groups, "Sonido y pantalla" y "Datos y partida".
- **Sliders:** horizontales, con una barra de 18 px y una manija circular de 34 × 34 (antes la manija se estiraba a todo el alto y parecía un óvalo vertical), y área de arrastre en toda la fila. Cada fila tiene la etiqueta a la izquierda, el slider en el centro y el porcentaje a la derecha. Los valores y el AudioManager no cambiaron.

### Ciencia real
Cada ficha muestra:
- **Bloque REAL:** la etiqueta, el texto completo y la fuente justo debajo, dentro del mismo bloque.
- **Bloque JUEGO:** la etiqueta y la explicación completa.

Los textos son más grandes (28/26/22), tienen márgenes internos que respetan el borde transparente del sprite y alineación superior izquierda. El alto de los bloques crece con el contenido dentro de la vista con desplazamiento, así que no se recorta información.

### Sprites nuevos (importación: Point, sin compresión ni mipmaps, Single, 4096, Read/Write)
- **`recurso vivienda 1.png` (cruz verde 59 × 59):** es el icono de Salud. Los iconos ahora se colocan por su centro.
- **`edificio agua y comida (…) 1.png`:** misma composición con el rover sobre la explanada, junto al tanque. Reemplaza al anterior en la misma posición. Se eliminó el marcador provisional del rover, así que no hay duplicados; `RoverView` solo muestra humo si está averiado. Como la variante encendida del eclipse no dibuja el rover, durante el eclipse se muestra encima, con una máscara (RectMask2D), el recorte (98, 304)–(395, 482) del sprite normal: el rover aparece en todas las variantes sin modificar el arte. La zona clicable del invernadero sigue sin incluir la explanada ni el tanque.
- **`boton seleccionado universal 1.png` (447 × 135):** es el botón aclarado, ubicado en (46, 17), con un destello a la izquierda. Se muestra en los estados resaltado y seleccionado (`SelectedStateArt`) de los botones de la pantalla de inicio (INICIO, OPCIONES, CRÉDITOS, SALIR) y de la elección CONTINUAR / NUEVA PARTIDA / VOLVER. Como mide distinto que el botón (401 × 109), se superpone alineado en lugar de usar Sprite Swap, que lo deformaría; así no cambian ni el tamaño ni la zona clicable. No se aplica a los botones de gameplay ni de eventos.
- Los archivos anteriores (`recurso vivienda.png`, la versión sin rover y `boton seleccionado universal.png`, que estaba vacío) siguen en la carpeta, sin referencias activas. No se borraron.

## Corrección: invernadero en blanco y campos de nombre (15-nov-2026)

- **Invernadero en blanco (causa):** el PNG "edificio agua y comida (…) 1" se volvió a guardar y Unity reimportó su `.meta` en modo **Multiple**, con dos recortes automáticos y la configuración por defecto (Bilinear, compresión, 2048). `ArtSet.asset` apunta al sprite completo del modo Single (`fileID 21300000`), que dejó de existir; la Image quedó sin sprite y Unity la dibuja blanca.
- **Invernadero (solución):** el `.meta` vuelve a Single + Point, sin compresión ni mipmaps, 4096 y Read/Write; el PNG no se tocó. Las demás referencias de `ArtSet.asset` se revisaron y están en Single/Point.
- **Prevención:** `CRATER > Asignar arte original` ahora devuelve a Single (con aviso) cualquier sprite usado como imagen completa. Si un edificio queda sin sprite, se registra un error claro y no se dibuja la caja blanca.
- **Campos de nombre (causa):** el área de escritura de los campos de texto ocupaba también el borde transparente superior del sprite (23 px), así que el texto y el marcador se veían desplazados hacia arriba y pegados al borde.
- **Campos de nombre (solución):** el área empieza debajo de ese borde, con márgenes laterales de 22 px. El texto queda centrado verticalmente y los nombres largos se desplazan dentro del campo sin salirse. Los campos de la configuración miden 78 px y la tarjeta 960 px para que todo quepa.

## Botones de inicio: se restaura el estilo anterior (15-nov-2026)

A pedido del equipo, se revirtió la integración de "boton seleccionado universal 1":
- **Botones INICIO / OPCIONES / CRÉDITOS / SALIR:** vuelven a la transición de color anterior (normal 0.88, resaltado y seleccionado 1.0, presionado 0.75).
- **Diálogo CONTINUAR / NUEVA PARTIDA / VOLVER:** vuelve a los botones de panel.
- **Código:** se eliminó el componente `SelectedStateArt` y la constante de desplazamiento. `ArtSet.buttonSelected` vuelve a apuntar al archivo anterior y no se usa en ningún botón.
- **Lo que se conserva:** el sonido de clic en estos botones, porque es audio y no estilo. El archivo `boton seleccionado universal 1.png` no se borró ni se modificó.

## Pantalla de opciones: corrección estructural (15-nov-2026)
Causas encontradas:
- Las dos columnas iban en un `HorizontalLayoutGroup` que reparte el ancho según el ancho preferido de cada hijo. En TextMeshPro ese valor es el largo del texto sin saltos de línea, así que la columna de datos se llevaba casi todo el ancho y los sliders quedaban comprimidos.
- La manija del slider tenía `sizeDelta = (34, 34)`, pero `Slider` fuerza sus anclas verticales a 0..1 en cada actualización: la altura se SUMABA al alto del área y la manija medía 34×68 (óvalo).

Cambios:
- `AdaptiveColumns` (nuevo): asigna anchos explícitos. Dos columnas iguales si cada una mide ≥ 640 px; si no, una sola columna (la segunda sección debajo). Ajusta la altura del contenido.
- El cuerpo de la tarjeta es un `ScrollRect` con `RectMask2D`: nada se dibuja fuera de la ventana y VOLVER queda fijo abajo.
- `Section()` crea cada columna (título + VerticalLayoutGroup).
- `SoundRow_<nombre>` = `Label` (250 px) | `SliderContainer` (flexible, mín. 220) | `PercentageLabel` (86 px), con `LayoutElement` en cada parte.
- Slider: `Background` (barra con bordes redondeados), `FillArea/Fill`, `HandleSlideArea/Handle` con `sizeDelta = (34, 0)` → manija circular de 34×34.
- Se mantienen el botón de pantalla completa y los valores por defecto 100 % / 60 % / 80 % (`AudioManager`); si hay valores guardados en PlayerPrefs se muestran esos.

Verificado con un render real de Unity 6000.3 (copia del proyecto, modo batch) en 1920×1080, 1280×720 y 2560×1080 (dos columnas) y 1440×1080 y 1080×1080 (una columna, con desplazamiento), además de las opciones del menú de pausa.

## Sistema de audio y música (15-nov-2026)

### Inventario de Assets/Audios
| Archivo | Categoría | Uso | Momento |
|---|---|---|---|
| Musica/menu.mp3 (2:40) | Música | Menú principal, opciones, ciencia, pantalla final | En loop mientras no hay partida |
| Musica/main.mp3 (3:06) | Música | Gameplay | En loop durante la partida; se pausa en los eventos grandes y continúa donde iba |
| Musica/MusicaEventoGrande.mp3 (0:59) | Música | Los tres acontecimientos principales | Desde la notificación inicial hasta cerrar la última ventana del evento (loop) |
| Efectos/botones.mp3 | Efecto UI | Botones, selección de edificios, encender/apagar, TERMINAR DÍA, menú ☰ | Al hacer clic (una vez por clic, solo si la acción ocurre) |
| Efectos/Radiacion.mp3 (~10 s) | Efecto | Tormenta solar | Cuando aparece el destello de la tormenta |
| Efectos/real meteorits.mp3 (~5 s) | Efecto | Micrometeoritos | Cuando aparecen los impactos |
| Efectos/eclipse.mp3 (~10 s) | Efecto | Sombra de la Tierra | Cuando la base se oscurece |

- El equipo movió `eclipse.mp3` de Musica a Efectos y reemplazó `ataque(suspenso).mp3` por `MusicaEventoGrande.mp3` (Unity conservó los GUID, por eso se usan `FormerlySerializedAs`).
- `turbina (generador).mp3` ya no existe: se quitó su campo y TERMINAR DÍA usa el clic normal.
- No hay audios para los eventos menores ni para la emergencia: no se les asigna ninguno (no se usa la música de evento grande).

### Arquitectura
- `AudioManager` (único sistema; el proyecto no tenía AudioMixer): Master = `AudioListener.volume`; Music = una AudioSource por pista con fundido de 1 s; SFX = una fuente para la interfaz y otra para los acontecimientos. Volúmenes en PlayerPrefs (100 / 60 / 80 por defecto).
- `SetMusic(pista)` ignora la pista que ya suena: abrir pausa, opciones o ciencia nunca reinicia la canción.
- `CraterArtSet` centraliza los clips: `musicMenu`, `musicGame`, `musicEvent`, `sfxClick`, `sfxEclipse`, `sfxRadiation`, `sfxMeteors`.
- Importación: música en Streaming (sin precarga), efectos Decompress On Load con precarga (suenan sin retraso). No se modificaron los archivos fuente.

### Secuencia de un acontecimiento principal (`EventPrompt.arrival`, `UIManager.EventIntro`)
1. Termina la transición del día (y la pausa, si está abierta).
2. Notificación inicial (aviso en pantalla) y empieza la música del evento.
3. 1,6 s después aparece el fenómeno (oscurecimiento / impactos / destello) y suena su efecto.
4. 1,4 s después aparece la ventana de decisiones.
5. Al cerrar la última ventana del evento (resultado, reparaciones), vuelve la música de gameplay.

Los avisos previos ("ocurre en N días") ponen la música del evento, pero no el efecto, porque el fenómeno aún no sucede.
En pausa la música sigue sonando, la secuencia se detiene y el efecto del evento se pausa y luego continúa. Al salir al menú se detienen el efecto y la música de la partida.

Verificado con una prueba PlayMode de Unity 6000.3 (copia del proyecto): menú → partida → clic en edificio → aviso → eclipse, micrometeoritos y tormenta → pausa → salir al menú → volúmenes → nueva partida.

## Reparación del audio: AudioListener, AudioMixer y volúmenes (15-nov-2026)

### Causa
- **Sin AudioListener en la escena.** `Assets/Scenes/Crater.unity` tenía la cámara solo con Transform + Camera, y el código no creaba ningún listener. Las AudioSource sí reproducían (la música del menú figuraba como sonando a volumen 0.6), pero Unity no envía nada a la salida sin listener y avisaba "There are no audio listeners in the scene". Por eso no se oía nada al iniciar el juego.
- **El volumen general se aplicaba en `AudioListener.volume`.** Sin listener ese valor no tenía efecto estable. Música y efectos multiplicaban su volumen en la propia AudioSource junto con el fundido, así que el fundido y el volumen del jugador se pisaban en el mismo valor.
- **`AudioManager.Awake` modificado a mano.** Las pistas de música tenían `playOnAwake = true` y ya no empezaban en volumen 0.
- **Al reabrir Opciones** los sliders se sincronizaban con `slider.value = …`. Eso disparaba `OnValueChanged`, que volvía a guardar y aplicar.

### Corrección
- **AudioListener:** se agregó a la cámara de `Crater.unity` y al constructor de escena. `AudioManager.EnsureSingleListener()` garantiza exactamente uno activo (agrega uno si falta y desactiva duplicados con aviso).
- **AudioMixer:** `Assets/Data/Crater/CraterMixer.mixer` con los grupos Master > Music, SFX y los parámetros expuestos `MasterVolume`, `MusicVolume` y `SFXVolume`, referenciado en `ArtSet.mixer`. Las fuentes de música salen por Music y las de efectos por SFX.
- **Volumen en decibelios:** `AudioManager.ToDecibels`: 20·log10(v), y 0 = −80 dB. Ejemplos: 100 % = 0 dB, 50 % ≈ −6 dB, 25 % ≈ −12 dB. Si `SetFloat` devuelve false se registra un error.
- **Una sola vía por categoría:** las propiedades `Master`, `Music` y `Effects` guardan el valor normalizado y aplican los dB al mixer. El fundido solo cambia `AudioSource.volume` (0..1) y nunca toca el mixer. Mover un slider no ejecuta Play, Stop ni reinicia pistas.
- **Carga de preferencias:** `PlayerPrefs.HasKey` con predeterminados 100 / 60 / 80 %. Los valores NaN o inválidos se descartan. Los volúmenes se aplican en Start y en Configure (`SetFloat` no es fiable en Awake). Los sliders se sincronizan con `SetValueWithoutNotify`.
- **Diagnóstico:** `AudioManager.LogDiagnostics` (apagado por defecto) registra los valores de los sliders y los dB aplicados.

### Verificado (prueba PlayMode en Unity 6000.3 con la escena real, midiendo la salida del listener)
- **A, inicio:** un listener, mixer conectado, valores 100/60/80 → 0 / −4,4 / −1,9 dB. La música del menú suena a ≈ −26 dBFS sin tocar nada.
- **B, general 50 %:** −6 dB, la salida baja 5,8 dB sin silencios, recupera el nivel al 100 % y la canción no se reinicia.
- **C, música 30 %:** −10,5 dB, la música baja y SFX no cambia.
- **D, efectos 20 %:** −14 dB, el efecto baja 11,3 dB y Music no cambia.
- **E, silencio y recuperación:** con música o efectos en 0 % no sale sonido; al subir a 50 % vuelve a oírse de inmediato.
- **F, persistencia:** los valores 70/45/35 se conservan al volver al menú, al iniciar partida y al recargar la escena.
- **G, evento:** la música del evento empieza con la notificación, `eclipse.mp3` suena al aparecer el fenómeno y después vuelve la música de gameplay. En pausa la música sigue sin reiniciarse.

## Alerta anticipada y música persistente de los acontecimientos (15-nov-2026)

### Anticipación
- Los tres acontecimientos principales se anuncian **2 días antes**: `radiationWarningLead`, `meteorWarningLead` y `eclipseWarningLead` valen 2 en `BalanceConfig` y en `GameBalance.asset`, y se pueden ajustar ahí. Antes, micrometeoritos y eclipse avisaban con 1 día.
- Todo se mide en días del juego (calendario existente `EventScheduler`/`TurnManager`). No hay temporizadores en segundos.
- **Texto de la alerta:** se usa la ventana de evento existente con el subtítulo "¡ALERTA! Una tormenta solar fuerte llegará en 2 días.", "¡ALERTA! Se aproxima una lluvia de micrometeoritos: llegará en 2 días." o "¡ALERTA! La Sombra de la Tierra llegará en 2 días.". Al cerrarla el jugador vuelve al gameplay con acceso completo a los edificios. El día de preparación no abre ninguna ventana.
- Si el monitoreo estuvo apagado, el siguiente evento llega sin aviso (regla existente). Su música empieza con la notificación de llegada.

### Fases (`EventPhase`, calculadas por `EventScheduler.Phase` a partir del estado guardado)
| Fase | Cuándo | Música |
|---|---|---|
| Scheduled | Antes de la alerta | Gameplay |
| Warning | Día de la alerta (`warningDay`) | Evento (empieza) |
| Preparation | Días entre la alerta y el evento | Evento |
| Active | Día del evento, hasta resolverlo | Evento (la misma pista, sin reiniciar) |
| Resolving | Resto del día en que se resolvió (`resolvedDay`) y mientras duran sus efectos de varios turnos: eclipse → sobrecarga de energía o penalización de comida; micrometeoritos o radiación → reparación con tiempo | Evento |
| Resolved | Terminó | Gameplay (fundido de 3 s) |

- Al ser un cálculo sobre el estado guardado, la música es correcta también al CONTINUAR una partida.
- `ScheduledEvent.Uid` identifica cada acontecimiento.
- `EventScheduler.MusicEvent` elige uno solo si se solapan: primero el activo, luego el que se acerca y luego el que termina.

### Audio
- `AudioManager.BeginEventMusic(uid)` y `EndEventMusic()` son las solicitudes explícitas de inicio y fin.
  - Pedir la música del mismo acontecimiento no hace nada.
  - Si cambia el dueño, la pista sigue sin reiniciarse.
  - Al terminar, fundido de 3 s de vuelta al gameplay, que retoma donde iba.
- `UIManager.UpdateMusic` decide solo por la fase del acontecimiento, nunca por las ventanas abiertas. Seleccionar edificios, encenderlos o apagarlos, abrir paneles, cerrar notificaciones o pasar de día no reinicia ni detiene la música.
- El efecto del acontecimiento suena una sola vez, cuando aparece el fenómeno (`EventIntro`).

Verificado con tests del núcleo (alerta a 2 días, fases, efectos de varios turnos, solapamiento) y con una prueba PlayMode en Unity con días reales. Recorrido: alerta → cerrar → edificios → preparación → eclipse (efecto una vez) → decisión → efectos de varios turnos → fundido a gameplay → continuar partida guardada.
