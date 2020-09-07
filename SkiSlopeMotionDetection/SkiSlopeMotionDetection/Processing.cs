using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection
{
    static class Processing
    {
        public static Bitmap GetAverage(int frameCount, int startFrame)
        {
            FrameReaderSingleton reader = FrameReaderSingleton.GetInstance();
            (double, double, double)[,] mean = new (double, double, double)[reader.FrameWidth, reader.FrameHeight];
            for (int i = 0; i < frameCount; i++)
            {
                Bitmap frame = reader.GetFrame(startFrame + i);
                unsafe
                {
                    BitmapData bitmapData = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.ReadWrite, frame.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(frame.PixelFormat) / 8;
                    int heightInPixels = bitmapData.Height;
                    int widthInBytes = bitmapData.Width * bytesPerPixel;
                    byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                    Parallel.For(0, heightInPixels, y =>
                    {
                        byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            int oldBlue = currentLine[x];
                            int oldGreen = currentLine[x + 1];
                            int oldRed = currentLine[x + 2];

                            mean[x / bytesPerPixel, y].Item1 += (double)oldBlue / (double)frameCount;
                            mean[x / bytesPerPixel, y].Item2 += (double)oldGreen / (double)frameCount;
                            mean[x / bytesPerPixel, y].Item3 += (double)oldRed / (double)frameCount;
                        }
                    });
                    frame.UnlockBits(bitmapData);
                }
            }

            Bitmap returnBitmap = new Bitmap(reader.FrameWidth, reader.FrameHeight);
            unsafe
            {
                BitmapData bitmapData = returnBitmap.LockBits(new Rectangle(0, 0, returnBitmap.Width, returnBitmap.Height), ImageLockMode.ReadWrite, returnBitmap.PixelFormat);

                int bytesPerPixel = Image.GetPixelFormatSize(returnBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        currentLine[x] = (byte)mean[x / bytesPerPixel, y].Item1;
                        currentLine[x + 1] = (byte)mean[x / bytesPerPixel, y].Item2;
                        currentLine[x + 2] = (byte)mean[x / bytesPerPixel, y].Item3;
                    }
                });
                returnBitmap.UnlockBits(bitmapData);
            }

            return returnBitmap;
        }
    }
}
