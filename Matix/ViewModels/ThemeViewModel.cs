using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class ThemeViewModel : ViewModelBase
    {
        private string _selectedTheme;

        public string[] Themes { get; } =
        {
            "Light",
            "Dark",
        };

        public string SelectedTheme
        {
            get => _selectedTheme;
            set 
            { 
                if (SetField(ref _selectedTheme, value))
                {
                    App.ApplyTheme(value);
                }
            }
        }

        public ICommand GoBackCommand { get; }

        /// <summary>
        /// Ініціалізує новий екземпляр класу ThemeViewModel.
        /// </summary>
        /// <param name="goBack">Дія для повернення до попереднього екрана.</param>
        public ThemeViewModel(System.Action goBack)
        {
            _selectedTheme = App.Settings.Theme;
            GoBackCommand = new RelayCommand(goBack);
        }
    }
}
