using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;


namespace SkiSlopeMotionDetection
{
    public static class BlobDetection
    {
        public static MKeyPoint[] ReturnBlobs(Mat img, EmguBlobDetectionOptions opt)
        {
            var par = opt.ToSimpleBlobDetectorParams();
            SimpleBlobDetector detector = new SimpleBlobDetector(par);

            return detector.Detect(img);
        }

        public static MKeyPoint[] ReturnBlobs(Image<Bgr,byte> img, EmguBlobDetectionOptions opt)
        {
            return ReturnBlobs(img.Mat, opt);
        }

        public static MKeyPoint[] ReturnBlobs(Bitmap bm1, EmguBlobDetectionOptions opt)
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

        public static Bitmap GetResultImage(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount)
        {
            switch (detectionParams.DetectionMethod)
            {
                case DetectionMethod.DiffWithAverage:
                    return GetImageByDiffWithAverage(sourceBitmap, detectionParams, out blobsCount);
                default:
                    throw new ArgumentException($"No method has been implemented for {detectionParams.DetectionMethod}");
            }
        }

        private static Bitmap GetImageByDiffWithAverage(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount)
        {
            if (detectionParams.BlobDetectionOptions == null)
                throw new ArgumentException("Unable to get blobs, blob detection options must be specified");

            Bitmap average = detectionParams.AverageBitmap;
            if(average == null)
            {
                if (!detectionParams.AvgRangeBegin.HasValue || !detectionParams.AvgRangeEnd.HasValue)
                    throw new ArgumentException("Unable to get average frame, either bitmap or ranges have to be specified");

                average = Processing.GetAverage(detectionParams.AvgRangeBegin.Value, detectionParams.AvgRangeEnd.Value);
            }

            Bitmap difference = (GetDifference(average, sourceBitmap, detectionParams.DifferenceThreshold)).ToBitmap();
            MKeyPoint[] mKeys = ReturnBlobs(difference, detectionParams.BlobDetectionOptions);
            blobsCount = mKeys.Length;

            Bitmap result = sourceBitmap;
            if (detectionParams.MarkBlobs)
            {
                Mat imWithKeypoints = new Mat();
                Image<Bgr, byte> im2 = new Image<Bgr, byte>(sourceBitmap);
                Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), imWithKeypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                MemoryStream ms = new MemoryStream();

                Bitmap final = (imWithKeypoints.ToImage<Bgr, byte>()).ToBitmap();
                final.Save(ms, ImageFormat.Bmp);
                var image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();

                result = new Bitmap(image.StreamSource);
            }

            return result;
        }
    }
}
