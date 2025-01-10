using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using Models;

namespace ViewModels;

public partial class DicomViewModel : ObservableObject
{
    //[ObservableProperty]
    private uint currentSpaceSlice;

    //[ObservableProperty]
    private uint currentTimeSlice;

    [ObservableProperty]
    private AnatomicPlane displayedPlane;

    [ObservableProperty]
    private Point selectedPixel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NumberOfSpacePositions), nameof(NumberOfTemporalPositions))]
    private DicomSeries series;

    public DicomViewModel(DicomSeries dicomSlices)
    {
        series = dicomSlices;
    }

    public uint CurrentSpaceSlice
    {
        get => currentSpaceSlice; set
        {
            if (value < NumberOfSpacePositions)
            {
                SetProperty(ref currentSpaceSlice, value);
            }
        }
    }

    public uint CurrentTimeSlice
    {
        get => currentTimeSlice; set
        {
            if (value < NumberOfTemporalPositions)
            {
                SetProperty(ref currentTimeSlice, value);
            }
        }
    }

    public int NumberOfSpacePositions => (int) CoordsPixelLength.ZPixels;

    public int NumberOfTemporalPositions => (int) Series.NumberOfTemporalPositions;
    private CoordsPixelLength CoordsPixelLength => new(Series, DisplayedPlane);
}