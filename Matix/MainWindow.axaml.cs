using Avalonia.Controls;
using Matix.ViewModels;

namespace Matix
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new AppViewModel();
        }
    }
}