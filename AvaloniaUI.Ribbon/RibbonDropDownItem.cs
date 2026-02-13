using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using AvaloniaUI.Ribbon.Contracts;

using System;

namespace AvaloniaUI.Ribbon;

public class RibbonDropDownItem : MenuItem, IRibbonCommand
{
    public static readonly StyledProperty<KeyGesture?> ShortcutKeysProperty =
        AvaloniaProperty.Register<RibbonDropDownItem, KeyGesture?>(nameof(ShortcutKeys));

    #region Properties

    protected override Type StyleKeyOverride => typeof(RibbonDropDownItem);

    public KeyGesture? ShortcutKeys
    {
        get => GetValue(ShortcutKeysProperty);
        set => SetValue(ShortcutKeysProperty, value);
    }

    #endregion Properties
}