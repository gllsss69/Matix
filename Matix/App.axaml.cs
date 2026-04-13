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
    }
}