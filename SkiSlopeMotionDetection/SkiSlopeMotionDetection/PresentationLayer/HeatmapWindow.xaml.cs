using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Reflection;
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
using System.IO;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    /// <summary>
    /// Interaction logic for HeatmapWindow.xaml
    /// </summary>
    public partial class HeatmapWindow : Window
    {
        public Heatmap HeatMap { get; set; }

        public HeatmapWindow(int width, int height)
        {
            int framesToAvg=50;
            HeatMap = new Heatmap(width, height);
            FrameReaderSingleton reader = FrameReaderSingleton.GetInstance();
            Bitmap bm, bm2, bm3;
            int count = (int)(reader.FrameCount / framesToAvg);
            for (int i=0; i< framesToAvg; i++)
            {
                bm = Processing.GetAverage(framesToAvg, framesToAvg * i + framesToAvg/2);
                bm2 = reader.GetFrame(framesToAvg * i);
                bm3 = (BlobDetection.GetDifference(bm, bm2, 60)).ToBitmap();

                EmguBlobDetectionOptions opts = new EmguBlobDetectionOptions(100);
                Emgu.CV.Structure.MKeyPoint[] mKeys = BlobDetection.ReturnBlobs(bm3, opts);
                HeatMap.UpdateSeries(mKeys);
 
            }
            ImageConverter converter = new ImageConverter();
            HeatMap.HeatMap.Annotations.Add(new OxyPlot.Annotations.ImageAnnotation
            { 
                ImageSource = new OxyPlot.OxyImage((byte[])converter.ConvertTo(reader.GetFrame(1), typeof(byte[]))),
                Opacity = 0.2,
                X = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Y = new PlotLength(0.5, PlotLengthUnit.RelativeToPlotArea),
                Width = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea),
                Height = new PlotLength(1, PlotLengthUnit.RelativeToPlotArea)
            });;
            HeatMap.HeatMap.InvalidatePlot(true);

            HeatMap.saveToFile("heatmap.png");
            InitializeComponent();
        }
    }
}
