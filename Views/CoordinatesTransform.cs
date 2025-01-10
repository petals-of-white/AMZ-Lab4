using System.Drawing;
using System.Numerics;
using WPF = System.Windows;

namespace Views;

public static class CoordinatesTransform
{
    public static PointF WPF_ToGL(WPF.Point wpfPoint, WPF.Size viewport)
    {
        wpfPoint.Y = viewport.Height - wpfPoint.Y; // invert Y

        PointF asFloat = new((float) wpfPoint.X, (float) wpfPoint.Y);

        PointF newPoint = new(asFloat.ToVector2() / new Vector2((float) viewport.Width, (float) viewport.Height) * 2 - Vector2.One);

        return newPoint;
    }
}