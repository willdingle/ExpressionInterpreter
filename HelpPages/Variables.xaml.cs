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

            string[] titles = ["Single Variable Assignment", "Multiple Variable Assignment", "Variable Assignment in Expression", "Function Assignment", "Derivatives"];
            string[] mainTexts =
            [
                "varName = x \n" +
                       "\tExample: foo = 9",
                "varOne = varTwo = x\n" +
                       "\tExample: foo = bar = 9",
                "(varName = 2)\n" +
                       "\tExample: 6E(x = 3)",
                "Use any letter followed by () or (x) followed by = [function]\n" +
                       "\tExample: f(x) = x^2 + 2*x + 3",
                "Check 'Calculate Derivates' above the input box to calculate the derivative of a function when it is defined.\n" +
                       "\tWARNING: Only works for standard polynomials (e.g. def f(x) = 2*x^2 + 3*x - 5\t -->\t fDeriv = 4*x + 3)"
            ];
            int rowIndex = 0;

            for (int i = 0; i < titles.Length; i++)
            {
                TextBlock title = new TextBlock();
                title.FontSize = 18;
                title.FontWeight = FontWeights.Bold;
                title.Text = titles[i];

                TextBlock mainText = new TextBlock();
                mainText.FontSize = 14;
                mainText.Text = mainTexts[i];

                VariablesGrid.RowDefinitions.Add(new RowDefinition());
                VariablesGrid.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(title, rowIndex++);
                Grid.SetRow(mainText, rowIndex++);
                VariablesGrid.Children.Add(title);
                VariablesGrid.Children.Add(mainText);
            }
        }
    }
}
