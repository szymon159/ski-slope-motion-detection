using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkiSlopeMotionDetection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private variables

        private int currentFrameNumber = 0;
        private int totalFrameNumber = 0;
        private float fpsCounter = 0;
        private int countedPeople = 0;
        private bool shouldAdjustVideoRefreshRate = false;
        private bool shouldMarkPeopleInRealTime = false;
        private bool isVideoPaused = true;
        private bool isVideoLoaded = false;
        private bool isVideoEnded = false;

        #endregion

        #region Properties

        public int CurrentFrameNumber
        {
            get { return currentFrameNumber; }
            set { currentFrameNumber = value; NotifyPropertyChanged(); }
        }
        public int TotalFrameNumber
        {
            get { return totalFrameNumber; }
            set { totalFrameNumber = value; NotifyPropertyChanged(); }
        }
        public float FPScounter
        {
            get { return fpsCounter; }
            set { fpsCounter = value; NotifyPropertyChanged(); }
        }
        public int CountedPeople
        {
            get { return countedPeople; }
            set { countedPeople = value; NotifyPropertyChanged(); }
        }
        public bool UseAdjustedRefreshRate
        {
            get { return shouldAdjustVideoRefreshRate; }
            set { shouldAdjustVideoRefreshRate = value; NotifyPropertyChanged(); NotifyPropertyChanged("UseOriginalRefreshRate"); }
        }
        public bool UseOriginalRefreshRate
        {
            get { return !shouldAdjustVideoRefreshRate; }
            set { shouldAdjustVideoRefreshRate = !value; NotifyPropertyChanged(); NotifyPropertyChanged("UseAdjustedRefreshRate"); }
        }
        public bool MarkPeopleOnPausedFrame
        {
            get { return !shouldMarkPeopleInRealTime; }
            set { shouldMarkPeopleInRealTime = !value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleOnEachFrame"); }
        }
        public bool MarkPeopleOnEachFrame
        {
            get { return shouldMarkPeopleInRealTime; }
            set { shouldMarkPeopleInRealTime = value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkPeopleOnPausedFrame"); }
        }
        public Visibility LoadVideoButtonVisibility
        {
            get { return isVideoLoaded ? Visibility.Collapsed : Visibility.Visible; }
        }
        public string PlayPauseButtonText
        {
            get { return isVideoPaused ? "Play" : "Pause"; }
        }
        public bool IsVideoLoaded
        {
            get { return isVideoLoaded; }
            set { isVideoLoaded = value; NotifyPropertyChanged("IsVideoLoaded"); }
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
            isVideoEnded = true;
            isVideoPaused = true;
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

            if (isVideoPaused)
                PlayVideo(isVideoEnded);
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
            isVideoPaused = true;
            NotifyPropertyChanged("PlayPauseButtonText");
        }

        private void PlayVideo(bool fromBeginning = false)
        {
            if (fromBeginning)
                videoControl.Stop();

            videoControl.Play();
            isVideoPaused = false;
            NotifyPropertyChanged("PlayPauseButtonText");
        }

        #endregion
    }
}
