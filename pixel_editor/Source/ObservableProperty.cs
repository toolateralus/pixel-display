using System.CodeDom;
using System.ComponentModel;

namespace Pixel_Editor
{
    public class ObservableProperty<T> : INotifyPropertyChanged
    {
        private T m_value;
        public T Value
        {
            get => m_value;
            set
            {
                m_value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
        public ObservableProperty(T value)
        {
            m_value = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
