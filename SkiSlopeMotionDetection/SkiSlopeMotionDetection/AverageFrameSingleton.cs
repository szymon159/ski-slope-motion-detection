using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection
{
    public class AverageFrameSingleton
    {
        private static AverageFrameSingleton _instance = null;
        private static readonly object _padlock = new object();

        public static AverageFrameSingleton GetInstance(bool generateNew = false)
        {
            lock (_padlock)
            {
                if (_instance == null || generateNew == true)
                    _instance = new AverageFrameSingleton();

                return _instance;
            }
        }

        public Bitmap GetAverageBitmap()
        {
            if (_bitmaps.Count == 0)
                return new Bitmap(_frameWidth, _frameHeight);
            if (_hasChanged)
            {
                Bitmap returnBitmap = new Bitmap(_frameWidth, _frameHeight);
                unsafe
                {
                    BitmapData bitmapData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.ReadWrite, returnBitmap.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(returnBitmap.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                    int frameCount = _bitmaps.Count;

                    Parallel.For(0, heightInPixels, y =>
                    {
                        byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            int index = x / bytesPerPixel;
                            currentLine[x] = (byte)(_mean[index, y].Item1 / frameCount);
                            currentLine[x + 1] = (byte)(_mean[index, y].Item2 / frameCount);
                            currentLine[x + 2] = (byte)(_mean[index, y].Item3 / frameCount);
                        }
                    });
                    returnBitmap.UnlockBits(bitmapData);
                }
                _hasChanged = false;
                _lastAverage = returnBitmap;
                return returnBitmap;
            }
            return _lastAverage;
        }

        public void AddFrame(Bitmap bitmap)
        {
            _hasChanged = true;
            if (_bitmaps.Count >= _desiredSize)
            {
                UpdateMean(_bitmaps.First.Value, false);
                _bitmaps.RemoveFirst();
            }

            _bitmaps.AddLast(bitmap);
            UpdateMean(bitmap, true);
        }

        private int _desiredSize = 80;

        private LinkedList<Bitmap> _bitmaps = new LinkedList<Bitmap>();

        private (long, long, long)[,] _mean;

        private int _frameWidth;

        private int _frameHeight;

        private bool _hasChanged = true;

        private Bitmap _lastAverage = null;

        private AverageFrameSingleton()
        {
            var reader = FrameReaderSingleton.GetInstance();
            this._frameWidth = reader.FrameWidth;
            this._frameHeight = reader.FrameHeight;
            _mean = new (long, long, long)[_frameWidth, _frameHeight];
            for (int i = 0; i < _frameWidth; i++)
            {
                for (int j = 0; j < _frameHeight; j++)
                {
                    _mean[i, j] = (0, 0, 0);
                }
            }
        }

        private void UpdateMean(Bitmap bitmap, bool opPlus)
        {
            unsafe
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        byte oldBlue = currentLine[x];
                        byte oldGreen = currentLine[x + 1];
                        byte oldRed = currentLine[x + 2];
                        int index = x / bytesPerPixel;

                        if (opPlus)
                        {
                            _mean[index, y].Item1 += oldBlue;
                            _mean[index, y].Item2 += oldGreen;
                            _mean[index, y].Item3 += oldRed;
                        }
                        else
                        {
                            _mean[index, y].Item1 -= oldBlue;
                            _mean[index, y].Item2 -= oldGreen;
                            _mean[index, y].Item3 -= oldRed;
                        }
                    }
                });
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
