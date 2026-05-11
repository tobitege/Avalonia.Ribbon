using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flowery.Controls;

using AvaloniaUI.Ribbon.Demo.Flowery.ViewModels;
using AvaloniaUI.Ribbon.Demo.Flowery.Views;

namespace AvaloniaUI.Ribbon.Demo.Flowery;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DaisyThemeManager.ApplyTheme("Dark");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }
}
