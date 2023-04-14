using Lookit.Models;
using System.Windows;
using System.Windows.Controls;

namespace Lookit.UserControls
{
    /// <summary>
    /// Interaction logic for ScalePicker.xaml
    /// </summary>
    public partial class ScalePicker : UserControl
    {
        public double ScaleDistance
        {
            get => (double)GetValue(ScaleDistanceProperty);
            set => SetValue(ScaleDistanceProperty, value);
        }

        public ScaleUnit ScaleUnit
        {
            get { return (ScaleUnit)GetValue(ScaleUnitProperty); }
            set { SetValue(ScaleUnitProperty, value); }
        }


        public static readonly DependencyProperty ScaleUnitProperty =
            DependencyProperty.Register(nameof(ScaleUnit), typeof(ScaleUnit), typeof(ScalePicker), new PropertyMetadata(ScaleUnit.None));

        public static readonly DependencyProperty ScaleDistanceProperty =
            DependencyProperty.Register(nameof(ScaleDistance), typeof(double), typeof(ScalePicker), new PropertyMetadata(double.MinValue));

        public ScalePicker()
        {
            InitializeComponent();
        }

        private void txtScale_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Dispatcher.BeginInvoke(() => { txtScale.SelectAll(); });
        }
    }
}
