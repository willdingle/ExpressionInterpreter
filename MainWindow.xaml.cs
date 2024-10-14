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
                var oList = Interpreter.lexer(inputBox.Text);
                var sList = Interpreter.printTList(oList);
                //var pList = Interpreter.printTList(Interpreter.parser(oList));
                var Out = Interpreter.parseNeval(oList, varTable);

                //TODO: below line should display output of inputted code
                outputBox.Text = Out.Item2.ToString();
                inputBox.Text = "";
                e.Handled = true;
            }
        }
    }
}