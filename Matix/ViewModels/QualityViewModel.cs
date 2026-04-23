using System.Collections.ObjectModel;
using System.Windows.Input;
using Matix.Commands;

namespace Matix.ViewModels
{
    public class QualityViewModel : ViewModelBase
    {
        private string _selectedQuality = "High (320 kbps)";

        public ObservableCollection<string> Qualities { get; } = new()
        {
            "Low (128 kbps)",
            "Medium (192 kbps)",
            "High (320 kbps)",
            "Lossless (FLAC)",
        };

        public string SelectedQuality
        {
            get => _selectedQuality;
            set { SetField(ref _selectedQuality, value); }
        }

        public ICommand GoBackCommand { get; }

        /// <summary>
        /// Ініціалізує новий екземпляр класу QualityViewModel.
        /// </summary>
        /// <param name="goBack">Дія для повернення до попереднього екрана.</param>
        public QualityViewModel(System.Action goBack)
        {
            GoBackCommand = new RelayCommand(goBack);
        }
    }
}
