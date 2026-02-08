using System;

using Avalonia.Controls;
using Avalonia.Layout;
using Flowery.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUI.Ribbon.Demo.Flowery.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private string _help = "Help requested!";

    [ObservableProperty] private string _lastActionText = "none";

    [ObservableProperty] private Orientation _ribbonOrientation = Orientation.Horizontal;

    [ObservableProperty] private SystemDecorations _selectedDecoration;

    [ObservableProperty] private bool _showContextualGroup1 = true;

    [ObservableProperty] private bool _showContextualGroup2;

    [ObservableProperty] private bool _showContextualGroup3;

    [ObservableProperty] private bool _switchOrientation = true;

    [ObservableProperty] private string _darkRadioThemeName = "Dark";

    [ObservableProperty] private string _lightRadioThemeName = "Light";

    public string Greeting => "Welcome to Avalonia!";

    public MainViewModel()
    {
        DaisyThemeManager.ThemeChanged += OnThemeChanged;
        UpdateThemeRadioTargets(DaisyThemeManager.CurrentThemeName);
    }

    public void HelpCommand(object parameter)
    {
        Console.WriteLine(Help);
        LastActionText = Help;
    }

    [RelayCommand]
    public void OnClickCommand(object parameter)
    {
        var paramString = "[NO CONTENT]";

        if (parameter != null)
        {
            if (parameter is string str)
                paramString = str;
            else
                paramString = parameter.ToString();
        }

        Console.WriteLine("OnClickCommand invoked: " + paramString);
        LastActionText = paramString;
    }

    /// <summary>
    ///     Handle Orientation Change event
    /// </summary>
    /// <param name="value"></param>
    partial void OnSwitchOrientationChanged(bool value)
    {
        if (value)
            RibbonOrientation = Orientation.Horizontal;
        else
            RibbonOrientation = Orientation.Vertical;
    }

    private void OnThemeChanged(object sender, string themeName)
    {
        UpdateThemeRadioTargets(themeName);
    }

    private void UpdateThemeRadioTargets(string themeName)
    {
        var currentTheme = themeName;
        if (string.IsNullOrWhiteSpace(currentTheme))
            currentTheme = DaisyThemeManager.CurrentThemeName;

        if (string.IsNullOrWhiteSpace(currentTheme))
            return;

        if (DaisyThemeManager.IsDarkTheme(currentTheme))
        {
            DarkRadioThemeName = currentTheme;
            LightRadioThemeName = "Light";
        }
        else
        {
            LightRadioThemeName = currentTheme;
            DarkRadioThemeName = "Dark";
        }
    }

    public void Dispose()
    {
        DaisyThemeManager.ThemeChanged -= OnThemeChanged;
        GC.SuppressFinalize(this);
    }
}
