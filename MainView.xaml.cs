using ExpressionInterpreter.HelpPages;
using FsharpLib;
using System;
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
        private readonly Dictionary<string, string> funcTable = [];
        PlotView plotView;

        public MainView()
        {
            InitializeComponent();
            plotView = new PlotView();
            PlotContent.Content = plotView;
        }

        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //MessageBox.Show(inputBox.Text);

                // Process inputted code
                outputBox.Text = "";
                errorBox.Text = "";
                sListBox.Text = "";

                try
                {
                    var oList = Interpreter.lexer(inputBox.Text);

                    try
                    {
                        var Out = Interpreter.parseNeval(oList, varTable, funcTable);
                        foreach (var func in funcTable)
                        {
                            Trace.WriteLine(func);
                        }

                        // Plot the function inputted
                        if (oList.Contains(Interpreter.terminal.Plot))
                        {
                            Trace.WriteLine("plot graph " + funcTable["" + oList[3]]);
                            outputBox.Text = "plot graph " + funcTable["" + oList[3]];
                        }
                        else if (oList.Contains(Interpreter.terminal.Func))
                        {
                            outputBox.Text = funcTable["" + oList[2]];
                        }
                        else
                        {
                            outputBox.Text = Out.Item2.ToString();
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        errorBox.Text = ex.Message;
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
                    errorBox.Text = ex.Message;
                }
            }
        }
    }
}
