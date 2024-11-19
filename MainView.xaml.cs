using ExpressionInterpreter.HelpPages;
using FsharpLib;
using Microsoft.FSharp.Collections;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FsharpLib.Interpreter.num;

namespace ExpressionInterpreter
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {

        private readonly Dictionary<string, FsharpLib.Interpreter.num> varTable = [];
        private readonly Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable = [];
        PlotView plotView;

        public MainView()
        {
            InitializeComponent();
            plotView = new PlotView();
            PlotContent.Content = plotView;
        }

        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            // Run inputted code
            if (e.Key == Key.F5)
            {
                // Clear output boxes
                outputBox.Text = "";
                errorBox.Text = "";
                sListBox.Text = "";

                // Split up lines of code
                var codeLines = inputBox.Text.Split("\n");

                //Process each line of code
                foreach (var codeLine in codeLines)
                {
                    if (string.IsNullOrWhiteSpace(codeLine))
                        continue;

                    try
                    {
                        var oList = Interpreter.lexer(codeLine);

                        try
                        {
                            var Out = Interpreter.parseNeval(oList, varTable, funcTable);
                            /*
                            foreach (var func in funcTable)
                            {
                                Trace.WriteLine(func);
                            }
                            */

                            // Plot the function inputted
                            if (oList.Contains(Interpreter.terminal.Plot))
                            {
                                Trace.WriteLine("" + oList[1]);
                                var funcName = "" + oList[1];
                                funcName = funcName.Replace("Var ", "");
                                funcName = funcName.Replace("\"", "");

                                plotView.PlotFunc(funcName, funcTable);
                                outputBox.Text += "plot graph\n";
                            }
                            //else if (oList.Contains(Interpreter.terminal.Func))
                            //{
                            //   outputBox.Text = funcTable["" + oList[2]];
                            //}
                            else
                            {
                                outputBox.Text += Out.Item2.ToString() + "\n";
                            }

                        }
                        catch (Exception ex)
                        {
                            errorBox.Text += ex.Message + "\n";
                        }

                        // Output data from lexer
                        string sListStr = "";
                        for (int i = 0; i < oList.Length; i++)
                        {
                            sListStr += oList[i] + " , ";
                        }
                        Trace.WriteLine(sListStr);
                        sListBox.Text = sListStr;
                        e.Handled = true;

                    }
                    catch (Exception ex)
                    {
                        errorBox.Text += "\n" + ex.Message;
                    }
                }
            }
        }
    }
}
