using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class SplitButtonControl : SplitButton, IRibbonControl, ICanAddToQuickAccess
{
    static SplitButtonControl()
    {
        RibbonControlHelper<SplitButtonControl>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public bool DroppedDown
    {
        get => IsDropDownOpen;
        set => IsDropDownOpen = value;
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

    public KeyGesture? ShortcutKeys
    {
        get => GetValue(ShortcutKeysProperty);
        set => SetValue(ShortcutKeysProperty, value);
    }

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

    public bool CanBeAddedToQat
    {
        get => CanAddToQuickAccess;
        set => CanAddToQuickAccess = value;
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
        set
        {
            SetValue(SizeProperty, value);

            switch (value)
            {
                case RibbonControlSize.Large:
                    //TODO: Figure out a way to optimize this.
                    if (Application.Current?.Resources != null)
                    {
                        Application.Current.Resources.TryGetResource("LargeSplitButton", null, out var theme);
                        if (theme is ControlTheme controlTheme)
                            Theme = controlTheme;
                    }
                    break;

                case RibbonControlSize.Small:
                    break;

                case RibbonControlSize.Medium:
                    break;
            }
        }
    }

    //protected override Type StyleKeyOverride => typeof(SplitButton);

    #region Static Properties

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<object?> IconProperty =
        RibbonButton.IconProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        ComboBox.IsDropDownOpenProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<object?> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object?> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        RibbonButton.QuickAccessTemplateProperty.AddOwner<SplitButton>();

    public static readonly StyledProperty<RibbonControlSize> SizeProperty;

    public static readonly StyledProperty<KeyGesture?> ShortcutKeysProperty =
        AvaloniaProperty.Register<SplitButtonControl, KeyGesture?>(nameof(ShortcutKeys));

    #endregion Static Properties
}
