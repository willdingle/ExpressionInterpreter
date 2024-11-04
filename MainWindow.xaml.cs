using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FsharpLib;
using Microsoft.FSharp.Collections;

namespace ExpressionInterpreter
{
    public partial class MainWindow : Window
    {

        private readonly Dictionary<string, double> varTable = [];

        public MainWindow()
        {
            InitializeComponent();
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
                        var Out = Interpreter.parseNeval(oList, varTable);
                        outputBox.Text = Out.Item2.ToString();
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