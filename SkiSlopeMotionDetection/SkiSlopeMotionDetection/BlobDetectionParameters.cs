using System.Drawing;

namespace SkiSlopeMotionDetection
{
    public class BlobDetectionParameters
    {
        public DetectionMethod DetectionMethod { get; set; } = DetectionMethod.DiffWithBackground;
        public int AvgRangeBegin { get; set; } = 0;
        public int AvgFramesCount { get; set; } = 100;
        public bool MarkBlobs { get; set; } = true;
        public int DifferenceThreshold { get; set; } = 30;


        public Bitmap AverageBitmap { get; set; }
        public EmguBlobDetectionOptions BlobDetectionOptions { get; set; }
    }
}
