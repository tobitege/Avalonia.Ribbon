using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUI.Ribbon.Demo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int MinRibbonGroupRows = 1;
    private const int MaxRibbonGroupRows = 10;
    private const int TotalClusterBanks = 6;
    private const int MinClusterBanksPerRow = 1;
    private const int MaxClusterBanksPerRow = TotalClusterBanks;

    [ObservableProperty] private string _help = "Help requested!";

    [ObservableProperty] private string _lastActionText = "none";

    [ObservableProperty] private Orientation _ribbonOrientation = Orientation.Horizontal;

    [ObservableProperty] private WindowDecorations _selectedDecoration;

    [ObservableProperty] private bool _showContextualGroup1 = true;

    [ObservableProperty] private bool _showContextualGroup2;

    [ObservableProperty] private bool _showContextualGroup3;

    [ObservableProperty] private bool _switchOrientation = true;

    [ObservableProperty] private bool _enableRibbonGroupWrap = true;

    [ObservableProperty] private RibbonGroupOverflowBehavior _ribbonGroupOverflowBehavior =
        RibbonGroupOverflowBehavior.WrapThenShrink;

    [ObservableProperty] private int _ribbonMaxGroupRows = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ClusterBankLineCount))]
    private int _clusterBanksPerRow = 2;

    [ObservableProperty] private bool _switchTheme = true;
    public string Greeting => "Welcome to Avalonia!";

    public IReadOnlyList<int> RibbonMaxGroupRowOptions { get; } =
        Enumerable.Range(MinRibbonGroupRows, MaxRibbonGroupRows).ToArray();

    public IReadOnlyList<int> ClusterBanksPerRowOptions { get; } =
        Enumerable.Range(MinClusterBanksPerRow, MaxClusterBanksPerRow).ToArray();

    public int ClusterBankLineCount => (int)Math.Ceiling((double)TotalClusterBanks / ClusterBanksPerRow);

    public MainViewModel()
    {
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
    ///     Called when [switch theme changed].
    /// </summary>
    /// <param name="value">if set to <c>true</c> [value].</param>
    partial void OnSwitchThemeChanged(bool value)
    {
        switch (value)
        {
            case true:
                App.ThemeManager.Switch(0);
                break;

            case false:
                App.ThemeManager.Switch(1);
                break;
        }
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
            RibbonMaxGroupRows = clamped;
    }

    partial void OnClusterBanksPerRowChanged(int value)
    {
        var clamped = Math.Clamp(value, MinClusterBanksPerRow, MaxClusterBanksPerRow);
        if (clamped != value)
            ClusterBanksPerRow = clamped;
    }
}
