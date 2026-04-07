using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;

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
            } 
        }

        private TimeSpan _totalTime = TimeSpan.FromMinutes(3);
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
            }
        }
        public string VolumeText => Math.Round(Volume).ToString();
        public double VolumeWidth => (Volume / 100.0) * MaxVolumeWidth;
        public Thickness VolumeThumbMargin => new Thickness(Math.Max(0, VolumeWidth - 2), 0, 0, 0);
        public Thickness VolumeBadgeMargin => new Thickness(VolumeWidth, 0, 0, 0);

        public ICommand PlayPauseCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }

        private DispatcherTimer _timer;

        public MainWindowViewModel()
        {
            PlayPauseCommand = new RelayCommand(TogglePlay);
            NextCommand = new RelayCommand(NextTrack);
            PrevCommand = new RelayCommand(PrevTrack);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                if (IsPlaying)
                {
                    if (CurrentTime < TotalTime)
                        CurrentTime = CurrentTime.Add(TimeSpan.FromSeconds(1));
                    else
                    {
                        IsPlaying = false;
                        CurrentTime = TimeSpan.Zero;
                    }
                }
            };
        }

        private void TogglePlay()
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying) _timer.Start();
            else _timer.Stop();
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
