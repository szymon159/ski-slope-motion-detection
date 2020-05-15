using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobDetectionEmguCv
{
    public class BlobDetectionOptions
    {
        public int blobColor = 255;
        public bool filterByColor = true;

        public bool filterByCircularity = false;
        public float minCircularity = 0.5F;
        public float maxCircularity = 1F;

        public bool filterByInertia;
        public float minInertia = 0.5F;
        public float maxInertia = 1F;

        public bool filterByConvexity;
        public float minConvexity = 0.5F;
        public float maxConvexity = 1F;

        public bool filterByArea = true;
        public int minArea = 50;
        public int maxArea = 5000;
    }
}
