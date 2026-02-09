using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public abstract class RibbonGroupContainer : Panel, IRibbonGroupContainer
{
    private RibbonControlSize _currentSize = RibbonControlSize.Large;

    static RibbonGroupContainer()
    {
        AffectsArrange<RibbonGroupContainer>(DisplayModeProperty, MinimumSizeProperty, MaximumSizeProperty, ItemSpacingProperty);
        AffectsMeasure<RibbonGroupContainer>(DisplayModeProperty, MinimumSizeProperty, MaximumSizeProperty, ItemSpacingProperty);

        DisplayModeProperty.Changed.AddClassHandler<RibbonGroupContainer>((sender, _) => sender.OnContainerStateChanged());
        MinimumSizeProperty.Changed.AddClassHandler<RibbonGroupContainer>((sender, _) => sender.OnContainerStateChanged());
        MaximumSizeProperty.Changed.AddClassHandler<RibbonGroupContainer>((sender, _) => sender.OnContainerStateChanged());
    }

    protected RibbonGroupContainer()
    {
        Children.CollectionChanged += OnChildrenCollectionChanged;
    }

    public static readonly StyledProperty<GroupDisplayMode> DisplayModeProperty =
        RibbonGroupBox.DisplayModeProperty.AddOwner<RibbonGroupContainer>();

    public static readonly StyledProperty<RibbonControlSize> MinimumSizeProperty =
        AvaloniaProperty.Register<RibbonGroupContainer, RibbonControlSize>(nameof(MinimumSize), RibbonControlSize.Small);

    public static readonly StyledProperty<RibbonControlSize> MaximumSizeProperty =
        AvaloniaProperty.Register<RibbonGroupContainer, RibbonControlSize>(nameof(MaximumSize), RibbonControlSize.Large);

    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<RibbonGroupContainer, double>(nameof(ItemSpacing), 2d);

    public static readonly DirectProperty<RibbonGroupContainer, RibbonControlSize> CurrentSizeProperty =
        AvaloniaProperty.RegisterDirect<RibbonGroupContainer, RibbonControlSize>(nameof(CurrentSize), container => container.CurrentSize);

    public GroupDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public RibbonControlSize MinimumSize
    {
        get => GetValue(MinimumSizeProperty);
        set => SetValue(MinimumSizeProperty, value);
    }

    public RibbonControlSize MaximumSize
    {
        get => GetValue(MaximumSizeProperty);
        set => SetValue(MaximumSizeProperty, value);
    }

    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public RibbonControlSize CurrentSize
    {
        get => _currentSize;
        private set => SetAndRaise(CurrentSizeProperty, ref _currentSize, value);
    }

    public void ApplyDisplayMode(GroupDisplayMode displayMode)
    {
        if (DisplayMode == displayMode)
        {
            OnContainerStateChanged();
            return;
        }

        DisplayMode = displayMode;
    }

    protected virtual RibbonControlSize ResolveTargetSize(GroupDisplayMode displayMode)
    {
        return displayMode == GroupDisplayMode.Small ? MinimumSize : MaximumSize;
    }

    protected static RibbonControlSize ClampControlSize(RibbonControlSize value, RibbonControlSize min, RibbonControlSize max)
    {
        if (min > max)
            (min, max) = (max, min);

        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    protected void EnsureChildControlSizes()
    {
        var targetSize = ClampControlSize(ResolveTargetSize(DisplayMode), MinimumSize, MaximumSize);
        if (CurrentSize != targetSize)
            CurrentSize = targetSize;

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is IRibbonGroupContainer groupContainer)
                groupContainer.ApplyDisplayMode(DisplayMode);

            if (Children[i] is IRibbonControl ribbonControl)
                ribbonControl.Size = ClampControlSize(targetSize, ribbonControl.MinSize, ribbonControl.MaxSize);
        }
    }

    private void OnContainerStateChanged()
    {
        EnsureChildControlSizes();
        InvalidateMeasure();
        InvalidateArrange();
    }

    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnContainerStateChanged();
    }
}
