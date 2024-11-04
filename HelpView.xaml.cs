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
using ExpressionInterpreter.HelpPages;

namespace ExpressionInterpreter
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
        }

        private void OperatorsHelpButtonClicked(object sender, RoutedEventArgs e)
        {
            HelpContent.Content = new Operators();
        }

        private void VariablesHelpButtonClicked(object sender, RoutedEventArgs e)
        {
            HelpContent.Content = new Variables();
        }

        private void PlottingHelpButtonClicked(object sender, RoutedEventArgs e)
        {
            HelpContent.Content = new Plotting();
        }
    }
}
