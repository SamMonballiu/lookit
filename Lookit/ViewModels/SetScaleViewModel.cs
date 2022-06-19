using Lookit.Context;
using Lookit.Logic;
using Lookit.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace Lookit.ViewModels
{
    public class SetScaleViewModel : BaseViewModel
    {
        public double Scale { get; set; }
        public (Point, Point) Points { get; private set; }
        public ICommand ConfirmScale { get; private set; }
        public ICommand SetPoints { get; private set; }
        public event Action OnScaleConfirmed;

        public SetScaleViewModel()
        {
            Title = "Set scale";
            ConfirmScale = new Command<(Point, Point)>(UpdateScale);
            SetPoints = new Command<(Point, Point)>((points) =>
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
