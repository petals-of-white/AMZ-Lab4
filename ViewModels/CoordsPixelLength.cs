using Models;

namespace ViewModels;

public class CoordsPixelLength
{
    public CoordsPixelLength(DicomSeries sourceSeries, AnatomicPlane targetPlane)
    {
        switch (targetPlane)
        {
            case AnatomicPlane.Axial:
                XPixels = sourceSeries.Width;
                YPixels = sourceSeries.Height;
                ZPixels = (uint) sourceSeries.NumberOfSpacePositions;
                break;

            case AnatomicPlane.Coronal:
                XPixels = sourceSeries.Width;
                YPixels = (uint) sourceSeries.NumberOfSpacePositions;
                ZPixels = sourceSeries.Height;
                break;

            case AnatomicPlane.Sagittal:
                XPixels = (uint) sourceSeries.NumberOfSpacePositions;
                YPixels = sourceSeries.Height;
                ZPixels = sourceSeries.Width;
                break;
        }
    }

    public uint XPixels { get; private init; }
    public uint YPixels { get; private init; }
    public uint ZPixels { get; private init; }
}