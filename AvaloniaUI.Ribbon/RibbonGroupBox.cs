using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupBox : HeaderedItemsControl
{
    static RibbonGroupBox()
    {
        AffectsArrange<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);
        AffectsMeasure<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);
        AffectsRender<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);
    }

    #region Static Properties

    public static readonly StyledProperty<ICommand?> DialogLauncherCommandProperty =
        Button.CommandProperty.AddOwner<RibbonGroupBox>();

    public static readonly StyledProperty<object?> DialogLauncherCommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<RibbonGroupBox>();

    public static readonly StyledProperty<ICommand?> CommandProperty = DialogLauncherCommandProperty;

    public static readonly StyledProperty<object?> CommandParameterProperty = DialogLauncherCommandParameterProperty;

    public static readonly StyledProperty<GroupDisplayMode> DisplayModeProperty =
        StyledProperty<RibbonGroupBox>.Register<RibbonGroupBox, GroupDisplayMode>(nameof(DisplayMode),
            GroupDisplayMode.Small);

    public static readonly StyledProperty<bool> AllowCollapsedPopupProperty =
        AvaloniaProperty.Register<RibbonGroupBox, bool>(nameof(AllowCollapsedPopup), false);

    public static readonly DirectProperty<RibbonGroupBox, bool> IsCollapsedToPopupProperty =
        AvaloniaProperty.RegisterDirect<RibbonGroupBox, bool>(nameof(IsCollapsedToPopup),
            group => group.IsCollapsedToPopup);

    #endregion Static Properties

    #region Properties

    public event EventHandler? Rearranged;

    public event EventHandler? Remeasured;


    protected override Size ArrangeOverride(Size finalSize)
    {
        Rearranged?.Invoke(this, null);
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Remeasured?.Invoke(this, null);
        return base.MeasureOverride(availableSize);
    }

    protected override Type StyleKeyOverride => typeof(RibbonGroupBox);

    public ICommand? DialogLauncherCommand
    {
        get => GetValue(DialogLauncherCommandProperty);
        set => SetValue(DialogLauncherCommandProperty, value);
    }

    public object? DialogLauncherCommandParameter
    {
        get => GetValue(DialogLauncherCommandParameterProperty);
        set => SetValue(DialogLauncherCommandParameterProperty, value);
    }

    public ICommand? Command
    {
        get => DialogLauncherCommand;
        set => DialogLauncherCommand = value;
    }

    public object? CommandParameter
    {
        get => DialogLauncherCommandParameter;
        set => DialogLauncherCommandParameter = value;
    }

    public GroupDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool AllowCollapsedPopup
    {
        get => GetValue(AllowCollapsedPopupProperty);
        set => SetValue(AllowCollapsedPopupProperty, value);
    }

    public bool IsCollapsedToPopup
    {
        get => _isCollapsedToPopup;
        private set => SetAndRaise(IsCollapsedToPopupProperty, ref _isCollapsedToPopup, value);
    }

    #endregion

    #region Methods

    internal bool SetCollapsedToPopup(bool value)
    {
        if (IsCollapsedToPopup == value)
            return false;

        IsCollapsedToPopup = value;
        return true;
    }

    #endregion

    #region Fields

    private bool _isCollapsedToPopup;

    #endregion
}