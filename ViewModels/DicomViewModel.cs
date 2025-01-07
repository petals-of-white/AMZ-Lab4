using CommunityToolkit.Mvvm.ComponentModel;
using Models;

namespace ViewModels;

public partial class DicomViewModel : ObservableObject
{
    [ObservableProperty]
    private AnatomicPlane displayedPlane;

    [ObservableProperty]
    private int currentSpaceSlice;

    [ObservableProperty]
    private int currentTimeSlice;

    [ObservableProperty]
    private IReadOnlyList<DicomSlice> slices;
}