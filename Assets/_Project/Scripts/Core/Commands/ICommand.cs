namespace EcosDelLaberinto.Core.Commands
{
    /// <summary>
    /// Command pattern abstraction. A command encapsulates a reversible action so the gameplay
    /// can support undo, deterministic replay and (in the future) networked rollback.
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
