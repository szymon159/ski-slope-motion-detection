using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for HeatmapWindow.xaml
    /// </summary>
    public partial class HeatmapWindow : Window
    {
        public Heatmap Heatmap { get; set; }

        public HeatmapWindow(int width, int height)
        {
            Heatmap = new Heatmap(width, height);
            FrameReaderSingleton reader = FrameReaderSingleton.GetInstance();
            Bitmap bm, bm2, bm3;
            int count = (int)(reader.FrameCount / 50);
            for (int i=0; i<count; i++)
            {
                bm = Processing.GetAverage(50, 50*i);

                //for (int j = 0; j < 50; j++)
                //{
                bm2 = reader.GetFrame(50*i);
                bm3 = (BlobDetection.GetDifference(bm, bm2, 30)).ToBitmap();

                EmguBlobDetectionOptions opts = new EmguBlobDetectionOptions(80);
                Emgu.CV.Structure.MKeyPoint[] mKeys = BlobDetection.ReturnBlobs(bm3, opts);
                Heatmap.UpdateSeries(mKeys);
                //}
            }
            InitializeComponent();
        }
    }
}
