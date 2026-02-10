using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonButton : Button, IRibbonInputControl, IRibbonCommand, ICanAddToQuickAccess
{
    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        AvaloniaProperty.Register<RibbonButton, bool>(nameof(CanAddToQuickAccess), true);

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<RibbonButton, object?>(nameof(Icon));

    public static readonly StyledProperty<object?> LargeIconProperty =
        AvaloniaProperty.Register<RibbonButton, object?>(nameof(LargeIcon));

    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object?> QuickAccessIconProperty =
        AvaloniaProperty.Register<RibbonButton, object?>(nameof(QuickAccessIcon));

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        AvaloniaProperty.Register<RibbonButton, IControlTemplate>(nameof(QuickAccessTemplate));

    public static readonly StyledProperty<RibbonControlSize> SizeProperty;

    static RibbonButton()
    {
        RibbonControlHelper<RibbonButton>.SetProperties(out SizeProperty, out MinSizeProperty, out MaxSizeProperty);
        FocusableProperty.OverrideDefaultValue<RibbonButton>(false);
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

    public object? QuickAccessIcon
    {
        get => GetValue(QuickAccessIconProperty);
        set => SetValue(QuickAccessIconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RibbonButton);

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

    public IControlTemplate QuickAccessTemplate
    {
        get => GetValue(QuickAccessTemplateProperty);
        set => SetValue(QuickAccessTemplateProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public RibbonControlSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}