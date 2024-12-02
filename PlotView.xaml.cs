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
        private Dictionary<string, FsharpLib.Interpreter.num> varTable = [];
        private Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable = [];

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

        private void ZoomPanChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Trace.WriteLine("Zoom pan changed");

            var newMin = Model.Axes[0].ActualMinimum;
            var newMax = Model.Axes[0].ActualMaximum;

            if (newMax > Int32.MaxValue)
            {
                Trace.WriteLine("Limiting to max value size");
                newMax = Int32.MaxValue;
                newMin = -Int32.MaxValue;
            }

            double increment = (newMax - newMin) / 100;

            foreach (LineSeries line in Model.Series)
            {
                LineSeries lineSeries = new LineSeries();
                var oldMin = line.Points[0].X;
                var oldMax = line.Points[line.Points.Count - 1].X;

                for (double x = newMin; x < oldMin; x += increment)
                {
                    var lineName = line.Title[0];
                    DataPoint data = new DataPoint(x, getValue(x, "" + lineName, funcTable, varTable));

                    lineSeries.Points.Add(data);
                }
                lineSeries.Points.AddRange(line.Points);

                for (double x = oldMax; x < newMax; x += increment)
                {
                    var lineName = line.Title[0];
                    DataPoint data = new DataPoint(x, getValue(x, "" + lineName, funcTable, varTable));

                    lineSeries.Points.Add(data);
                }
                line.Points.Clear();
                line.Points.AddRange(lineSeries.Points);
            }

            Trace.WriteLine("new min: " + newMin);
            Trace.WriteLine("new max: " + newMax);

            Model.InvalidatePlot(true);
        }

        public double getValue(double x, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            var oList = Interpreter.lexer(funcName+"(" + x.ToString() + ")");
            var result = Interpreter.parseNeval(oList, vartable, funcTable);
            return result.Item2.GetValue;
        }

        public FunctionSeries Function(int bound1, int bound2, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
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
            Model.Series.Add(Function(-10, 10, funcName, funcTable, vartable));


            /*
            foreach (var ax in Model.Axes)
            {
                ax.Maximum = ax.Minimum = Double.NaN;
            }
            */

            this.varTable = vartable;
            this.funcTable = funcTable;

            Model.InvalidatePlot(true);
        }
    }
}
