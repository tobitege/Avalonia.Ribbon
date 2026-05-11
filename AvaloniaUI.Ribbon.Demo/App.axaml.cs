#nullable enable

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ThemeManager;

using AvaloniaUI.Ribbon.Demo.ViewModels;
using AvaloniaUI.Ribbon.Demo.Views;

namespace AvaloniaUI.Ribbon.Demo;

public class App : Application
{
    public static IThemeManager? ThemeManager;

    public override void Initialize()
    {
        ThemeManager = new FluentThemeManager();
        ThemeManager.Initialize(this);
        ThemeManager.Switch(0);
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
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
