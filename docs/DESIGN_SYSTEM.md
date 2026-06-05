# Guía de diseño · ECOS DEL LABERINTO: CRONOFRAGMENTOS

Sistema de diseño extraído del proyecto oficial de V0 e implementado en **Unity UI Toolkit**
(UXML + USS). Estética: *sci-fi* futurista, neón cian sobre vacío oscuro, paneles
holográficos. Esta guía es la fuente de verdad reutilizable; **no rediseñar ni simplificar**.

Archivos:
- Tokens y componentes: `Assets/_Project/UIToolkit/Theme/EcosTheme.uss`
- Pantallas: `Assets/_Project/UIToolkit/Screens/*.uxml`
- Iconos: `Assets/_Project/UIToolkit/Theme/Icons/*.png` (blancos, se tiñen con `--unity-image-tint-color`)
- Fuentes: `Assets/_Project/UIToolkit/Fonts/` (Orbitron, Rajdhani)

---

## 1. Colores

| Token (USS)            | Valor                         | Uso |
|------------------------|-------------------------------|-----|
| `--bg-0`               | `rgb(5,7,13)`                 | Fondo base (vacío) |
| `--bg-1` / `--bg-2`    | `rgb(8,12,22)` / `rgb(12,18,30)` | Superficies oscuras |
| `--cyan`               | `rgb(34,211,238)` `#22D3EE`   | Acento primario, bordes activos |
| `--cyan-bright`        | `rgb(125,249,255)` `#7DF9FF`  | Títulos, texto sobre acento |
| `--cyan-soft`          | `rgba(45,212,238,0.55)`       | Bordes hover, glow |
| `--green`              | `rgb(52,211,153)` `#34D399`   | Subtítulos, estados OK, barras |
| `--amber`              | `rgb(251,191,36)` `#FBBF24`   | Moneda, precios, estrellas |
| `--red`                | `rgb(239,68,68)` `#EF4444`    | Descuentos, muertes, peligro |
| `--purple`             | `rgb(168,85,247)` `#A855F7`   | Mundos/aspectos legendarios |
| `--text`               | `rgb(215,238,246)`            | Texto principal |
| `--text-dim`           | `rgb(138,161,178)`            | Texto secundario |
| `--text-mut`           | `rgb(95,115,132)`             | Texto deshabilitado |
| `--panel-fill`         | `rgba(12,20,34,0.66)`         | Relleno de panel holográfico |
| `--panel-border`       | `rgba(45,212,238,0.40)`       | Borde de panel |

---

## 2. Tipografía

- **Display / títulos / botones:** Orbitron (`.font-display`, `.title-xl`, `.screen-title`,
  `.card-title`, `.menu-btn__label`, `.btn`, `.tab`). Mayúsculas, `letter-spacing` amplio
  (2–16px), `text-shadow` cian para glow.
- **Cuerpo / descripciones / stats:** Rajdhani SemiBold (`.font-body`, `.screen` por defecto).

| Clase            | Tamaño | Peso | Tracking | Color |
|------------------|--------|------|----------|-------|
| `.title-xl`      | 64px   | bold | 6px      | cyan-bright + glow |
| `.title-sub`     | 22px   | —    | 16px     | green |
| `.screen-title`  | 34px   | bold | 4px      | cyan-bright + glow |
| `.card-title`    | 20px   | bold | 1px      | cyan-bright |
| `.card-desc`     | 13px   | —    | —        | text-dim |
| `.menu-btn__label` | 16px | —    | 2px      | cyan-bright |

> Las fuentes se aplican vía `-unity-font: url(...ttf); -unity-font-definition: initial;`
> para forzar el uso del `Font` clásico importado. Orbitron es una fuente variable.

---

## 3. Componentes

| Clase              | Componente |
|--------------------|------------|
| `.screen`          | Lienzo a pantalla completa (fondo `--bg-0`, fuente base) |
| `.bg-grid`         | Capa de rejilla holográfica repetida (`grid_tile.png`) |
| `.bg-vignette`     | Viñeta radial para profundidad (`vignette.png`) |
| `.panel`           | Panel holográfico (relleno translúcido + borde cian + radio 14px) |
| `.menu-btn`        | Fila de menú (icono + label + sufijo), estados hover/selected |
| `.btn`             | Botón de acción outline neón (hover escala 1.03, active 0.98) |
| `.btn--ghost` / `--disabled` | Variantes |
| `.carousel-arrow`  | Botón circular (flechas de carrusel) |
| `.card`            | Tarjeta (mundo/item/logro), hover escala + borde cian |
| `.card--locked` / `--purple` / `--gold` | Variantes de estado/rareza |
| `.stat-row` + `.bar-track` + `.bar-fill` | Barra de estadística/progreso animable |
| `.tab` / `.tab--active` | Pestañas (tienda, inventario) |
| `.chip`            | Píldora translúcida (HUD, contadores) |
| `.badge`           | Etiqueta de descuento (rojo, esquina) |
| `.stars` + `.star` / `.star--empty` | Dificultad (1–5 estrellas ámbar) |

---

## 4. Iconografía

25 iconos de línea (64×64, blancos, tintables): `play, continue, people, person, globe,
cog, trophy, bag, cart, cloud, bolt, shield, clock, hourglass, star, coin, lock,
chevron-left, chevron-right, gift, crown, gem, close, heart, orbit`.

Uso: `background-image: url('.../Icons/<n>.png')` + `--unity-image-tint-color: var(--cyan)`.

---

## 5. Layouts (pantallas)

1. **Menú principal** — logo orbital con glow, `title-xl` + `title-sub`, panel con 9
   `menu-btn` (Nueva partida, Continuar, Personajes, Mundos, Inventario, Tienda, Logros,
   Configuración, Salir).
2. **Selección de personaje** — barra Volver + título; fila central: flecha · retrato
   (`panel` 320×380) · flecha · panel de info (nombre, rol, descripción, 4 barras de stat);
   paginación de puntos; botón Seleccionar.
3. **Selección de mundo** — rejilla `flex-wrap` de `card` (360×210): estrellas, título,
   subtítulo, descripción, barra de progreso; mundos bloqueados con `card--locked` + candado.
4. **HUD** — barra superior de `chip` (tiempo, muertes, ecos, cristales · fragmentos,
   puntuación, pausa); indicador de habilidad con barra de recarga; pista de controles.
5. **Inventario** — pestañas (Personajes/Reliquias/Cosméticos), rejilla de items +
   panel de detalle (icono, nombre, tipo, descripción, botón Equipar).
6. **Tienda** — pestañas (Destacados/Fragmentos/Aspectos/Mejoras), saldo, rejilla de 4
   `card` con `badge` de descuento, precio antiguo/nuevo y Comprar; banner de oferta `card--gold`.
7. **Logros** — barra de progreso total; rejilla de tarjetas con icono, título, descripción
   y estado (trofeo verde = desbloqueado / candado = bloqueado, `card--locked`).

---

## 6. Estados visuales

| Estado       | Tratamiento |
|--------------|-------------|
| **Normal**   | Borde `--panel-border-soft`, relleno translúcido |
| **Hover**    | Borde `--cyan-soft`/`--cyan`, fondo más claro, `scale: 1.01–1.03`, `transition 0.14s` |
| **Selected/Active** | Borde `--cyan` pleno, fondo cian translúcido |
| **Active (press)** | `scale: 0.98` |
| **Disabled** | Borde/texto `--text-mut`, fondo apagado |
| **Locked**   | `opacity: 0.45` + icono de candado |
| **Discount** | `.badge` rojo en esquina superior derecha |

---

## 7. Uso en Unity

1. Abrir el proyecto con **Unity 6 LTS**.
2. Menú **`Ecos → Create UI Toolkit Showcase Scene`**: genera el tema de runtime (`.tss`),
   un `PanelSettings` y la escena `Assets/_Project/Scenes/EcosUI.unity` con un `UIDocument`
   y el componente `EcosUINavigator` ya cableado a las 7 pantallas.
3. Pulsar **Play**: aparece el Menú principal. Teclas **1–7** conmutan entre pantallas;
   los botones navegan entre sí.
4. Editar el diseño con **UI Builder** abriendo cualquier `.uxml` de `UIToolkit/Screens`.

> Las referencias de estilo/imagen usan rutas `project://database/Assets/...`, por lo que
> funcionan sin depender de GUIDs de `.meta`.
