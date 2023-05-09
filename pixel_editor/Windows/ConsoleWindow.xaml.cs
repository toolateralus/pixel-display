using PixelLang;
using PixelLang.Tools;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : UserControl
    {
        public ObservableCollection<string> Messages { get; } = new();
        public ObservableProperty<string> CommandLine { get; } = new("");
        static Action<string>? SendMessageAction;
        public Action<string>? CommandSent;
        static Action? ClearAllAction;
        public ActionCommand? SendCommand { get; }
        public static void SendMessage(string message) => SendMessageAction?.Invoke(message);
        public static void ClearAll() => ClearAllAction?.Invoke();
        public ConsoleWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnLoaded;
            SendCommand = new ActionCommand(OnSendCommand);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction += AddMessage;
            ClearAllAction += ClearMessages;
            CommandSent += OnCommandSent;
        }
        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction -= AddMessage;
            ClearAllAction -= ClearMessages;
            CommandSent -= OnCommandSent;
        }

        public static void OnCommandSent(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;
            InputProcessor.TryCallLine(command);
        }
        private void OnSendCommand()
        {
            CommandSent?.Invoke(CommandLine.Value);
            CommandLine.Value = "";
        }
        public void AddMessage(string message)
        {
            Messages.Add(message);
            if (Messages.Count >= Editor.Current.settings.ConsoleMaxLines)
                Messages.RemoveAt(0);
            if (VisualTreeHelper.GetChildrenCount(messagesBox) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(messagesBox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
        internal void ClearMessages() => Messages.Clear();
    }
}
