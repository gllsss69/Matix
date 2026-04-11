using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Matix.ViewModels;

namespace Matix.Views
{
    public partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            InitializeComponent();
        }

        private PlayerViewModel? VM => DataContext as PlayerViewModel;

        private void CloseSettings_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (VM is { } vm) vm.IsSettingsOpen = false;
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

            if (files.Count > 0 && VM is { } vm)
                vm.LoadAudio(files[0].Path.LocalPath);
        }

        private void UpdateProgress(PointerEventArgs e, Visual relativeTo)
        {
            if (VM is not { } vm) return;
            var point = e.GetCurrentPoint(relativeTo);
            if (point.Properties.IsLeftButtonPressed)
            {
                double ratio = Math.Clamp(point.Position.X / vm.MaxProgressWidth, 0, 1);
                vm.CurrentTime = TimeSpan.FromSeconds(vm.TotalTime.TotalSeconds * ratio);
            }
        }

        private void ProgressGrid_PointerMoved(object? sender, PointerEventArgs e)
            => UpdateProgress(e, (Visual)sender!);
        private void ProgressGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
            => UpdateProgress(e, (Visual)sender!);

        private void UpdateVolume(PointerEventArgs e, Visual relativeTo)
        {
            if (VM is not { } vm) return;
            var point = e.GetCurrentPoint(relativeTo);
            if (point.Properties.IsLeftButtonPressed)
            {
                double ratio = Math.Clamp(point.Position.X / vm.MaxVolumeWidth, 0, 1);
                vm.Volume = ratio * 100;
            }
        }

        private void VolumeGrid_PointerMoved(object? sender, PointerEventArgs e)
            => UpdateVolume(e, (Visual)sender!);
        private void VolumeGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
            => UpdateVolume(e, (Visual)sender!);

        private void ProgressTrackGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (VM is { } vm && e.NewSize.Width > 0)
                vm.MaxProgressWidth = e.NewSize.Width;
        }

        private void VolumeTrackGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (VM is { } vm && e.NewSize.Width > 0)
                vm.MaxVolumeWidth = e.NewSize.Width;
        }
    }
}
