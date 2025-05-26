using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonDropDownButton : ItemsControl, IRibbonControl, ICanAddToQuickAccess
{
    static RibbonDropDownButton()
    {
        RibbonControlHelper<RibbonDropDownButton>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
    }

    #region Static Properties

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object> IconProperty =
        RibbonButton.IconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        ComboBox.IsDropDownOpenProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly AvaloniaProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        RibbonButton.QuickAccessTemplateProperty.AddOwner<RibbonDropDownButton>();

    public static readonly AvaloniaProperty<RibbonControlSize> SizeProperty;

    #endregion Static Properties

    #region Properties

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public object LargeIcon
    {
        get => GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => (RibbonControlSize)(GetValue(MaxSizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(MaxSizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => (RibbonControlSize)(GetValue(MinSizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(MinSizeProperty, value);
    }

    public object QuickAccessIcon
    {
        get => GetValue(QuickAccessIconProperty);
        set => SetValue(QuickAccessIconProperty, value);
    }

    public IControlTemplate QuickAccessTemplate
    {
        get => GetValue(QuickAccessTemplateProperty);
        set => SetValue(QuickAccessTemplateProperty, value);
    }

    public RibbonControlSize Size
    {
        get => (RibbonControlSize)(GetValue(SizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(SizeProperty, value);
    }

    #endregion Properties
}