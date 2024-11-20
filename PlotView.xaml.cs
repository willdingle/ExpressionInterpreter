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
using ExpressionInterpreter.HelpPages;

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

        public double getValue(double x, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            var a = Math.Round(x,5);
            var oList = Interpreter.lexer(funcName+"(" +a.ToString()+ ")");
            var result = Interpreter.parseNeval(oList, vartable, funcTable);
            return result.Item2.GetValue;
        }

        public FunctionSeries function(int bound1, int bound2, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            FunctionSeries serie = new FunctionSeries();
            for (double x = bound1; x < bound2; x += 0.01)
            {
                //adding the points based off x
                DataPoint data = new DataPoint(x, getValue(x, funcName, funcTable, vartable));

                //adding the point to the serie
                serie.Points.Add(data);
                
            }
            serie.Title = funcName + "(x)";
            //returning the serie
            return serie;
        }

        public void PlotFunc(string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            // -10 to 10 range here but we can specify that later
            Model.Series.Add(function(-10,10,funcName, funcTable,vartable));
            Model.InvalidatePlot(true);
        }
    }
}
