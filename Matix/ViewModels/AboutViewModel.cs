using System.Diagnostics;
using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string AppName => "Matix Player";
        public string Version => "Version 1.1.0";
        public string Description => "A beautiful Material Design 3 audio player.";
        public string Tech => "Created with Avalonia UI and LibVLCSharp.";

        public ICommand GoBackCommand { get; }
        public ICommand OpenGitHubCommand { get; }

        /// <summary>
        /// Ініціалізує новий екземпляр класу AboutViewModel.
        /// </summary>
        /// <param name="goBack">Дія для повернення до попереднього екрана.</param>
        public AboutViewModel(System.Action goBack)
        {
            GoBackCommand = new RelayCommand(goBack);
            OpenGitHubCommand = new RelayCommand(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/gllsss69/Matix",
                        UseShellExecute = true
                    });
                }
                catch { }
            });
        }
    }
}
