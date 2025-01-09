using System.Drawing;
using System.Numerics;
using WPF = System.Windows;

namespace Views;

public static class CoordinatesTransform
{
    //public static WPF.Rect OverlayInfoCoordinates(Models.Shapes.Rectangle roiRect)
    //{
    //    WPF.Rect roiRectWPF = ToWPFRectangle(roiRect);

    //    // size for a new overlay rectangle
    //    double width = 70, height = 100;

    //    var overlayTopLeft = roiRectWPF.TopRight - new WPF.Vector(0, height);

    //    return new WPF.Rect(overlayTopLeft, new WPF.Size(width, height));
    //}

    //public static WPF.Rect ToWPFRectangle(Models.Shapes.Rectangle roiRect) => roiRect switch
    //{
    //    {
    //        P1: { X: float x1, Y: float y1 },
    //        P2: { X: float x2, Y: float y2 }
    //    } => new(
    //            new WPF.Point((double) x1, (double) y1),
    //            new WPF.Point((double) x2, (double) y2)
    //        ) // Тут я просто повірю що прямокутник будується, розраховуючи, де верхня ліва точка
    //};

    public static PointF WPF_ToGL(WPF.Point wpfPoint, WPF.Size viewport)
    {
        wpfPoint.Y = viewport.Height - wpfPoint.Y; // invert Y

        PointF asFloat = new((float) wpfPoint.X, (float) wpfPoint.Y);

        PointF newPoint = new(asFloat.ToVector2() / new Vector2((float) viewport.Width, (float) viewport.Height) * 2 - Vector2.One);

        return newPoint;
    }
}