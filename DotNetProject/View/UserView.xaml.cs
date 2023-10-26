using DotNetProject.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DotNetProject.View
{
    /// <summary>
    /// Логика взаимодействия для UserView.xaml
    /// </summary>
    public partial class UserView : Window
    {
        private static Mutex? mutex;
        private static String mutexName = "VIEW_MUTTEX";
        public Data.Entity.User? User { get; set; }
        public UserView()
        {
            CheckPreviousLunch();
            InitializeComponent();
        }

        public void CheckPreviousLunch()
        {
            try
            {
                mutex = Mutex.OpenExisting(mutexName);
            }
            catch { }
            if (mutex != null)
            {
                if (!mutex.WaitOne(1))
                {
                    String message = "Enother crud window started";
                    MessageBox.Show(message);
                    throw new Exception(message);
                }

            }
            else
            {
                mutex = new Mutex(true, mutexName);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            nameTextBox.Text = User?.Name ?? "";
            surnameTextBox.Text = User?.Surname ?? "";
            statusTextBox.Text = User?.Status ?? "";
            avatarTextBox.Text = User?.Avatar ?? "";
            genderTextBox.Text = User?.IdGender.ToString() == "bfc0cbac-d739-420a-990d-0f7e08c826f7" ? "Male" : "Female";
            birthdayTextBox.Text = User?.Birthday.ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }
    }
}
