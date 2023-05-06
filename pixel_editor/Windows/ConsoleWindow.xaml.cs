using PixelLang;
using PixelLang.Tools;
using System;
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
        ConsoleViewModel viewModel = new();
        static Action<string>? SendMessageAction;
        static Action? ClearAllAction;
        public static void SendMessage(string message) => SendMessageAction?.Invoke(message);
        public static void ClearAll() => ClearAllAction?.Invoke();
        public ConsoleWindow()
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += OnLoaded;
            Unloaded += OnUnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction += AddMessage;
            ClearAllAction += viewModel.ClearMessages;
            viewModel.CommandSent += OnCommandSent;
        }

        private void AddMessage(string message)
        {
            viewModel.AddMessage(message);
            if (VisualTreeHelper.GetChildrenCount(messagesBox) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(messagesBox, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            SendMessageAction -= AddMessage;
            ClearAllAction -= viewModel.ClearMessages;
            viewModel.CommandSent -= OnCommandSent;
        }
        public void OnCommandSent(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;
            InputProcessor.TryCallLine(command);
        }
    }
}
