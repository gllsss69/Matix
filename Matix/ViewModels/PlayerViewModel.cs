using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using Matix.Commands;
using Matix.Models;
using NAudio.Wave;
using System.Threading.Tasks;

namespace Matix.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        // Navigation 
        private readonly Action<ViewModelBase> _navigateTo;

        public ICommand GoToAboutCommand { get; }
        public ICommand GoToLanguageCommand { get; }
        public ICommand GoToQualityCommand { get; }
        public ICommand GoToThemeCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand TogglePlaylistCommand { get; }
        public ICommand RemoveTrackCommand { get; }
        public ICommand ToggleShuffleCommand { get; }

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set => SetField(ref _isSettingsOpen, value);
        }

        private bool _isPlaylistOpen;
        public bool IsPlaylistOpen
        {
            get => _isPlaylistOpen;
            set => SetField(ref _isPlaylistOpen, value);
        }

        public ObservableCollection<Track> Playlist { get; } = new();

        private Track? _selectedTrack;
        public Track? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                SetField(ref _selectedTrack, value);
                if (value != null && value.FilePath != _loadedFilePath)
                {
                    LoadAudio(value.FilePath);
                    TogglePlay(); // optional auto-play
                }
            }
        }

        private string _loadedFilePath = string.Empty;

        // Playback
        private bool _isRepeatEnabled;
        public bool IsRepeatEnabled
        {
            get => _isRepeatEnabled;
            set => SetField(ref _isRepeatEnabled, value);
        }

        private bool _isShuffleEnabled;
        public bool IsShuffleEnabled
        {
            get => _isShuffleEnabled;
            set => SetField(ref _isShuffleEnabled, value);
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { SetField(ref _isPlaying, value); OnPropertyChanged(nameof(PlayIcon)); }
        }

        public string PlayIcon => IsPlaying ? "Pause" : "PlayArrow";

        private string _songTitle = "No track loaded";
        public string SongTitle
        {
            get => _songTitle;
            set => SetField(ref _songTitle, value);
        }

        private string _artistName = "Unknown Artist";
        public string ArtistName
        {
            get => _artistName;
            set => SetField(ref _artistName, value);
        }

        private Bitmap? _albumArt;
        public Bitmap? AlbumArt
        {
            get => _albumArt;
            set => SetField(ref _albumArt, value);
        }

        private TimeSpan _currentTime = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                SetField(ref _currentTime, value);
                OnPropertyChanged(nameof(CurrentTimeString));
                OnPropertyChanged(nameof(ProgressWidth));
                OnPropertyChanged(nameof(ProgressThumbMargin));

                if (_audioFile != null && Math.Abs(_audioFile.CurrentTime.TotalSeconds - value.TotalSeconds) > 0.5)
                    _audioFile.CurrentTime = value;
            }
        }

        private TimeSpan _totalTime = TimeSpan.Zero;
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set { SetField(ref _totalTime, value); OnPropertyChanged(nameof(TotalTimeString)); }
        }

        public string CurrentTimeString => $"{(int)CurrentTime.TotalMinutes}:{CurrentTime.Seconds:D2}";
        public string TotalTimeString => $"{(int)TotalTime.TotalMinutes}:{TotalTime.Seconds:D2}";

        // Progress bar pixel widths — updated by the view via SizeChanged
        private double _maxProgressWidth = 550.0;
        public double MaxProgressWidth
        {
            get => _maxProgressWidth;
            set
            {
                if (Math.Abs(_maxProgressWidth - value) < 0.5) return;
                _maxProgressWidth = value;
                OnPropertyChanged(nameof(ProgressWidth));
                OnPropertyChanged(nameof(ProgressThumbMargin));
            }
        }

        private double _maxVolumeWidth = 120.0;
        public double MaxVolumeWidth
        {
            get => _maxVolumeWidth;
            set
            {
                if (Math.Abs(_maxVolumeWidth - value) < 0.5) return;
                _maxVolumeWidth = value;
                OnPropertyChanged(nameof(VolumeWidth));
                OnPropertyChanged(nameof(VolumeThumbMargin));
                OnPropertyChanged(nameof(VolumeBadgeMargin));
            }
        }

        public double ProgressWidth => TotalTime.TotalSeconds == 0 ? 0
            : Math.Clamp((CurrentTime.TotalSeconds / TotalTime.TotalSeconds) * MaxProgressWidth, 0, MaxProgressWidth);
        public Thickness ProgressThumbMargin => new(ProgressWidth, 0, 0, 0);

        // Volume
        private double _volume = 50;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0, 100);
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeText));
                OnPropertyChanged(nameof(VolumeWidth));
                OnPropertyChanged(nameof(VolumeThumbMargin));
                OnPropertyChanged(nameof(VolumeBadgeMargin));
                if (_waveOut != null) _waveOut.Volume = (float)(_volume / 100.0);
            }
        }
        public string    VolumeText        => Math.Round(Volume).ToString();
        public double    VolumeWidth        => (Volume / 100.0) * MaxVolumeWidth;
        public Thickness VolumeThumbMargin  => new(Math.Max(0, VolumeWidth - 2), 0, 0, 0);
        public Thickness VolumeBadgeMargin  => new(VolumeWidth, 0, 0, 0);

        public string LibraryPath => App.Settings.LastOpenedFolder;

        // Commands
        public ICommand PlayPauseCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand ToggleRepeatCommand { get; }

        // NAudio
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFile;
        private readonly DispatcherTimer _timer;

       
        public PlayerViewModel(Action<ViewModelBase> navigateTo)
        {
            _navigateTo = navigateTo;

            // Playback
            PlayPauseCommand = new RelayCommand(TogglePlay);
            NextCommand = new RelayCommand(NextTrack);
            PrevCommand = new RelayCommand(PrevTrack);
            ToggleRepeatCommand = new RelayCommand(() => IsRepeatEnabled = !IsRepeatEnabled);

            // Settings menu navigation
            ToggleSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
            GoToAboutCommand = new RelayCommand(() => { IsSettingsOpen = false; _navigateTo(new AboutViewModel(GoBack)); });
            GoToLanguageCommand = new RelayCommand(() => { IsSettingsOpen = false; _navigateTo(new LanguageViewModel(GoBack)); });
            GoToQualityCommand = new RelayCommand(() => { IsSettingsOpen = false; _navigateTo(new QualityViewModel(GoBack)); });
            GoToThemeCommand = new RelayCommand(() => { IsSettingsOpen = false; _navigateTo(new ThemeViewModel(GoBack)); });

            TogglePlaylistCommand = new RelayCommand(() => IsPlaylistOpen = !IsPlaylistOpen);
            RemoveTrackCommand = new RelayCommand<Track>(RemoveTrack);
            ToggleShuffleCommand = new RelayCommand(() => IsShuffleEnabled = !IsShuffleEnabled);

            // Timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += (_, _) =>
            {
                if (IsPlaying && _audioFile != null)
                {
                    CurrentTime = _audioFile.CurrentTime;
                    if (_waveOut?.PlaybackState == PlaybackState.Stopped)
                    {
                        if (IsRepeatEnabled)
                        {
                            _audioFile.Position = 0;
                            CurrentTime = TimeSpan.Zero;
                            _waveOut.Play();
                        }
                        else
                        {
                            NextTrack();
                        }
                    }
                }
            };

            // Load last opened folder
            if (!string.IsNullOrWhiteSpace(App.Settings.LastOpenedFolder) && Directory.Exists(App.Settings.LastOpenedFolder))
            {
                LoadFolder(App.Settings.LastOpenedFolder);
            }
        }

        public void LoadFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return;

            App.Settings.LastOpenedFolder = folderPath;
            App.Settings.Save();
            OnPropertyChanged(nameof(LibraryPath));

            Playlist.Clear();
            var extensions = new[] { ".mp3", ".wav", ".aiff", ".wma", ".m4a" };
            var files = Directory.GetFiles(folderPath)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f);

            foreach (var file in files)
            {
                string title = Path.GetFileNameWithoutExtension(file);
                // Attempt to get title from tags if desired, but keep it simple
                try
                {
                    using var tfile = TagLib.File.Create(file);
                    if (!string.IsNullOrWhiteSpace(tfile.Tag.Title))
                    {
                        title = tfile.Tag.Title;
                    }
                }
                catch { }

                var track = new Track(file, title);
                Playlist.Add(track);
                _ = track.LoadAlbumArtAsync();
            }

            if (Playlist.Any())
            {
                var track = Playlist.First();
                _selectedTrack = track;
                OnPropertyChanged(nameof(SelectedTrack));
                LoadAudio(track.FilePath);
            }
        }

        private void GoBack() => _navigateTo(this);  // navigate back to player

        // Audio loading
        public void LoadAudio(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _audioFile?.Dispose();

            try
            {
                _loadedFilePath = filePath;
                _audioFile = new AudioFileReader(filePath);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioFile);
                _waveOut.Volume = (float)(Volume / 100.0);

                TotalTime = _audioFile.TotalTime;
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;

                try
                {
                    using var file = TagLib.File.Create(filePath);
                    SongTitle = string.IsNullOrWhiteSpace(file.Tag.Title) 
                        ? Path.GetFileNameWithoutExtension(filePath) 
                        : file.Tag.Title;

                    ArtistName = file.Tag.Performers?.Length > 0 
                        ? string.Join(", ", file.Tag.Performers) 
                        : "Unknown Artist";

                    if (file.Tag.Pictures.Length > 0)
                    {
                        var bin = (byte[])(file.Tag.Pictures[0].Data.Data);
                        using var stream = new MemoryStream(bin);
                        AlbumArt = new Bitmap(stream);
                    }
                    else
                    {
                        AlbumArt = null;
                    }
                }
                catch
                {
                    SongTitle = Path.GetFileNameWithoutExtension(filePath);
                    ArtistName = "Unknown Artist";
                    AlbumArt = null;
                }
            }
            catch { }
        }

        // Playback logic
        private void TogglePlay()
        {
            if (_waveOut == null) return;
            IsPlaying = !IsPlaying;
            if (IsPlaying) { _waveOut.Play();  _timer.Start(); }
            else { _waveOut.Pause(); _timer.Stop();  }
        }

        private void NextTrack()
        {
            if (Playlist.Count == 0) return;

            if (IsShuffleEnabled && Playlist.Count > 1)
            {
                var rng = new Random();
                var currentIndex = Playlist.IndexOf(SelectedTrack!);
                int nextIndex;
                do
                {
                    nextIndex = rng.Next(Playlist.Count);
                } while (nextIndex == currentIndex);

                SelectedTrack = Playlist[nextIndex];
            }
            else
            {
                var index = Playlist.IndexOf(SelectedTrack!);
                if (index == -1 || index == Playlist.Count - 1)
                    SelectedTrack = Playlist.FirstOrDefault();
                else
                    SelectedTrack = Playlist[index + 1];
            }
        }

        private void PrevTrack()
        {
            if (Playlist.Count == 0) return;
            if (CurrentTime.TotalSeconds > 3)
            {
                CurrentTime = TimeSpan.Zero; return;
            }

            if (IsShuffleEnabled && Playlist.Count > 1)
            {
                var rng = new Random();
                var currentIndex = Playlist.IndexOf(SelectedTrack!);
                int prevIndex;
                do
                {
                    prevIndex = rng.Next(Playlist.Count);
                } while (prevIndex == currentIndex);

                SelectedTrack = Playlist[prevIndex];
            }
            else
            {
                var index = Playlist.IndexOf(SelectedTrack!);
                if (index == -1 || index == 0)
                    SelectedTrack = Playlist.LastOrDefault();
                else
                    SelectedTrack = Playlist[index - 1];
            }
        }

        private void RemoveTrack(object? param)
        {
            if (param is Track track)
            {
                Playlist.Remove(track);
                if (SelectedTrack == track)
                    NextTrack();
            }
        }
    }
}
