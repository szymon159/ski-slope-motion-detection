using System.Drawing;

namespace SkiSlopeMotionDetection
{
    public class BlobDetectionParameters
    {
        public DetectionMethod DetectionMethod { get; set; } = DetectionMethod.DiffWithBackground;
        public int AvgRangeBegin { get; set; } = 0;
        public int AvgFramesCount { get; set; } = 100;
        public bool MarkBlobs { get; set; } = true;
        public bool AddFrameToAverage { get; set; } = false;
        public int DifferenceThreshold { get; set; } = 30;

        public int? HueHSV { get; set; }
        public int? SaturationHSV { get; set; }
        public int? ValueHSV { get; set; }


        public Bitmap AverageBitmap { get; set; }
        public Bitmap BackgroundBitmap { get; set; }
        public EmguBlobDetectionOptions BlobDetectionOptions { get; set; }
    }
}
