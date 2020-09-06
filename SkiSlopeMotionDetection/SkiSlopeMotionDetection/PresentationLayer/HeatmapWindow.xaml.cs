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
    public partial class HeatmapWindow : Window, INotifyPropertyChanged
    {
        private Heatmap heatmap;

        public Heatmap Heatmap
        {
            get { return heatmap; }
            set { heatmap = value; NotifyPropertyChanged(); }
        }

        public HeatmapWindow(int width, int height)
        {
            heatmap = new Heatmap(width, height);
            Bitmap bm = Processing.GetAverage(400, 1000);
            FrameReaderSingleton reader = FrameReaderSingleton.GetInstance();
            for (int i=0; i<100; i++)
            {
                Bitmap bm2 = reader.GetFrame(900+i);
                Bitmap bm3 = (BlobDetection.GetDifference(bm, bm2, 30)).ToBitmap();

                BlobDetectionOptions opts = new BlobDetectionOptions(80);
                Emgu.CV.Structure.MKeyPoint[] mKeys = BlobDetection.ReturnBlobs(bm3, opts);
                Heatmap.UpdateSeries(mKeys);
            }
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
