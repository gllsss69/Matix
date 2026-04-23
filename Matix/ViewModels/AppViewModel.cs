using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    /// <summary>
    /// Кореневий ViewModel. Керує навігацією між сторінками.
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

        /// <summary>
        /// Ініціалізує новий екземпляр класу AppViewModel. Налаштовує плеєр та початкову сторінку.
        /// </summary>
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

        /// <summary>
        /// Переходить на вказану сторінку.
        /// </summary>
        /// <param name="page">Модель представлення сторінки для переходу.</param>
        public void NavigateTo(ViewModelBase page) => CurrentPage = page;

        /// <summary>
        /// Переходить на головну сторінку плеєра.
        /// </summary>
        public void NavigateToPlayer()  => CurrentPage = Player;
        /// <summary>
        /// Переходить на сторінку "Про програму".
        /// </summary>
        public void NavigateToAbout()   => CurrentPage = new AboutViewModel(NavigateToPlayer);
        /// <summary>
        /// Переходить на сторінку вибору мови.
        /// </summary>
        public void NavigateToLanguage()=> CurrentPage = new LanguageViewModel(NavigateToPlayer);
        /// <summary>
        /// Переходить на сторінку налаштування якості.
        /// </summary>
        public void NavigateToQuality() => CurrentPage = new QualityViewModel(NavigateToPlayer);
        /// <summary>
        /// Переходить на сторінку вибору теми.
        /// </summary>
        public void NavigateToTheme()   => CurrentPage = new ThemeViewModel(NavigateToPlayer);
    }
}
