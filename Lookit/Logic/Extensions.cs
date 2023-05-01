using System;
using System.Windows;
using System.Windows.Controls;

namespace Lookit.Logic
{
    internal static class Extensions
    {
        public static (bool Left, bool Right, bool Top, bool Bottom) IsCloseToEdges(this Point pos, ScrollViewer sv, double tolerance)
        {
            return (
                sv.ContentHorizontalOffset > 0 && Math.Abs(sv.ContentHorizontalOffset - pos.X) < tolerance,
                sv.ContentHorizontalOffset + sv.ViewportWidth - pos.X < tolerance,
                sv.ContentVerticalOffset > 0 && Math.Abs(sv.ContentVerticalOffset - pos.Y) < tolerance,
                Math.Abs(sv.ContentVerticalOffset + sv.ViewportHeight - pos.Y) < tolerance
                );
        }
    }
}
