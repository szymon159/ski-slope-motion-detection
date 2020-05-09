using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class ExportSettings : INotifyPropertyChanged
    {
        private bool _exportEntireVideo;
        private bool _includeMarking;

        public bool ExportEntireVideo
        { 
            get { return _exportEntireVideo; } 
            set { _exportEntireVideo = value; NotifyPropertyChanged(); }
        }
        public bool ExportSelectedFrame
        {
            get { return !_exportEntireVideo; }
            set { _exportEntireVideo = !value; NotifyPropertyChanged(); }
        }
        public bool IncludeMarking 
        { 
            get { return _includeMarking; } 
            set { _includeMarking = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExportSettings(bool exportEntireVideo = false, bool includeMarking = true)
        {
            ExportEntireVideo = exportEntireVideo;
            IncludeMarking = includeMarking;
        }
    }
}
