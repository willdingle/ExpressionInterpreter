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

            string[] titles = ["Basic", "Modulo", "Power", "Brackets", "Floating Point", "Exponent E"];
            string[] mainTexts = 
            [
                "+ - * / (addition, subtraction, multiplication and division)\n" +
                                "\tExample: 5 + 6 * 2 - 6 / 3     -->     15",
                "x % y (remainder from dividing x by y)\n" +
                       "\tExample: 8 % 3     -->     2",
                "x^y (x to the power of y)\n" +
                         "\tExample: 4^2     -->     16",
                "()\n" +
                            "\tExample: (5 + 6) * 2 - (6 / 3)     -->     20",
                ".\n" +
                                 "\tExample: 5.6 + 3.2     -->     8.8",
                "xEy (x times 10 to the power of y)\n" +
                             "\tExample: 6E3     -->     6000"
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

                OperatorsGrid.RowDefinitions.Add(new RowDefinition());
                OperatorsGrid.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(title, rowIndex++);
                Grid.SetRow(mainText, rowIndex++);
                OperatorsGrid.Children.Add(title);
                OperatorsGrid.Children.Add(mainText);
            }
        }
    }
}
