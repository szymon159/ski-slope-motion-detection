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

        public ExportWindow(BlobDetectionParameters blobDetectionParameters, bool exportEntireVideo = false, bool includeMarking = true)
        {
            InitializeComponent();

            ExportSettings = new ExportSettings(exportEntireVideo, includeMarking);

            _blobDetectionParameters = blobDetectionParameters;

            _exportWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _exportWorker.DoWork += ExportWorker_DoWork;
            _exportWorker.RunWorkerCompleted += ExportWorker_RunWorkerCompleted;
            _exportWorker.ProgressChanged += ExportWorker_ProgressChanged;
        }

        #endregion

        #region Private methods

        private void ExportWorker_DoWork(object sender, DoWorkEventArgs e)
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

        private void ExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _exportProgressWindow.Close();

            if (e.Error != null)
                throw e.Error;

            Close();
        }

        private void ExportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _exportProgressWindow.ProgressValue = e.ProgressPercentage;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var videoFilter = 
                "MP4 video file (*.mp4)|*.mp4|" +
                "Audio Video Interleave (*.avi)|*.avi";
            
            var imageFilter = 
                "Bitmap (*.bmp)|*.bmp|" +
                "Graphics Interchange Format (*.gif)|*.gif|" +
                "Exchangeable Image File Format (*.exif)|*.exif|" +
                "JPEG Image (*.jpg)|*.jpg|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Tagged Image File Format (*.tiff)|*.tiff";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = ExportSettings.ExportEntireVideo ? videoFilter : imageFilter,
                InitialDirectory = Environment.CurrentDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                if (ExportSettings.ExportSelectedFrame)
                    ExportFrame(saveFileDialog.FileName);
                else
                    ExportVideo(saveFileDialog.FileName);
            }
        }

        private void ExportFrame(string outputFileName)
        {
            if (!(Owner is MainWindow mainWindow))
                throw new ApplicationException("Unable to get current frame");

            var extension = Path.GetExtension(outputFileName);
            Bitmap currentFrame;

            if(!ExportSettings.IncludeMarking)
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

            _exportWorker.RunWorkerAsync(outputFileName);
            if(_exportProgressWindow.ShowDialog() == false && _exportWorker.IsBusy)
                _exportWorker.CancelAsync();
        }

        #endregion
    }
}
