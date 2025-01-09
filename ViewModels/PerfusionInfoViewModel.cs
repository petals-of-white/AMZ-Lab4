using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModels;

public partial class PerfusionInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private float baseline;

    [ObservableProperty]
    private IReadOnlyList<float> concentration = [];

    [ObservableProperty]
    private float fwhm;

    [ObservableProperty]
    private float pe;

    [ObservableProperty]
    private float rTtp;

    [ObservableProperty]
    private float tta;

    [ObservableProperty]
    private float ttp;

    [ObservableProperty]
    private float wir;

    [ObservableProperty]
    private float wor;
}