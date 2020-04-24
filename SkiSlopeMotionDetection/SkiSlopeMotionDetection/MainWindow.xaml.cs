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
        private int currentFrameNumber = 0;
        private int totalFrameNumber = 0;
        private float fpsCounter = 0;
        private int countedPeople = 0;
        private bool shouldAdjustVideoRefreshRate = false;
        private bool shouldMarkPeopleInRealTime = false;

        public event PropertyChangedEventHandler PropertyChanged;
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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
