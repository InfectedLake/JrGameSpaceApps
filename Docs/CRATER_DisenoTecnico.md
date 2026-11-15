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
| Edificio Vivienda (reemplaza al escudo) | Energía −5/día, empieza encendida. Encendida: integridad +2/día. Apagada o sin energía: −5/día. Por eso el día 1 proyecta Energía 90 (antes 95 con el escudo apagado). |
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
