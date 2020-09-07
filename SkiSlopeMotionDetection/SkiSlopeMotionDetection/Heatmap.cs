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
using OxyPlot.Wpf;
using HeatMapSeries = OxyPlot.Series.HeatMapSeries;

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

        private OxyPlot.Series.HeatMapSeries series;

        public OxyPlot.Series.HeatMapSeries Series
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

            Series = new OxyPlot.Series.HeatMapSeries
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

            heatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });
            heatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false
            });
            heatMap.Axes.Add(new OxyPlot.Axes.LinearColorAxis
            {
                Position = AxisPosition.Right,
                IsAxisVisible = false,
            });

        }

        public void UpdateSeries(MKeyPoint[] keyPoints)
        {
            int blobrange;
            for(int i=0; i<keyPoints.Count(); i++)
            {
                blobrange = (int)keyPoints[i].Size / 2;
                for(int j=0; j<blobrange+1; j++)
                {
                    for (int k = 0; k < blobrange+1; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobrange + j + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobrange + 1.0 - dist) / (blobrange + 1.0);
                        if (keyPoints[i].Point.X - blobrange + j + k > Series.Data.GetLength(0) || keyPoints[i].Point.Y - j + k > Series.Data.GetLength(1)) break;
                        if (keyPoints[i].Point.Y - j + k < 0 || keyPoints[i].Point.X - blobrange + j + k < 0) continue;
                        Series.Data[(int)keyPoints[i].Point.X - blobrange + j + k, (int)keyPoints[i].Point.Y - j + k] += val;
                    }
                }
                for (int j = 0; j < blobrange; j++)
                {
                    for (int k = 0; k < blobrange; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobrange +1 + j + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobrange + 1.0 - dist) / (blobrange + 1.0);
                        if (keyPoints[i].Point.X - blobrange +1 + j + k > Series.Data.GetLength(0) || keyPoints[i].Point.Y - j + k > Series.Data.GetLength(1)) break;
                        if (keyPoints[i].Point.Y - j + k < 0 || keyPoints[i].Point.X - blobrange +1 + j + k < 0) continue;
                        Series.Data[(int)keyPoints[i].Point.X - blobrange +1 + j + k, (int)keyPoints[i].Point.Y - j + k] += val;
                    }
                }
            }
            heatMap.InvalidatePlot(true);
        }

        public void saveToFile (string path)
        {
            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(HeatMap, path);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
