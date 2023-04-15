using Lookit.Models;
using Lookit.ViewModels;
using System;
using System.Windows;

namespace Lookit.Views
{
    /// <summary>
    /// Interaction logic for SetScaleView.xaml
    /// </summary>
    public partial class SetScaleView : Window
    {
        public event Action<Scale> OnConfirm;

        public SetScaleViewModel Viewmodel => DataContext as SetScaleViewModel;

        public SetScaleView()
        {
            InitializeComponent();
            DataContext = new SetScaleViewModel();
            Viewmodel.OnScaleConfirmed += (Scale scale) =>
            {
                OnConfirm?.Invoke(scale);
                Close();
            };
        }

        public SetScaleView(System.Drawing.Point first, System.Drawing.Point second) : this()
        {
            Viewmodel.Points = (first, second);
        }

        public SetScaleView(Scale scale) : this()
        {
            Viewmodel.Points = (scale.First, scale.Second);
            Viewmodel.ScaleUnit = scale.Unit;
            Viewmodel.ScaleDistance = scale.EnteredDistance;
        }
    }
}
