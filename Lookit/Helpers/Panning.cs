using System;
using System.Windows;
using System.Windows.Controls;

namespace Lookit.Helpers
{
    internal sealed class Panning
    {
        private readonly Velocity _velocity = new() { Horizontal = 0, Vertical = 0 };

        public static void UpdateManual(ScrollViewer scrollViewer, double horizontalOffset, double verticalOffset)
        {
            if (horizontalOffset is 0 && verticalOffset is 0)
            {
                return;
            }

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + horizontalOffset);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + verticalOffset);
        }

        public void Update(double zoomLevel, Point previousMousePosition, Point currentMousePosition, ScrollViewer scrollViewer, double modifier = 0.3)
        {
            _velocity.Vertical = Math.Abs(currentMousePosition.Y - previousMousePosition.Y);
            _velocity.Horizontal = Math.Abs(currentMousePosition.X - previousMousePosition.X);

            var verticalIncrement = _velocity.Vertical * zoomLevel * modifier;
            var horizontalIncrement = _velocity.Horizontal * zoomLevel * modifier;

            if (currentMousePosition.Y < previousMousePosition.Y)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - verticalIncrement);
            if (currentMousePosition.Y > previousMousePosition.Y)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + verticalIncrement);

            if (currentMousePosition.X < previousMousePosition.X)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - horizontalIncrement);
            if (currentMousePosition.X > previousMousePosition.X)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + horizontalIncrement);
        }
    }

    public class Velocity
    {
        public double Horizontal { get; set; } = 0;
        public double Vertical { get; set; } = 0;
    }
}
