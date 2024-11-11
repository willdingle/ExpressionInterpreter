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
    public partial class Plotting : UserControl
    {
        public Plotting()
        {
            InitializeComponent();
            PlottingHelp.Text = "Use 'plot [variable]' to plot a line / polynomial.\n" +
                                "\t\t\tExample( when f(x) = x^2 ): plot f(x)          -->          INSERT PICTURE OF x^2 GRAPH";
        }
    }
}
