﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace SkiSlopeMotionDetection
{
    public static class BlobDetection
    {
        public static MKeyPoint[] ReturnBlobs(Mat img, EmguBlobDetectionOptions opt)
        {
            var par = opt.ToSimpleBlobDetectorParams();
            using (var detector = new SimpleBlobDetector(par))
            {
                return detector.Detect(img);
            }
        }

        public static MKeyPoint[] ReturnBlobs(Image<Bgr, byte> img, EmguBlobDetectionOptions opt)
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
            var im1 = (CvInvoke.Imread(path1, LoadImageType.Unchanged)).ToImage<Bgr, byte>();
            var im2 = (CvInvoke.Imread(path2, LoadImageType.Unchanged)).ToImage<Bgr, byte>();

            return GetDifference(im1, im2, threshold);
        }

        public static Image<Bgr, byte> GetDifference(Bitmap bm1, Bitmap bm2, int threshold)
        {
            Image<Bgr, byte> im1 = new Image<Bgr, byte>(bm1);
            Image<Bgr, byte> im2 = new Image<Bgr, byte>(bm2);

            return GetDifference(im1, im2, threshold);
        }

        public static Image<Bgr, byte> GetDifference(Image<Bgr, byte> im1, Image<Bgr, byte> im2, int threshold)
        {
            if (im1.Height != im2.Height || im1.Width != im2.Width)
                throw new ArgumentException("Both video and background image must have the same size");

            Image<Bgr, byte> diff = im1.AbsDiff(im2);
            diff = diff.ThresholdBinary(new Bgr(threshold, threshold, threshold), new Bgr(255, 255, 255));
            return diff;
        }

        public static Bitmap GetResultImage(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount)
        {
            return GetResultImage(sourceBitmap, detectionParams, out blobsCount, out _);
        }

        public static Bitmap GetResultImage(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount, out MKeyPoint[] keyPoints)
        {
            switch (detectionParams.DetectionMethod)
            {
                case DetectionMethod.DiffWithBackground:
                    return GetImageByDiffWithBackground(sourceBitmap, detectionParams, out blobsCount, out keyPoints);

                case DetectionMethod.DiffWithAverage:
                    return GetImageByDiffWithAverage(sourceBitmap, detectionParams, out blobsCount, out keyPoints);

                case DetectionMethod.Naive:
                    return GetImageBySimpleMethod(sourceBitmap, detectionParams, out blobsCount, out keyPoints);

                default:
                    throw new ArgumentException($"No method has been implemented for {detectionParams.DetectionMethod}");
            }
        }

        private static Bitmap GetImageByDiffWithBackground(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount, out MKeyPoint[] keyPoints)
        {
            detectionParams.AverageBitmap = detectionParams.BackgroundBitmap;

            return GetImageByDiff(sourceBitmap, detectionParams, out blobsCount, out keyPoints);
        }

        private static Bitmap GetImageByDiffWithAverage(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount, out MKeyPoint[] keyPoints)
        {
            var avgCounter = AverageFrameSingleton.GetInstance();
            if (detectionParams.AddFrameToAverage)
            {
                avgCounter.AddFrame(sourceBitmap);
                detectionParams.AddFrameToAverage = false;
            }
            detectionParams.AverageBitmap = avgCounter.GetAverageBitmap();

            return GetImageByDiff(sourceBitmap, detectionParams, out blobsCount, out keyPoints);
        }

        private static Bitmap GetImageByDiff(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount, out MKeyPoint[] keyPoints)
        {
            if (detectionParams.BlobDetectionOptions == null)
                throw new ArgumentException("Unable to get blobs, blob detection options must be specified");

            if (detectionParams.AverageBitmap == null)
                throw new ArgumentException("Unable to get difference, no image to compare");

            Bitmap difference = GetDifference(detectionParams.AverageBitmap, sourceBitmap, detectionParams.DifferenceThreshold).ToBitmap();
            MKeyPoint[] mKeys = ReturnBlobs(difference, detectionParams.BlobDetectionOptions);
            blobsCount = mKeys.Length;

            Bitmap result = sourceBitmap;
            if (detectionParams.MarkBlobs)
            {
                Mat imWithKeypoints = new Mat();
                Image<Bgr, byte> im2 = new Image<Bgr, byte>(sourceBitmap);
                Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), imWithKeypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

                result = (imWithKeypoints.ToImage<Bgr, byte>()).ToBitmap();
            }

            keyPoints = detectionParams.GetKeyPoints ? mKeys : null;
            return result;
        }

        private static Bitmap GetImageBySimpleMethod(Bitmap sourceBitmap, BlobDetectionParameters detectionParams, out int blobsCount, out MKeyPoint[] keyPoints)
        {
            if (detectionParams.BlobDetectionOptions == null)
                throw new ArgumentException("Unable to get blobs, blob detection options must be specified");

            var conv_image = new Image<Bgr, byte>(sourceBitmap);
            var hsv_img = conv_image.Convert<Hsv, byte>();
            var diff = hsv_img.ThresholdBinaryInv(new Hsv(detectionParams.HueHSV, detectionParams.SaturationHSV, detectionParams.ValueHSV), new Hsv(0, 0, 255));

            var conv_diff = diff.Convert<Bgr, byte>();
            MKeyPoint[] mKeys = ReturnBlobs(conv_diff, detectionParams.BlobDetectionOptions);
            blobsCount = mKeys.Length;

            Bitmap result = sourceBitmap;
            if (detectionParams.MarkBlobs)
            {
                Mat imWithKeypoints = new Mat();
                Image<Bgr, byte> im2 = new Image<Bgr, byte>(sourceBitmap);
                Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), imWithKeypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                result = (imWithKeypoints.ToImage<Bgr, byte>()).ToBitmap();
            }

            keyPoints = detectionParams.GetKeyPoints ? mKeys : null;
            return result;
        }
    }
}
