using Lookit.Context;
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

        public SetScaleView(Point first, Point second)
        {
            InitializeComponent();
            DataContext = new SetScaleViewModel();
            (DataContext as SetScaleViewModel).Points = (first, second);
            (DataContext as SetScaleViewModel).OnScaleConfirmed += (Scale scale) =>
            {
                OnConfirm?.Invoke(scale);
                Close();
            };
        }
    }
}
