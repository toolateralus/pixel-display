using PixelLang;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Pixel_Editor
{
    public class EditorViewModel : INotifyPropertyChanged
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

        public MethodCommand? SendCommand { get; }
        public ObservableCollection<string> Messages { get;} = new();
        public ICollectionView MessagesView { get; }

        public EditorViewModel(Window window)
        {
            commandLine = "";
            SendCommand = new MethodCommand(OnCommandSent);
            MessagesView = CollectionViewSource.GetDefaultView(Messages);
            Messages.CollectionChanged += OnMessagesChanged;
        }

        public string? LastMessage => Messages.LastOrDefault();

        void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LastMessage));
        }

        public async void OnCommandSent()
        {
            // the max amt of lines that a script can consist of
            int cap = 5;
            var lines = CommandLine.Split('\n');
            int lineCt = lines.Length;

            if (lineCt < cap)
                cap = lineCt;

            for (int i = 0; i < cap; ++i)
            {

                string line = lines[i];

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line != "")
                    Interpreter.TryCallLine(line);
            }
            CommandLine = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
