using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;

using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Helpers;
using AvaloniaUI.Ribbon.Models;

using System;

namespace AvaloniaUI.Ribbon;

[TemplatePart("PART_PrimaryButton", typeof(Button))]
public class RibbonDropDownButton : ItemsControl, IRibbonControl, ICanAddToQuickAccess
{
    #region Fields

    private Flyout? _flyout;
    private Button? _primaryButton = null;
    private bool _suppressDropDownSync;

    #endregion Fields

    static RibbonDropDownButton()
    {
        RibbonControlHelper<RibbonDropDownButton>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
        IsDropDownOpenProperty.Changed.AddClassHandler<RibbonDropDownButton, bool>((sender, args) =>
        {
            if (sender._suppressDropDownSync)
                return;

            sender.SyncFlyoutWithDropDownState(args.NewValue.Value);
        });
    }

    #region Static Properties

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object?> IconProperty =
        RibbonButton.IconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        ComboBox.IsDropDownOpenProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object?> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly StyledProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object?> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        RibbonButton.QuickAccessTemplateProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<RibbonControlSize> SizeProperty;

    public static readonly StyledProperty<KeyGesture?> ShortcutKeysProperty =
        AvaloniaProperty.Register<RibbonDropDownButton, KeyGesture?>(nameof(ShortcutKeys));

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

    public object? LargeIcon
    {
        get => GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
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

    public object? QuickAccessIcon
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
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public KeyGesture? ShortcutKeys
    {
        get => GetValue(ShortcutKeysProperty);
        set => SetValue(ShortcutKeysProperty, value);
    }

    #endregion Properties

    /// <summary>
    /// overrides the OnApplyTemplate method to set up the primary button and its flyout.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UnregisterFlyoutEvents(_flyout);

        _primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");

        if (_primaryButton != null)
        {
            _flyout = _primaryButton.Flyout as Flyout;
            RegisterFlyoutEvents(_flyout);
            SyncFlyoutWithDropDownState(IsDropDownOpen);
        }
    }

    private void Flyout_Opened(object? sender, EventArgs e)
    {
        SyncDropDownStateFromFlyout(true);
    }

    private void Flyout_Closed(object? sender, EventArgs e)
    {
        SyncDropDownStateFromFlyout(false);
    }

    private void SyncFlyoutWithDropDownState(bool isDropDownOpen)
    {
        if (_flyout is null || _primaryButton is null)
            return;

        if (isDropDownOpen)
        {
            if (!_flyout.IsOpen)
            {
                _flyout.Placement = PlacementMode.Bottom;
                _flyout.ShowAt(_primaryButton);
            }
        }
        else if (_flyout.IsOpen)
        {
            _flyout.Hide();
        }
    }

    private void SyncDropDownStateFromFlyout(bool isOpen)
    {
        if (IsDropDownOpen == isOpen)
            return;

        _suppressDropDownSync = true;
        try
        {
            IsDropDownOpen = isOpen;
        }
        finally
        {
            _suppressDropDownSync = false;
        }
    }

    /// <summary>
    /// Explicitly unregisters all flyout events.
    /// </summary>
    /// <param name="flyout">The flyout to disconnect events from.</param>
    private void UnregisterFlyoutEvents(FlyoutBase? flyout)
    {
        if (flyout is null)
            return;

        flyout.Opened -= Flyout_Opened;
        flyout.Closed -= Flyout_Closed;
    }

    private void RegisterFlyoutEvents(FlyoutBase? flyout)
    {
        if (flyout is null)
            return;

        flyout.Opened += Flyout_Opened;
        flyout.Closed += Flyout_Closed;
    }

}
