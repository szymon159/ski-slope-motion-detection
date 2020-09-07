using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection
{
    public class AverageFrameSingleton
    {
        private static AverageFrameSingleton _instance = null;
        private static readonly object _padlock = new object();
        public static AverageFrameSingleton GetInstance()
        {
            lock (_padlock)
            {
                if (_instance == null)
                    _instance = new AverageFrameSingleton();

                return _instance;
            }
        }

        public static void InitializeNewInstance()
        {
            _instance = new AverageFrameSingleton();
        }

        public Bitmap GetAverageBitmap()
        {
            if (bitmaps.Count == 0)
                return new Bitmap(frameWidth, frameHeight);
            if (hasChanged)
            {
                Bitmap returnBitmap = new Bitmap(frameWidth, frameHeight);
                unsafe
                {
                    BitmapData bitmapData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.ReadWrite, returnBitmap.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(returnBitmap.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                    int frameCount = bitmaps.Count;

                    Parallel.For(0, heightInPixels, y =>
                    {
                        byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            int index = x / bytesPerPixel;
                            currentLine[x] = (byte)(mean[index, y].Item1 / frameCount);
                            currentLine[x + 1] = (byte)(mean[index, y].Item2 / frameCount);
                            currentLine[x + 2] = (byte)(mean[index, y].Item3 / frameCount);
                        }
                    });
                    returnBitmap.UnlockBits(bitmapData);
                }
                hasChanged = false;
                lastAverage = returnBitmap;
                return returnBitmap;
            }
            return lastAverage;
        }

        public void AddFrame(Bitmap bitmap)
        {
            hasChanged = true;
            if (bitmaps.Count >= DesiredSize)
            {
                DeleteFrameFromMean(bitmaps.First.Value);
                bitmaps.RemoveFirst();
            }

            bitmaps.AddLast(bitmap);
            AddFrameToMean(bitmap);
        }

        private int DesiredSize = 80;

        private LinkedList<Bitmap> bitmaps = new LinkedList<Bitmap>();

        private (long, long, long)[,] mean;

        private int frameWidth;

        private int frameHeight;

        private bool hasChanged = true;

        private Bitmap lastAverage = null;

        private AverageFrameSingleton()
        {
            var reader = FrameReaderSingleton.GetInstance();
            this.frameWidth = reader.FrameWidth;
            this.frameHeight = reader.FrameHeight;
            mean = new (long, long, long)[frameWidth, frameHeight];
            for (int i = 0; i < frameWidth; i++)
            {
                for (int j = 0; j < frameHeight; j++)
                {
                    mean[i, j] = (0, 0, 0);
                }
            }
        }

        private void DeleteFrameFromMean(Bitmap bitmap)
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

                        mean[index, y].Item1 -= oldBlue;
                        mean[index, y].Item2 -= oldGreen;
                        mean[index, y].Item3 -= oldRed;
                    }
                });
                bitmap.UnlockBits(bitmapData);
            }
        }

        private void AddFrameToMean(Bitmap bitmap)
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

                        mean[index, y].Item1 += oldBlue;
                        mean[index, y].Item2 += oldGreen;
                        mean[index, y].Item3 += oldRed;
                    }
                });
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
