using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lookit.ViewModels
{
    public partial class SetScaleViewModel : ObservableObject
    {
        public double Scale { get; set; }
        public (Point, Point) Points { get; private set; }
        public ICommand ConfirmScale { get; private set; }
        public ICommand SetPoints { get; private set; }
        public event Action OnScaleConfirmed;
        [ObservableProperty] private string _title;

        public SetScaleViewModel()
        {
            _title = "Set scale";
            ConfirmScale = new RelayCommand<(Point, Point)>(UpdateScale);
            SetPoints = new RelayCommand<(Point, Point)>((points) =>
            {
                Points = points;
            });
        }

        private void UpdateScale((Point First, Point Second) points)
        {
            ScaleContext.Scale = new Scale(points.First.ToPoint(), points.Second.ToPoint(), Scale);
            OnScaleConfirmed?.Invoke();
        }
    }
}
