namespace EcosDelLaberinto.Data
{
    /// <summary>
    /// The unique active/passive trait of each playable character. The concrete behaviour is
    /// resolved through the Strategy pattern (<c>ICharacterAbility</c>) so new characters only
    /// require new data + a strategy, never changes to the player controller.
    /// </summary>
    public enum CharacterAbility
    {
        /// <summary>Nova — passive: echoes last longer (records more ticks).</summary>
        LongerEchoes = 0,

        /// <summary>Atlas — can push heavy blocks an ordinary echo cannot move.</summary>
        MoveHeavyBlocks = 1,

        /// <summary>Echo — swap grid position with the nearest echo.</summary>
        SwapWithEcho = 2,

        /// <summary>Chrona — rewind the live player three seconds.</summary>
        RewindTime = 3
    }
}
