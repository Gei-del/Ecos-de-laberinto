using System.Collections.Generic;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Gameplay.Level;
using EcosDelLaberinto.Input;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Player
{
    /// <summary>
    /// The live, player-controlled actor. Each simulation tick it reads input, resolves grid
    /// movement (with block pushing), optionally fires the character ability, and appends an
    /// <see cref="EchoFrame"/> to the current recording. When the loop ends, that recording becomes
    /// an echo.
    /// </summary>
    public sealed class PlayerController : GridActor
    {
        public override bool IsLivePlayer => true;
        public override bool CanPushHeavy => _ability != null && _ability.CanPushHeavy;

        private MovementResolver _resolver;
        private ICharacterAbility _ability;
        private IEventBus _eventBus;
        private string _characterId;

        private EchoRecording _recording;
        private readonly List<Vector2Int> _history = new();

        private int _abilityCooldownTicks;
        private int _lastAbilityTick = int.MinValue;
        private IReadOnlyList<IGridActor> _echoesForAbility;

        public EchoRecording CurrentRecording => _recording;

        public void Configure(LevelGrid grid, MovementResolver resolver, ICharacterAbility ability,
            IEventBus eventBus, string characterId, int abilityCooldownTicks)
        {
            Grid = grid;
            _resolver = resolver;
            _ability = ability;
            _eventBus = eventBus;
            _characterId = characterId;
            _abilityCooldownTicks = Mathf.Max(0, abilityCooldownTicks);
        }

        public void BeginLoop(Vector2Int spawnCell, IReadOnlyList<IGridActor> echoes)
        {
            IsAlive = true;
            SetAlpha(1f);
            SetCellImmediate(spawnCell);
            SetFacing(GridDirection.Down);
            _recording = new EchoRecording(_characterId);
            _history.Clear();
            _history.Add(spawnCell);
            _lastAbilityTick = int.MinValue;
            _echoesForAbility = echoes;
        }

        public void Step(int tick, IInputService input)
        {
            if (!IsAlive)
            {
                return;
            }

            var abilityUsed = false;
            if (input.AbilityPressed && tick - _lastAbilityTick >= _abilityCooldownTicks)
            {
                var ctx = new AbilityContext
                {
                    Player = this,
                    Echoes = _echoesForAbility,
                    Grid = Grid,
                    CurrentTick = tick
                };

                if (_ability != null && _ability.Activate(ctx))
                {
                    abilityUsed = true;
                    _lastAbilityTick = tick;
                }
            }

            var dir = input.MoveDirection;
            var newCell = _resolver.Resolve(this, dir);
            if (dir != GridDirection.None)
            {
                SetFacing(dir);
            }

            SetCellAnimated(newCell);
            InteractingThisTick = input.InteractHeld;

            _recording?.Append(new EchoFrame(Cell, Facing, InteractingThisTick, abilityUsed));
            _history.Add(Cell);
        }

        /// <summary>Cell the player occupied <paramref name="ticksBack"/> ticks ago (Chrona rewind).</summary>
        public Vector2Int GetHistoricalCell(int ticksBack)
        {
            if (_history.Count == 0)
            {
                return Cell;
            }

            var index = _history.Count - 1 - ticksBack;
            if (index < 0)
            {
                index = 0;
            }

            return _history[index];
        }

        public EchoRecording SealRecording()
        {
            var sealed_ = _recording;
            _recording = null;
            return sealed_;
        }

        protected override void OnKilled(string cause)
        {
            SetAlpha(0.25f);
            _eventBus?.Publish(new PlayerDiedEvent(cause));
        }
    }
}
