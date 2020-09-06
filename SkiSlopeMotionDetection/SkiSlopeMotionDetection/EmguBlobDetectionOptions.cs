using Emgu.CV.Features2D;

namespace SkiSlopeMotionDetection
{
    public class EmguBlobDetectionOptions
    {
        public int BlobColor { get; set; }
        public bool FilterByColor { get; set; }

        public bool FilterByCircularity { get; set; }
        public float MinCircularity { get; set; }
        public float MaxCircularity { get; set; }

        public bool FilterByInertia;
        public float MinInertia { get; set; }
        public float MaxInertia { get; set; }

        public bool FilterByConvexity;
        public float MinConvexity { get; set; }
        public float MaxConvexity { get; set; }

        public bool FilterByArea { get; set; }
        public int MinArea { get; set; }
        public int MaxArea { get; set; }
    
        public EmguBlobDetectionOptions(int minArea = 50)
        {
            // Set defaults

            BlobColor = 255;
            FilterByColor = true;

            FilterByCircularity = false;
            MinCircularity = 0.5F;
            MaxCircularity = 1F;

            FilterByInertia = false;
            MinInertia = 0.5F;
            MaxInertia = 1F;

            FilterByConvexity = false;
            MinConvexity = 0.5F;
            MaxConvexity = 1F;

            FilterByArea = true;
            MinArea = minArea;
            MaxArea = 5000;
        }

        public SimpleBlobDetectorParams ToSimpleBlobDetectorParams()
        {
            return new SimpleBlobDetectorParams()
            {
                FilterByColor = FilterByColor,
                blobColor = (byte)BlobColor,

                FilterByCircularity = FilterByCircularity,
                MinCircularity = MinCircularity,
                MaxCircularity = MaxCircularity,

                FilterByConvexity = FilterByConvexity,
                MinConvexity = MinConvexity,
                MaxConvexity = MaxConvexity,

                FilterByInertia = FilterByInertia,
                MinInertiaRatio = MinInertia,
                MaxInertiaRatio = MaxInertia,

                FilterByArea = FilterByArea,
                MinArea = MinArea,
                MaxArea = MaxArea
            };
        }
    }
}
