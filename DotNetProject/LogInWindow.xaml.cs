using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Mail;

namespace DotNetProject
{
    /// <summary>
    /// Логика взаимодействия для LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        private Data.DataContext dataContext;
        private String? connectionString = App.connectionString;
        [DllImport("user32.dll")]
        public static extern int MessageBoxA(IntPtr hWnd, String lpText, String lpCaption, uint uType);
        public delegate void ThreadMethod();
        [DllImport("Kernel32.dll", EntryPoint = "CreateThread")]
        public static extern IntPtr NewThread(
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            ThreadMethod lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
        );
        public LogInWindow()
        {
            InitializeComponent();
            dataContext = new Data.DataContext();
            this.DataContext = this;
        }

        private void LogIn_button_Click(object sender, RoutedEventArgs e)
        {
            if (emailTextBox.Text != ""  && passwordTextBox.Text != "")
            {
                var user = dataContext.Users.FirstOrDefault(u => u.Email == $"{emailTextBox.Text}");
                string password;
                if(user != null)
                {
                    if(user.Password != null)
                    {
                        password = user.Password;
                        if (passwordTextBox.Text == password)
                        {
                            this.Hide();
                            new NetWorkWindow(emailTextBox.Text).ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show(
                                "Wrong password. Do you want to set a new password?",
                                "Error while logging in",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Error
                            );

                            if (result == MessageBoxResult.Yes)
                            {
                                ConfirmContainer.Visibility = Visibility.Visible;
                                using SqlConnection connection = new(connectionString);
                                connection.Open();
                                String code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                                using SqlCommand command = connection.CreateCommand();
                                command.CommandText = $"UPDATE [Users] SET [ConfirmationCode] = N'{code}' " +
                                        $"WHERE Email = '{emailTextBox.Text}'";
                                command.ExecuteNonQuery();
                                connection.Close();
                                SmtpClient? smtpClient = GetSmtpClient();
                                MailMessage mailMessage = new(
                                    App.getConfiguration("smtp:email")!,
                                    emailTextBox.Text,
                                    "Signup successfull",
                                    $"<h2>Dear, {user.Name}</h2> Your code for confirmation is <b style='font-weight:bold'>{code}</b>")
                                {
                                    IsBodyHtml = true,
                                };
                                smtpClient?.Send(mailMessage);
                                MessageBox.Show("Chek Email");
                            }
                        }
                    }
                    else
                    {
                        setPassword();
                        TitleLable.Content = "Password resseted";
                    }
                }
                else
                {
                    var method = new ThreadMethod(() =>
                    {
                        MessageBoxA(
                            IntPtr.Zero,
                            "You are not signed up yet\nPlease, sign up first",
                            "Error while log in",
                            0x10
                        );
                        methodHandle.Free();
                    });
                    methodHandle = GCHandle.Alloc(method);
                    NewThread(IntPtr.Zero, 0, method, IntPtr.Zero, 0, IntPtr.Zero);
                }
            }
            else
            {
                var method = new ThreadMethod(ErrorMessage);
                methodHandle = GCHandle.Alloc(method);
                NewThread(IntPtr.Zero, 0, method, IntPtr.Zero, 0, IntPtr.Zero);
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM [Users] WHERE " +
                $" [Email]=N'{emailTextBox.Text}' ";
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (!reader.IsDBNull("ConfirmationCode"))
                {
                    String code = reader.GetString("ConfirmationCode");
                    if (textboxCode.Text.Equals(code))
                    {
                        TitleLable.Content = "Write a new password";
                        deletePassword();
                        ConfirmContainer.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        TitleLable.Content = "Email not confirmed\n";
                    }
                }
            }
            else
            {
                MessageBox.Show("Credentials incorrect");
            }
            connection.Close();
        }

        private void deletePassword()
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE [Users] SET [Password] = NULL " +
                    $"WHERE Email = '{emailTextBox.Text}'";
            command.ExecuteNonQuery();
            connection.Close();
        }

        private void setPassword()
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = $"UPDATE [Users] SET [Password] = N'{passwordTextBox.Text}' " +
                    $"WHERE Email = '{emailTextBox.Text}'";
            command.ExecuteNonQuery();
            connection.Close();
        }

        private SmtpClient? GetSmtpClient()
        {
            #region get and check config
            String? host = App.getConfiguration("smtp:host");
            if (host == null)
            {
                MessageBox.Show("Error getting host");
                return null;
            }
            String? portString = App.getConfiguration("smtp:port");
            if (portString == null)
            {
                MessageBox.Show("Error getting port");
                return null;
            }
            int port;
            try { port = int.Parse(portString); }
            catch
            {
                MessageBox.Show("Error parsing port");
                return null;
            }
            String? email = App.getConfiguration("smtp:email");
            if (email == null)
            {
                MessageBox.Show("Error getting email");
                return null;
            }
            String? password = App.getConfiguration("smtp:password");
            if (password == null)
            {
                MessageBox.Show("Error getting password");
                return null;
            }
            String? sslString = App.getConfiguration("smtp:ssl");
            if (sslString == null)
            {
                MessageBox.Show("Error getting ssl");
                return null;
            }
            bool ssl;
            try { ssl = bool.Parse(sslString); }
            catch
            {
                MessageBox.Show("Error parsing ssl");
                return null;
            }
            #endregion

            return new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(email, password)
            };
        }

        public void ErrorMessage()
        {
            MessageBoxA(
                IntPtr.Zero,
                "You need to fill email and password",
                "Error while log in",
                0x10
            );
            methodHandle.Free();
        }
        GCHandle methodHandle;
    }
}
