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

namespace DotNetProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SignUp_button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new RegestrationWindow().ShowDialog();
            this.Show();
        }

        private void LogIn_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new LogInWindow().ShowDialog();
            this.Show();
        }
    }
}
