using Pixel.Types;
using PixelLang;
using PixelLang.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        int commandHistoryIndex = 0;
        List<string> commandHistory = new();
        public ObservableCollection<string> Messages { get; } = new();
        public ObservableCollection<string> DebugMessages { get; } = new();
        public ObservableProperty<string> CommandLine { get; } = new("");
        static Action<string>? SendMessageAction;
        static Action<string>? SendDebugAction;
        static Action? ClearDebugAction;
        static Action? ClearAllAction;
        public ActionCommand? SendCommand { get; } 
        public ActionCommand? ContinueCommand { get; }
        public ActionCommand? DebugCommand { get; }
        public ActionCommand? NextCommand { get; }
        public ActionCommand? PreviousHistoryCommand { get; }
        public ActionCommand? NextHistoryCommand { get; }
        public ConsoleControl()
        {
            InitializeComponent();
            commandHistory.Add("");
            DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SendCommand = new ActionCommand(OnSend);
            ContinueCommand = new ActionCommand(OnContinue);
            DebugCommand = new ActionCommand(OnDebug);
            NextCommand = new ActionCommand(OnNext);
            PreviousHistoryCommand = new ActionCommand(OnPreviousHistory);
            NextHistoryCommand = new ActionCommand(OnNextHistory);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction += AddMessage;
            ClearAllAction += ClearMessages;
            SendDebugAction += SendDebug;
            ClearDebugAction += ClearDebug;
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction -= AddMessage;
            ClearAllAction -= ClearMessages;
            SendDebugAction -= SendDebug;
            ClearDebugAction -= ClearDebug;
        }
        private void OnNextHistory(object? sender)
        {
            commandHistoryIndex = (commandHistoryIndex + commandHistory.Count + 1) % commandHistory.Count;
            CommandLine.Value = commandHistory[commandHistoryIndex];
        }
        private void OnPreviousHistory(object? sender)
        {
            commandHistoryIndex = (commandHistoryIndex + commandHistory.Count - 1) % commandHistory.Count;
            CommandLine.Value = commandHistory[commandHistoryIndex];
        }
        private void OnNext(object? sender) => InterpreterOutput.Continue?.Invoke();
        private void OnDebug(object? sender)
        {
            InterpreterOutput.IsDebugMode = !InterpreterOutput.IsDebugMode;
            AddMessage($"DEBUG : {InterpreterOutput.IsDebugMode}");
        }
        private void OnContinue(object? sender)
        {
            InterpreterOutput.IsDebugMode = false;
           InterpreterOutput.Continue?.Invoke();
        }
        private void OnSend(object? sender)
        {
            string cmd = CommandLine.Value;
            CommandLine.Value = "";
            if (string.IsNullOrEmpty(cmd))
                return;
            if (!commandHistory.Contains(cmd))
                commandHistory.Add(cmd);
            commandHistoryIndex = 0;

            Dispatcher.InvokeAsync(async () => await InputProcessor.TryCallLine(cmd));
        }
        private void ClearDebug() => Dispatcher.Invoke(() => DebugMessages.Clear());
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
        internal void ClearMessages() => Dispatcher.Invoke(() => Messages.Clear());
        void SendDebug(string message) => Dispatcher.Invoke(() => DebugMessages.Add(message));
        public static void SendMessage(string message) => Editor.Current.Dispatcher.Invoke(() => SendMessageAction?.Invoke(message));
        public static void ClearAll() => Editor.Current.Dispatcher.Invoke(() => ClearAllAction?.Invoke());
        internal static void ShowMetrics(string[] strings)
        {
            ClearDebugAction?.Invoke();
            foreach(string str in strings)
                SendDebugAction?.Invoke(str);
        }
    }
}
