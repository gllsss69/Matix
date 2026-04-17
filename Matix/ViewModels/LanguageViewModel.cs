using System.Collections.ObjectModel;
using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class LanguageViewModel : ViewModelBase
    {
        private string _selectedLanguage = App.Settings.Language;

        public ObservableCollection<string> Languages { get; } = new()
        {
            "Czech",
            "English",
            "German",
            "Japanese",
            "Polish",
            "Slovak",
            "Ukrainian",
        };

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set 
            { 
                if (SetField(ref _selectedLanguage, value))
                {
                    App.ApplyLanguage(value);
                }
            }
        }

        public ICommand GoBackCommand { get; }

        public LanguageViewModel(System.Action goBack)
        {
            GoBackCommand = new RelayCommand(goBack);
        }
    }
}
