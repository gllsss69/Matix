using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    /// <summary>
    /// Root ViewModel. Manages navigation between pages.
    /// </summary>
    public class AppViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => SetField(ref _currentPage, value);
        }

        public PlayerViewModel Player { get; }

        public AppViewModel()
        {
            Player = new PlayerViewModel(NavigateTo);
            _currentPage = Player;
            Player.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PlayerViewModel.IsPlaylistOpen))
                {
                    OnPropertyChanged(nameof(AppMinWidth));
                }
            };
        }

        public double AppMinWidth => Player.IsPlaylistOpen ? 1100 : 800;

        public void NavigateTo(ViewModelBase page) => CurrentPage = page;

        public void NavigateToPlayer()  => CurrentPage = Player;
        public void NavigateToAbout()   => CurrentPage = new AboutViewModel(NavigateToPlayer);
        public void NavigateToLanguage()=> CurrentPage = new LanguageViewModel(NavigateToPlayer);
        public void NavigateToQuality() => CurrentPage = new QualityViewModel(NavigateToPlayer);
        public void NavigateToTheme()   => CurrentPage = new ThemeViewModel(NavigateToPlayer);
    }
}
