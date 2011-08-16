using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace B1.Windows.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ThoughtBubble : UserControl
    {
        /// <summary>
        /// Gets or sets the background color of the thought bubble.
        /// </summary>
        public SolidColorBrush BubbleBackground
        {
            get { return (SolidColorBrush)GetValue(BubbleBackgroundProperty); }
            set { SetValue(BubbleBackgroundProperty, value); }
        }

        public static readonly DependencyProperty BubbleBackgroundProperty =
            DependencyProperty.Register("BubbleBackground",
            typeof(SolidColorBrush),
            typeof(ThoughtBubble),
            new UIPropertyMetadata(Brushes.White));
    }
}
