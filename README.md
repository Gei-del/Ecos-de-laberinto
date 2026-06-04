# ECOS DEL LABERINTO: CRONOFRAGMENTOS

Juego indie de puzzles top-down sobre **manipulación temporal**: cada muerte o reinicio
genera un **eco** que repite exactamente tus acciones anteriores. Coordina varios ecos para
resolver acertijos imposibles para una sola persona.

- **Motor:** Unity 6 LTS (6000.0.x)
- **Lenguaje:** C# (namespace raíz `EcosDelLaberinto`)
- **Plataformas objetivo:** PC + Steam Deck
- **Arquitectura:** Clean Architecture, SOLID, Event-Driven, Data-Driven, Dependency Injection
- **Patrones:** Observer, State Machine, Command, Factory, Strategy

---

## Cómo ejecutar

1. Abre la carpeta del proyecto con **Unity 6 LTS** (Unity Hub → Add → selecciona la carpeta).
2. Deja que Unity importe los paquetes (uGUI, etc.).
3. **Opción A — Cero configuración:** abre cualquier escena vacía y pulsa **Play**.
   Un bootstrap (`GameApp.AutoBoot`, vía `[RuntimeInitializeOnLoadMethod]`) crea
   automáticamente todo el juego, incluyendo cámara, UI y contenido por defecto.
4. **Opción B — Escena y assets para diseñadores:**
   - Menú **`Ecos → Generate Content Assets`**: genera los ScriptableObjects
     (`GameConfig`, 4 personajes, 25 logros, 10 niveles, `GameDatabase`, `AudioLibrary`)
     bajo `Assets/_Project/ScriptableObjects/`.
   - Menú **`Ecos → Create Main Scene`**: crea `Assets/_Project/Scenes/Main.unity` con un
     `GameApp` ya enlazado a la `GameDatabase` y la añade a Build Settings.

> El contenido completo del juego se genera en código (`Bootstrap/DefaultContent.cs`), de modo
> que el proyecto es jugable inmediatamente tras importarlo, sin pasos de configuración manual.

### Requisito de Input
El input usa el **Input Manager clásico** (`UnityEngine.Input`). Si Unity te pregunta por el
sistema de input, deja **Active Input Handling** en *Input Manager (Old)* o *Both*
(Project Settings → Player).

---

## Controles

| Acción | Teclas |
|---|---|
| Mover (rejilla, sin diagonales) | `WASD` / Flechas |
| Interactuar (botón/interruptor) | `Espacio` / `E` |
| Habilidad del personaje | `Shift` / `Q` |
| Reiniciar (¡crea un eco!) | `R` |
| Pausa | `Esc` / `P` |

---

## Bucle de juego

1. Te mueves por el nivel; tus acciones se graban tick a tick (simulación de paso fijo
   determinista, independiente del frame rate).
2. Al **morir** (láser) o pulsar **R**, la grabación se sella y se convierte en un **eco**.
3. En cada bucle siguiente, **todos los ecos repiten** sus acciones en perfecta sincronía:
   mantienen botones, bloquean láseres, etc.
4. Coordina ecos + jugador vivo para abrir puertas y alcanzar la salida (`X`),
   recogiendo los 3 cristales (`C`) para optar a 3 estrellas.

---

## Estructura del proyecto

```
Assets/_Project/
  Scripts/
    Core/         DI, EventBus, StateMachine, Command, utils
    Domain/       GridDirection, EchoFrame, EchoRecording, LevelResult
    Data/         ScriptableObjects: CharacterData, LevelData, GameConfig, GameDatabase, AchievementData
    Gameplay/
      Time/         TickService (reloj determinista)
      Level/        LevelGrid, MovementResolver, GridActor, LevelBuilder, LevelSession, BuiltLevel
      Player/       PlayerController, ICharacterAbility, CharacterAbilities, AbilityFactory
      Echoes/       EchoActor, EchoFactory
      Interactables/ ButtonPad, ToggleSwitch, Door, LaserEmitter, Crystal, ExitPad, PushableBlock
    Input/        IInputService, UnityInputService
    Persistence/  SaveData, ISaveService, JsonSaveService (JSON en persistentDataPath)
    Economy/      EconomyService (Fragmentos Temporales)
    Scoring/      ScoreService (estrellas), AchievementService
    Audio/        IAudioService, AudioService, AudioLibrary
    UI/           UIFactory, UIController (menú, HUD, pausa, resultados, toasts — todo por código)
    Bootstrap/    GameApp (composition root), GameStates, DefaultContent
  Editor/         ContentGenerator (genera assets + escena)
  ScriptableObjects/  (generados por el menú Ecos)
  Scenes/             (Main.unity generada por el menú Ecos)
docs/             ROADMAP, STEAM_STRATEGY, ARCHITECTURE
```

---

## Formato de niveles (data-driven)

Los niveles se definen como rejillas ASCII en `LevelData.Rows` (fila superior primero):

```
#########
#P..B..D#
#...#..C#
#..L#..X#
#########
```

| Símbolo | Significado |
|---|---|
| `#` | Muro |
| `.` / espacio | Suelo |
| `P` | Spawn del jugador |
| `X` | Salida |
| `C` | Cristal |
| `B` | Botón (momentáneo) |
| `T` | Interruptor (palanca) |
| `D` | Puerta |
| `L` | Emisor láser |
| `O` | Bloque empujable |

`LevelBuilder` parsea esto a GameObjects vivos en runtime — sin prefabs ni escenas por nivel.

---

## Guardado

JSON único en `Application.persistentDataPath/ecos_save.json` (escritura atómica).
Persiste progreso por nivel, estrellas, fragmentos, personajes y logros desbloqueados,
y configuración.

---

## Documentación

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — arquitectura y patrones.
- [`docs/ROADMAP.md`](docs/ROADMAP.md) — roadmap de producción (MVP → 1.0).
- [`docs/STEAM_STRATEGY.md`](docs/STEAM_STRATEGY.md) — estrategia de publicación en Steam.

## Estado actual (MVP)

Implementado: movimiento, sistema de ecos (grabación/reproducción determinista), reinicio que
crea ecos, botones, interruptores, puertas, láseres, cristales, bloques, guardado JSON, menú
principal + HUD + pausa + resultados, 10 niveles del Mundo 1, 4 personajes con habilidades,
25 logros y economía de fragmentos. Extensible hacia los Mundos 2–4 y el jefe final.
