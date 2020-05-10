﻿using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;


namespace BlobDetectionEmguCv
{
    static class BlobDetection
    {
        public static MKeyPoint[] ReturnBlobs(Mat img, BlobDetectionOptions opt)
        {
            var par = new SimpleBlobDetectorParams();

            par.FilterByColor = opt.filterByColor;
            par.blobColor = (byte)opt.blobColor;

            par.FilterByCircularity = opt.filterByCircularity;
            par.MinCircularity = opt.minCircularity;
            par.MaxCircularity = opt.maxCircularity;

            par.FilterByConvexity = opt.filterByConvexity;
            par.MinConvexity = opt.minConvexity;
            par.MaxConvexity = opt.maxConvexity;

            par.FilterByInertia = opt.filterByInertia;
            par.MinInertiaRatio = opt.minInertia;
            par.MaxInertiaRatio = opt.maxInertia;

            par.FilterByArea = opt.filterByArea;
            par.MinArea = opt.minArea;
            par.MaxArea = opt.maxArea;

            SimpleBlobDetector detector = new SimpleBlobDetector(par);

            return detector.Detect(img);
        }
        public static MKeyPoint[] ReturnBlobs(Image<Bgr,byte> img, BlobDetectionOptions opt)
        {
            return ReturnBlobs(img.Mat, opt);
        }
        public static MKeyPoint[] ReturnBlobs(Bitmap bm1, BlobDetectionOptions opt)
        {
            Image<Bgr, byte> image1 = new Image<Bgr, byte>(bm1);
            Mat img = image1.Mat;

            return ReturnBlobs(img, opt);
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