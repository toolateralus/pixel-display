using PixelLang;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Pixel_Editor
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        TextBox editorMessages = new()
        {
            FontSize = 3.95,
            FontFamily = new FontFamily("MS Gothic"),
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true
        };
        public TextBox EditorMessages
        {
            get => editorMessages;
            set
            {
                editorMessages = value;
                OnPropertyChanged(nameof(EditorMessages));
            }
        }
        ListBox consoleOutput = new()
        {
            FontSize = 3.95,
            FontFamily = new FontFamily("MS Gothic"),
        };
        public ListBox ConsoleOutput
        {
            get => consoleOutput;
            set
            {
                consoleOutput = value;
                OnPropertyChanged(nameof(ConsoleOutput));
            }
        }
        private bool consoleOpen = true;
        public ICommand? SendCommandKeybindCommand { get; }
        public ICommand? ToggleConsoleCommand { get; }

        public EditorViewModel(Window window)
        {
            if (window.FindResource("editorMessages") is TextBox textBox)
                editorMessages = textBox;
            if (window.FindResource("consoleOutput") is ListBox listBox)
                consoleOutput = listBox;
        }

        public void SendCommandKeybind()
        {
            if (!editorMessages.IsKeyboardFocusWithin)
                return;

            OnCommandSent();
            editorMessages.Clear();
        }

        internal Action<object?> RedText(object? o = null)
        {
            return (o) =>
            {
                consoleOutput.Foreground = Brushes.Red;
            };
        }
        /// <summary>
        /// if the color's brightness is under 610 (a+r+g+b) this continually gets a new color every ms for 1000 ms or until it's bright enough then returns it
        /// </summary>
        /// <param name="c"></param>
        /// <returns>A color with a brightness value greater than 610 </returns>
        internal Action<object?> BlackText(object? o = null)
        {
            return (o) =>
            {
                consoleOutput.Foreground = Brushes.White;
                consoleOutput.Background = Brushes.Black;
            };
        }

        public async void OnCommandSent()
        {
            // the max amt of lines that a script can consist of
            int cap = 5;

            int lineCt = editorMessages.LineCount - 1;

            if (lineCt < cap)
                cap = lineCt;

            for (int i = 0; i < cap; ++i)
            {

                string line = editorMessages.GetLineText(i);

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line != "")
                    Interpreter.TryCallLine(line);
            }
        }
        public void OnToggleConsole()
        {
            if (!consoleOpen)
            {
                consoleOpen = true;

                editorMessages.Visibility = Visibility.Collapsed;
                consoleOutput.Visibility = Visibility.Collapsed;

                return;
            }

            editorMessages.Visibility = Visibility.Visible;
            consoleOutput.Visibility = Visibility.Visible;

            consoleOpen = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
