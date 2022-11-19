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
        [ObservableProperty] private double _scale;
        [ObservableProperty] private (Point First, Point Second) _points;
        public ICommand ConfirmScale { get; set; }
        public event Action OnScaleConfirmed;

        public SetScaleViewModel()
        {
            _title = "Set scale";
            ConfirmScale = new RelayCommand(UpdateScale);
        }

        public void UpdateScale()
        {
            ScaleContext.Scale = new Scale(Points.First.ToPoint(), Points.Second.ToPoint(), Scale, ScaleUnit.None);
            OnScaleConfirmed?.Invoke();
        }
    }
}
