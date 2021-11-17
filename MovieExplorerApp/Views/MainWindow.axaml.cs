using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MovieExplorerApp.ViewModels;

namespace MovieExplorerApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <inheritdoc />
        protected override void OnOpened(EventArgs e)
        {
            (DataContext as ViewModelBase).OnOpened();
        }
    }
}
