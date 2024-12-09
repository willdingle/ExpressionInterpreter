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
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {

        private readonly Dictionary<string, FsharpLib.Interpreter.num> varTable = [];
        private readonly Dictionary<string, FSharpList<FsharpLib.Interpreter.terminal>> funcTable = [];
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

                            // Plot the function inputted if "plot" is used
                            if (oList.Contains(Interpreter.terminal.Plot))
                            {
                                Trace.WriteLine("" + oList[1]);
                                var funcName = "" + oList[1];
                                funcName = funcName.Replace("Var ", "");
                                funcName = funcName.Replace("\"", "");

                                plotView.PlotFunc(funcName, funcTable, varTable);
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
