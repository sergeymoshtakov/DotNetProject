using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace DotNetProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static String configFilename = "email-settings.json";
        private static JsonElement? settings = null;
        public static String? Host = getConfiguration("smtp:host");
        public static String? connectionString = getConfiguration("db:connectionstring");
        public static Data.DataContext dataContext = new Data.DataContext();
        public static String? getConfiguration(String name)
        {
            if (settings == null)
            {
                if (!System.IO.File.Exists(configFilename))
                {
                    MessageBox.Show(
                        $"Configuration file '{configFilename}' not found ",
                        "Operation cannot be ended",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return null;
                }
                try
                {
                    settings = JsonSerializer.Deserialize<dynamic>(System.IO.File.ReadAllText(configFilename));
                }
                catch (JsonException)
                {
                    MessageBox.Show("Wrong JSON file configuration!!!");
                    return null;
                }
            }
            JsonElement? jsonElement = settings;
            try
            {
                foreach (String key in name.Split(':'))
                {
                    jsonElement = jsonElement?.GetProperty(key);
                }
            }
            catch
            {
                return null;
            }
            return jsonElement?.GetString();
        }
    }
}
