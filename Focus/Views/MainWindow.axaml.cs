using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Focus.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Width = 520;
            this.Height = 360;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
