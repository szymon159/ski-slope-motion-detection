using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiSlopeMotionDetection
{
    public static class ExtensionMethods
    {
        public static ImageFormat ToImageFormat(this string imageFormatExtension)
        {
            switch (imageFormatExtension)
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                case ".exif":
                    return ImageFormat.Exif;
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".tiff":
                    return ImageFormat.Tiff;
                default:
                    throw new ArgumentException($"No image format defined for following extension: {imageFormatExtension}");
            }
        }
    }
}
