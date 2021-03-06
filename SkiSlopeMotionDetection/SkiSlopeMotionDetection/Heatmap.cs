﻿using System;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using Emgu.CV.Structure;
using OxyPlot.Wpf;
using HeatMapSeries = OxyPlot.Series.HeatMapSeries;

namespace SkiSlopeMotionDetection.PresentationLayer
{
    public class Heatmap
    {
        public PlotModel HeatMap { get; set; }
        public HeatMapSeries Series { get; set; }

        public Heatmap(int width, int height)
        {
            HeatMap = new PlotModel
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
            HeatMap.Series.Add(Series);

            HeatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });
            HeatMap.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false
            });
            HeatMap.Axes.Add(new OxyPlot.Axes.LinearColorAxis
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

                        int indexX = (int)keyPoints[i].Point.X - blobRange + j + k;
                        int indexY = Series.Data.GetLength(1) - ((int)keyPoints[i].Point.Y - j + k) + 1;
                        if (indexX >= Series.Data.GetLength(0) || indexY >= Series.Data.GetLength(1))
                            break;
                        if (indexX < 0 || indexY < 0)
                            continue;
                        Series.Data[indexX, indexY] += val;
                    }
                }
                for (int j = 0; j < blobRange; j++)
                {
                    for (int k = 0; k < blobRange; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobRange + 1 + j + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobRange + 1.0 - dist) / (blobRange + 1.0);

                        int indexX = (int)keyPoints[i].Point.X - blobRange + 1 + j + k;
                        int indexY = Series.Data.GetLength(1) - ((int)keyPoints[i].Point.Y - j + k) + 1;
                        if (indexX >= Series.Data.GetLength(0) || indexY >= Series.Data.GetLength(1))
                            break;
                        if (indexX < 0 || indexY < 0)
                            continue;
                        Series.Data[indexX, indexY] += val;
                    }
                }
            }
            HeatMap.InvalidatePlot(true);
        }

        public void SaveToFile(string path)
        {
            var pngExporter = new PngExporter { Width = 1200, Height = 800, Background = OxyColors.White };
            pngExporter.ExportToFile(HeatMap, path);
        }
    }
}
