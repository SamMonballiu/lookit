using Lookit.Context;
using Lookit.ViewModels;
using System.Windows;

namespace Lookit.Views
{
    /// <summary>
    /// Interaction logic for SetScaleView.xaml
    /// </summary>
    public partial class SetScaleView : Window
    {


        public SetScaleView(Point first, Point second)
        {
            InitializeComponent();
            txtScale.Focus();
            DataContext = new SetScaleViewModel();
            (DataContext as SetScaleViewModel).SetPoints?.Execute((first, second));
            (DataContext as SetScaleViewModel).OnScaleConfirmed += () => Close();
        }
    }
}
