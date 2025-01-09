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
                //currentSpaceSlice = value;
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
    private CoordsPixelLength CoordsPixelLength => new(Series,DisplayedPlane);
    //[ObservableProperty]
    //private int numberOfTemporalPositions;
    //public int Width => series.
    //public int Height { get; set; }
}