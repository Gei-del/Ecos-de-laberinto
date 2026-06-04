# Roadmap de Producción — ECOS DEL LABERINTO: CRONOFRAGMENTOS

Plan por fases desde el MVP actual hasta el lanzamiento 1.0 en Steam. Las duraciones son
estimaciones para un equipo indie pequeño (1–3 personas) y deben ajustarse al equipo real.

## Fase 0 — MVP técnico ✅ (completado)

Núcleo jugable de extremo a extremo:
- Movimiento en rejilla determinista (ticks de paso fijo).
- Sistema de ecos: grabación, sellado en muerte/reinicio y reproducción sincronizada.
- Mecanismos: botones, interruptores, puertas (AND), láseres, cristales, bloques.
- Mundo 1 completo (10 niveles), 4 personajes con habilidades (Strategy).
- Puntuación por estrellas, economía de Fragmentos Temporales, 25 logros.
- Guardado JSON persistente; menú, HUD, pausa, resultados y toasts (UI por código).
- Arquitectura DI + Event Bus + State Machine + Factory + Command.

## Fase 1 — Vertical slice pulido (4–6 semanas)

- **Arte:** reemplazar sprites primitivos por tileset y sprites de personaje/eco; paleta y
  post-procesado (bloom/cromatic aberration para el tono sci-fi).
- **Audio:** poblar `AudioLibrary` (música ambiental electrónica + SFX de ecos, puertas,
  cristales, distorsión, muerte). Mezcla y volúmenes en ajustes.
- **Feel:** animaciones de tween, partículas al crear eco, screenshake sutil, feedback de láser.
- **UX:** selección de personaje y de nivel con previsualización; tutorial integrado en los
  primeros niveles; rebinding de controles; soporte de mando (Steam Input) para Steam Deck.
- **QA:** validador de niveles (resolubilidad), pruebas de determinismo de ecos.

## Fase 2 — Mundo 2 "Núcleo Temporal" (4–5 semanas)

- 15 niveles. Introduce **plataformas móviles** y **coordinación de 2 ecos**.
- Reliquias: **Núcleo Temporal** (+1 eco), **Memoria Infinita** (mayor duración de ecos).
- Curva de dificultad y métricas de telemetría (muertes/nivel, tiempo medio).

## Fase 3 — Mundo 3 "Laboratorio Cuántico" (5–6 semanas)

- 20 niveles. **Portales/teletransportadores**, **ecos corruptos enemigos**, trampas avanzadas
  (minas temporales, pisos fantasma, torres de vigilancia, campos de distorsión).
- Reliquias: **Cronómetro Cuántico** (menos recarga), **Corazón de Paradoja** (vida extra).

## Fase 4 — Mundo 4 "Fractura Temporal" + Jefe (6–8 semanas)

- 20 niveles centrados en **sincronización avanzada hasta 10 ecos**.
- **Jefe final "El Devorador del Tiempo"** en 4 fases (distorsión, invasión de ecos corruptos,
  inversión de controles, colapso dimensional) que exige coordinar múltiples ecos.

## Fase 5 — Contenido meta y economía (3–4 semanas)

- Tienda de cosméticos/skins/efectos (sin pay-to-win), desbloqueo de personajes.
- Logros Steam reales (mapear `AchievementData.SteamApiName` a Steamworks).
- Modo speedrun con cronómetro, tablas de tiempos y replays de ecos.

## Fase 6 — Beta, optimización y certificación (3–4 semanas)

- Optimización para Steam Deck (60 fps, batería, texto legible, controles).
- Localización (ES/EN como mínimo; el contenido ya está externalizado en datos).
- Playtesting cerrado, pulido de dificultad, corrección de bugs.
- Verificación "Steam Deck Verified", build de release y página de Steam lista.

## Fase 7 — Lanzamiento 1.0 y post-launch

- Demo para Steam Next Fest, wishlists, lanzamiento.
- Soporte post-launch: parches, nivel comunitario/editor, posibles DLC de mundos.

## Hitos / KPIs

| Hito | Criterio de salida |
|---|---|
| MVP | Bucle completo jugable, 10 niveles, guardado. ✅ |
| Vertical slice | Mundo 1 con arte+audio final; "se siente" comercial. |
| Contenido | 65 niveles + jefe, todas las reliquias. |
| Beta | Estable en PC y Steam Deck, sin bloqueantes. |
| 1.0 | Página Steam, logros, localización, certificación. |
