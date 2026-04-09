using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia;
using System;

namespace Matix
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void CloseSettings_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsSettingsOpen = false;
            }
        }

        private async void OpenFileButton_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Audio File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Audio Files")
                    {
                        Patterns = new[] { "*.mp3", "*.wav", "*.m4a", "*.ogg", "*.flac", "*.wma" }
                    }
                }
            });

            if (files.Count > 0 && DataContext is MainWindowViewModel vm)
            {
                vm.LoadAudio(files[0].Path.LocalPath);
            }
        }

        private void UpdateProgress(PointerEventArgs e, Visual relativeTo)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var point = e.GetCurrentPoint(relativeTo);
                if (point.Properties.IsLeftButtonPressed)
                {
                    double ratio = Math.Clamp(point.Position.X / MainWindowViewModel.MaxProgressWidth, 0, 1);
                    vm.CurrentTime = TimeSpan.FromSeconds(vm.TotalTime.TotalSeconds * ratio);
                }
            }
        }

        private void ProgressGrid_PointerMoved(object? sender, PointerEventArgs e) => UpdateProgress(e, (Visual)sender!);
        private void ProgressGrid_PointerPressed(object? sender, PointerPressedEventArgs e) => UpdateProgress(e, (Visual)sender!);

        private void UpdateVolume(PointerEventArgs e, Visual relativeTo)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                var point = e.GetCurrentPoint(relativeTo);
                if (point.Properties.IsLeftButtonPressed)
                {
                    double ratio = Math.Clamp(point.Position.X / MainWindowViewModel.MaxVolumeWidth, 0, 1);
                    vm.Volume = ratio * 100;
                }
            }
        }

        private void VolumeGrid_PointerMoved(object? sender, PointerEventArgs e) => UpdateVolume(e, (Visual)sender!);
        private void VolumeGrid_PointerPressed(object? sender, PointerPressedEventArgs e) => UpdateVolume(e, (Visual)sender!);
    }
}