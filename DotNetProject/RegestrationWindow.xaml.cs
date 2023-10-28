using DotNetProject.Data.Entity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DotNetProject.LogInWindow;

namespace DotNetProject
{
    /// <summary>
    /// Логика взаимодействия для RegestrationWindow.xaml
    /// </summary>
    public partial class RegestrationWindow : Window
    {
        private Data.DataContext dataContext;
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
        public RegestrationWindow()
        {
            InitializeComponent();
            dataContext = App.dataContext;
            dataContext.Users.Load();
            this.DataContext = this;
        }

        private void CreateAccount_button_Click(object sender, RoutedEventArgs e)
        {
            if(emailTextBox.Text != "" && nameTextBox.Text != "" && surnnameTextBox.Text != "" && passwordTextBox.Text != "")
            {
                bool isValid = IsValidEmail(emailTextBox.Text);
                if (isValid)
                {
                    String code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

                    User newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Name = nameTextBox.Text,
                        Surname = surnnameTextBox.Text,
                        Email = emailTextBox.Text,
                        Password = passwordTextBox.Text,
                        ConfirmationCode = code,
                        CreateDt = DateTime.Now,
                        Birthday = DateTime.Now
                    };

                    dataContext.Users.Add(newUser);
                    dataContext.SaveChanges();

                    SmtpClient? smtpClient = GetSmtpClient();
                    MailMessage mailMessage = new(
                        App.getConfiguration("smtp:email")!,
                        emailTextBox.Text,
                        "Signup successfull",
                        $"<h2>Dear, {nameTextBox.Text}</h2> Your code for confirmation is <b style='font-weight:bold'>{code}</b>")
                    {
                        IsBodyHtml = true,
                    };
                    smtpClient?.Send(mailMessage);
                    MessageBox.Show("Chek Email");
                    ConfirmContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    var method = new ThreadMethod(() =>
                    {
                        MessageBoxA(
                            IntPtr.Zero,
                            "Your email is not valid\nPlease, use format example@mail.com",
                            "Error while registration",
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

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var user = dataContext.Users.Where(u => u.Email == emailTextBox.Text && u.Password == passwordTextBox.Text).FirstOrDefault();
            if (user != null)
            {
                if (user.ConfirmationCode != null) 
                {
                    String code = user.ConfirmationCode;
                    if (textboxCode.Text.Equals(code))
                    {
                        TitleLable.Content = "Email  confirmed\n";
                        user.ConfirmationCode = null;
                        dataContext.SaveChanges();
                        ConfirmContainer.Visibility = Visibility.Hidden;
                        CreateAccount_button.Visibility = Visibility.Hidden;
                        LogIn_button.Visibility = Visibility.Visible;
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
        }

        private void LogIn_button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new LogInWindow().ShowDialog();
            this.Close();
        }

        public void ErrorMessage()
        {
            MessageBoxA(
                IntPtr.Zero,
                "You need to fill email and password",
                "Error while log in",
                0x40
            );
            methodHandle.Free();
        }
        GCHandle methodHandle;

        public bool IsValidEmail(string email)
        {
            string pattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

            return Regex.IsMatch(email, pattern);
        }
    }
}