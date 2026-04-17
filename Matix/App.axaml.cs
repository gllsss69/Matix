using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Matix.Models;

namespace Matix
{
    public partial class App : Application
    {
        private static AppSettings? _settings;
        public static AppSettings Settings => _settings ??= AppSettings.Load();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ApplyTheme(Settings.Theme);
            ApplyLanguage(Settings.Language);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void ApplyTheme(string themeName)
        {
            if (Current == null) return;

            if (themeName == "Dark")
                Current.RequestedThemeVariant = ThemeVariant.Dark;
            else if (themeName == "Light")
                Current.RequestedThemeVariant = ThemeVariant.Light;
            else
                Current.RequestedThemeVariant = ThemeVariant.Default;

            Settings.Theme = themeName;
            Settings.Save();
        }

        public static void ApplyLanguage(string languageName)
        {
            if (Current == null) return;

            string dictName = "en";
            switch (languageName)
            {
                case "Czech": dictName = "cs"; break;
                case "English": dictName = "en"; break;
                case "German": dictName = "de"; break;
                case "Japanese": dictName = "ja"; break;
                case "Polish": dictName = "pl"; break;
                case "Slovak": dictName = "sk"; break;
                case "Ukrainian": dictName = "uk"; break;
            }

            var uri = new System.Uri($"avares://Matix/Resources/Langs/{dictName}.axaml");
            var include = new Avalonia.Markup.Xaml.Styling.ResourceInclude(uri) { Source = uri };
            
            // Remove the old language dictionary if present (we assume it's at index 1 or we search for it)
            var mergedDicts = Current.Resources.MergedDictionaries;
            for (int i = mergedDicts.Count - 1; i >= 0; i--)
            {
                if (mergedDicts[i] is Avalonia.Markup.Xaml.Styling.ResourceInclude ri && ri.Source?.ToString().Contains("/Langs/") == true)
                {
                    mergedDicts.RemoveAt(i);
                }
            }
            
            mergedDicts.Add(include);

            Settings.Language = languageName;
            Settings.Save();
        }
    }
}