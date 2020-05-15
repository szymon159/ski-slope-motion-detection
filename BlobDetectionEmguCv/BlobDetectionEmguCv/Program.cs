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

namespace BlobDetectionEmguCv
{
    class Program
    {
        //Example of usage:
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Image<Bgr, byte> diff = BlobDetection.GetDifference((Bitmap)Bitmap.FromFile("imagediff1.jpg"), (Bitmap)Bitmap.FromFile("imagediff2.jpg"),30);
            stopwatch.Start();

            BlobDetectionOptions opts = new BlobDetectionOptions();
            Emgu.CV.Structure.MKeyPoint[] mKeys = BlobDetection.ReturnBlobs(diff,opts);
            Image<Bgr, byte> im2 = (CvInvoke.Imread("imagediff2.jpg", LoadImageType.Unchanged)).ToImage<Bgr, byte>();

            int count = mKeys.Length;
            Mat im_with_keypoints = new Mat();
            Features2DToolbox.DrawKeypoints(im2, new VectorOfKeyPoint(mKeys), im_with_keypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

            stopwatch.Stop();
            Console.WriteLine("Elapsed time is {0} miliseconds", stopwatch.ElapsedMilliseconds);

            CvInvoke.NamedWindow("Window", NamedWindowType.Normal);
            CvInvoke.Imshow("Window", im_with_keypoints);
            CvInvoke.WaitKey(0);
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
