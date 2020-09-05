using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Threading;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Configuration;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private variables

        private int _currentFrameNumber = 0;
        private int _totalFrameNumber = 0;
        private double _fpsCounter = 0;
        private int _countedPeople = 0;
        private bool _useOriginalRefreshRate = true;
        private bool _shouldMarkPeopleInRealTime = false;
        private bool _isVideoPaused = true;
        private bool _isVideoLoaded = false;
        private bool _isVideoEnded = false;
        private bool _isBackgroundImageLoaded = false;
        private Bitmap _backgroundImage;

        #endregion

        #region Properties

        public int CurrentFrameNumber
        {
            get { return _currentFrameNumber; }
            set { _currentFrameNumber = value; NotifyPropertyChanged(); }
        }
        public int TotalFrameNumber
        {
            get { return _totalFrameNumber; }
            set { _totalFrameNumber = value; NotifyPropertyChanged(); }
        }
        public double FPScounter
        {
            get { return _fpsCounter; }
            set { _fpsCounter = value; NotifyPropertyChanged(); }
        }
        public int CountedPeople
        {
            get { return _countedPeople; }
            set { _countedPeople = value; NotifyPropertyChanged(); }
        }
        public bool UseAdjustedRefreshRate
        {
            get { return !_useOriginalRefreshRate; }
            set { _useOriginalRefreshRate = !value; NotifyPropertyChanged(); NotifyPropertyChanged("UseOriginalRefreshRate"); }
        }
        public bool UseOriginalRefreshRate
        {
            get { return _useOriginalRefreshRate; }
            set { _useOriginalRefreshRate = value; NotifyPropertyChanged(); NotifyPropertyChanged("UseAdjustedRefreshRate"); }
        }
        public bool MarkPeopleOnPausedFrame
        {
            get { return !_shouldMarkPeopleInRealTime; }
            set { _shouldMarkPeopleInRealTime = !value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleOnEachFrame"); }
        }
        public bool MarkPeopleOnEachFrame
        {
            get { return _shouldMarkPeopleInRealTime; }
            set { _shouldMarkPeopleInRealTime = value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleOnPausedFrame"); }
        }
        public Visibility LoadVideoButtonVisibility
        {
            get { return _isVideoLoaded ? Visibility.Collapsed : Visibility.Visible; }
        }
        public string PlayPauseButtonText
        {
            get { return _isVideoPaused ? "Play" : "Pause"; }
        }
        public bool IsVideoLoaded
        {
            get { return _isVideoLoaded; }
            set { _isVideoLoaded = value; NotifyPropertyChanged("IsVideoLoaded"); }
        }
        public bool IsVideoPaused
        {
            get { return _isVideoPaused; }
            set { _isVideoPaused = value; NotifyPropertyChanged("PlayPauseButtonText"); NotifyPropertyChanged("IsVideoPaused"); }
        }
        public bool IsVideoEnded
        {
            get { return _isVideoEnded; }
            set { _isVideoEnded = value; }
        }
        public bool IsBackgroundImageLoaded
        {
            get { return _isBackgroundImageLoaded; }
            set { _isBackgroundImageLoaded = value; NotifyPropertyChanged("BackgroundImageLoadedLabel"); NotifyPropertyChanged("BackgroundImageLoadedLabelColor"); }
        }
        public string BackgroundImageLoadedLabel
        {
            get { return IsBackgroundImageLoaded ? "Background loaded" : "Background not loaded"; }
        }
        public string BackgroundImageLoadedLabelColor
        {
            get { return IsBackgroundImageLoaded ? "Green" : "Red"; }
        }

        #endregion

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void VideoControl_MediaOpened()
        {
            IsVideoLoaded = true;
            NotifyPropertyChanged("LoadVideoButtonVisibility");
        }

        private void VideoControl_MediaEnded()
        {
            IsVideoEnded = true;
            IsVideoPaused = true;
        }

        private void VideoControl_FrameChanged(FrameData frameData)
        {
            CurrentFrameNumber = frameData.CurrentFrame;
            FPScounter = frameData.FPS;
            CountedPeople = frameData.CountedPeople;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.avi)|*.mp4;*.avi",
                InitialDirectory = Environment.CurrentDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                videoControl.Source = path;

                TotalFrameNumber = (int)FrameReaderSingleton.GetInstance(path).FrameCount;
            }
        }

        private void LoadBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff)|*.bmp;*.gif;*.exif;*.jpg;*.png;*.tiff",
                InitialDirectory = Environment.CurrentDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                _backgroundImage = new Bitmap(path);
                IsBackgroundImageLoaded = true;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var exportWindow = new ExportWindow();
            exportWindow.Owner = Window.GetWindow(this);

            exportWindow.Show();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVideoLoaded)
                return;

            if (IsVideoPaused)
                PlayVideo(IsVideoEnded);
            else
                PauseVideo();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVideoLoaded)
                return;

            PlayVideo(true);
        }

        //private void RewindButton_Click(object sender, RoutedEventArgs e)
        //{
        //    videoControl.Rewind();
        //}

        //private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        //{
        //    videoControl.FastForward();
        //}

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Private methods

        private void PauseVideo()
        {
            videoControl.Pause();
            IsVideoPaused = true;
        }

        // For play with natural ratio
        // Run (background worker ?) fetching frames in endless loop, after fetching frame sleep for few milisecods to match the ratio
        // If natural ratio is not set then disable the sleep part and just fetch the frames in endless loop (or until they're finished)
        private void PlayVideo(bool fromBeginning = false)
        {
            if (fromBeginning)
                videoControl.Stop();

            Task.Delay(100).ContinueWith(t =>
            {
                videoControl.Play();
                IsVideoPaused = false;
            });
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reader = FrameReaderSingleton.GetInstance();
            var source = reader.GetFrame(1400);
            var detectionParams = new BlobDetectionParameters()
            {
                DetectionMethod = DetectionMethod.DiffWithAverage,
                AvgRangeBegin = 400,
                AvgRangeEnd = 1000,
                BlobDetectionOptions = new EmguBlobDetectionOptions(80)
            };
            var image = BlobDetection.GetResultImage(source, detectionParams, out int countedPeople);
            CountedPeople = countedPeople;

            CurrentFrameNumber = 1400;
            videoControl.SetFrameContent(image);
        }
    }
}
