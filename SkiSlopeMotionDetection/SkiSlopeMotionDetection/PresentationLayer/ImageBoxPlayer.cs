using Accord;
using Accord.Math;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class ImageBoxPlayer : ImageBox
    {
        private string _source;
        private int _currentFrame;
        private FrameReaderSingleton _frameReader;
        private BackgroundWorker _frameReaderWorker;
        private long _frameCount;
        private double _frameRate;

        public string Source 
        { 
            get { return _source; } 
            set { _source = value; _frameReader = FrameReaderSingleton.GetInstance(Source); MediaOpened?.Invoke(); DisplayFirstFrame(); } 
        }
        public Action MediaOpened { get; set; }
        public Action MediaEnded { get; set; }
        public Action<FrameData> FrameChanged { get; set; }
        public bool IsVideoPlaying { get; set; }
        public bool UseOriginalRefreshRate { get; set; }

        public ImageBoxPlayer()
        {
            _currentFrame = 0;
            _frameReaderWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _frameReaderWorker.DoWork += FrameReaderWorker_DoWork;
            _frameReaderWorker.RunWorkerCompleted += FrameReaderWorker_RunWorkerCompleted;
            //_worker.ProgressChanged += _worker_ProgressChanged;
        }

        public void Play()
        {
            if (Source == null)
                throw new ArgumentException("Unable to play video. No source has been set");

            if (_frameReader == null)
                _frameReader = FrameReaderSingleton.GetInstance(Source);

            _frameCount = _frameReader.FrameCount;
            _frameRate = _frameReader.FrameRate;
            if (!_frameReaderWorker.IsBusy)
                _frameReaderWorker.RunWorkerAsync();

            IsVideoPlaying = true;
        }

        public void Pause()
        {
            if (_frameReaderWorker.IsBusy)
                _frameReaderWorker.CancelAsync();

            IsVideoPlaying = false;
        }

        public void Stop()
        {
            if (_frameReaderWorker.IsBusy)
                _frameReaderWorker.CancelAsync();

            _currentFrame = 0;
            IsVideoPlaying = false;
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

        //public void SetFrameContent(BitmapImage bitmapImage, bool pauseVideo = false)
        //{
        //    if(pauseVideo)
        //        Pause();

        //    Image = new Image<Bgr, Byte>(new Bitmap(bitmapImage.StreamSource));
        //    Invalidate();
        //}

        public void SetFrameContent(Bitmap bitmap, bool pauseVideo = false)
        {
            if (pauseVideo)
                Pause();

            Image = new Image<Bgr, Byte>(bitmap);
            Invalidate();
        }

        private void DisplayFirstFrame()
        {
            var frame = _frameReader.GetFrame(0);
            SetFrameContent(frame);
        }

        //private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void FrameReaderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsVideoPlaying = false;
            
            if(!e.Cancelled)
                MediaEnded?.Invoke();
        }

        private void FrameReaderWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if(_frameReader == null)
                throw new ArgumentException("Unable to fetch next frame. Frame reader has not been set");

            while (_currentFrame < _frameCount)
            {
                if (_frameReaderWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                var startTime = DateTime.Now;
                var frame = _frameReader.GetFrame(_currentFrame);
                SetFrameContent(frame);

                FrameData frameData = new FrameData()
                {
                    CurrentFrame = _currentFrame
                };

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    var time = DateTime.Now - startTime;
                    frameData.FPS = 1 / time.TotalSeconds;
                    FrameChanged?.Invoke(frameData);
                }, null);

                _currentFrame++;
            }
        }
    }
}
