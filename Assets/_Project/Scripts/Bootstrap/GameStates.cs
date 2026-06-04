using EcosDelLaberinto.Core.StateMachine;

namespace EcosDelLaberinto.Bootstrap
{
    /// <summary>
    /// Game-flow states (State Machine pattern). Each one only knows about the <see cref="GameApp"/>
    /// facade, never about other states, so transitions stay centralised and testable.
    /// </summary>
    public sealed class MenuState : IState
    {
        private readonly GameApp _app;
        public MenuState(GameApp app) => _app = app;

        public void Enter()
        {
            _app.Tick.IsPaused = true;
            _app.UI.ShowMenu();
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }

    /// <summary>Active simulation: advances the deterministic tick clock and watches for
    /// pause/restart/win transitions.</summary>
    public sealed class PlayingState : IState
    {
        private readonly GameApp _app;
        public PlayingState(GameApp app) => _app = app;

        public void Enter()
        {
            _app.UI.ShowHud();
            _app.UI.HidePause();
            _app.Tick.IsPaused = false;
        }

        public void Tick(float deltaTime)
        {
            // Pause is handled before advancing so a tick never consumes the latch first.
            if (_app.Input.PausePressed)
            {
                _app.Input.ConsumeOneShots();
                _app.Flow.ChangeState(_app.Paused);
                return;
            }

            _app.Tick.Advance(deltaTime);

            if (_app.Session.LevelCompleted)
            {
                _app.UI.ShowResults(_app.LastResult);
                _app.Flow.ChangeState(_app.Results);
                return;
            }

            if (_app.Session.NeedsRestart)
            {
                _app.Session.ApplyPendingRestart();
            }
        }

        public void Exit()
        {
        }
    }

    public sealed class PausedState : IState
    {
        private readonly GameApp _app;
        public PausedState(GameApp app) => _app = app;

        public void Enter()
        {
            _app.Tick.IsPaused = true;
            _app.UI.ShowPause();
        }

        public void Tick(float deltaTime)
        {
            if (_app.Input.PausePressed)
            {
                _app.Input.ConsumeOneShots();
                _app.Flow.ChangeState(_app.Playing);
            }
        }

        public void Exit()
        {
            _app.UI.HidePause();
        }
    }

    public sealed class ResultsState : IState
    {
        private readonly GameApp _app;
        public ResultsState(GameApp app) => _app = app;

        public void Enter()
        {
            _app.Tick.IsPaused = true;
        }

        public void Tick(float deltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
