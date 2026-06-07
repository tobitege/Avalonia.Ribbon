using System;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using AvaloniaUI.Ribbon.Automation;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonToggleButton : ToggleButton, IRibbonControl, ICanAddToQuickAccess
{
    public static readonly StyledProperty<RibbonControlSize> SizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;

    public static readonly StyledProperty<object?> IconProperty =
        RibbonButton.IconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<object?> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<object?> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        AvaloniaProperty.Register<RibbonButton, IControlTemplate>(nameof(Template));

    public static readonly StyledProperty<KeyGesture?> ShortcutKeysProperty =
        AvaloniaProperty.Register<RibbonToggleButton, KeyGesture?>(nameof(ShortcutKeys));

    static RibbonToggleButton()
    {
        RibbonControlHelper<RibbonToggleButton>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
        FocusableProperty.OverrideDefaultValue<RibbonToggleButton>(false);
    }

    protected override Type StyleKeyOverride => typeof(RibbonToggleButton);

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

    public KeyGesture? ShortcutKeys
    {
        get => GetValue(ShortcutKeysProperty);
        set => SetValue(ShortcutKeysProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new RibbonToggleButtonAutomationPeer(this);
    }
}
