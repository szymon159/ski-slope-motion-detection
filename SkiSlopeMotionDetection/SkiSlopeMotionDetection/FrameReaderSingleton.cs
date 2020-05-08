using System;

namespace SkiSlopeMotionDetection
{
    public class FrameReaderSingleton
    {
        private static FrameReaderSingleton instance = null;
        private static readonly object padlock = new object();
        public static FrameReaderSingleton GetInstance(string videoPath = null)
        {
            lock (padlock)
            {
                if (instance == null)
                    instance = new FrameReaderSingleton(videoPath);

                return instance;
            }
        }

        public string VideoPath { get; set; }
        private FrameReaderSingleton(string videoPath)
        {
            if (instance != null)
                throw new NotSupportedException("Instance of FrameReaderSingleton already exists. Use FrameReaderSingleton.GetInstance()");
            else if (videoPath == null)
                throw new ArgumentNullException("You must provide a path to video file when creating instance of singleton");

            VideoPath = videoPath;            
        }
    }
}
