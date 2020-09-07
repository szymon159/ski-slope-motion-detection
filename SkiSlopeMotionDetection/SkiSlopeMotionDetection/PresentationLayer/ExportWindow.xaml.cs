using Accord.Math;
using Accord.Video.FFMPEG;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Annotations;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window, INotifyPropertyChanged
    {
        #region Private variables

        private ExportSettings _settings;
        private BackgroundWorker _exportWorker;
        private ExportProgressWindow _exportProgressWindow;
        private BlobDetectionParameters _blobDetectionParameters;

        #endregion Private variables

        #region Properties

        public ExportSettings ExportSettings
        {
            get { return _settings; }
            set { _settings = value; NotifyPropertyChanged(); }
        }

        #endregion Properties

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Event handlers

        #region Public methods

        public ExportWindow(BlobDetectionParameters blobDetectionParameters, ExportMode exportMode = ExportMode.CurrentFrame, bool includeMarking = true)
        {
            ExportSettings = new ExportSettings(exportMode, includeMarking);

            InitializeComponent();

            _blobDetectionParameters = blobDetectionParameters;

            _exportWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _exportWorker.RunWorkerCompleted += ExportWorker_RunWorkerCompleted;
            _exportWorker.ProgressChanged += ExportWorker_ProgressChanged;
        }

        #endregion Public methods

        #region Private methods

        private void ExportWorker_DoWorkVideo(object sender, DoWorkEventArgs e)
        {
            var outputFileName = e.Argument as string;
            var reader = FrameReaderSingleton.GetInstance();

            (int first, int last) = GetFirstAndLastFrameNumber((int)reader.FrameCount);
            int totalFrames = last - first;

            using (var writer = new VideoFileWriter())
            {
                writer.Open(outputFileName, reader.FrameWidth, reader.FrameHeight, new Rational(reader.FrameRate), VideoCodec.Default);
                if (!writer.IsOpen)
                    throw new ArgumentException("Unable to open file for writing");

                for (int i = first; i < last; i++)
                {
                    if (_exportWorker.CancellationPending)
                        break;

                    Bitmap frame = reader.GetFrame(i);

                    if (ExportSettings.IncludeMarking)
                    {
                        if (_blobDetectionParameters.DetectionMethod == DetectionMethod.DiffWithAverage)
                        {
                            if (i % 10 == 0)
                            {
                                _blobDetectionParameters.AddFrameToAverage = true;
                            }
                        }

                        // Hack to prevent from deep copy of _blobDetectionParameters
                        var temp = _blobDetectionParameters.MarkBlobs;

                        _blobDetectionParameters.MarkBlobs = true;
                        frame = BlobDetection.GetResultImage(frame, _blobDetectionParameters, out _);
                        _blobDetectionParameters.MarkBlobs = temp;
                    }

                    writer.WriteVideoFrame(frame);

                    var progress = 100 * i / (double)totalFrames;
                    _exportWorker.ReportProgress((int)progress);
                }
            }
        }

        private void ExportWorker_DoWorkStats(object sender, DoWorkEventArgs e)
        {
            var outputFileName = e.Argument as string;
            var reader = FrameReaderSingleton.GetInstance();

            (int first, int last) = GetFirstAndLastFrameNumber((int)reader.FrameCount);
            int framesForAverage = GetFramesForAverageCount((int)Math.Round(reader.FrameRate));
            int totalFrames = last - first;

            double peopleSum = 0;
            int frameCount = 0;

            using (var writer = new StreamWriter(outputFileName))
            {
                if (writer == null)
                    throw new ArgumentException("Unable to open file for writing");

                for (int i = first; i < last; i++)
                {
                    if (_exportWorker.CancellationPending)
                        break;

                    if (_blobDetectionParameters.DetectionMethod == DetectionMethod.DiffWithAverage)
                    {
                        if (i % 10 == 0)
                        {
                            _blobDetectionParameters.AddFrameToAverage = true;
                        }
                    }

                    Bitmap frame = reader.GetFrame(i);
                    BlobDetection.GetResultImage(frame, _blobDetectionParameters, out int peopleInFrame);

                    if (i % framesForAverage == 0 && i != 0)
                    {
                        writer.WriteLine($"{ExportSettings.TimeSpan} {i / framesForAverage} (frames {i - frameCount} - {i - 1}): {peopleSum / frameCount} people in average");

                        peopleSum = 0;
                        frameCount = 0;
                    }

                    peopleSum += peopleInFrame;
                    frameCount++;

                    if (i == last - 1 && (i % framesForAverage != 0 || i == 0))
                    {
                        writer.WriteLine($"{ExportSettings.TimeSpan} {i / framesForAverage + 1} (frames {i - frameCount + 1} - {i}): {peopleSum / frameCount} people in average");
                    }

                    var progress = 100 * i / (double)totalFrames;
                    _exportWorker.ReportProgress((int)progress);
                }
            }
        }

        private void ExportWorker_DoWorkHeatmap(object sender, DoWorkEventArgs e)
        {
            var outputFileName = e.Argument as string;
            var reader = FrameReaderSingleton.GetInstance();

            (int first, int last) = GetFirstAndLastFrameNumber((int)reader.FrameCount);
            int totalFrames = last - first;

            var heatmap = new Heatmap(reader.FrameWidth, reader.FrameHeight);

            for (int i = first; i < last; i++)
            {
                if (_exportWorker.CancellationPending)
                    break;

                if (_blobDetectionParameters.DetectionMethod == DetectionMethod.DiffWithAverage)
                {
                    if (i % 10 == 0)
                    {
                        _blobDetectionParameters.AddFrameToAverage = true;
                    }
                }

                Bitmap frame = reader.GetFrame(i);
                _blobDetectionParameters.GetKeyPoints = true;
                BlobDetection.GetResultImage(frame, _blobDetectionParameters, out _, out Emgu.CV.Structure.MKeyPoint[] mKeys);
                _blobDetectionParameters.GetKeyPoints = true;

                heatmap.UpdateSeries(mKeys);

                var progress = 100 * i / (double)totalFrames;
                _exportWorker.ReportProgress((int)progress);
            }

            ImageConverter converter = new ImageConverter();
            heatmap.HeatMap.Annotations.Add(new ImageAnnotation
            {
                ImageSource = new OxyImage((byte[])converter.ConvertTo(reader.GetFrame(1), typeof(byte[]))),
                Opacity = 0.2,
                X = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Y = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Width = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
                Height = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea)
            }); ;
            heatmap.HeatMap.InvalidatePlot(true);

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            { 
                heatmap.SaveToFile(outputFileName);
            }, null);
        }

        private void ExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _exportProgressWindow.Close();

            if (e.Error != null)
                MessageBox.Show(e.Error.Message, "Invalid argument", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            Close();
        }

        private void ExportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _exportProgressWindow.ProgressValue = e.ProgressPercentage;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string filter = "All files (*.*)|*.*";
            switch (ExportSettings.ExportMode)
            {
                case ExportMode.CurrentFrame:
                    filter =
                        "Bitmap (*.bmp)|*.bmp|" +
                        "Graphics Interchange Format (*.gif)|*.gif|" +
                        "Exchangeable Image File Format (*.exif)|*.exif|" +
                        "JPEG Image (*.jpg)|*.jpg|" +
                        "Portable Network Graphics (*.png)|*.png|" +
                        "Tagged Image File Format (*.tiff)|*.tiff";
                    break;

                case ExportMode.EntireVideo:
                    filter =
                        "MP4 video file (*.mp4)|*.mp4|" +
                        "Audio Video Interleave (*.avi)|*.avi";
                    break;

                case ExportMode.Stats:
                    filter =
                        "Text file (*.txt)|*.txt";
                    break;

                case ExportMode.Heatmap:
                    filter =
                        "Portable Network Graphics (*.png)|*.png";
                    break;

                default:
                    break;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                InitialDirectory = Environment.CurrentDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                switch (ExportSettings.ExportMode)
                {
                    case ExportMode.CurrentFrame:
                        ExportFrame(saveFileDialog.FileName);
                        break;

                    case ExportMode.EntireVideo:
                        ExportVideo(saveFileDialog.FileName);
                        break;

                    case ExportMode.Stats:
                        ExportStats(saveFileDialog.FileName);
                        break;

                    case ExportMode.Heatmap:
                        ExportHeatmap(saveFileDialog.FileName);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ExportFrame(string outputFileName)
        {
            if (!(Owner is MainWindow mainWindow))
                throw new ApplicationException("Unable to get current frame");

            var extension = Path.GetExtension(outputFileName);
            var reader = FrameReaderSingleton.GetInstance();
            var currentFrame = reader.GetFrame(mainWindow.CurrentFrameNumber);

            if (ExportSettings.IncludeMarking)
            {
                // Hack to prevent from deep copy of _blobDetectionParameters
                var temp = _blobDetectionParameters.MarkBlobs;

                _blobDetectionParameters.MarkBlobs = true;
                currentFrame = BlobDetection.GetResultImage(currentFrame, _blobDetectionParameters, out _);
                _blobDetectionParameters.MarkBlobs = temp;
            }

            currentFrame.Save(outputFileName, extension.ToImageFormat());

            Close();
        }

        private void ExportVideo(string outputFileName)
        {
            _exportProgressWindow = new ExportProgressWindow
            {
                Owner = GetWindow(this)
            };

            _exportWorker.DoWork += ExportWorker_DoWorkVideo;
            _exportWorker.RunWorkerAsync(outputFileName);
            if (_exportProgressWindow.ShowDialog() == false && _exportWorker.IsBusy)
                _exportWorker.CancelAsync();
        }

        private void ExportStats(string outputFileName)
        {
            _exportProgressWindow = new ExportProgressWindow
            {
                Owner = GetWindow(this)
            };

            _exportWorker.DoWork += ExportWorker_DoWorkStats;
            _exportWorker.RunWorkerAsync(outputFileName);
            if (_exportProgressWindow.ShowDialog() == false && _exportWorker.IsBusy)
                _exportWorker.CancelAsync();
        }

        private void ExportHeatmap(string outputFileName)
        {
            _exportProgressWindow = new ExportProgressWindow
            {
                Owner = GetWindow(this)
            };

            _exportWorker.DoWork += ExportWorker_DoWorkHeatmap;
            _exportWorker.RunWorkerAsync(outputFileName);
            if (_exportProgressWindow.ShowDialog() == false && _exportWorker.IsBusy)
                _exportWorker.CancelAsync();
        }

        private (int first, int last) GetFirstAndLastFrameNumber(int frameCount)
        {
            int first = 0, last = 0;
            if (!ExportSettings.FirstFrame.HasValue)
                first = 0;
            if (!ExportSettings.LastFrame.HasValue)
                last = frameCount;
            if (ExportSettings.FirstFrame.HasValue && ExportSettings.LastFrame.HasValue)
            {
                first = ExportSettings.FirstFrame.Value;
                if (first < 0)
                    first = 0;
                if (first >= frameCount)
                    first = frameCount - 1;
                last = ExportSettings.LastFrame.Value;
                if (last < 0)
                    last = 0;
                if (last > frameCount)
                    last = frameCount;
            }
            return (first, last);
        }

        private int GetFramesForAverageCount(int frameRate)
        {
            switch (ExportSettings.TimeSpan)
            {
                case TimeSpan.Second:
                    return frameRate;
                case TimeSpan.Minute:
                    return 60 * frameRate;
                case TimeSpan.Hour:
                    return 3600 * frameRate;
                default:
                    return frameRate;
            }
        }

        #endregion Private methods
    }
}
