using System.Windows.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Pixel_Editor.Behaviors
{
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty ScrollIntoViewTriggerProperty =
            DependencyProperty.Register(nameof(ScrollIntoViewTrigger), typeof(object), typeof(ScrollIntoViewBehavior),
                new FrameworkPropertyMetadata(null, OnScrollIntoViewTriggerChanged));

        public object ScrollIntoViewTrigger
        {
            get { return GetValue(ScrollIntoViewTriggerProperty); }
            set { SetValue(ScrollIntoViewTriggerProperty, value); }
        }

        private static void OnScrollIntoViewTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollIntoViewBehavior behavior && behavior.AssociatedObject is ListBox listBox)
            {
                listBox.ScrollIntoView(e.NewValue);
            }
        }
    }
}
