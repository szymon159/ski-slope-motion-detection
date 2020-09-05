using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using Emgu.CV.Structure;

namespace SkiSlopeMotionDetection
{
    public class Heatmap
    {
        private PlotModel heatMap;
        public PlotModel HeatMap
        {
            get { return heatMap; }
            set { heatMap = value; }
        }
        private HeatMapSeries series;

        public Heatmap(int width, int height)
        {
            heatMap = new PlotModel
            {
                Title = "Areas used by skiers",
                PlotType = PlotType.XY,
                Background = OxyColors.White,
                IsLegendVisible = false,
            };

            series = new HeatMapSeries
            {
                Title = "HMSeries",
                Interpolate = true,
                X0 = 0,
                X1 = width,
                Y0 = 0,
                Y1 = height
            };

            series.Data = new double[width, height];
            heatMap.Series.Add(series);

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
                    for (int k = 0; j < blobrange; k++)
                    {
                        double dist = Math.Sqrt(Math.Pow(-blobrange + k, 2) + Math.Pow(-j + k, 2));
                        double val = (blobrange + 1.0 - dist) / (blobrange + 1.0);
                        series.Data[(int)keyPoints[i - blobrange + k].Point.X, (int)keyPoints[i - j + k].Point.Y] += val;

                    }
                }
            }
        }


    }
}
