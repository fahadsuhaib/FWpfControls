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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = Employee.GetEmployees();
        }
    }

    public class Employee : INotifyPropertyChanged, IDataErrorInfo
    {
        public int EmployeeID
        {
            get;
            set;
        }

        private string name = string.Empty;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.RaisePropertyChanged("Name");
            }
        }

        public static List<Employee> GetEmployees()
        {
            var emps = new List<Employee>();
            emps.Add(new Employee() { EmployeeID = 1, Name = "Robert" });
            emps.Add(new Employee() { EmployeeID = 2, Name = "William" });
            emps.Add(new Employee() { EmployeeID = 3, Name = "Kate" });
            emps.Add(new Employee() { EmployeeID = 4, Name = "Zack" });
            emps.Add(new Employee() { EmployeeID = 5, Name = "Mark" });
            return emps;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Error
        {
            get { return string.Empty; }
        }

        public string this[string columnName]
        {
            get
            {
                var result = string.Empty;
                switch (columnName)
                {
                    case "Name":
                        if (this.Name == String.Empty)
                        {
                            result = "Name cannot be empty";
                        }

                        break;
                }

                return result;
            }
        }
    }

}
