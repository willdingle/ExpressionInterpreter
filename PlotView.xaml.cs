using System.Windows.Controls;
using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Microsoft.FSharp.Collections;
using FsharpLib;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ExpressionInterpreter
{
    public partial class PlotView : UserControl
    {
        public PlotModel Model { get; set; }

        public PlotView()
        {
            InitializeComponent();

            Model = new PlotModel { Title = "Plot View" };

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

            DataContext = this;
        }

        public void PlotFunc(string funcName, FSharpList<FsharpLib.Interpreter.terminal> func)
        {
            string theNum = func[0].ToString();

            theNum = Regex.Replace(theNum, "[^0-9]", "");

            // x^1
            Func<double, double> newFunc = (x) => Int32.Parse(theNum) * x;
            Model.Series.Add(new FunctionSeries(newFunc, 0, 10, 0.1, funcName + "(x)"));

            Model.InvalidatePlot(true);
        }
    }
}
