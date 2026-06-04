# Arquitectura — ECOS DEL LABERINTO

Objetivo: un núcleo **determinista**, desacoplado y data-driven que permita escalar de 10 a
65+ niveles y a un jefe final sin reescribir sistemas.

## Principios

- **Clean Architecture / SOLID:** dependencias hacia abstracciones (`IEventBus`, `ISaveService`,
  `IInputService`, `ITickService`, `IScoreService`, `IEconomyService`, `IAudioService`,
  `ICharacterAbility`). El gameplay no conoce implementaciones concretas.
- **Event-Driven (Observer):** los sistemas se comunican por `EventBus` con structs `IGameEvent`
  (`LevelLoadedEvent`, `LoopStartedEvent`, `PlayerDiedEvent`, `CrystalCollectedEvent`,
  `LevelCompletedEvent`, `InteractableToggledEvent`, `FragmentsChangedEvent`,
  `AchievementUnlockedEvent`, …). Ningún sistema mantiene referencias directas a otro.
- **Data-Driven:** todo el contenido vive en ScriptableObjects (`LevelData` ASCII, `CharacterData`,
  `GameConfig`, `AchievementData`) agregados en una `GameDatabase`.
- **Dependency Injection:** `ServiceContainer` (registro de instancias y factorías) es el
  *composition root* construido en `GameApp`.

## Patrones de diseño

| Patrón | Dónde |
|---|---|
| **Observer** | `EventBus` / `IGameEvent` |
| **State Machine** | `StateMachine` + estados de flujo (`MenuState`, `PlayingState`, `PausedState`, `ResultsState`) |
| **Command** | `ICommand` + `CommandInvoker` (base para undo/acciones) |
| **Factory** | `AbilityFactory` (habilidades), `EchoFactory` (ecos), `LevelBuilder` (mundo) |
| **Strategy** | `ICharacterAbility` con 4 implementaciones (Nova/Atlas/Echo/Chrona) |

## Determinismo: el corazón de los ecos

La simulación avanza en **ticks de paso fijo** (`TickService`, 8–10 ticks/s configurables en
`GameConfig`), no por frame. Cada tick el jugador añade un `EchoFrame` (celda, orientación,
interacción, uso de habilidad) a una `EchoRecording`. Al sellar la grabación, un `EchoActor`
la reproduce **celda a celda** (no por input), garantizando que el eco repite exactamente lo
sucedido sin depender del frame rate ni de la reacción del mundo. Esto es lo que hace justos
los speedruns y fiable la coordinación de múltiples ecos.

### Orden de evaluación por tick (`LevelSession.OnTick`)

1. Los ecos reproducen su frame.
2. El jugador vivo actúa (input → movimiento → habilidad → graba frame).
3. Botones (`ButtonPad`) — activos mientras un actor está encima.
4. Interruptores (`ToggleSwitch`) — flanco de subida de "interactuar".
5. Puertas (`Door`) — regla AND sobre sus activadores.
6. Láseres (`LaserEmitter`) — calculan haz; matan al jugador vivo, los ecos lo bloquean.
7. Cristales — sólo el jugador vivo los recoge.
8. Comprobación de salida (victoria).
9. Difusión del temporizador (HUD).
10. Detección de muerte/reinicio → marca un nuevo bucle.

El movimiento se valida en un único lugar (`MovementResolver`): muros, puertas cerradas
(blockers dinámicos en `LevelGrid`) y empuje de bloques. Sin diagonales.

## Flujo de juego (State Machine)

```
Menu ──Play──▶ Playing ──Esc──▶ Paused ──Continuar──▶ Playing
                  │                  └──Menú──▶ Menu
                  ├──Victoria──▶ Results ──Siguiente/Reintentar──▶ Playing
                  └──────────────────────────└──Menú──▶ Menu
```

`PlayingState` gestiona la pausa **antes** de avanzar ticks (para que un tick no consuma el
latch de input antes), llama a `TickService.Advance(dt)` y, tras el lote, aplica el reinicio
pendiente (`LevelSession.ApplyPendingRestart`) o transiciona a resultados al completar.

## Persistencia

`JsonSaveService` serializa `SaveData` a `Application.persistentDataPath/ecos_save.json` con
escritura atómica (temp + replace) y un hook de migración por `SaveVersion`.

## Bootstrap

`GameApp` es el composition root: construye `GameDatabase` (asset asignado o `DefaultContent`),
registra servicios en el contenedor, crea la UI y la `LevelSession`, suscribe la sesión al
reloj y arranca la máquina de estados. `AutoBoot` (`[RuntimeInitializeOnLoadMethod]`) instancia
un `GameApp` si la escena no trae uno, de modo que el juego corre sin cableado manual.

## Extensión a Mundos 2–4 y jefe

- Nuevas trampas/elementos: implementar el comportamiento + un símbolo en `LevelBuilder`.
- Nuevos niveles: añadir `LevelData` (ASCII) a la `GameDatabase`.
- Jefe final: una `LevelSession` especializada o un estado dedicado que orqueste fases
  (distorsión, invasión de ecos corruptos, inversión de controles, colapso) reutilizando los
  mismos sistemas de ecos y mecanismos.
