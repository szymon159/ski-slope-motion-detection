﻿using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Drawing;
using System.Threading.Tasks;

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
        private bool _useOriginalRefreshRate = false;
        private bool _isVideoPaused = true;
        private bool _isVideoLoaded = false;
        private bool _isBackgroundImageLoaded = false;
        private BlobDetectionParameters _blobDetectionParameters = new BlobDetectionParameters()
        {
            // TODO: Parametrize
            DetectionMethod = DetectionMethod.DiffWithBackground,
            BlobDetectionOptions = new EmguBlobDetectionOptions(80)
        };

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
            set { _useOriginalRefreshRate = !value; videoControl.UseOriginalRefreshRate = !value; NotifyPropertyChanged(); NotifyPropertyChanged("UseOriginalRefreshRate"); }
        }
        public bool UseOriginalRefreshRate
        {
            get { return _useOriginalRefreshRate; }
            set { _useOriginalRefreshRate = value; videoControl.UseOriginalRefreshRate = value; if (value) MarkPeopleOnEachFrame = false; NotifyPropertyChanged(); NotifyPropertyChanged("UseAdjustedRefreshRate"); }
        }
        public bool MarkPeopleDisabled
        {
            get { return !_blobDetectionParameters.MarkBlobs; }
            set { _blobDetectionParameters.MarkBlobs = !value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleOnEachFrame"); }
        }
        public bool MarkPeopleOnEachFrame
        {
            get { return _blobDetectionParameters.MarkBlobs; }
            set { _blobDetectionParameters.MarkBlobs = value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleDisabled"); }
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
            get;
            set;
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
        public BlobDetectionParameters BlobDetectionParameters
        {
            get { return _blobDetectionParameters; }
            set { _blobDetectionParameters = value; NotifyPropertyChanged(); }
        }
        public Bitmap CurrentFrame
        {
            get { return videoControl.Image.Bitmap; }
        }

        public int HueHSV
        {
            get { return _blobDetectionParameters.HueHSV; }
            set { _blobDetectionParameters.HueHSV = value; NotifyPropertyChanged(); }
        }

        public int SaturationHSV
        {
            get { return _blobDetectionParameters.SaturationHSV; }
            set { _blobDetectionParameters.SaturationHSV = value; NotifyPropertyChanged(); }
        }

        public int ValueHSV
        {
            get { return _blobDetectionParameters.ValueHSV; }
            set { _blobDetectionParameters.ValueHSV = value; NotifyPropertyChanged(); }
        }
        public int MinBlob
        {
            get { return _blobDetectionParameters.BlobDetectionOptions.MinArea; }
            set { _blobDetectionParameters.BlobDetectionOptions.MinArea = value; NotifyPropertyChanged(); }
        }

        public int DifferenceThreshold
        {
            get { return _blobDetectionParameters.DifferenceThreshold; }
            set { _blobDetectionParameters.DifferenceThreshold = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "BlobDetectionParameters")
            {
                videoControl.UpdateBlobDetectionParameters(BlobDetectionParameters);
            }
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
                BlobDetectionParameters.BackgroundBitmap = new Bitmap(path);
                IsBackgroundImageLoaded = true;
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var exportWindow = new ExportWindow(BlobDetectionParameters)
            {
                Owner = GetWindow(this)
            };

            exportWindow.ShowDialog();
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

        #region Callbacks

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

        private void VideoControl_MediaPaused()
        {
            IsVideoPaused = true;
        }

        private void VideoControl_FrameChanged(FrameData frameData)
        {
            CurrentFrameNumber = frameData.CurrentFrame;
            FPScounter = frameData.FPS;
            CountedPeople = frameData.CountedPeople;
        }

        #endregion

        public MainWindow()
        {
            PropertyChanged += MainWindow_PropertyChanged;
            InitializeComponent();

            videoControl.UpdateBlobDetectionParameters(BlobDetectionParameters);
        }

        #region Private methods

        private void PauseVideo()
        {
            videoControl.Pause();
            IsVideoPaused = true;
        }

        private void PlayVideo(bool fromBeginning = false)
        {
            if (fromBeginning)
                videoControl.Stop();

            try
            {
                Task.Delay(100).ContinueWith(t =>
                {
                    videoControl.Play();
                    IsVideoPaused = false;
                }).Wait();
            }
            catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        #endregion

    }
}
