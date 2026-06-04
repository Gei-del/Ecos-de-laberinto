using System.Collections.Generic;

namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// Static catalogue of the special relics and their gameplay effects. Relics are persisted by id
    /// in the save file; <see cref="EcosDelLaberinto.Gameplay.Level.LevelSession"/> reads the owned
    /// set at level start to apply their bonuses (extra echo, longer echoes, faster cooldown…).
    /// </summary>
    public static class RelicCatalog
    {
        public const string NucleoTemporal = "nucleo_temporal";
        public const string MemoriaInfinita = "memoria_infinita";
        public const string CronometroCuantico = "cronometro_cuantico";
        public const string CorazonParadoja = "corazon_paradoja";

        private static readonly Dictionary<string, string> Names = new()
        {
            { NucleoTemporal, "Nucleo Temporal" },
            { MemoriaInfinita, "Memoria Infinita" },
            { CronometroCuantico, "Cronometro Cuantico" },
            { CorazonParadoja, "Corazon de Paradoja" }
        };

        public static string DisplayName(string relicId)
        {
            return relicId != null && Names.TryGetValue(relicId, out var name) ? name : relicId;
        }
    }
}
