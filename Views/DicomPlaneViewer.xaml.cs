using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Models;
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
        //private DicomSeries? lastLoadedSeries;
        //private PerfusionInfoViewModel? perfusionVM;
        private DicomViewModel? viewModel;

        public DicomPlaneViewer()
        {
            InitializeComponent();
        }

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

        private void OpenTkControl_Render(TimeSpan obj)
        {
            openTkControl.Context?.MakeCurrent();
            GL.ClearColor(0.3f, 0.6f, 0.2f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            if (viewModel is not null)
                glState?.DrawVertices(viewModel.DisplayedPlane, viewModel.CurrentSpaceSlice, viewModel.CurrentTimeSlice);
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(viewModel.Series))
            {
                glState?.LoadDicomSeries(viewModel!.Series);
                //lastLoadedSeries = viewModel.Series;
            }
        }
    }
}