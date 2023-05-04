using System;
using System.Windows.Input;

namespace Pixel_Editor
{
    public class MethodCommand : ICommand
    {
        private readonly Action? _execute;

        public MethodCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute?.Invoke();

        public event EventHandler? CanExecuteChanged;
    }
}
