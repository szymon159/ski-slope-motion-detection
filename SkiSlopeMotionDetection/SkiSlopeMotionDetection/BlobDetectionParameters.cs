using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection
{
    public class BlobDetectionParameters
    {
        private int? _differenceThreshold = null;
        private bool? _markBlobs = null;

        public DetectionMethod? DetectionMethod { get; set; }
        public int? AvgRangeBegin { get; set; }
        public int? AvgRangeEnd { get; set; }
        public bool MarkBlobs
        {
            get { return _markBlobs ?? true; }
            set { _markBlobs = value; }
        }
        public int DifferenceThreshold 
        { 
            get { return _differenceThreshold ?? 30; }
            set { _differenceThreshold = value; }
        }


        public Bitmap AverageBitmap { get; set; }
        public EmguBlobDetectionOptions BlobDetectionOptions { get; set; }
    }
}
