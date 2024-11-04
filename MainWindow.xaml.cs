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

        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new MainView();
        }

        private void MainButtonClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MainView();
        }

        private void HelpButtonClicked(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HelpView();
        }
    }
}