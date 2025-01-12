using ExpressionInterpreter.HelpPages;
using FsharpLib;
using Microsoft.FSharp.Collections;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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
    public partial class MainView : UserControl
    {

        private readonly Dictionary<string, FsharpLib.Interpreter.num> varTable = [];
        private readonly Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable = [];
        private Dictionary<string, float[]> funcCoeffs = [];
        PlotView plotView;
        bool errorsHighlighted = false;

        public MainView()
        {
            InitializeComponent();
            plotView = new PlotView();
            PlotContent.Content = plotView;
        }

        private void highlightLexerError(Paragraph newPara, string oldText, string word)
        {
            // Get current code line (paragraph) from input box
            var oldTextSplit = oldText.Split(word);

            // [text before char] + <Underline> + [theChar] + </Underline> + [text after char]
            for (int i = 0; i < oldTextSplit.Length - 1; i++)
            {
                newPara.Inlines.Add(oldTextSplit[i]);
                newPara.Inlines.Add(new Run(word) { Foreground = Brushes.Red });
            }
            //newPara.Inlines.Add(new Run(" ") { Foreground = Brushes.Black });
            //newPara.Inlines.Add(oldTextSplit[oldTextSplit.Length - 1].Remove(oldTextSplit[1].Length - 1));
        }

        private void InputBoxKeyUp(object sender, KeyEventArgs e)
        {
            // Run inputted code
            if (e.Key == Key.F5)
            {
                TextRange textRange = new TextRange(inputBox.Document.ContentStart, inputBox.Document.ContentEnd);
                Trace.WriteLine(textRange.Text);

                // Clear output boxes
                outputBox.Text = "";
                errorBox.Text = "";
                // sListBox.Text = "";
                plotView.Model.Series.Clear();

                // Split up lines of code
                var codeLines = textRange.Text.Split("\n");
                var lineNum = 0;

                // Set up error highlighting
                inputBox.Document = new FlowDocument();
                inputBox.Document.LineHeight = 1;

                // Process each line of code
                foreach (var codeLine in codeLines)
                {
                    lineNum += 1;
                    if (string.IsNullOrWhiteSpace(codeLine))
                        continue;

                    // Lex code line
                    FSharpList<Interpreter.terminal> oList = null;
                    try
                    {
                        oList = Interpreter.lexer(codeLine);

                        // Evaluate and output code line
                        try
                        {
                            var Out = Interpreter.parseNeval(oList, varTable, funcTable);

                            // Plot the function specified if "plot" is used
                            if (oList.Contains(Interpreter.terminal.Plot))
                            {
                                Trace.WriteLine("" + oList[1]);
                                var funcName = "" + oList[1];
                                funcName = funcName.Replace("Func ", "");
                                funcName = funcName.Replace("\"", "");

                                plotView.PlotFunc(funcName, funcTable, varTable);
                                outputBox.Text += "plot graph\n";
                            }

                            // Find derivative of function when function is defined (only works for standard polynomials)
                            else if (codeLine.Contains("def") && CalcDerivs.IsChecked.GetValueOrDefault())
                            {
                                var function = codeLine.Split("=")[1]; //e.g. " 3*x^2 + 2*x - 5"
                                function = function.Replace(" ", ""); //e.g. "3*x^2+2*x-5"
                                function = function.Replace("-", "+-"); //e.g. "3*x^2+2*x+-5
                                var functionParts = function.Split("+"); //e.g. {"3*x^2", "2*x", "-5"}

                                // Find order of function
                                var orderOfFunc = 0;
                                foreach (var part in functionParts)
                                {
                                    var almostPower = part.Split("^");
                                    
                                    if (almostPower.Length == 1)
                                    {
                                        if (almostPower[0].Contains("x"))
                                        {
                                            if (orderOfFunc < 1) orderOfFunc = 1;
                                            continue;
                                        }
                                        continue;
                                    }
                                    var power = int.Parse(almostPower[1]);
                                    if (power > orderOfFunc)
                                        orderOfFunc = power;
                                }

                                // Find coeffs
                                double[] coeffs = new double[orderOfFunc + 1];
                                for (int i = 0; i < functionParts.Length; i++)
                                {
                                    var components = functionParts[i].Split("*");
                                    if (components.Length == 1)
                                    {
                                        if (components[0].Contains("x"))
                                        {
                                            var xPartComps = components[0].Split("^");
                                            if (xPartComps.Length == 1)
                                            {
                                                coeffs[orderOfFunc - 1] += 1;
                                                continue;
                                            }
                                            coeffs[orderOfFunc - int.Parse(xPartComps[1])] += 1;
                                        }
                                        coeffs[orderOfFunc] += double.Parse(components[0]);
                                        continue;
                                    }

                                    var coeff = components[0];
                                    var xPart = components[1];
                                    var xPartComponents = xPart.Split("^");
                                    if (xPartComponents.Length == 1)
                                    {
                                        coeffs[orderOfFunc - 1] += double.Parse(coeff);
                                        continue;
                                    }
                                    coeffs[orderOfFunc - int.Parse(xPartComponents[1])] += double.Parse(coeff);

                                }

                                foreach (var co in coeffs)
                                    Trace.Write(co + ",");
                                Trace.WriteLine("");

                                var fsCoeffs = ListModule.OfSeq(coeffs);
                                var derivative = Interpreter.deriv(fsCoeffs, orderOfFunc + 1);

                                Trace.WriteLine("--- Derivative: ---");
                                foreach (var co in derivative)
                                    Trace.Write(co + ",");
                                Trace.WriteLine("");

                                // Process derivative function and store it
                                var funcName = "" + oList[1];
                                funcName = funcName.Replace("Func ", "");
                                funcName = funcName.Replace("\"", "");

                                // TODO: finish
                                string derivStr = "def " + funcName + "Deriv(x) = ";
                                for (int i = 0; i < derivative.Length ; i++)
                                {
                                    derivStr += derivative[i] + "*x^" + (orderOfFunc - 1 - i);
                                    if (i < derivative.Length - 1)
                                        derivStr += " + ";
                                }

                                var derivOList = Interpreter.lexer(derivStr);
                                var derivOut = Interpreter.parseNeval(derivOList, varTable, funcTable);
                                outputBox.Text += "Function '" + funcName + "' defined\n";
                                var derivName = "" + derivOList[1];
                                derivName = derivName.Replace("Func ", "");
                                derivName = derivName.Replace("\"", "");
                                outputBox.Text += "Function '" + derivName + "' defined\n";

                            }

                            // Output for when a function is defined
                            else if (codeLine.Contains("def"))
                            {
                                var funcName = "" + oList[1];
                                funcName = funcName.Replace("Func ", "");
                                funcName = funcName.Replace("\"", "");
                                outputBox.Text += "Function '" + funcName + "' defined\n";
                            }

                            // Regular output
                            else
                            {
                                outputBox.Text += Out.Item2.ToString() + "\n";
                            }

                            Paragraph newPara = new Paragraph();
                            newPara.Inlines.Add(codeLine.Remove(codeLine.Length - 1));
                            inputBox.Document.Blocks.Add(newPara);
                            inputBox.CaretPosition = inputBox.Document.ContentEnd;
                        }
                        // Parser and evaluation errors
                        catch (Exception ex)
                        {
                            var errMessage = ex.Message.Split(": ")[1];
                            Trace.WriteLine(errMessage);

                            Paragraph newParaParEv = new Paragraph();
                            newParaParEv.Inlines.Add(codeLine.Remove(codeLine.Length - 1));
                            inputBox.Document.Blocks.Add(newParaParEv);
                            inputBox.CaretPosition = inputBox.Document.ContentEnd;

                            errorBox.Text += ex.Message + " (Line " + lineNum + ")\n";
                            plotView.Model.InvalidatePlot(true);
                        }

                        // Output data from lexer
                        string sListStr = "";
                        for (int i = 0; i < oList.Length; i++)
                        {
                            sListStr += oList[i] + " , ";
                        }
                        Trace.WriteLine(sListStr);
                        //sListBox.Text = sListStr;
                        e.Handled = true;

                    }
                    // Lexer errors
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        var invalChar = ex.Message.Split(": ")[1].Split(" ")[2].Split("'")[1];
                        Trace.WriteLine(invalChar);
                        Paragraph newParaL = new Paragraph();
                        highlightLexerError(newParaL, codeLine, invalChar);
                        inputBox.Document.Blocks.Add(newParaL);
                        Trace.WriteLine(newParaL.Inlines.ElementAt(1));
                        errorsHighlighted = true;
                        inputBox.CaretPosition = inputBox.Document.ContentEnd;

                        errorBox.Text += ex.Message + " (Line " + lineNum + ")\n";
                    }

                    
                }
            }
            else if (errorsHighlighted)
            {
                var currentText = new TextRange(inputBox.Document.ContentStart, inputBox.Document.ContentEnd).Text;
                inputBox.Document = new FlowDocument();
                inputBox.Document.LineHeight = 1;
                var currentTextList = currentText.Split("\n");
                foreach (string currentTextLine in currentTextList)
                {
                    if (string.IsNullOrWhiteSpace(currentTextLine))
                        continue;

                    Paragraph newPara = new Paragraph();
                    newPara.Inlines.Add(currentTextLine.Remove(currentTextLine.Length - 1));
                    inputBox.Document.Blocks.Add(newPara);
                }
                inputBox.CaretPosition = inputBox.Document.ContentEnd;
                errorsHighlighted = false;
            }
        }
    }
}
