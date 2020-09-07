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
        private PlotModel _heatMap;
        private HeatMapSeries _series;

        public PlotModel HeatMap
        {
            get { return _heatMap; }
            set { _heatMap = value; NotifyPropertyChanged(); }
        }
        public string Title
        {
            get { return _heatMap.Title; }
            set { _heatMap.Title = value; }
        }
        public HeatMapSeries Series
        {
            get { return _series; }
            set { _series = value; NotifyPropertyChanged(); }
        }

        public Heatmap(int width, int height)
        {
            _heatMap = new PlotModel
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
            _heatMap.Series.Add(Series);

            _heatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });
            _heatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false
            });
            _heatMap.Axes.Add(new OxyPlot.Axes.LinearColorAxis
            {
                Position = AxisPosition.Right,
                IsAxisVisible = false,
            });

        }

        public void UpdateSeries(MKeyPoint[] keyPoints)
        {
            int blobRange;
            for (int i = 0; i < keyPoints.Count(); i++)
            {
                blobRange = (int)keyPoints[i].Size / 2;
                for (int j = 0; j < blobRange + 1; j++)
                {
                    for (int k = 0; k < blobRange + 1; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobRange + j + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobRange + 1.0 - dist) / (blobRange + 1.0);
                        if (keyPoints[i].Point.X - blobRange + j + k > Series.Data.GetLength(0) || keyPoints[i].Point.Y - j + k > Series.Data.GetLength(1))
                            break;
                        if (keyPoints[i].Point.Y - j + k < 0 || keyPoints[i].Point.X - blobRange + j + k < 0)
                            continue;
                        Series.Data[(int)keyPoints[i].Point.X - blobRange + j + k, (int)keyPoints[i].Point.Y - j + k] += val;
                    }
                }
                for (int j = 0; j < blobRange; j++)
                {
                    for (int k = 0; k < blobRange; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobRange + 1 + j + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobRange + 1.0 - dist) / (blobRange + 1.0);
                        if (keyPoints[i].Point.X - blobRange + 1 + j + k > Series.Data.GetLength(0) || keyPoints[i].Point.Y - j + k > Series.Data.GetLength(1)) 
                            break;
                        if (keyPoints[i].Point.Y - j + k < 0 || keyPoints[i].Point.X - blobRange + 1 + j + k < 0)
                            continue;
                        Series.Data[(int)keyPoints[i].Point.X - blobRange + 1 + j + k, (int)keyPoints[i].Point.Y - j + k] += val;
                    }
                }
            }
            _heatMap.InvalidatePlot(true);
        }

        public void SaveToFile(string path)
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
