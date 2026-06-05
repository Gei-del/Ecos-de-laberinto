# Maze Runner v4.0 — Documentación técnica y funcional

Juego web cooperativo (evolución de la visión "laberinto cooperativo") incluido en
`web/index.html` + `web/game.js`. No reemplaza al proyecto Unity (que permanece
intacto en la raíz del repo); es la línea web desplegable en Vercel.

- **Motor:** Canvas 2D puro, ~60 fps, sin dependencias externas ni build step.
- **Entrada:** `web/index.html` (HTML + CSS inline) carga `web/game.js`.
- **Despliegue:** `vercel.json` → `outputDirectory: "web"`, `cleanUrls: true`.
- **Diseño V0 original:** preservado en `web/design/` (no se eliminó nada).

---

## 1. Cómo jugar (funcional)

### Acceso
- **Invitado:** escribe un nombre y elige avatar/personaje. Sin registro.
- **Cuenta (local):** botón de perfil → guardar con correo. Persiste progreso,
  cartera, logros y avatar en `localStorage`.
- **Google:** estructura lista pero **deshabilitada** hasta configurar un OAuth
  Client ID: `localStorage.setItem("mzr_gcid","TU_CLIENT_ID")` y recargar.

### Modos cooperativos (1–4 jugadores, misma pantalla)
| Jugadores | Controles |
|-----------|-----------|
| P1 | WASD o flechas (+ D-pad / swipe táctil) |
| P2 | IJKL |
| P3 | TFGH |
| P4 | Teclado numérico (8/4/5/6) |

- **Vidas de equipo:** compartidas (3 base + 1 por jugador extra).
- **Caído / Revivir:** si un fantasma atrapa a un jugador, queda *caído*
  (translúcido, con barra de revivir). Un compañero **vivo y adyacente** lo
  revive en `REVIVE_T = 0.9 s`. Si **todos** caen → se pierde una vida y el
  equipo reaparece con invulnerabilidad temporal (*team wipe*).
- **Salida cooperativa:** el nivel avanza cuando **todos** los jugadores vivos
  alcanzan la salida (se abre tras recoger ≥50 % de los puntos).

### Objetivo
Recoge puntos/cristales, evita (o cómete, con power-up) a los fantasmas y
encuentra la salida antes de que el equipo se quede sin vidas.

---

## 2. Sistemas de juego

### Personajes (6)
`Max 🐶` (velocidad ×1.25), `Luna 🐱` (imán ×1.5), `Kiko 🐵` (power-ups +50 %),
`Ember 🐲` (invencibilidad ×2), `Loro 🦜` (combo +60 %), `Bandit 🦝` (puntos ×1.5).
Se desbloquean con monedas (0/0/100/200/350/500).

### Fantasmas (4 personalidades)
- **Hunter** — persecución directa con memoria de la última posición.
- **Whisper** — se posiciona detrás del objetivo y emite susurros espaciales.
- **Guardian** — patrulla una zona y ataca si te acercas.
- **Chaos** — cambia de objetivo de forma aleatoria.

### Audio inmersivo (Web Audio API, generativo)
- **Paneo espacial estéreo:** cada efecto se panea según la celda del emisor
  (`panOfCell`). Susurros, monedas, power-ups y "comer fantasma" son posicionales.
- **Tensión adaptativa:** el volumen/respiración sube con la cercanía del fantasma
  más próximo (`setTension`).
- Se inicializa tras la primera interacción del usuario (política de autoplay).

### Retención
- **Misiones diarias** (rotativas) con recompensa en monedas.
- **Logros** (primeros pasos, combos, niveles, sin morir, rescatar compañero,
  escape de escuadrón…).
- **Cartera persistente** + **recompensa diaria**.
- **Ranking local** (top puntuaciones en el dispositivo).

---

## 3. Arquitectura del código (`game.js`, 25 módulos)

El archivo está dividido en secciones numeradas (ver cabecera del archivo). Claves:

- **Estado multijugador:** `players[]` — cada jugador tiene posición de rejilla
  (`gx,gy`), posición visual interpolada (`wx,wy,tx,ty`), dirección, combo,
  power-ups, `state` (`alive`/`downed`/`escaped`) y temporizadores propios.
- **Bucle principal (`loop`)**: `requestAnimationFrame` con *delta time*; itera
  `players[]` para interpolación, auto-repeat de movimiento y temporizadores;
  luego IA de fantasmas, colisiones, `processRevives()` y comprobación de
  victoria/wipe cooperativo.
- **Entrada:** un único `keydown`/`keyup` global mapea cada tecla a
  `[jugador, dirección]`. Se ignoran pulsaciones cuando el foco está en un
  `input`/`textarea` (`isTypingTarget`) para no interferir al escribir nombre/correo.
- **Persistencia:** claves `mzr_profile`, `mzr_mission`, `mzr_lb`, `mzr_best`.

---

## 4. Despliegue (Vercel)

El repo ya está conectado a Vercel. Con `vercel.json` apuntando a `web/`, al
hacer merge a `base` se genera el deploy automático. URL pública en el panel de
Vercel del proyecto.

Para probar localmente:

```bash
cd web
python3 -m http.server 8099
# abrir http://localhost:8099/index.html
```

---

## 5. Notas y límites conocidos
- **Cooperativo = local** (misma pantalla/teclado). El online requiere backend/netcode.
- **Google Login** requiere Client ID (ver arriba); mientras tanto, invitado/correo.
- El proyecto Unity original no se modificó.
</content>
</invoke>
