using CommunityToolkit.Mvvm.ComponentModel;
using Models;

namespace ViewModels;

public partial class PerfusionInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private float baseline;

    private (float time, float conc) [] concentration = [];

    [ObservableProperty]
    private (float time, float intensity)[] intensities = [];


    [ObservableProperty]
    private float fwhm;

    private int indexPE;
    private int indexTRec;

    [ObservableProperty]
    private (float time, float conc) pe;

    [ObservableProperty]
    private float rTtp;

    [ObservableProperty]
    private float t0;

    [ObservableProperty]
    private float tRec;

    [ObservableProperty]
    private float tta;

    [ObservableProperty]
    private float ttp;

    [ObservableProperty]
    private float wir;

    [ObservableProperty]
    private float wor;

    public (float time, float conc) [] Concentration
    {
        get => concentration; set
        {
            SetProperty(ref concentration, value);
            Baseline = PerfusionAnalysis.Baseline(Concentration);

            Pe = PerfusionAnalysis.PE(Concentration);
            indexPE = Array.FindIndex(Concentration, point => point == Pe);

            T0 = PerfusionAnalysis.T0(Concentration, Pe, Baseline);

            Tta = T0;
            Ttp = Pe.time;
            RTtp = PerfusionAnalysis.RTTP(Pe, T0);
            Fwhm = PerfusionAnalysis.FWHM(Concentration, Pe, Baseline);
            Wir = PerfusionAnalysis.WiR(Concentration, T0, indexPE);
            TRec = PerfusionAnalysis.TRec(Concentration, indexPE);
            Wor = PerfusionAnalysis.WoR(Concentration, TRec, indexPE);
        }
    }

    public void UpdateIntensities((float time, float intensity)[] intensities, float echotime)
    {
        float intensityBaseline = intensities.Take(10).Select(p => p.intensity).Average();
        Intensities = intensities;
        Concentration = PerfusionAnalysis.IntensityToConcentration(intensities, intensityBaseline, echotime);
    }
}