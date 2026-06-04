using EcosDelLaberinto.Data;

namespace EcosDelLaberinto.Gameplay.Player
{
    /// <summary>
    /// Factory pattern: maps the data-driven <see cref="CharacterAbility"/> enum to a concrete
    /// <see cref="ICharacterAbility"/> strategy. Adding a character means adding data + one case.
    /// </summary>
    public sealed class AbilityFactory
    {
        private readonly GameConfig _config;

        public AbilityFactory(GameConfig config)
        {
            _config = config;
        }

        public ICharacterAbility Create(CharacterData character)
        {
            if (character == null)
            {
                return new LongerEchoesAbility(1f);
            }

            return character.Ability switch
            {
                CharacterAbility.LongerEchoes => new LongerEchoesAbility(character.EchoLifetimeMultiplier),
                CharacterAbility.MoveHeavyBlocks => new HeavyPushAbility(),
                CharacterAbility.SwapWithEcho => new SwapWithEchoAbility(),
                CharacterAbility.RewindTime => new RewindTimeAbility(_config.TicksPerSecond),
                _ => new LongerEchoesAbility(1f)
            };
        }
    }
}
