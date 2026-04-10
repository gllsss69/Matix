using System.Collections.ObjectModel;
using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class LanguageViewModel : ViewModelBase
    {
        private string _selectedLanguage = "English";

        public ObservableCollection<string> Languages { get; } = new()
        {
            "English",
            "Ukrainian",
            "German",
            "French",
            "Spanish",
            "Polish",
            "Japanese",
        };

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { SetField(ref _selectedLanguage, value); }
        }

        public ICommand GoBackCommand { get; }

        public LanguageViewModel(System.Action goBack)
        {
            GoBackCommand = new RelayCommand(goBack);
        }
    }
}
