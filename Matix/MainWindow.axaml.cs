using Avalonia.Controls;
using Avalonia.Input;
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