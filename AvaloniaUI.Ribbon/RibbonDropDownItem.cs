using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon;

public class RibbonDropDownItem : MenuItem, IRibbonCommand
{
    #region Fields


    #endregion Fields

    #region Static Property
 

    #endregion Static Property

    #region Properties

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RibbonDropDownItem);

    #endregion Properties
}