using System.Collections.Generic;

namespace EcosDelLaberinto.Core.Commands
{
    /// <summary>
    /// Executes <see cref="ICommand"/>s and keeps a history so they can be undone (used by the
    /// level editor tooling and Chrona's "rewind" ability prototype).
    /// </summary>
    public sealed class CommandInvoker
    {
        private readonly Stack<ICommand> _history = new();

        public int HistoryCount => _history.Count;

        public void Execute(ICommand command)
        {
            if (command == null)
            {
                return;
            }

            command.Execute();
            _history.Push(command);
        }

        public bool UndoLast()
        {
            if (_history.Count == 0)
            {
                return false;
            }

            _history.Pop().Undo();
            return true;
        }

        public void Clear()
        {
            _history.Clear();
        }
    }
}
