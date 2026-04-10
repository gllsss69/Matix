using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class ThemeViewModel : ViewModelBase
    {
        private string _selectedTheme = "Purple (Default)";

        public string[] Themes { get; } =
        {
            "Purple (Default)",
            "Indigo",
            "Blue",
            "Teal",
            "Green",
            "Dark",
        };

        public string SelectedTheme
        {
            get => _selectedTheme;
            set { SetField(ref _selectedTheme, value); }
        }

        public ICommand GoBackCommand { get; }

        public ThemeViewModel(System.Action goBack)
        {
            GoBackCommand = new RelayCommand(goBack);
        }
    }
}
