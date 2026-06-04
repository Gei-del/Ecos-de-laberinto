namespace EcosDelLaberinto.Core.StateMachine
{
    /// <summary>
    /// Lightweight finite state machine. Used for the game flow (Menu, Loading, Playing,
    /// Paused, Win, GameOver) and reusable for AI / boss phases.
    /// </summary>
    public sealed class StateMachine
    {
        public IState Current { get; private set; }

        public void ChangeState(IState next)
        {
            if (ReferenceEquals(next, Current))
            {
                return;
            }

            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick(float deltaTime)
        {
            Current?.Tick(deltaTime);
        }
    }
}
