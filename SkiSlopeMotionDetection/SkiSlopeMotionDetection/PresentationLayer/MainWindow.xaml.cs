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
        private float _fpsCounter = 0;
        private int _countedPeople = 0;
        private bool _shouldAdjustVideoRefreshRate = false;
        private bool _shouldMarkPeopleInRealTime = false;
        private bool _isVideoPaused = true;
        private bool _isVideoLoaded = false;
        private bool _isVideoEnded = false;

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
        public float FPScounter
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
            get { return _shouldAdjustVideoRefreshRate; }
            set { _shouldAdjustVideoRefreshRate = value; NotifyPropertyChanged(); NotifyPropertyChanged("UseOriginalRefreshRate"); }
        }
        public bool UseOriginalRefreshRate
        {
            get { return !_shouldAdjustVideoRefreshRate; }
            set { _shouldAdjustVideoRefreshRate = !value; NotifyPropertyChanged(); NotifyPropertyChanged("UseAdjustedRefreshRate"); }
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

        #endregion

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void VideoControl_MediaOpened(object sender, RoutedEventArgs e)
        {
            IsVideoLoaded = true;
            NotifyPropertyChanged("LoadVideoButtonVisibility");
        }

        private void VideoControl_MediaEnded(object sender, RoutedEventArgs e)
        {
            _isVideoEnded = true;
            _isVideoPaused = true;
            NotifyPropertyChanged("PlayPauseButtonText");
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4;*.avi)|*.mp4;*.avi";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                videoControl.Source = new Uri(path);
                videoControl.Play();
                videoControl.Pause();

                FrameReaderSingleton.GetInstance(path);
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

            if (_isVideoPaused)
                PlayVideo(_isVideoEnded);
            else
                PauseVideo();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsVideoLoaded)
                return;

            PlayVideo(true);
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Private methods

        private void PauseVideo()
        {
            videoControl.Pause();
            _isVideoPaused = true;
            NotifyPropertyChanged("PlayPauseButtonText");
        }

        // For play with natural ratio
        // Run (background worker ?) fetching frames in endless loop, after fetching frame sleep for few milisecods to match the ratio
        // If natural ratio is not set then disable the sleep part and just fetch the frames in endless loop (or until they're finished)
        private void PlayVideo(bool fromBeginning = false)
        {
            if (fromBeginning)
                videoControl.Stop();

            videoControl.Play();
            _isVideoPaused = false;
            NotifyPropertyChanged("PlayPauseButtonText");
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bm = Processing.GetAverage(400,1000);
            FrameReaderSingleton reader = FrameReaderSingleton.GetInstance();
            Bitmap bm2 = reader.GetFrame(1400);
            Bitmap bm3 = (BlobDetection.GetDifference(bm, bm2, 30)).ToBitmap();

            BlobDetectionOptions opts = new BlobDetectionOptions(80);
            Emgu.CV.Structure.MKeyPoint[] mKeys = BlobDetection.ReturnBlobs(bm3, opts);
            Mat im_with_keypoints = new Mat();
            Image<Bgr, byte> im2 = new Image<Bgr, byte>(bm2);
            Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), im_with_keypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
            MemoryStream ms = new MemoryStream();

            Bitmap final = (im_with_keypoints.ToImage<Bgr, byte>()).ToBitmap();
            final.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            playerFrameBox.Image = new Image<Bgr, Byte>(new Bitmap(image.StreamSource));
            playerFrameBox.Invalidate();
        }
    }
}
