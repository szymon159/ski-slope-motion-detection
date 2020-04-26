using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Drawing;
namespace BlobDetectionEmguCv
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            string path = "blob.jpg";
            Bitmap image = (Bitmap)Bitmap.FromFile(path);
            Image<Bgr, byte> image1 = new Image<Bgr, byte>(image);

            Mat img = image1.Mat; //new Mat(path,Emgu.CV.CvEnum.LoadImageType.Unchanged);
            stopwatch.Start();
     
            Emgu.CV.Structure.MKeyPoint[] mKeys = Program.ReturnBlobs(img, 255, true, true, false, false, false, 50, 5000);

            int count = mKeys.Length;
            Mat im_with_keypoints = new Mat();

            Features2DToolbox.DrawKeypoints(img, new VectorOfKeyPoint(mKeys), im_with_keypoints, new Bgr(0, 0, 255), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);

            stopwatch.Stop();
            Console.WriteLine("Elapsed time is {0} miliseconds", stopwatch.ElapsedMilliseconds);
            CvInvoke.NamedWindow("Window", NamedWindowType.Normal);
            CvInvoke.Imshow("Window", im_with_keypoints);
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


    }
}
