using System.Drawing;

namespace SkiSlopeMotionDetection
{
    public class BlobDetectionParameters
    {
        public DetectionMethod? DetectionMethod { get; set; }
        public int? AvgRangeBegin { get; set; }
        public int? AvgRangeEnd { get; set; }
        public bool MarkBlobs { get; set; } = true;
        public int DifferenceThreshold { get; set; } = 30;

        public int? HueHSV { get; set; }
        public int? SaturationHSV { get; set; }
        public int? ValueHSV { get; set; }


        public Bitmap AverageBitmap { get; set; }
        public EmguBlobDetectionOptions BlobDetectionOptions { get; set; }
    }
}
