using EcosDelLaberinto.Core.Utils;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Level;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Player
{
    /// <summary>Nova — passive: echoes last longer. No active effect.</summary>
    public sealed class LongerEchoesAbility : ICharacterAbility
    {
        public LongerEchoesAbility(float multiplier) => EchoLifetimeMultiplier = multiplier;
        public float EchoLifetimeMultiplier { get; }
        public bool CanPushHeavy => false;
        public bool Activate(AbilityContext context) => false;
    }

    /// <summary>Atlas — passive: can push heavy blocks. No active effect.</summary>
    public sealed class HeavyPushAbility : ICharacterAbility
    {
        public float EchoLifetimeMultiplier => 1f;
        public bool CanPushHeavy => true;
        public bool Activate(AbilityContext context) => false;
    }

    /// <summary>Echo — active: swap grid position with the nearest living echo.</summary>
    public sealed class SwapWithEchoAbility : ICharacterAbility
    {
        public float EchoLifetimeMultiplier => 1f;
        public bool CanPushHeavy => false;

        public bool Activate(AbilityContext context)
        {
            if (context?.Player == null || context.Echoes == null)
            {
                return false;
            }

            IGridActor nearest = null;
            var bestDist = int.MaxValue;
            foreach (var echo in context.Echoes)
            {
                if (!echo.IsAlive)
                {
                    continue;
                }

                var d = Mathf.Abs(echo.Cell.x - context.Player.Cell.x) +
                        Mathf.Abs(echo.Cell.y - context.Player.Cell.y);
                if (d > 0 && d < bestDist)
                {
                    bestDist = d;
                    nearest = echo;
                }
            }

            if (nearest == null)
            {
                return false;
            }

            var playerCell = context.Player.Cell;
            context.Player.ForceMoveTo(nearest.Cell);
            nearest.ForceMoveTo(playerCell);
            GameLogger.Info("Echo ability: swapped position with nearest echo.");
            return true;
        }
    }

    /// <summary>
    /// Chrona — active: rewind the live player three seconds. The player keeps a short ring buffer
    /// of recent cells; activating teleports back to the cell from ~3s ago.
    /// </summary>
    public sealed class RewindTimeAbility : ICharacterAbility
    {
        private readonly int _rewindTicks;

        public RewindTimeAbility(int ticksPerSecond) => _rewindTicks = ticksPerSecond * 3;

        public float EchoLifetimeMultiplier => 1f;
        public bool CanPushHeavy => false;

        public bool Activate(AbilityContext context)
        {
            if (context?.Player is not PlayerController player)
            {
                return false;
            }

            var target = player.GetHistoricalCell(_rewindTicks);
            if (target == player.Cell)
            {
                return false;
            }

            player.ForceMoveTo(target);
            GameLogger.Info("Chrona ability: rewound the player 3 seconds.");
            return true;
        }
    }
}
