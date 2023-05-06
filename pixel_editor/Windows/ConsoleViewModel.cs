using PixelLang;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Pixel_Editor
{
    internal class ConsoleViewModel : INotifyPropertyChanged
    {
        string commandLine;
        public string CommandLine
        {
            get => commandLine;
            set
            {
                commandLine = value;
                OnPropertyChanged(nameof(CommandLine));
            }
        }
        public ActionCommand? SendCommand { get; }
        public Action<string>? CommandSent;
        public ObservableCollection<string> Messages { get; } = new();
        public ConsoleViewModel()
        {
            commandLine = "";
            SendCommand = new ActionCommand(OnSendCommand);
        }
        private void OnSendCommand()
        {
            CommandSent?.Invoke(CommandLine);
            CommandLine = "";
        }
        public void AddMessage(string message)
        {
            Messages.Add(message);
            if (Messages.Count >= Editor.Current.settings.ConsoleMaxLines)
                Messages.RemoveAt(0);
        }
        internal void ClearMessages() => Messages.Clear();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
