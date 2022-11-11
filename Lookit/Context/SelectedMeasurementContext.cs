﻿using Lookit.Models;
using System;

namespace Lookit.Context
{
    public static class SelectedMeasurementContext
    {
        private static PolygonalMeasurement _selectedMeasurement;

        public static PolygonalMeasurement SelectedMeasurement
        {
            get => _selectedMeasurement;
            set
            {
                _selectedMeasurement = value;
                OnSelectedMeasurementChanged?.Invoke();
            }
        }

        public static event Action OnSelectedMeasurementChanged;
    }
}