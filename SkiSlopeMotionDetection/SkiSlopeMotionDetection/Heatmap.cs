using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Emgu.CV.Structure;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class Heatmap : INotifyPropertyChanged
    {
        private PlotModel heatMap;
        public PlotModel HeatMap
        {
            get { return heatMap; }
            set { heatMap = value; NotifyPropertyChanged(); }
        }

        public string Title
        {
            get { return heatMap.Title; }
            set { heatMap.Title = value; }
        }

        private HeatMapSeries series;

        public HeatMapSeries Series
        {
            get { return series; }
            set { series = value; NotifyPropertyChanged(); }
        }

        public Heatmap(int width, int height)
        {
            heatMap = new PlotModel
            {
                Title = "Areas used by skiers",
                PlotType = PlotType.XY,
                Background = OxyColors.White,
                IsLegendVisible = false,
            };

            Series = new HeatMapSeries
            {
                Title = "HMSeries",
                Interpolate = true,
                X0 = 0,
                X1 = width,
                Y0 = 0,
                Y1 = height
            };

            Series.Data = new double[width, height];
            heatMap.Series.Add(Series);

            heatMap.Axes.Add(new LinearColorAxis
            {
                Position = AxisPosition.Right,
                IsAxisVisible = false,
            });

        }

        public void UpdateSeries(MKeyPoint[] keyPoints)
        {
            int blobrange = 5;
            for(int i=0; i<keyPoints.Count(); i++)
            {
                for(int j=0; j<blobrange; j++)
                {
                    for (int k = 0; k < blobrange; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobrange + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobrange + 1.0 - dist) / (blobrange + 1.0);
                        if ((int)keyPoints[i].Point.X - blobrange + k > Series.Data.GetLength(0) || (int)keyPoints[i].Point.Y - j + k > Series.Data.GetLength(1)) break;
                        if ((int)keyPoints[i].Point.Y - j + k < 0 || (int)keyPoints[i].Point.X - blobrange + k < 0) continue;
                        Series.Data[(int)keyPoints[i].Point.X - blobrange + k, (int)keyPoints[i].Point.Y - j + k] += val;
                    }
                }
            }
            heatMap.InvalidatePlot(true);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
