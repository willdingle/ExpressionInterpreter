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

            string[] titles = ["Plot Graph", "Panning", "Zooming"];
            string[] mainTexts =
            [
                "Use plot [function variable] to plot the corresponding function \n" +
                       "\tExample (when f() is assigned): plot f()",
                "Hold the right mouse button while dragging to pan over the plotting area",
                "Use the scroll wheel on a mouse or pinch gesture on a trackpad to zoom over the plotting area"
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

                PlottingGrid.RowDefinitions.Add(new RowDefinition());
                PlottingGrid.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(title, rowIndex++);
                Grid.SetRow(mainText, rowIndex++);
                PlottingGrid.Children.Add(title);
                PlottingGrid.Children.Add(mainText);
            }
        }
    }
}
