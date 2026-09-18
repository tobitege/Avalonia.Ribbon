using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupBox : HeaderedItemsControl
{
    static RibbonGroupBox()
    {
        AffectsArrange<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);
        AffectsMeasure<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);
        AffectsRender<RibbonGroupBox>(DisplayModeProperty, IsCollapsedToPopupProperty);

        DisplayModeProperty.Changed.AddClassHandler<RibbonGroupBox>((sender, args) =>
        {
            if (args.NewValue is GroupDisplayMode displayMode)
                sender.SetPopupState(displayMode == GroupDisplayMode.Popup);

            sender.InvalidateDescendantMeasures();
        });
    }

    // The group sizing loop (RibbonGroupsStackPanel) measures the group synchronously right after a
    // mode change. Avalonia does not mark ancestors invalid when a descendant (a control whose template
    // changed with its Size) invalidates itself, so the template chain between group and controls
    // (DockPanel, ItemsPresenter, items panel) would keep its previous DesiredSize and the group would
    // report the width of the previous mode.
    private void InvalidateDescendantMeasures()
    {
        foreach (var descendant in this.GetVisualDescendants())
        {
            if (descendant is Layoutable layoutable)
                layoutable.InvalidateMeasure();
        }
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
        Rearranged?.Invoke(this, EventArgs.Empty);
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Remeasured?.Invoke(this, EventArgs.Empty);
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

    private void SetPopupState(bool value)
    {
        if (IsCollapsedToPopup == value)
            return;

        IsCollapsedToPopup = value;
    }

    #endregion

    #region Fields

    private bool _isCollapsedToPopup;

    #endregion
}
