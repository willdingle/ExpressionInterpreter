using System.Windows.Controls;
using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ExpressionInterpreter
{
    public partial class PlotView : UserControl
    {
        public PlotModel Model { get; set; }

        public PlotView()
        {
            InitializeComponent();

            Model = new PlotModel { Title = "Hello, team" };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -10,
                Maximum = 10,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,
                PositionAtZeroCrossing = true
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -10,
                Maximum = 10,
                MajorGridlineStyle = LineStyle.Solid,
                AxislineStyle = LineStyle.Solid,
                AxislineColor = OxyColors.Black,
                AxislineThickness = 1,
                PositionAtZeroCrossing = true
            };

            Model.Axes.Add(xAxis);
            Model.Axes.Add(yAxis);

            // Adding series
            Model.Series.Add(new LineSeries
            {
                Title = "Arthur's Series",
                Points = { new DataPoint(0, 0), new DataPoint(10, 18), new DataPoint(20, 12) }
            });

            Model.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));

            DataContext = this;
        }
    }
}
