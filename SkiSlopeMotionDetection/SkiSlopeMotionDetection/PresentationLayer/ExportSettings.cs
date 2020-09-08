using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class ExportSettings : INotifyPropertyChanged
    {
        private ExportMode _exportMode;
        private bool _includeMarking;

        private TimeSpan _timeSpan;
        private int? _firstFrame;
        private int? _lastFrame;

        public ExportMode ExportMode
        {
            get { return _exportMode; }
            set { _exportMode = value; NotifyPropertyChanged(); NotifyPropertyChanged("MarkingEnabled"); NotifyPropertyChanged("IncludeMarkingChecked"); }
        }
        public bool IncludeMarking 
        { 
            get { return _includeMarking; } 
            set { _includeMarking = value; NotifyPropertyChanged(); NotifyPropertyChanged("IncludeMarkingChecked"); }
        }
        public bool IncludeMarkingChecked
        {
            get { return IncludeMarking && MarkingEnabled; }
            set { IncludeMarking = value; }
        }
        public bool MarkingEnabled
        {
            get { return ExportMode == ExportMode.CurrentFrame || ExportMode == ExportMode.EntireVideo; }
        }
        public TimeSpan TimeSpan
        {
            get { return _timeSpan; }
            set { _timeSpan = value; NotifyPropertyChanged(); }
        }
        public int? FirstFrame
        {
            get { return _firstFrame; }
            set { _firstFrame = value; NotifyPropertyChanged(); }
        }
        public int? LastFrame
        {
            get { return _lastFrame; }
            set { _lastFrame = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExportSettings(ExportMode exportMode = ExportMode.CurrentFrame, bool includeMarking = true)
        {
            ExportMode = exportMode;
            IncludeMarking = includeMarking;
        }
    }
}
