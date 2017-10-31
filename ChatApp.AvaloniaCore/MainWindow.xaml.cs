using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatApp.AvaloniaCore.ViewModels;

namespace ChatApp.AvaloniaCore
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainViewModel();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
