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
    public partial class Variables : UserControl
    {
        public Variables()
        {
            InitializeComponent();
            SingVarAssign.Text = "Single Variable Assignment: varName = x \n" +
                                 "\t\t\tExample: foo = 9\n";

            MultVarAssign.Text = "Multiple Variable Assignment: varOne = varTwo = x\n" +
                       "\t\t\tExample: foo = bar = 9\n";

            VarAssignInExpr.Text = "Variable Assignment in an Expression: (varName = 2)\n" +
                         "\t\t\tExample: 6E(x = 3)\n";
        }
    }
}
