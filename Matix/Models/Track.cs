using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Matix.Models
{
    public class Track : INotifyPropertyChanged
    {
        public string FilePath { get; set; } = string.Empty;
        private string _title = "Unknown Title";
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private Bitmap? _albumArt;
        public Bitmap? AlbumArt
        {
            get => _albumArt;
            set
            {
                if (_albumArt != value)
                {
                    _albumArt = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasAlbumArt));
                }
            }
        }

        public bool HasAlbumArt => AlbumArt != null;
        
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Викликає подію PropertyChanged для сповіщення про зміну властивості.
        /// </summary>
        /// <param name="propertyName">Назва властивості, що змінилася.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Конструктор за замовчуванням для класу Track.
        /// </summary>
        public Track() { }

        /// <summary>
        /// Ініціалізує новий екземпляр класу Track із вказаним шляхом до файлу та назвою.
        /// </summary>
        /// <param name="filePath">Повний шлях до аудіофайлу.</param>
        /// <param name="title">Назва треку.</param>
        public Track(string filePath, string title)
        {
            FilePath = filePath;
            Title = title;
        }

        /// <summary>
        /// Асинхронно завантажує обкладинку альбому з метаданих файлу.
        /// </summary>
        /// <returns>Завдання, що представляє асинхронну операцію.</returns>
        public async Task LoadAlbumArtAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    using var tfile = TagLib.File.Create(FilePath);
                    if (tfile.Tag.Pictures.Length > 0)
                    {
                        var bin = (byte[])(tfile.Tag.Pictures[0].Data.Data);
                        using var stream = new MemoryStream(bin);
                        var bitmap = Bitmap.DecodeToWidth(stream, 64);
                        
                        Dispatcher.UIThread.Post(() => AlbumArt = bitmap);
                    }
                }
                catch { }
            });
        }
    }
}
