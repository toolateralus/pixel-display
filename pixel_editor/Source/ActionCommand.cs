using System;
using System.Windows.Input;

namespace Pixel_Editor
{
    public class ActionCommand : ICommand
    {
        public Action? _execute;
        public ActionCommand(Action execute)
        {
            _execute = execute;
        }
        public ActionCommand() {}
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute?.Invoke();
        public event EventHandler? CanExecuteChanged;
    }
}
