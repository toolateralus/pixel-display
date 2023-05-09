using Pixel.Types;
using PixelLang;
using PixelLang.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        public static void SendMessage(string message) => SendMessageAction?.Invoke(message);
        public static void ClearAll() => ClearAllAction?.Invoke();
        public ConsoleControl()
        {
            InitializeComponent();
            commandHistory.Add("");
            DataContext = this;
            Loaded += OnLoaded;
            Unloaded += OnUnLoaded;
            SendCommand = new ActionCommand(OnSendCommand);
            ContinueCommand = new ActionCommand(OnContinueCommand);
            DebugCommand = new ActionCommand(OnDebugCommand);
            NextCommand = new ActionCommand(OnNextCommand);
            PreviousHistoryCommand = new ActionCommand(PreviousHistory);
            NextHistoryCommand = new ActionCommand(NextHistory);
        }

        private void NextHistory()
        {
            commandHistoryIndex = (commandHistoryIndex + commandHistory.Count + 1) % commandHistory.Count;
            CommandLine.Value = commandHistory[commandHistoryIndex];
        }

        private void PreviousHistory()
        {
            commandHistoryIndex = (commandHistoryIndex + commandHistory.Count - 1) % commandHistory.Count;
            CommandLine.Value = commandHistory[commandHistoryIndex];
        }

        private void OnNextCommand() => InterpreterOutput.Continue?.Invoke();

        private void OnDebugCommand()
        {
            InterpreterOutput.IsDebugMode = !InterpreterOutput.IsDebugMode;
            AddMessage($"DEBUG : {InterpreterOutput.IsDebugMode}");
        }

        private void OnContinueCommand()
        {
            InterpreterOutput.IsDebugMode = false;
            InterpreterOutput.Continue?.Invoke();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction += AddMessage;
            ClearAllAction += ClearMessages;
            SendDebugAction += SendDebug;
            ClearDebugAction += ClearDebug;
        }

        private void ClearDebug() => DebugMessages.Clear();

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction -= AddMessage;
            ClearAllAction -= ClearMessages;
            SendDebugAction -= SendDebug;
            ClearDebugAction -= ClearDebug;
        }

        private void OnSendCommand()
        {
            string cmd = CommandLine.Value;
            CommandLine.Value = "";
            if (string.IsNullOrEmpty(cmd))
                return;
            if (!commandHistory.Contains(cmd))
                commandHistory.Add(cmd);
            commandHistoryIndex = 0;
            InputProcessor.TryCallLine(cmd);
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
        void SendDebug(string message) => DebugMessages.Add(message);
        internal static void ShowMetrics(string[] strings)
        {
            ClearDebugAction?.Invoke();
            foreach(string str in strings)
                SendDebugAction?.Invoke(str);
        }
    }
}
