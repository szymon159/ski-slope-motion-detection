using Accord.Video.FFMPEG;
using System;
using System.Drawing;
using System.Threading;

namespace SkiSlopeMotionDetection
{
    public class FrameReaderSingleton
    {
        private static FrameReaderSingleton _instance = null;
        private static readonly object _padlock = new object();
        public static FrameReaderSingleton GetInstance(string videoPath = null)
        {
            lock (_padlock)
            {
                if (_instance == null || _instance.FilePath != videoPath)
                    _instance = new FrameReaderSingleton(videoPath);

                return _instance;
            }
        }

        private VideoFileReader _reader;
        private FrameReaderSingleton(string videoPath)
        {
            if (_instance != null && _instance.FilePath == videoPath)
                throw new NotSupportedException("Instance of FrameReaderSingleton already exists. Use FrameReaderSingleton.GetInstance()");
            else if (videoPath == null)
                throw new ArgumentNullException("You must provide a path to video file when creating instance of singleton");

            if (_reader != null && _reader.IsOpen)
                _reader.Close();

            _reader = new VideoFileReader();
            _reader.Open(videoPath);

            UpdateProperties();
            FileIdentifier = Guid.NewGuid();
            FilePath = videoPath;
        }
        private void UpdateProperties()
        {
            if (_reader == null)
            {
                _reader = new VideoFileReader();
                _reader.Open(FilePath);
            }

            FrameWidth = _reader.Width;
            FrameHeight = _reader.Height;
            FrameCount = _reader.FrameCount;
            FrameRate = _reader.FrameRate.Value;
        }

        public Guid FileIdentifier { get; private set; }
        public string FilePath { get; private set; }
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        public long FrameCount { get; private set; }
        public double FrameRate { get; private set; }
        public Bitmap GetFrame(int frameIndex)
        {
            Bitmap result = null;

            if (frameIndex > FrameCount || frameIndex < 0)
                throw new ArgumentOutOfRangeException($"Unable to get frame number: {frameIndex}. File has only {FrameCount} frames");

            if(_reader == null)
            {
                _reader = new VideoFileReader();
                _reader.Open(FilePath);
            }

            while (true)
            {
                try
                {
                    result = _reader.ReadVideoFrame(frameIndex);
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
