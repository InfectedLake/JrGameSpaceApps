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
