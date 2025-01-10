using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using ViewModels;
using Views.Graphics;

namespace Views
{
    /// <summary>
    /// Interaction logic for DicomPlaneViewer.xaml
    /// </summary>
    public partial class DicomPlaneViewer : UserControl
    {
        private DicomScene? glState;
        private DicomViewModel? viewModel;

        public DicomPlaneViewer()
        {
            InitializeComponent();
            PerfusionVM.PropertyChanged += PerfusionVM_PropertyChanged;
        }

        public PerfusionInfoViewModel PerfusionVM => (PerfusionInfoViewModel) Resources ["perfusionVM"];

        public DicomViewModel? ViewModel
        {
            get => viewModel;

            set
            {
                if (viewModel is not null)
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;

                if (value is not null)
                    value.PropertyChanged += ViewModel_PropertyChanged;

                DataContext = value;
                viewModel = value;

                glState?.LoadDicomSeries(viewModel!.Series);
            }
        }

        public IGraphicsContext InitOpenGL(GLWpfControlSettings settings)
        {
            openTkControl.Start(settings);

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                Debug.WriteLine($"OpenGL Debug: {Marshal.PtrToStringAnsi(message)}");
            }, IntPtr.Zero);

            return openTkControl.Context!;
        }

        public void LoadScene(DicomScene? dicomScene = null)
        {
            glState = dicomScene is null ? new() : dicomScene;
        }

        private void openTkControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ViewModel is not null && glState?.IsTextureLoaded == true)
            {
                var control = (GLWpfControl) sender;
                var coords = e.GetPosition(control);
                coords.Y = control.Height - coords.Y;

                System.Drawing.Point intCoords = new((int) coords.X, (int) coords.Y);

                ViewModel.SelectedPixel = intCoords;
                var intensities = glState.GetIntensities(ViewModel.SelectedPixel, ViewModel.DisplayedPlane, ViewModel.CurrentSpaceSlice);
                PerfusionVM.UpdateIntensities(intensities, ViewModel.Series.EchoTime);
            }
        }

        private void OpenTkControl_Render(TimeSpan obj)
        {
            openTkControl.Context?.MakeCurrent();
            GL.ClearColor(0.3f, 0.6f, 0.2f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            if (viewModel is not null)
                glState?.DrawVertices(viewModel.DisplayedPlane, viewModel.CurrentSpaceSlice, viewModel.CurrentTimeSlice);
        }

        private void PerfusionVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PerfusionInfoViewModel.Concentration))
            {
                var vm = (PerfusionInfoViewModel) sender!;
                var scottPlotPoints = vm.Concentration.Select(p =>
                    new ScottPlot.Coordinates(p.time, p.conc)).ToList();

                WpfPerfusionPlot.Plot.Clear();
                WpfPerfusionPlot.Plot.Add.Scatter(scottPlotPoints);

                WpfPerfusionPlot.Plot.YLabel("Концентрація", 20);
                WpfPerfusionPlot.Plot.XLabel("Час, мс", 20);
                WpfPerfusionPlot.Plot.Axes.AutoScale();
                WpfPerfusionPlot.Refresh();
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.Series))
            {
                glState?.LoadDicomSeries(viewModel!.Series);
            }
        }
    }
}