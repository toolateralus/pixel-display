using System;
using System.Windows.Input;

namespace Pixel_Editor
{
    public class ActionCommand : ICommand
    {
        private Action<object?>? action;

        public Action<object?>? Action
        {
            get => action;
            set
            {
                action = value;
                CanExecuteChanged?.Invoke(this, new());
            }
        }
        public ActionCommand(Action<object?>? execute) => Action = execute;
        public ActionCommand() { }
        public bool CanExecute(object? parameter) => Action != null;
        public void Execute(object? parameter) => Action?.Invoke(parameter);
        public event EventHandler? CanExecuteChanged;
    }
}
