using System;
using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using CSharpCaliburnMicro1.ViewInterfaces;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace CSharpCaliburnMicro1.ViewModels
{
    public class HistogramViewModelBase : PropertyChangedBase
    {

        private const int PlotWidth = 400;
        private int plotHeight = 450;

        private PlotModel plotModel;

        // https://github.com/oxyplot/oxyplot/issues/1671
        //Removed ColumnSeries: use BarSeries
        //private ColumnSeries columnSeries;
        private BarSeries columnSeries;

        public IAssetClassHistogramItem[] MainSeries { get; set; }

        public void InitializePlot()
        {
            this.plotModel = new PlotModel();
            this.plotModel.PlotAreaBorderThickness = new OxyThickness(0);
            //this.plotModel.Padding = new OxyThickness(12, 2, 12, 0);

            // Legend 
            this.plotModel.IsLegendVisible = false;

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.MinorStep = 1;
            //categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.IsAxisVisible = true;
            categoryAxis1.PositionAtZeroCrossing = false;
            categoryAxis1.TickStyle = TickStyle.None;
            categoryAxis1.AxislineStyle = LineStyle.Solid;
            categoryAxis1.AxislineThickness = 0;
            categoryAxis1.AxislineColor = OxyColors.Black;
            categoryAxis1.Angle = -50;
            categoryAxis1.AxisTickToLabelDistance = 10;
            categoryAxis1.AxisDistance = -0.8;
            categoryAxis1.FontSize = 11;
            foreach (var item in this.MainSeries)
            {
                categoryAxis1.Labels.Add(item.AssetClass);
            }
            this.plotModel.Axes.Add(categoryAxis1);

            // asse Y
            var linearAxis1 = new LinearAxis();
            //linearAxis1.AbsoluteMinimum = 0;
            linearAxis1.PositionAtZeroCrossing = true;
            //linearAxis1.MaximumPadding = 0.06;
            //linearAxis1.MinimumPadding = 0;
            linearAxis1.Maximum = this.MaxY;
            linearAxis1.Minimum = this.MinY;
            linearAxis1.MajorStep = this.StepY;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.MajorGridlineStyle = LineStyle.DashDot;
            linearAxis1.MajorGridlineColor = OxyColors.Gray;
            linearAxis1.MajorGridlineThickness = 0.5;
            linearAxis1.AxislineStyle = LineStyle.None;
            linearAxis1.TickStyle = TickStyle.None;
            linearAxis1.LabelFormatter = x => { return String.Format("{0}%", x); };
            linearAxis1.AxisTickToLabelDistance = (1.0 / this.MainSeries.Count()) / 0.00685; // 20;
            this.plotModel.Axes.Add(linearAxis1);

            //// Creo un asse fake per mettere la linea sullo zero
            //var linearAxis2 = new LinearAxis();
            //linearAxis2.Position = AxisPosition.Left;
            //linearAxis2.AxislineStyle = LineStyle.Solid;
            //linearAxis2.PositionAtZeroCrossing = true;
            //linearAxis2.TickStyle = TickStyle.None;
            //linearAxis2.LabelFormatter = x => { return ""; };
            //this.plotModel.Axes.Add(linearAxis2);

            // Aggiungo la serie
            this.plotModel.Series.Add(this.columnSeries);
        }

        public BitmapSource ChartImage
        {
            get
            {
                //https://github.com/oxyplot/oxyplot/issues/1498
                var pngExporter = new OxyPlot.Wpf.PngExporter();
                pngExporter.Width = PlotWidth;
                pngExporter.Height = this.plotHeight;
                BitmapSource bitmap = pngExporter.ExportToBitmap(this.plotModel);
                return bitmap;
            }
        }

        public double MaxY
        {
            get
            {
                if (!this.columnSeries.Items.Any())
                    return 100d;

                double maxValue = this.columnSeries.Items.Max(x => x.Value);
                if (maxValue > 0)
                {
                    return maxValue + (maxValue * 0.2);
                }
                else
                {
                    return 0d;
                }
            }
        }

        public double MinY
        {
            get
            {
                if (!this.columnSeries.Items.Any())
                    return 0d;

                double minValue = this.columnSeries.Items.Min(x => x.Value);
                if (minValue < 0)
                {
                    return minValue + (minValue * 0.2);
                }
                else
                {
                    return 0d;
                }
            }
        }

        public double StepY
        {
            get
            {
                double gap = this.MaxY - this.MinY;

                if (gap < 0.6)
                {
                    return 0.1d;
                }
                if (gap < 1.1)
                {
                    return 0.25d;
                }
                if (gap < 3)
                {
                    return 0.5d;
                }
                if (gap < 6)
                {
                    return 1d;
                }
                if (gap < 11)
                {
                    return 2.5d;
                }
                if (gap < 30)
                {
                    return 5d;
                }
                if (gap < 60)
                {
                    return 10d;
                }
                if (gap < 90)
                {
                    return 15d;
                }
                if (gap < 120)
                {
                    return 20d;
                }
                return 25d;
            }
        }

        public void LoadData()
        {
            this.columnSeries = new BarSeries();
            //this.columnSeries.StrokeThickness = 0;

            for (int i = 0; i < this.MainSeries.Count(); i++)
            {
                columnSeries.Title = this.MainSeries[i].AssetClass;
                BarItem columnItem = new BarItem(Convert.ToDouble(this.MainSeries[i].Weight * 100), -1);
                columnItem.Color = OxyColor.Parse(FormatOxyColor(this.MainSeries[i].Color));
                //columnItem.Color = OxyColor.FromRgb(178, 178, 178);
                this.columnSeries.Items.Add(columnItem);
            }
        }

        private string FormatOxyColor(string color)
        {
            string formatted = string.IsNullOrEmpty(color) ? "#FFB2B2B2" : color.Insert(1, "FF");
            if (formatted.Length != 9)
                throw new ArgumentException(string.Format("Resulting OxyColor {0} is not in the correct format. The format must be #AARRGGBB", formatted));

            return formatted;
        }
    }
}
