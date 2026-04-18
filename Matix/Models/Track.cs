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
        public string Title { get; set; } = "Unknown Title";

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Track() { }

        public Track(string filePath, string title)
        {
            FilePath = filePath;
            Title = title;
        }

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
