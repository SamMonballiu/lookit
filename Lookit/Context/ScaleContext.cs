using Lookit.Models;
using MvvmHelpers;
using System;

namespace Lookit.Context
{
    public static class ScaleContext 
    {
        private static Scale _scale = Scale.Default;

        public static Scale Scale
        {
            get => _scale;
            set {
                _scale = value;
                OnScaleChanged?.Invoke(value);
            }
        }

        public static event Action<Scale> OnScaleChanged;
    }
}
