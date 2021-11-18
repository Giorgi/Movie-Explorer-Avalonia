using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MovieExplorerApp.Services;
using MovieExplorerApp.ViewModels;
using MovieExplorerApp.Views;

namespace MovieExplorerApp
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = new MainWindowViewModel(new TmdbMovieService()),
                };
            }

            LibVLCSharp.Shared.Core.Initialize();
            base.OnFrameworkInitializationCompleted();
        }
    }
}
