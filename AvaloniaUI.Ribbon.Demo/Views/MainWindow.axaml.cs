using System;
using System.IO;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Demo.Views;

public partial class MainWindow : RibbonWindow
{
    public MainWindow()
    {
        InitializeComponent();

        if (!OperatingSystem.IsBrowser() && QuickAccessToolbar != null)
        {
            var applicationDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AvaloniaUI.Ribbon.Demo");
            var settingsFilePath = Path.Combine(applicationDataDirectory, "quick-access-toolbar.json");
            QuickAccessToolbar.PersistenceProvider =
                new JsonQuickAccessToolbarPersistenceProvider(settingsFilePath);
        }
    }
}
