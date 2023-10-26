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
    /// Логика взаимодействия для CRUDWindow.xaml
    /// </summary>
    public partial class CRUDWindow : Window
    {
        private static Mutex? mutex;
        private static String mutexName = "CRUD_MUTEX";
        public Data.Entity.User? User { get; set; }
        public bool IsDeleted { get; set; }
        public bool enabled = false;
        public string InitialName;
        public string InitialSurname;
        public string InitialStatus;
        public string InitialAvatar;
        public CRUDWindow()
        {
            CheckPreviousLunch();
            InitializeComponent();
            SaveButton.IsEnabled = enabled;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IdBox.Text = User?.Id.ToString() ?? "";
            NameBox.Text = User?.Name?.ToString() ?? "";
            SurnameBox.Text = User?.Surname?.ToString() ?? "";
            AvatarBox.Text = User?.Avatar?.ToString() ?? "";
            StatusBox.Text = User?.Status ?? "";
            if (User?.IdGender == new Guid("bfc0cbac-d739-420a-990d-0f7e08c826f7"))
            {
                maleRadioButton.IsChecked = true;
                femaleRadioButton.IsChecked = false;
            }
            else
            {
                maleRadioButton.IsChecked = false;
                femaleRadioButton.IsChecked = true;
            }
            InitialName = User.Name;
            InitialSurname = User.Surname;
            InitialStatus = User.Avatar ?? "";
            InitialStatus = User.Avatar ?? "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mutex?.ReleaseMutex();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            User.Name = NameBox.Text;
            User.Surname = SurnameBox.Text;
            User.Avatar = AvatarBox.Text;
            User.Status = StatusBox.Text;
            if (dobDatePicker.SelectedDate != null)
            {
                User.Birthday = dobDatePicker.SelectedDate.Value;
            }
            if (maleRadioButton.IsChecked == true)
            {
                User.IdGender = new Guid("bfc0cbac-d739-420a-990d-0f7e08c826f7");
            }
            else
            {
                User.IdGender = new Guid("70958c27-2ff5-4f21-9b3a-4d38661e6e22");
            }
            DialogResult = true;
        }

        private void SoftDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = true;
            DialogResult = true;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            IsDeleted = false;
            User.DeleteDt = null;
            DialogResult = true;
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = NameBox.Text != InitialName;
            SaveButton.IsEnabled = enabled;
        }

        private void SurnameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = SurnameBox.Text != InitialSurname;
            SaveButton.IsEnabled = enabled;
        }

        private void AvatarBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = AvatarBox.Text != InitialAvatar;
            SaveButton.IsEnabled = enabled;
        }

        private void StatusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enabled = StatusBox.Text != InitialStatus;
            SaveButton.IsEnabled = enabled;
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
    }
}
