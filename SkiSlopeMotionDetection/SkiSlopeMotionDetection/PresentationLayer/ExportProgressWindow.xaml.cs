using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for ExportProgressWindow.xaml
    /// </summary>
    public partial class ExportProgressWindow : Window, INotifyPropertyChanged
    {
        private int _progressValue = 0;

        public int ProgressValue 
        { 
            get { return _progressValue; }
            set { _progressValue = value; NotifyPropertyChanged("ProgressValue"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExportProgressWindow()
        {
            InitializeComponent();
        }
    }
}
