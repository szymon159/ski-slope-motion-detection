using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class ExportSettings : INotifyPropertyChanged
    {
        private bool exportEntireVideo;
        private bool includeMarking;

        public bool ExportEntireVideo
        { 
            get { return exportEntireVideo; } 
            set { exportEntireVideo = value; NotifyPropertyChanged(); }
        }
        public bool ExportSelectedFrame
        {
            get { return !exportEntireVideo; }
            set { exportEntireVideo = !value; NotifyPropertyChanged(); }
        }
        public bool IncludeMarking 
        { 
            get { return includeMarking; } 
            set { includeMarking = value; NotifyPropertyChanged(); }
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
