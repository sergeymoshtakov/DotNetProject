using DotNetProject.Data;
using DotNetProject.Data.Entity;
using DotNetProject.View;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DotNetProject
{
    /// <summary>
    /// Логика взаимодействия для NetWorkWindow.xaml
    /// </summary>
    public partial class NetWorkWindow : Window
    {
        private Data.DataContext dataContext;
        private String userEmail;
        private List<NBURate> rates;
        private string[] popularCc = { "CHF", "USD", "EUR", "JPY", "CNY" };
        private WeatherForecast weatherForecast;
        public ObservableCollection<Pair> Pairs { get; set; }
        public List<Data.Entity.Message> MessagesView { get; set; }
        public ObservableCollection<Data.Entity.User> UsersView { get; set; }
        private ICollectionView usersListView;
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
        public NetWorkWindow(String email)
        {
            InitializeComponent();
            this.userEmail = email;
            Pairs = new ObservableCollection<Pair>();
            dataContext = App.dataContext;
            UsersView = new ObservableCollection<Data.Entity.User>();
            this.DataContext = this;
            weatherForecast = new WeatherForecast();
        }
        private void myProfileButton_Click(object sender, RoutedEventArgs e)
        {
            myProfileZone.Visibility = Visibility.Visible;
            friendsTable.Visibility = Visibility.Hidden;
            blockMessages.Visibility = Visibility.Hidden;
            newsZone.Visibility = Visibility.Hidden;
        }

        private void newsButton_Click(object sender, RoutedEventArgs e)
        {
            myProfileZone.Visibility = Visibility.Hidden;
            friendsTable.Visibility = Visibility.Hidden;
            blockMessages.Visibility = Visibility.Hidden;
            newsZone.Visibility = Visibility.Visible;
        }

        private void messagesButton_Click(object sender, RoutedEventArgs e)
        {
            myProfileZone.Visibility = Visibility.Hidden;
            friendsTable.Visibility = Visibility.Hidden;
            blockMessages.Visibility = Visibility.Visible;
            newsZone.Visibility = Visibility.Hidden;
        }

        private void friendsButton_Click(object sender, RoutedEventArgs e)
        {
            myProfileZone.Visibility = Visibility.Hidden;
            friendsTable.Visibility = Visibility.Visible;
            blockMessages.Visibility = Visibility.Hidden;
            newsZone.Visibility = Visibility.Hidden;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            var user = dataContext.Users.FirstOrDefault(u => u.Email == $"{userEmail}");
            if(user != null)
            {
                View.CRUDWindow dialog = new() { User = user };
                if (dialog.ShowDialog() ?? false)
                {
                    if (dialog.User != null)
                    {
                        if (dialog.IsDeleted)
                        {
                            var dep = dataContext.Users.Find(user.Id);
                            user.DeleteDt = DateTime.Now;
                            dataContext.SaveChanges();
                            newsButton.IsEnabled = false;
                            messagesButton.IsEnabled = false;
                            friendsButton.IsEnabled = false;
                        }
                        else
                        {
                            var dep = dataContext.Users.Find(user.Id);
                            if (dep != null)
                            {
                                dep.Name = user.Name;
                            }
                            dataContext.SaveChanges();
                            updateUserInfo();
                            newsButton.IsEnabled = true;
                            messagesButton.IsEnabled = true;
                            friendsButton.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private async Task loadRatesAsync()
        {
            HttpClient httpClient = new HttpClient();
            String body = await httpClient.GetStringAsync("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json");
            rates = JsonSerializer.Deserialize<List<NBURate>>(body);
            if (rates == null)
            {
                MessageBox.Show("Error desirializing");
                return;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var user = dataContext.Users.FirstOrDefault(u => u.Email == $"{userEmail}");
            if (user != null)
            {
                nameTextBox.Text = user.Name ?? "";
                surnameTextBox.Text = user.Surname ?? "";
                emailTextBox.Text = user.Email ?? "";
                statusTextBox.Text = user.Status ?? "";
                avatarTextBox.Text = user.Avatar ?? "";
                genderTextBox.Text = user.IdGender.ToString() == "bfc0cbac-d739-420a-990d-0f7e08c826f7" ? "Male" : "Female";
                birthdayTextBox.Text = user.Birthday.ToString();
                if(user.DeleteDt != null)
                {
                    newsButton.IsEnabled = false;
                    messagesButton.IsEnabled = false;
                    friendsButton.IsEnabled = false;
                }
                else
                {
                    newsButton.IsEnabled = true;
                    messagesButton.IsEnabled = true;
                    friendsButton.IsEnabled = true;
                }
            }
            await FetchUsersAsync();
            await ShowMessages();
            if (rates == null)
            {
                await loadRatesAsync();
            }
            if (rates == null)
            {
                return;
            }
            foreach (var rate in rates)
            {
                if (popularCc.Contains(rate.cc))
                {
                    exchangeRatesTextBlock.Text += $"1 {rate.cc} equals {rate.rate} UAH\n";
                }
            }
        }
        private void updateUserInfo()
        {
            var user = dataContext.Users.FirstOrDefault(u => u.Email == $"{userEmail}");
            if (user != null)
            {
                nameTextBox.Text = user.Name ?? "";
                surnameTextBox.Text = user.Surname ?? "";
                emailTextBox.Text = user.Email ?? "";
                statusTextBox.Text = user.Status ?? "";
                avatarTextBox.Text = user.Avatar ?? "";
                genderTextBox.Text = user.IdGender.ToString() == "bfc0cbac-d739-420a-990d-0f7e08c826f7" ? "Male" : "Female";
                birthdayTextBox.Text = user.Birthday.ToString();
            }
        }
        private void GenderButton_Click(object sender, RoutedEventArgs e)
        {
            var query = dataContext
                .Users
                .Where(u => u.DeleteDt == null)
                .Select(m => new Pair() { Key = $"{m.Surname} {m.Name[0]}.", Value = m.Gender.Name });
            Pairs.Clear();
            foreach (var pair in query)
            {
                Pairs.Add(pair);
            }
        }

        private void AgeButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var query = dataContext.Users
                .Where(u => u.DeleteDt == null)
                .Join(dataContext.Users, m1 => m1.Id, m2 => m2.Id, (m1, m2) => new Pair()
                {
                    Key = $"{m1.Surname} {m1.Name[0]}.",
                    Value = (today.Year - m2.Birthday.Year).ToString() // calculate age based on current date
                })
                .ToList()
                .OrderByDescending(d => int.Parse(d.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                Pairs.Add(pair);
            }
        }

        private void RegisteredButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var query = dataContext.Users.Where(u => u.DeleteDt == null).Join(dataContext.Users, m1 => m1.Id, m2 => m2.Id,
                (m1, m2) => new Pair()
                {
                    Key = $"{m1.Surname} {m1.Name[0]}.",
                    Value = (today - m2.CreateDt).Days.ToString()
                })
                .ToList()
                .OrderByDescending(d => int.Parse(d.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                pair.Value = pair.Value + " days ago";
                Pairs.Add(pair);
            }
        }
        
        private void GenderStatisticButton_Click(object sender, RoutedEventArgs e)
        {
            var query = dataContext.Genders.Select(d => new Pair()
            {
                Key = d.Name,
                Value = d.Users.Where(u => u.DeleteDt == null).Count().ToString()
            }).ToList()
                .OrderByDescending(pair => int.Parse(pair.Value));

            Pairs.Clear();
            foreach (var pair in query)
            {
                if (pair.Value == "0")
                {
                    pair.Value = "closed";
                }
                Pairs.Add(pair);
            }
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is Data.Entity.User user)
                {
                    View.UserView dialog = new() { User = user };
                    dialog.Show();
                }
            }
        }
        private async Task FetchUsersAsync()
        {
            try
            {
                await dataContext.Users.LoadAsync();
                UsersView = dataContext.Users.Local.ToObservableCollection();
                usersList.ItemsSource = UsersView;
                usersListView = CollectionViewSource.GetDefaultView(UsersView);
                usersListView.Filter = item => (item as Data.Entity.User)?.DeleteDt == null;
                usersListView.Filter = item => (item as Data.Entity.User)?.Email != userEmail;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching users: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowMessages()
        {
            try
            {
                await dataContext.Messages.LoadAsync();
                MessagesView = dataContext.Messages.Local.ToList();
                messageTextBlock.Text = string.Empty;
                foreach (var message in MessagesView)
                {
                    messageTextBlock.Text += $"{message.SendDT}\n{message.Sender.Name} {message.Sender.Surname}: {message.Content}\r\n";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while fetching users: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var user = dataContext.Users.FirstOrDefault(u => u.Email == $"{userEmail}");
            if (user != null)
            {
                if (messagesTextBox.Text != "")
                {
                    Message newMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        Content = messagesTextBox.Text,
                        SenderId = user.Id,
                        SendDT = DateTime.Now
                    };

                    dataContext.Messages.Add(newMessage);
                    dataContext.SaveChanges();
                    await ShowMessages();
                    messagesTextBox.Text = String.Empty;
                }
                else
                {
                    var method = new ThreadMethod(() =>
                    {
                        MessageBoxA(
                            IntPtr.Zero,
                            "Your message is empty\nPlease, write something",
                            "Error while sending message",
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
                var method = new ThreadMethod(() =>
                {
                    MessageBoxA(
                        IntPtr.Zero,
                        "Your user account is null",
                        "Error while sending message",
                        0x10
                    );
                    methodHandle.Free();
                });
                methodHandle = GCHandle.Alloc(method);
                NewThread(IntPtr.Zero, 0, method, IntPtr.Zero, 0, IntPtr.Zero);
            }
        }
        GCHandle methodHandle;

        private void searchForecast_Click(object sender, RoutedEventArgs e)
        {
            if (cityName.Text != null)
            {
                string city = cityName.Text;
                WeatherData weatherData = weatherForecast.GetWeatherData(city);
                weatherForecastBlock.Text = "";
                string forecastText = $"Feather forecast for city {city}:\n";
                weatherForecastBlock.Text += forecastText;
                weatherForecastBlock.Text += "Temperature: " + weatherData.Current.Temp_c.ToString() + "\n";
                weatherForecastBlock.Text += "Humidity: " + weatherData.Current.Humidity.ToString() + "\n";
                weatherForecastBlock.Text += "Wind speed: " + weatherData.Current.Wind_mph.ToString() + "\n";
                weatherForecastBlock.Text += "Local time: " + weatherData.Location.Localtime + "\n";
                weatherForecastBlock.Text += "Condition: " + weatherData.Current.Condition.Text + "\n";
            }
            else
            {
                weatherForecastBlock.Text = "";
                weatherForecastBlock.Text = "Enter city first";
            }
        }
    }
    public class Pair
    {
        public String Key { get; set; } = null!;
        public String? Value { get; set; }
    }
    public class NBURate
    {
        public int r030 { get; set; }
        public string txt { get; set; }
        public double rate { get; set; }
        public string cc { get; set; }
        public string exchangedate { get; set; }
    }
}
