using Accord.Math;
using Accord.Video.FFMPEG;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

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

        #endregion

        #region Properties

        public ExportSettings ExportSettings 
        { 
            get { return _settings; }
            set { _settings = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

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

        #endregion

        #region Private methods

        private void ExportWorker_DoWorkVideo(object sender, DoWorkEventArgs e)
        {
            var outputFileName = e.Argument as string;
            var reader = FrameReaderSingleton.GetInstance();

            using (var writer = new VideoFileWriter())
            {
                writer.Open(outputFileName, reader.FrameWidth, reader.FrameHeight, new Rational(reader.FrameRate), VideoCodec.Default);
                if (!writer.IsOpen)
                    throw new ArgumentException("Unable to open file for writing");
               
                for (int i = 0; i < reader.FrameCount; i++)
                {
                    if (_exportWorker.CancellationPending)
                        break;

                    Bitmap frame = reader.GetFrame(i);

                    if(ExportSettings.IncludeMarking)
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

                    var progress = 100 * i / (double)reader.FrameCount;
                    _exportWorker.ReportProgress((int)progress);
                }
            }
        }

        private void ExportWorker_DoWorkStats(object sender, DoWorkEventArgs e)
        {
            var outputFileName = e.Argument as string;
            var reader = FrameReaderSingleton.GetInstance();

            using (var writer = new StreamWriter(outputFileName))
            {
                if (writer == null)
                    throw new ArgumentException("Unable to open file for writing");

                for (int i = 0; i < reader.FrameCount; i++)
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
                    BlobDetection.GetResultImage(frame, _blobDetectionParameters, out int peopleCount);

                    writer.WriteLine($"Frame {i}: {peopleCount} people");

                    var progress = 100 * i / (double)reader.FrameCount;
                    _exportWorker.ReportProgress((int)progress);
                }
            }
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
                case ExportMode.Histogram:
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
                    case ExportMode.Histogram:
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
            Bitmap currentFrame;

            if (!ExportSettings.IncludeMarking)
            {
                var reader = FrameReaderSingleton.GetInstance();
                currentFrame = reader.GetFrame(mainWindow.CurrentFrameNumber);
            }
            else
            {
                currentFrame = mainWindow.CurrentFrame;
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
            if(_exportProgressWindow.ShowDialog() == false && _exportWorker.IsBusy)
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

        #endregion
    }
}
