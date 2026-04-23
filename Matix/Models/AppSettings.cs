using System.IO;
using System.Text.Json;

namespace Matix.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "English";
        public string LastOpenedFolder { get; set; } = string.Empty;
        public double Volume { get; set; } = 50.0;

        private const string SettingsFile = "settings.json";

        /// <summary>
        /// Завантажує налаштування програми з файлу JSON.
        /// </summary>
        /// <returns>Об'єкт AppSettings із завантаженими параметрами або новими значеннями за замовчуванням.</returns>
        public static AppSettings Load()
        {
            if (!File.Exists(SettingsFile))
                return new AppSettings();

            try
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        /// <summary>
        /// Зберігає поточні налаштування програми у файл JSON.
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                // Ignore save errors for now
            }
        }
    }
}
