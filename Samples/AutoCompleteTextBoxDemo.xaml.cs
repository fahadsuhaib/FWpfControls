using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iBlue.Windows.Controls;

namespace Samples
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBoxDemo.xaml
    /// </summary>
    public partial class AutoCompleteTextBoxDemo : Window
    {
        public AutoCompleteTextBoxDemo()
        {
            InitializeComponent();
            this.DataContext = Employee.GetEmployees();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.acTextBox.Text = "Hello";
        }
    }
}
