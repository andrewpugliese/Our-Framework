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

namespace B1.Utility.DatabaseSetupWpf
{
    /// <summary>
    /// Interaction logic for TaskControl.xaml
    /// </summary>
    public partial class TaskControl : UserControl
    {
        public TaskControl()
        {
            InitializeComponent();
        }

        public void SelectButton()
        {
            // Return the offset vector for the TextBlock object.
            Vector vector = VisualTreeHelper.GetOffset(radioButton1);

            // Convert the vector to a point value.
            Point currentPoint = new Point(vector.X, vector.Y);

            //?? Canvas.SetLeft(radioButton2, currentPoint.X);

            var tran = new TranslateTransform(0, 0);
            selectRectangle.RenderTransform = tran;
            //?? selectRectangle.
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            SelectButton();
        }
    }
}
