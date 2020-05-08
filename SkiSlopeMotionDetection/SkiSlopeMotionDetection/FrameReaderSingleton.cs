using Accord.Video.FFMPEG;
using System;
using System.Drawing;
using System.Threading;

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
                if (instance == null || instance.FilePath != videoPath)
                    instance = new FrameReaderSingleton(videoPath);

                return instance;
            }
        }

        private VideoFileReader reader;
        private FrameReaderSingleton(string videoPath)
        {
            if (instance != null && instance.FilePath == videoPath)
                throw new NotSupportedException("Instance of FrameReaderSingleton already exists. Use FrameReaderSingleton.GetInstance()");
            else if (videoPath == null)
                throw new ArgumentNullException("You must provide a path to video file when creating instance of singleton");

            if (reader != null && reader.IsOpen)
                reader.Close();

            reader = new VideoFileReader();
            reader.Open(videoPath);

            FileIdentifier = Guid.NewGuid();
            FilePath = videoPath;
        }

        public Guid FileIdentifier { get; private set; }
        public string FilePath { get; private set; }
        public int FrameWidth => reader.Width;
        public int FrameHeight => reader.Height;
        public long FrameCount => reader.FrameCount;
        public double FrameRate => reader.FrameRate.Value;
        public Bitmap GetFrame(int frameIndex)
        {
            Bitmap result = null;

            if (frameIndex > FrameCount || frameIndex < 0)
                throw new ArgumentOutOfRangeException($"Unable to get frame number: {frameIndex}. File has only {FrameCount} frames");

            while (true)
            {
                try
                {
                    result = reader.ReadVideoFrame(frameIndex);
                    break;
                }
                catch
                {
                    // If we get to many frames without disposing previous ones need to wait for garbage collector as we run out of memory
                    Thread.Sleep(50);
                }
            }

            return result;
        }
    }
}
