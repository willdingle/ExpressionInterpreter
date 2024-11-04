using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ExpressionInterpreter.HelpPages
{
    public partial class Operators : UserControl
    {
        public Operators()
        {
            InitializeComponent();
            BasicOperators.Text = "Basic: + - * / (addition, subtraction, multiplication and division)\n" +
                                 "\t\t\tExample: 5 + 6 * 2 - 6 / 3     -->     15\n";

            Mod.Text = "Modulo: x % y (remainder from dividing x by y)\n" +
                       "\t\t\tExample: 8 % 3     -->     2\n";

            Power.Text = "Power: x^y (x to the power of y)\n" +
                         "\t\t\tExample: 4^2     -->     16\n";

            Brackets.Text = "Brackets: ()\n" +
                            "\t\t\tExample: (5 + 6) * 2 - (6 / 3)     -->     20\n";

            FloatingPoint.Text = "Floating Point: .\n" +
                                 "\t\t\tExample: 5.6 + 3.2     -->     8.8\n";

            ExponentE.Text = "Exponent E: xEy (x times 10 to the power of y)\n" +
                             "\t\t\tExample: 6E3     -->     6000\n";
        }
    }
}
