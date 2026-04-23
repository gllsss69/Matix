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
        /// <summary>
        /// Конструктор класу PlayerView. Ініціалізує компоненти представлення.
        /// </summary>
        public PlayerView()
        {
            InitializeComponent();
        }

        private PlayerViewModel? VM => DataContext as PlayerViewModel;

        /// <summary>
        /// Обробник події натискання для закриття панелі налаштувань.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події вказівника.</param>
        private void CloseSettings_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (VM is { } vm) vm.IsSettingsOpen = false;
        }

        /// <summary>
        /// Обробник події натискання кнопки для відкриття аудіофайлу.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події маршрутизації.</param>
        private async void OpenFileButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VM is { } vmOpen) vmOpen.IsSettingsOpen = false;

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
            {
                vm.LoadAudio(files[0].Path.LocalPath);
                vm.Playlist.Clear();
                var track = new Models.Track(files[0].Path.LocalPath, System.IO.Path.GetFileNameWithoutExtension(files[0].Path.LocalPath));
                vm.Playlist.Add(track);
                _ = track.LoadAlbumArtAsync();
                vm.SelectedTrack = track;
            }
        }

        /// <summary>
        /// Обробник події натискання кнопки для вибору папки з музикою.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події маршрутизації.</param>
        private async void OpenFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0 && VM is { } vm)
                vm.LoadFolder(folders[0].Path.LocalPath);
        }

        /// <summary>
        /// Оновлює прогрес відтворення на основі положення вказівника.
        /// </summary>
        /// <param name="e">Аргументи події вказівника.</param>
        /// <param name="relativeTo">Візуальний елемент, відносно якого розраховуються координати.</param>
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

        /// <summary>
        /// Обробник переміщення вказівника по сітці прогресу.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події вказівника.</param>
        private void ProgressGrid_PointerMoved(object? sender, PointerEventArgs e)
            => UpdateProgress(e, (Visual)sender!);
        /// <summary>
        /// Обробник натискання вказівника на сітці прогресу.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події вказівника.</param>
        private void ProgressGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
            => UpdateProgress(e, (Visual)sender!);

        /// <summary>
        /// Оновлює рівень гучності на основі положення вказівника.
        /// </summary>
        /// <param name="e">Аргументи події вказівника.</param>
        /// <param name="relativeTo">Візуальний елемент, відносно якого розраховуються координати.</param>
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

        /// <summary>
        /// Обробник переміщення вказівника по сітці гучності.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події вказівника.</param>
        private void VolumeGrid_PointerMoved(object? sender, PointerEventArgs e)
            => UpdateVolume(e, (Visual)sender!);
        /// <summary>
        /// Обробник натискання вказівника на сітці гучності.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події вказівника.</param>
        private void VolumeGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
            => UpdateVolume(e, (Visual)sender!);

        /// <summary>
        /// Обробник зміни розміру сітки треку прогресу. Оновлює максимальну ширину в моделі представлення.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події зміни розміру.</param>
        private void ProgressTrackGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (VM is { } vm && e.NewSize.Width > 0)
                vm.MaxProgressWidth = e.NewSize.Width;
        }

        /// <summary>
        /// Обробник зміни розміру сітки треку гучності. Оновлює максимальну ширину гучності в моделі представлення.
        /// </summary>
        /// <param name="sender">Джерело події.</param>
        /// <param name="e">Аргументи події зміни розміру.</param>
        private void VolumeTrackGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (VM is { } vm && e.NewSize.Width > 0)
                vm.MaxVolumeWidth = e.NewSize.Width;
        }
    }
}
