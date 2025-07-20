using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PiSweeper.ViewModels;
using PiSweeper.Views;

namespace PiSweeper;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainView = new MainWindow();
            mainView.DataContext = new MainWindowViewModel();
            desktop.MainWindow = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}