using System;
using Avalonia;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonRadioButton : RadioButton, IRibbonInputControl
{
    public static readonly StyledProperty<RibbonControlSize> SizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<RibbonRadioButton, object?>(nameof(Icon));

    public static readonly StyledProperty<object?> LargeIconProperty =
        AvaloniaProperty.Register<RibbonRadioButton, object?>(nameof(LargeIcon));

    static RibbonRadioButton()
    {
        RibbonControlHelper<RibbonRadioButton>.SetProperties(out SizeProperty, out MinSizeProperty, out MaxSizeProperty);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? LargeIcon
    {
        get => GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
    }

    public RibbonControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }
}
