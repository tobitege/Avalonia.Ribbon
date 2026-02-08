using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Layout;
using Flowery.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Demo.Flowery.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private const int MinRibbonGroupRows = 1;
    private const int MaxRibbonGroupRows = 10;

    [ObservableProperty] private string _help = "Help requested!";

    [ObservableProperty] private string _lastActionText = "none";

    [ObservableProperty] private Orientation _ribbonOrientation = Orientation.Horizontal;

    [ObservableProperty] private SystemDecorations _selectedDecoration;

    [ObservableProperty] private bool _showContextualGroup1 = true;

    [ObservableProperty] private bool _showContextualGroup2;

    [ObservableProperty] private bool _showContextualGroup3;

    [ObservableProperty] private bool _switchOrientation = true;

    [ObservableProperty] private bool _enableRibbonGroupWrap = true;

    [ObservableProperty] private RibbonGroupOverflowBehavior _ribbonGroupOverflowBehavior =
        RibbonGroupOverflowBehavior.WrapThenShrink;

    [ObservableProperty] private int _ribbonMaxGroupRows = 2;

    [ObservableProperty] private string _darkRadioThemeName = "Dark";

    [ObservableProperty] private string _lightRadioThemeName = "Light";

    public string Greeting => "Welcome to Avalonia!";

    public IReadOnlyList<int> RibbonMaxGroupRowOptions { get; } = Enumerable.Range(MinRibbonGroupRows, MaxRibbonGroupRows).ToArray();

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

    partial void OnEnableRibbonGroupWrapChanged(bool value)
    {
        if (value)
        {
            RibbonGroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink;
            if (RibbonMaxGroupRows < 2)
                RibbonMaxGroupRows = 2;
        }
        else
        {
            RibbonGroupOverflowBehavior = RibbonGroupOverflowBehavior.ShrinkOnly;
        }
    }

    partial void OnRibbonMaxGroupRowsChanged(int value)
    {
        var clamped = Math.Clamp(value, MinRibbonGroupRows, MaxRibbonGroupRows);
        if (clamped != value)
        {
            RibbonMaxGroupRows = clamped;
            return;
        }
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
