using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;
using NAudio.Wave;

namespace Matix
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isPlaying;
        public bool IsPlaying 
        { 
            get => _isPlaying; 
            set { _isPlaying = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayIcon)); } 
        }

        public string PlayIcon => IsPlaying ? "Pause" : "PlayArrow";

        private TimeSpan _currentTime = TimeSpan.FromSeconds(0);
        public TimeSpan CurrentTime 
        { 
            get => _currentTime; 
            set { 
                _currentTime = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CurrentTimeString));
                OnPropertyChanged(nameof(ProgressWidth));
                OnPropertyChanged(nameof(ProgressThumbMargin));
                
                if (_audioFile != null && Math.Abs(_audioFile.CurrentTime.TotalSeconds - value.TotalSeconds) > 0.5)
                {
                    _audioFile.CurrentTime = value;
                }
            } 
        }

        private TimeSpan _totalTime = TimeSpan.Zero;
        public TimeSpan TotalTime
        {
            get => _totalTime;
            set { _totalTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalTimeString)); }
        }

        public string CurrentTimeString => $"{(int)CurrentTime.TotalMinutes}:{CurrentTime.Seconds:D2}";
        public string TotalTimeString => $"{(int)TotalTime.TotalMinutes}:{TotalTime.Seconds:D2}";

        // UI Pixel Max Widths based on roughly the width in XAML
        public const double MaxProgressWidth = 550.0;
        public const double MaxVolumeWidth = 120.0;

        public double ProgressWidth => TotalTime.TotalSeconds == 0 ? 0 : Math.Clamp((CurrentTime.TotalSeconds / TotalTime.TotalSeconds) * MaxProgressWidth, 0, MaxProgressWidth);
        public Thickness ProgressThumbMargin => new Thickness(ProgressWidth, 0, 0, 0);

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
                if (_waveOut != null)
                {
                    _waveOut.Volume = (float)(_volume / 100.0);
                }
            }
        }
        public string VolumeText => Math.Round(Volume).ToString();
        public double VolumeWidth => (Volume / 100.0) * MaxVolumeWidth;
        public Thickness VolumeThumbMargin => new Thickness(Math.Max(0, VolumeWidth - 2), 0, 0, 0);
        public Thickness VolumeBadgeMargin => new Thickness(VolumeWidth, 0, 0, 0);

        private bool _isSettingsOpen;
        public bool IsSettingsOpen
        {
            get => _isSettingsOpen;
            set { _isSettingsOpen = value; OnPropertyChanged(); }
        }

        private bool _isAboutVisible;
        public bool IsAboutVisible
        {
            get => _isAboutVisible;
            set { _isAboutVisible = value; OnPropertyChanged(); }
        }

        public ICommand PlayPauseCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand ToggleSettingsCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand CloseAboutCommand { get; }
        public ICommand OpenGitHubCommand { get; }

        private DispatcherTimer _timer;

        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFile;

        public void LoadAudio(string filePath)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _audioFile?.Dispose();

            try
            {
                _audioFile = new AudioFileReader(filePath);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_audioFile);
                _waveOut.Volume = (float)(Volume / 100.0);
                
                TotalTime = _audioFile.TotalTime;
                CurrentTime = TimeSpan.Zero;
                IsPlaying = false;
            }
            catch (Exception)
            {
            }
        }

        public MainWindowViewModel()
        {
            PlayPauseCommand = new RelayCommand(TogglePlay);
            NextCommand = new RelayCommand(NextTrack);
            PrevCommand = new RelayCommand(PrevTrack);
            ToggleSettingsCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
            ShowAboutCommand = new RelayCommand(() => { IsSettingsOpen = false; IsAboutVisible = true; });
            CloseAboutCommand = new RelayCommand(() => IsAboutVisible = false);
            OpenGitHubCommand = new RelayCommand(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/gllsss69/Matix",
                        UseShellExecute = true
                    });
                }
                catch { }
            });

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _timer.Tick += (s, e) =>
            {
                if (IsPlaying && _audioFile != null)
                {
                    CurrentTime = _audioFile.CurrentTime;
                    if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Stopped)
                    {
                        IsPlaying = false;
                        CurrentTime = TimeSpan.Zero;
                        _audioFile.Position = 0;
                    }
                }
            };
        }

        private void TogglePlay()
        {
            if (_waveOut == null) return;

            IsPlaying = !IsPlaying;
            if (IsPlaying)
            {
                _waveOut.Play();
                _timer.Start();
            }
            else 
            {
                _waveOut.Pause();
                _timer.Stop();
            }
        }

        private void NextTrack()
        {
            CurrentTime = TimeSpan.Zero;
            TotalTime = TimeSpan.FromMinutes(new Random().Next(2, 5));
            if (IsPlaying)
                _timer.Start();
        }

        private void PrevTrack()
        {
            CurrentTime = TimeSpan.Zero;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
    }
}
