using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;


namespace SkiSlopeMotionDetection
{
    public static class BlobDetection
    {
        public static MKeyPoint[] ReturnBlobs(Mat img, BlobDetectionOptions opt)
        {
            var par = opt.ToSimpleBlobDetectorParams();
            SimpleBlobDetector detector = new SimpleBlobDetector(par);

            return detector.Detect(img);
        }
        public static MKeyPoint[] ReturnBlobs(Image<Bgr,byte> img, BlobDetectionOptions opt)
        {
            return ReturnBlobs(img.Mat, opt);
        }
        public static MKeyPoint[] ReturnBlobs(Bitmap bm1, BlobDetectionOptions opt)
        {
            Image<Bgr, byte> im1 = new Image<Bgr, byte>(bm1);
            Mat mat = im1.Mat;

            return ReturnBlobs(mat, opt);
        }
        public static Image<Bgr, byte> GetDifference(string path1, string path2, int threshold)
        {
            Image<Bgr, byte> im1 = (CvInvoke.Imread(path1, LoadImageType.Unchanged)).ToImage<Bgr, byte>();
            Image<Bgr, byte> im2 = (CvInvoke.Imread(path2, LoadImageType.Unchanged)).ToImage<Bgr, byte>();
            Image<Bgr, byte> diff = im1.AbsDiff(im2);
            diff = diff.ThresholdBinary(new Bgr(threshold, threshold, threshold), new Bgr(255, 255, 255));
            return diff;
        }
        public static Image<Bgr, byte> GetDifference(Bitmap bm1, Bitmap bm2, int threshold)
        {
            Image<Bgr, byte> im1 = new Image<Bgr, byte>(bm1);
            Image<Bgr, byte> im2 = new Image<Bgr, byte>(bm2);
            Image<Bgr, byte> diff = im1.AbsDiff(im2);
            diff = diff.ThresholdBinary(new Bgr(threshold, threshold, threshold), new Bgr(255, 255, 255));
            return diff;
        }
    }
}
