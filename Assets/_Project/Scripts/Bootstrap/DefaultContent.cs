using System.Collections.Generic;
using EcosDelLaberinto.Data;
using UnityEngine;

namespace EcosDelLaberinto.Bootstrap
{
    /// <summary>
    /// Builds a complete, playable <see cref="GameDatabase"/> in code (config, the four characters,
    /// 25 achievements and the 10 MVP levels). Used as a fallback when no database asset is assigned
    /// so the project runs immediately on Play. The matching Editor generator writes the same data
    /// as real .asset files for designers.
    /// </summary>
    public static class DefaultContent
    {
        public static GameDatabase Build()
        {
            var db = ScriptableObject.CreateInstance<GameDatabase>();
            db.Config = BuildConfig();
            db.Characters = BuildCharacters();
            db.Achievements = BuildAchievements();
            db.Levels = BuildLevels();
            db.BuildIndices();
            return db;
        }

        public static GameConfig BuildConfig()
        {
            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.TicksPerSecond = 8;
            config.CellSize = 1f;
            config.MaxEchoes = 10;
            config.BaseEchoLifetimeSeconds = 40f;
            config.FragmentsPerStar = 25;
            config.FragmentsPerCrystal = 10;
            config.DefaultThreeStarTime = 25f;
            config.DefaultTwoStarTime = 55f;
            return config;
        }

        private static CharacterData NewCharacter(string id, string name, string desc,
            CharacterAbility ability, float echoMult, bool unlocked, int cost, Color tint)
        {
            var c = ScriptableObject.CreateInstance<CharacterData>();
            c.Id = id;
            c.DisplayName = name;
            c.Description = desc;
            c.Ability = ability;
            c.EchoLifetimeMultiplier = echoMult;
            c.UnlockedByDefault = unlocked;
            c.UnlockCost = cost;
            c.AbilityCooldown = 5f;
            c.TintColor = tint;
            return c;
        }

        public static List<CharacterData> BuildCharacters()
        {
            return new List<CharacterData>
            {
                NewCharacter("nova", "Nova", "Sus ecos duran mas tiempo.",
                    CharacterAbility.LongerEchoes, 1.5f, true, 0, new Color(0.6f, 0.9f, 1f)),
                NewCharacter("atlas", "Atlas", "Puede empujar bloques pesados.",
                    CharacterAbility.MoveHeavyBlocks, 1f, false, 300, new Color(1f, 0.7f, 0.4f)),
                NewCharacter("echo", "Echo", "Intercambia su posicion con un eco.",
                    CharacterAbility.SwapWithEcho, 1f, false, 500, new Color(0.8f, 0.6f, 1f)),
                NewCharacter("chrona", "Chrona", "Retrocede el tiempo tres segundos.",
                    CharacterAbility.RewindTime, 1f, false, 1000, new Color(1f, 0.5f, 0.8f))
            };
        }

        private static AchievementData NewAch(string id, string name, string desc)
        {
            var a = ScriptableObject.CreateInstance<AchievementData>();
            a.Id = id;
            a.DisplayName = name;
            a.Description = desc;
            a.SteamApiName = id.ToUpperInvariant();
            return a;
        }

        public static List<AchievementData> BuildAchievements()
        {
            return new List<AchievementData>
            {
                NewAch("first_echo", "Primer Eco", "Crea tu primer eco temporal."),
                NewAch("no_death", "Sin Morir", "Completa un nivel sin morir."),
                NewAch("collector", "Coleccionista", "Recoge todos los cristales de un nivel."),
                NewAch("perfectionist", "Perfeccionista", "Consigue 3 estrellas en un nivel."),
                NewAch("echo_master", "Maestro de Ecos", "Ten 5 ecos activos a la vez."),
                NewAch("temporal_master", "Maestro Temporal", "Completa 10 niveles."),
                NewAch("time_dominator", "Dominador del Tiempo", "Acumula 1000 fragmentos."),
                NewAch("speedrunner", "Velocista", "Completa un nivel en menos de 10 segundos."),
                NewAch("pacifist", "Pacifista", "Completa un mundo sin usar habilidades."),
                NewAch("explorer", "Explorador", "Encuentra un area secreta."),
                NewAch("relic_hunter", "Cazador de Reliquias", "Consigue tu primera reliquia."),
                NewAch("nova_unlock", "Despertar de Nova", "Comienza tu viaje con Nova."),
                NewAch("atlas_unlock", "Fuerza de Atlas", "Desbloquea a Atlas."),
                NewAch("echo_unlock", "El Reflejo", "Desbloquea a Echo."),
                NewAch("chrona_unlock", "Senora del Tiempo", "Desbloquea a Chrona."),
                NewAch("world1_clear", "Instalacion Despejada", "Completa el Mundo 1."),
                NewAch("world2_clear", "Nucleo Estabilizado", "Completa el Mundo 2."),
                NewAch("world3_clear", "Laboratorio Resuelto", "Completa el Mundo 3."),
                NewAch("world4_clear", "Fractura Sellada", "Completa el Mundo 4."),
                NewAch("boss_defeated", "Fin del Devorador", "Derrota al Devorador del Tiempo."),
                NewAch("ten_echoes", "Legion Temporal", "Ten 10 ecos activos a la vez."),
                NewAch("no_echo_clear", "Solitario", "Completa un nivel sin usar ecos."),
                NewAch("crystal_master", "Maestro de Cristales", "Recoge 50 cristales en total."),
                NewAch("three_star_world", "Mundo Perfecto", "Consigue 3 estrellas en todo un mundo."),
                NewAch("completionist", "Completista", "Completa el juego al 100%.")
            };
        }

        private static LevelData NewLevel(string id, int world, int index, string name,
            string hint, string[] rows)
        {
            var l = ScriptableObject.CreateInstance<LevelData>();
            l.Id = id;
            l.World = world;
            l.IndexInWorld = index;
            l.DisplayName = name;
            l.TutorialHint = hint;
            l.Rows = rows;
            l.ThreeStarMaxDeaths = 4;
            return l;
        }

        public static List<LevelData> BuildLevels()
        {
            return new List<LevelData>
            {
                NewLevel("w1_l1", 1, 1, "Despertar",
                    "Usa WASD o las flechas para moverte y alcanza la salida.",
                    new[]
                    {
                        "#########",
                        "#P.....C#",
                        "#.......#",
                        "#C.....C#",
                        "#.......#",
                        "#......X#",
                        "#########"
                    }),

                NewLevel("w1_l2", 1, 2, "Pasillos",
                    "Recorre los pasillos y recoge los cristales.",
                    new[]
                    {
                        "#########",
                        "#P.....C#",
                        "#.#####.#",
                        "#.#.C.#.#",
                        "#.#.#.#.#",
                        "#C.....X#",
                        "#########"
                    }),

                NewLevel("w1_l3", 1, 3, "El Primer Eco",
                    "Parate en el boton y pulsa R para crear un eco que lo mantenga. La puerta se abrira.",
                    new[]
                    {
                        "#########",
                        "#P.B#..X#",
                        "#...D..C#",
                        "#C..#...#",
                        "#########"
                    }),

                NewLevel("w1_l4", 1, 4, "Doble Eco",
                    "Dos botones, dos ecos. Manten ambos pulsados para abrir la puerta.",
                    new[]
                    {
                        "#########",
                        "#PB.#..X#",
                        "#...D..C#",
                        "#.B.#...#",
                        "#########"
                    }),

                NewLevel("w1_l5", 1, 5, "Interruptor",
                    "Parate en el interruptor y pulsa Espacio para activarlo (se queda encendido).",
                    new[]
                    {
                        "#########",
                        "#P..#..X#",
                        "#...D..C#",
                        "#..T#...#",
                        "#########"
                    }),

                NewLevel("w1_l6", 1, 6, "Espejismo Laser",
                    "El laser es mortal. Muere dentro del haz para dejar un eco que lo bloquee y cruza.",
                    new[]
                    {
                        "#########",
                        "#P.....C#",
                        "#.......#",
                        "#L......#",
                        "#.......#",
                        "#C....X.#",
                        "#########"
                    }),

                NewLevel("w1_l7", 1, 7, "Coordinacion",
                    "Coordina dos ecos en los botones mientras recoges los cristales.",
                    new[]
                    {
                        "#########",
                        "#PB.#..X#",
                        "#...#..C#",
                        "#.B.D...#",
                        "#...#...#",
                        "#C..#..C#",
                        "#########"
                    }),

                NewLevel("w1_l8", 1, 8, "Relevo",
                    "Combina interruptor y boton para abrir el camino.",
                    new[]
                    {
                        "#########",
                        "#P.T#..X#",
                        "#...D..C#",
                        "#..B#...#",
                        "#########"
                    }),

                NewLevel("w1_l9", 1, 9, "Barrera",
                    "Dos laseres. Necesitaras dos ecos como escudo para cruzar.",
                    new[]
                    {
                        "#########",
                        "#P.....C#",
                        "#L......#",
                        "#.......#",
                        "#......L#",
                        "#C....X.#",
                        "#########"
                    }),

                NewLevel("w1_l10", 1, 10, "Convergencia",
                    "Tres botones, tres ecos. Sincroniza para abrir la puerta final.",
                    new[]
                    {
                        "###########",
                        "#P.B.B.B#X#",
                        "#.......D.#",
                        "#.......#.#",
                        "#.......#.#",
                        "#.......#.#",
                        "###########"
                    })
            };
        }
    }
}
