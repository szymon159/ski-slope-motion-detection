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
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //string path = "blob.jpg";
            //Bitmap image = (Bitmap)Bitmap.FromFile(path);
            //Image<Bgr, byte> image1 = new Image<Bgr, byte>(image);

            Image<Bgr, byte> diff = GetDifference("imagediff1.jpg", "imagediff2.jpg");

            //Mat img = image1.Mat; //new Mat(path,Emgu.CV.CvEnum.LoadImageType.Unchanged);
            Mat img = diff.Mat;
            stopwatch.Start();
     
            Emgu.CV.Structure.MKeyPoint[] mKeys = Program.ReturnBlobs(img, 255, true, true, false, false, false, 50, 5000);
            Image<Bgr, byte> im2 = (CvInvoke.Imread("imagediff2.jpg", LoadImageType.Unchanged)).ToImage<Bgr, byte>();

            int count = mKeys.Length;
            Mat im_with_keypoints = new Mat();

            //Features2DToolbox.DrawKeypoints(image, new VectorOfKeyPoint(mKeys), im_with_keypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
            Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), im_with_keypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

            stopwatch.Stop();
            Console.WriteLine("Elapsed time is {0} miliseconds", stopwatch.ElapsedMilliseconds);

            // Image<Bgr, byte> diff = GetDifference("imagediff1.jpg", "imagediff2.jpg");
            List<Image<Bgr, byte >> frames = GetVideoFrames("videosample.mp4");
            // VectorOfMat mats = new VectorOfMat();
            // foreach (var el in frames)
            //    mats.Push(el.Mat);
            CvInvoke.NamedWindow("Window", NamedWindowType.Normal);
            CvInvoke.Imshow("Window", im_with_keypoints);
            //CvInvoke.Imshow("Window", frames[0].Mat);
           
            // CvInvoke.Mean((IInputArray)new InputArray(mats));
            CvInvoke.WaitKey(0);
        }

        static MKeyPoint[] ReturnBlobs(Mat img, int blobColor, bool filterByColor, bool filterByArea, bool filterByCircularity, bool filterByInertia, bool filterByConvexity,
                                               int minArea, int maxArea)
        {
            var par = new SimpleBlobDetectorParams();

            par.FilterByColor = true;
            par.blobColor = (byte)blobColor;
            par.FilterByCircularity = filterByCircularity;
            par.FilterByCircularity = filterByCircularity;
            par.FilterByConvexity = filterByConvexity;
            par.FilterByInertia = filterByInertia;
            par.FilterByArea = filterByArea;
            par.MinArea = minArea;
            par.MaxArea = maxArea;
            SimpleBlobDetector detector = new SimpleBlobDetector(par);

            return detector.Detect(img);
        }

        static Image<Bgr, byte> GetDifference(string path1, string path2)
        {
            Image<Bgr, byte> im1 = (CvInvoke.Imread(path1, LoadImageType.Unchanged)).ToImage<Bgr,byte>();
            Image<Bgr, byte> im2 = (CvInvoke.Imread(path2, LoadImageType.Unchanged)).ToImage<Bgr, byte>();
            Image<Bgr, byte> diff = im1.AbsDiff(im2);
            diff = diff.ThresholdBinary(new Bgr(30, 30, 30), new Bgr(255, 255, 255));
            return diff;
         }

        static List<Image<Bgr, Byte>> GetVideoFrames(String Filename)
        {
            List<Image<Bgr, Byte>> image_array = new List<Image<Bgr, Byte>>();
            Capture _capture = new Capture(Filename);

            bool Reading = true;
            int count = 0;
            int skip = 0;
            while (Reading && count<100)
            {
                if (skip > 4)
                {
                    skip = 0;
                    Image<Bgr, Byte> frame = _capture.QueryFrame().ToImage<Bgr, byte>();
                    if (frame != null)
                    {
                        image_array.Add(frame.Copy());
                    }
                    else
                    {
                        Reading = false;
                    }
                    count++;
                }
                skip++;
            }

            return image_array;
        }
    }
}
