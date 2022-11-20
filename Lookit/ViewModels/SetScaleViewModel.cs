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
        [ObservableProperty] private string _title;
        [ObservableProperty] private double _scaleDistance;
        [ObservableProperty] private (Point First, Point Second) _points;
        [ObservableProperty] private ScaleUnit _scaleUnit = ScaleContext.Scale.Equals(Scale.Default) 
            ? ScaleUnit.Meters 
            : ScaleContext.Scale.Unit;

        public ICommand ConfirmScale { get; set; }
        public event Action OnScaleConfirmed;

        public SetScaleViewModel()
        {
            _title = "Set scale";
            ConfirmScale = new RelayCommand(UpdateScale);
        }

        public void UpdateScale()
        {
            ScaleContext.Scale = new Scale(Points.First.ToPoint(), Points.Second.ToPoint(), _scaleDistance, _scaleUnit);
            OnScaleConfirmed?.Invoke();
        }
    }
}
