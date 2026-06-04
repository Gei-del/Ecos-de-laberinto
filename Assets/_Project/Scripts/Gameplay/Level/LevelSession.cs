using System.Collections.Generic;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Core.Utils;
using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Economy;
using EcosDelLaberinto.Gameplay.Echoes;
using EcosDelLaberinto.Gameplay.Player;
using EcosDelLaberinto.Gameplay.Time;
using EcosDelLaberinto.Input;
using EcosDelLaberinto.Persistence;
using EcosDelLaberinto.Scoring;
using UnityEngine;

namespace EcosDelLaberinto.Gameplay.Level
{
    /// <summary>
    /// Orchestrates a single level play: builds the world, drives the deterministic tick loop,
    /// records the live player, spawns and replays echoes each loop, evaluates every mechanism in
    /// a fixed order, and resolves win/lose. This is the heart of the "echoes" gameplay.
    ///
    /// Tick order (deterministic):
    ///   1. echoes replay  2. live player steps  3. buttons/switches  4. doors
    ///   5. lasers (may kill)  6. crystals  7. exit check  8. timer broadcast
    /// </summary>
    public sealed class LevelSession
    {
        private readonly IInputService _input;
        private readonly ITickService _tick;
        private readonly IEventBus _events;
        private readonly IScoreService _score;
        private readonly ISaveService _save;
        private readonly IEconomyService _economy;
        private readonly GameConfig _config;
        private readonly AbilityFactory _abilityFactory;
        private readonly Transform _root;

        private LevelData _level;
        private BuiltLevel _built;
        private MovementResolver _resolver;
        private PlayerController _player;
        private EchoFactory _echoFactory;
        private Transform _echoContainer;

        private readonly List<EchoRecording> _recordings = new();
        private readonly List<EchoActor> _echoes = new();
        private readonly List<IGridActor> _actors = new();

        private ICharacterAbility _ability;
        private int _echoLifetimeTicks;
        private int _abilityCooldownTicks;

        private int _deaths;
        private int _loopIndex;
        private float _elapsed;
        private int _crystalsCollected;
        private bool _needsRestart;
        private bool _completed;
        private bool _active;

        public bool IsActive => _active;
        public bool LevelCompleted => _completed;
        public LevelData CurrentLevel => _level;

        public LevelSession(IInputService input, ITickService tick, IEventBus events,
            IScoreService score, ISaveService save, IEconomyService economy, GameConfig config,
            AbilityFactory abilityFactory, Transform root)
        {
            _input = input;
            _tick = tick;
            _events = events;
            _score = score;
            _save = save;
            _economy = economy;
            _config = config;
            _abilityFactory = abilityFactory;
            _root = root;
        }

        public void StartLevel(LevelData level, CharacterData character)
        {
            Dispose();

            _level = level;
            _deaths = 0;
            _loopIndex = 0;
            _elapsed = 0f;
            _crystalsCollected = 0;
            _completed = false;
            _recordings.Clear();

            var builder = new LevelBuilder(_config, _events);
            _built = builder.Build(level, _root);
            _resolver = new MovementResolver(_built.Grid);

            _ability = _abilityFactory.Create(character);
            _echoLifetimeTicks = _config.EchoLifetimeTicks(_ability.EchoLifetimeMultiplier);
            _abilityCooldownTicks = character != null
                ? Mathf.RoundToInt(character.AbilityCooldown * _config.TicksPerSecond)
                : 0;

            _echoContainer = new GameObject("Echoes").transform;
            _echoContainer.SetParent(_root, false);
            _echoFactory = new EchoFactory(_echoContainer, _built.Grid);

            _player = CreatePlayer(character);
            _player.Configure(_built.Grid, _resolver, _ability, _events,
                character != null ? character.Id : "nova", _abilityCooldownTicks);

            FrameCamera();

            _events.Publish(new LevelLoadedEvent(level.Id, _built.CrystalsTotal));
            BeginLoop();
            _active = true;
        }

        /// <summary>Called by the bootstrap once per fixed tick.</summary>
        public void OnTick(int tick)
        {
            if (!_active || _completed || _needsRestart)
            {
                return;
            }

            // 1. Echoes replay.
            foreach (var echo in _echoes)
            {
                echo.Step(tick);
            }

            // 2. Live player.
            _player.Step(tick, _input);

            // Refresh the actor list view for mechanisms.
            RebuildActors();

            // 3. Activators.
            foreach (var b in _built.Buttons) b.Evaluate(_actors);
            foreach (var s in _built.Switches) s.Evaluate(_actors);

            // 4. Doors.
            foreach (var d in _built.Doors) d.Evaluate();

            // 5. Lasers (may kill the live player).
            foreach (var laser in _built.Lasers) laser.Evaluate(_actors);

            // 6. Crystals.
            foreach (var crystal in _built.Crystals)
            {
                if (crystal.TryCollect(_player))
                {
                    _crystalsCollected++;
                    _events.Publish(new CrystalCollectedEvent(_crystalsCollected, _built.CrystalsTotal));
                }
            }

            // 7. Exit / win.
            var remaining = _built.CrystalsTotal - _crystalsCollected;
            if (_built.Exit != null && _built.Exit.IsReached(_player, remaining))
            {
                CompleteLevel();
                _input.ConsumeOneShots();
                return;
            }

            // 8. Timer + bookkeeping.
            _elapsed += _tick.TickInterval;
            _events.Publish(new TimerTickedEvent(_elapsed));

            // Death or manual restart spawns a new echo.
            if (!_player.IsAlive)
            {
                _deaths++;
                _needsRestart = true;
            }
            else if (_input.RestartPressed)
            {
                _needsRestart = true;
            }

            _input.ConsumeOneShots();
        }

        /// <summary>Called by the bootstrap after a tick batch if a new loop is required.</summary>
        public void ApplyPendingRestart()
        {
            if (!_needsRestart || _completed)
            {
                return;
            }

            _needsRestart = false;
            SealCurrentRecording();
            BeginLoop();
        }

        public bool NeedsRestart => _needsRestart;

        public void Dispose()
        {
            _active = false;
            if (_built?.Root != null)
            {
                Object.Destroy(_built.Root.gameObject);
            }

            _echoes.Clear();
            _actors.Clear();
        }

        // -----------------------------------------------------------------------------------------

        private void BeginLoop()
        {
            // Reset world state for the new loop.
            foreach (var b in _built.Buttons) b.ResetState();
            foreach (var s in _built.Switches) s.ResetState();
            foreach (var d in _built.Doors) d.ResetState();
            foreach (var l in _built.Lasers) l.ResetState();
            foreach (var block in _built.Blocks) block.ResetToSpawn();
            _resolver.RebuildBlockIndex(_built.Blocks);

            // Rebuild echoes from recordings (respecting the simultaneous cap).
            RebuildEchoes();

            RebuildActors();
            _player.BeginLoop(_built.PlayerSpawn, GetEchoActors());

            _tick.ResetTicks();
            _loopIndex++;
            _events.Publish(new LoopStartedEvent(_loopIndex, _echoes.Count));
        }

        private void RebuildEchoes()
        {
            foreach (var echo in _echoes)
            {
                if (echo != null)
                {
                    Object.Destroy(echo.gameObject);
                }
            }

            _echoes.Clear();

            var start = Mathf.Max(0, _recordings.Count - _config.MaxEchoes);
            for (var i = start; i < _recordings.Count; i++)
            {
                var echo = _echoFactory.Create(_recordings[i], _echoLifetimeTicks, i);
                echo.BeginLoop();
                _echoes.Add(echo);
            }
        }

        private void SealCurrentRecording()
        {
            var recording = _player.SealRecording();
            if (recording != null && recording.Length > 0)
            {
                _recordings.Add(recording);
            }
        }

        private void RebuildActors()
        {
            _actors.Clear();
            _actors.Add(_player);
            foreach (var echo in _echoes)
            {
                _actors.Add(echo);
            }
        }

        private IReadOnlyList<IGridActor> GetEchoActors()
        {
            var list = new List<IGridActor>(_echoes.Count);
            foreach (var echo in _echoes)
            {
                list.Add(echo);
            }

            return list;
        }

        private PlayerController CreatePlayer(CharacterData character)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(_root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrimitiveSprites.Circle();
            sr.color = character != null ? character.TintColor : Color.white;
            sr.sortingOrder = 10;

            var player = go.AddComponent<PlayerController>();
            player.AssignRenderer(sr);
            return player;
        }

        private void FrameCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            var size = Mathf.Max(_built.Grid.Width, _built.Grid.Height) * 0.5f * _config.CellSize + 1.5f;
            cam.orthographicSize = size;
            var center = _built.Grid.Center;
            cam.transform.position = new Vector3(center.x, center.y, -10f);
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        }

        private void CompleteLevel()
        {
            _completed = true;
            _active = false;

            var result = _score.Evaluate(_level, _elapsed, _deaths, _recordings.Count,
                _crystalsCollected, _built.CrystalsTotal);

            PersistProgress(result);
            _economy.Add(result.FragmentsEarned);
            _save.Save();

            GameLogger.Info($"Level {_level.Id} complete: {result.Stars}* in {result.TimeSeconds:0.0}s, " +
                            $"{result.CrystalsCollected}/{result.CrystalsTotal} crystals, " +
                            $"{result.EchoesUsed} echoes, {result.Deaths} deaths.");

            _events.Publish(new LevelCompletedEvent(result));
        }

        private void PersistProgress(LevelResult result)
        {
            var progress = _save.Data.GetOrCreateLevel(_level.Id);
            progress.Completed = true;
            progress.BestStars = Mathf.Max(progress.BestStars, result.Stars);
            progress.CrystalsCollected = Mathf.Max(progress.CrystalsCollected, result.CrystalsCollected);
            if (progress.BestTimeSeconds < 0f || result.TimeSeconds < progress.BestTimeSeconds)
            {
                progress.BestTimeSeconds = result.TimeSeconds;
            }
        }
    }
}
