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
using System.Windows.Shapes;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        private void RedrawLine(LineSeries line, double newMin, double newMax, double increment)
        {
            string lineName = "" + line.Title[0];

            line.Points.AddRange(Function(newMin, newMax, increment, lineName, funcTable, varTable).Points);
        }

        private void ZoomPanChanged(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Trace.WriteLine("Zoom pan changed");

            var newMin = Model.Axes[0].ActualMinimum;
            var newMax = Model.Axes[0].ActualMaximum;

            if (newMax > Int32.MaxValue)
            {
                Trace.WriteLine("Limiting to max value size");
                newMax = Int32.MaxValue;
                newMin = -Int32.MaxValue;
            }

            double increment = (newMax - newMin) / 1000;
            int numOfThreads = 4;

            foreach (LineSeries line in Model.Series)
            {
                line.Points.Clear();
                string lineName = "";
                foreach (char i in line.Title)
                {
                    if (i == '(')
                    {
                        break;
                    }
                    lineName += i;
                }

                /*
                double threadIncrement = (newMax - newMin) / numOfThreads;
                Thread[] threads = new Thread[numOfThreads];

                for (int  i = 0; i < numOfThreads; i++)
                {
                    threads[i] = new Thread(() => line.Points.AddRange(Function(newMin + (i * threadIncrement), (newMin + (i + 1)) * threadIncrement, increment, lineName, funcTable, varTable).Points));
                    threads[i].Start();
                }
                */
                
                line.Points.AddRange(Function(newMin, newMax, increment, lineName, funcTable, varTable).Points);
            }

            Model.InvalidatePlot(true);
        }

        public double getValue(double x, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            var oList = Interpreter.lexer(funcName+"(" + x.ToString() + ")");
            var result = Interpreter.parseNeval(oList, vartable, funcTable);
            return result.Item2.GetValue;
        }

        public FunctionSeries Function(double bound1, double bound2, double increment, string funcName, Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable, Dictionary<string, FsharpLib.Interpreter.num> vartable)
        {
            FunctionSeries serie = new FunctionSeries();
            for (double x = bound1; x < bound2; x += increment)
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
            Model.Series.Add(Function(-10, 10, 0.01, funcName, funcTable, vartable));


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
