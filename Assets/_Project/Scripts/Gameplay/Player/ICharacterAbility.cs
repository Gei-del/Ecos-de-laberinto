using System.Collections.Generic;
using EcosDelLaberinto.Gameplay.Level;

namespace EcosDelLaberinto.Gameplay.Player
{
    /// <summary>Context handed to abilities so they can affect the player and the active echoes.</summary>
    public sealed class AbilityContext
    {
        public IGridActor Player;
        public IReadOnlyList<IGridActor> Echoes;
        public LevelGrid Grid;
        public int CurrentTick;
    }

    /// <summary>
    /// Strategy pattern: each character plugs in its own ability without the player controller
    /// knowing the concrete type. Passive traits are exposed as properties; active ones run in
    /// <see cref="Activate"/>.
    /// </summary>
    public interface ICharacterAbility
    {
        /// <summary>Multiplier applied to echo lifetime (Nova &gt; 1).</summary>
        float EchoLifetimeMultiplier { get; }

        /// <summary>Whether this character can push heavy blocks (Atlas).</summary>
        bool CanPushHeavy { get; }

        /// <summary>Invoked when the player triggers their active ability. Returns true if it fired.</summary>
        bool Activate(AbilityContext context);
    }
}
