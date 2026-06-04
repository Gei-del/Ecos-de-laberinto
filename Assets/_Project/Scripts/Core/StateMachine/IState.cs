namespace EcosDelLaberinto.Core.StateMachine
{
    /// <summary>
    /// A single state in a finite state machine. Lifecycle: <see cref="Enter"/> once when the
    /// state becomes active, <see cref="Tick"/> every frame while active, <see cref="Exit"/> once
    /// when leaving.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
