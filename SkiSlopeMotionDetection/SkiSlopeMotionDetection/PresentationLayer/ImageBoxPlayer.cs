using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class ImageBoxPlayer : ImageBox
    {
        #region Private variables

        private string _source;
        private int _currentFrame;
        private FrameReaderSingleton _frameReader;
        private BackgroundWorker _frameReaderWorker;
        private long _frameCount;
        private double _frameRate;
        private double _frameTime;
        private BlobDetectionParameters _blobDetectionParams;

        #endregion

        #region Properties

        public Action MediaOpened { get; set; }
        public Action MediaEnded { get; set; }
        public Action<FrameData> FrameChanged { get; set; }
        public bool IsVideoPlaying { get; set; }
        public bool UseOriginalRefreshRate { get; set; } = false;

        public string Source
        {
            get { return _source; }
            set { _source = value; SourceUpdated(); }
        }

        #endregion

        #region Public methods

        public ImageBoxPlayer()
        {
            _frameReaderWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            _frameReaderWorker.DoWork += FrameReaderWorker_DoWork;
            _frameReaderWorker.RunWorkerCompleted += FrameReaderWorker_RunWorkerCompleted;
        }

        public void Play()
        {
            if (Source == null)
                throw new ArgumentException("Unable to play video. No source has been set");

            if (_frameReader == null)
                _frameReader = FrameReaderSingleton.GetInstance(Source);

            if (!_frameReaderWorker.IsBusy)
                _frameReaderWorker.RunWorkerAsync();
        }

        public void Pause()
        {
            if (_frameReaderWorker.IsBusy)
                _frameReaderWorker.CancelAsync();
        }

        public void Stop()
        {
            Pause();

            _currentFrame = 0;
        }

        //public void Rewind()
        //{
        //    if(_currentFrame > 0)
        //        _currentFrame--;
        //}

        //public void FastForward()
        //{
        //    if(_frameReader != null && _currentFrame < _frameCount)
        //        _currentFrame++;
        //}

        public void SetFrameContent(Bitmap bitmap, bool pauseVideo = false)
        {
            if (pauseVideo)
                Pause();

            Image = new Image<Bgr, Byte>(bitmap);
            Invalidate();
        }

        public void UpdateBlobDetectionParameters(BlobDetectionParameters blobDetectionParameters)
        {
            _blobDetectionParams = blobDetectionParameters;
        }

        #endregion

        #region Private methods

        private void SourceUpdated()
        {
            _frameReader = FrameReaderSingleton.GetInstance(Source);
            _frameCount = _frameReader.FrameCount;
            _frameRate = _frameReader.FrameRate;
            _frameTime = 1000 / _frameRate;
            _currentFrame = 0;

            DisplayFirstFrame();
            MediaOpened?.Invoke(); 
        }

        private void DisplayFirstFrame()
        {
            var frame = _frameReader.GetFrame(0);
            SetFrameContent(frame);
        }

        private void FrameReaderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsVideoPlaying = false;
            
            if(!e.Cancelled)
                MediaEnded?.Invoke();
        }

        private void FrameReaderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int countedPeople = 0;
            IsVideoPlaying = true;

            if(_frameReader == null)
                throw new ArgumentException("Unable to fetch next frame. Frame reader has not been set");

            if(_blobDetectionParams.DetectionMethod == DetectionMethod.DiffWithAverage)
            {
                _blobDetectionParams.AvgRangeBegin = _currentFrame / _blobDetectionParams.AvgFramesCount;
                _blobDetectionParams.AverageBitmap = Processing.GetAverage(_blobDetectionParams.AvgFramesCount, _blobDetectionParams.AvgRangeBegin);
            }

            while (_currentFrame < _frameCount)
            {
                var startTime = DateTime.Now;

                if (_frameReaderWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                var frame = _frameReader.GetFrame(_currentFrame);

                // If using original refresh rate, just display the video without running computations
                if(UseOriginalRefreshRate)
                {
                    SetFrameContent(frame);
                    countedPeople = 0;

                    var frameTime = DateTime.Now - startTime;
                    var sleepTime = _frameTime - frameTime.TotalMilliseconds;
                    if (--sleepTime > 0)
                        Thread.Sleep((int)sleepTime);
                }
                else
                {
                    // Update average if necessary
                    if(_blobDetectionParams.DetectionMethod == DetectionMethod.DiffWithAverage 
                        && ((_currentFrame % _blobDetectionParams.AvgFramesCount == 0 && _currentFrame != 0) || _blobDetectionParams.BackgroundBitmap == _blobDetectionParams.AverageBitmap))
                    {
                        _blobDetectionParams.AvgRangeBegin = _currentFrame / _blobDetectionParams.AvgFramesCount;
                        _blobDetectionParams.AverageBitmap = Processing.GetAverage(_blobDetectionParams.AvgFramesCount, _blobDetectionParams.AvgRangeBegin);
                    }

                    var image = BlobDetection.GetResultImage(frame, _blobDetectionParams, out countedPeople);

                    SetFrameContent(image);
                }

                FrameData frameData = new FrameData()
                {
                    CurrentFrame = _currentFrame,
                    CountedPeople = countedPeople
                };
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    var time = DateTime.Now - startTime;
                    frameData.FPS = 1 / time.TotalSeconds;
                    FrameChanged?.Invoke(frameData);
                }, null);

                _currentFrame++;
            }
        }

        #endregion

    }
}
