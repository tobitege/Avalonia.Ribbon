using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupsStackPanel : Panel
{
    private const double Epsilon = 0.01;
    private const double OverflowButtonWidth = 30;
    private const double OverflowContentBuffer = 10;

    private bool _isSizingControls;
    private bool _isSizingDirty = true;
    private Orientation? _lastSizingOrientation;
    private double _lastSizingExtent = double.NaN;

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<RibbonGroupsStackPanel>();

    public static readonly StyledProperty<RibbonGroupOverflowBehavior> GroupOverflowBehaviorProperty =
        Ribbon.GroupOverflowBehaviorProperty.AddOwner<RibbonGroupsStackPanel>();

    public static readonly StyledProperty<int> MaxGroupRowsProperty =
        Ribbon.MaxGroupRowsProperty.AddOwner<RibbonGroupsStackPanel>();

    public static readonly StyledProperty<Ribbon?> OverflowOwnerProperty =
        AvaloniaProperty.Register<RibbonGroupsStackPanel, Ribbon?>(nameof(OverflowOwner));

    static RibbonGroupsStackPanel()
    {
        AffectsMeasure<RibbonGroupsStackPanel>(OrientationProperty, GroupOverflowBehaviorProperty, MaxGroupRowsProperty);
        AffectsArrange<RibbonGroupsStackPanel>(OrientationProperty, GroupOverflowBehaviorProperty, MaxGroupRowsProperty);

        ParentProperty.Changed.AddClassHandler<RibbonGroupsStackPanel>((sender, _) =>
        {
            sender.MarkSizingDirty();
            Dispatcher.UIThread.Post(sender.InvalidateMeasure);
        });

        OverflowOwnerProperty.Changed.AddClassHandler<RibbonGroupsStackPanel>((sender, args) =>
        {
            if (args.OldValue is Ribbon previousOwner)
                previousOwner.ClearGroupOverflow(sender);

            sender.MarkSizingDirty();
            sender.InvalidateMeasure();
        });
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public RibbonGroupOverflowBehavior GroupOverflowBehavior
    {
        get => GetValue(GroupOverflowBehaviorProperty);
        set => SetValue(GroupOverflowBehaviorProperty, value);
    }

    public int MaxGroupRows
    {
        get => GetValue(MaxGroupRowsProperty);
        set => SetValue(MaxGroupRowsProperty, value);
    }

    public Ribbon? OverflowOwner
    {
        get => GetValue(OverflowOwnerProperty);
        set => SetValue(OverflowOwnerProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        SizeControls(availableSize);
        MeasureChildren(availableSize);

        return Orientation == Orientation.Vertical
            ? MeasureVertical()
            : MeasureHorizontal(availableSize.Width);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (SizeControls(finalSize))
            MeasureChildren(finalSize);

        return Orientation == Orientation.Vertical
            ? ArrangeVertical(finalSize)
            : ArrangeHorizontal(finalSize);
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        MarkSizingDirty();
        InvalidateMeasure();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        MarkSizingDirty();
        InvalidateMeasure();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        OverflowOwner?.ClearGroupOverflow(this);
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty ||
            change.Property == GroupOverflowBehaviorProperty ||
            change.Property == MaxGroupRowsProperty)
        {
            MarkSizingDirty();
        }
    }

    private Size ArrangeHorizontal(Size finalSize)
    {
        var widthConstraint = GetFiniteExtent(finalSize.Width, Bounds.Width);
        var rows = BuildHorizontalRows(GetVisibleGroupsWidth(widthConstraint), GetHorizontalRowLimit());

        foreach (var group in Children.OfType<RibbonGroupBox>())
        {
            if (group.DisplayMode == GroupDisplayMode.Popup)
                group.Arrange(default);
        }

        var y = 0d;
        var maxWidth = 0d;

        foreach (var row in rows)
        {
            var x = 0d;
            foreach (var child in row.Children)
            {
                var childWidth = GetLayoutWidth(child);
                child.Arrange(new Rect(x, y, childWidth, row.Height));
                x += childWidth;
            }

            maxWidth = Math.Max(maxWidth, x);
            y += row.Height;
        }

        var arrangedWidth = double.IsInfinity(finalSize.Width) ? maxWidth : finalSize.Width;
        return new Size(arrangedWidth, y);
    }

    private Size ArrangeVertical(Size finalSize)
    {
        var y = 0d;
        var maxWidth = 0d;

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            child.Arrange(new Rect(0, y, Math.Max(finalSize.Width, childSize.Width), childSize.Height));
            y += childSize.Height;
            maxWidth = Math.Max(maxWidth, childSize.Width);
        }

        return new Size(Math.Max(finalSize.Width, maxWidth), y);
    }

    private List<RowLayout> BuildHorizontalRows(double availableWidth, int rowLimit)
    {
        var rows = new List<RowLayout>();
        var current = new RowLayout();
        rows.Add(current);

        foreach (var child in Children)
        {
            if (child is RibbonGroupBox { DisplayMode: GroupDisplayMode.Popup })
                continue;

            var childWidth = GetLayoutWidth(child);
            var childHeight = child.DesiredSize.Height;
            var canWrap = !double.IsInfinity(availableWidth) && rowLimit > rows.Count;
            var shouldWrap = canWrap && current.Width > Epsilon && current.Width + childWidth > availableWidth + Epsilon;

            if (shouldWrap)
            {
                current = new RowLayout();
                rows.Add(current);
            }

            current.Children.Add(child);
            current.Width += childWidth;
            current.Height = Math.Max(current.Height, childHeight);
        }

        return rows.Where(row => row.Children.Count > 0).ToList();
    }

    private static bool CanFitHorizontal(
        IReadOnlyList<RibbonGroupBox> groups,
        double availableWidth,
        int maxRows,
        Size groupConstraint)
    {
        if (groups.Count == 0 || double.IsInfinity(availableWidth))
            return true;

        if (availableWidth <= Epsilon)
            return false;

        var row = 1;
        var rowWidth = 0d;

        for (var i = 0; i < groups.Count; i++)
        {
            var width = MeasureGroupWidthForLayout(groups[i], groupConstraint);
            if (width > availableWidth + Epsilon)
                return false;

            var shouldWrap = rowWidth > Epsilon && rowWidth + width > availableWidth + Epsilon;
            if (shouldWrap)
            {
                row++;
                if (row > maxRows)
                    return false;

                rowWidth = 0d;
            }

            rowWidth += width;
        }

        return true;
    }

    private static bool CanFitVertical(IReadOnlyList<RibbonGroupBox> groups, double availableHeight)
    {
        if (groups.Count == 0 || double.IsInfinity(availableHeight))
            return true;

        if (availableHeight <= Epsilon)
            return false;

        var totalHeight = 0d;
        for (var i = 0; i < groups.Count; i++)
            totalHeight += MeasureGroupDesiredSize(groups[i]).Height;

        return totalHeight <= availableHeight + Epsilon;
    }

    private static double GetFiniteExtent(double candidate, double fallback)
    {
        if (!double.IsNaN(candidate) && candidate > 0 && !double.IsInfinity(candidate))
            return candidate;

        if (!double.IsNaN(fallback) && fallback > 0)
            return fallback;

        return double.PositiveInfinity;
    }

    private static bool ExtentsMatch(double left, double right)
    {
        if (double.IsNaN(left) || double.IsNaN(right))
            return false;

        if (double.IsInfinity(left) || double.IsInfinity(right))
            return left.Equals(right);

        return Math.Abs(left - right) <= Epsilon;
    }

    private int GetHorizontalRowLimit()
    {
        if (GroupOverflowBehavior != RibbonGroupOverflowBehavior.WrapThenShrink)
            return 1;

        return Math.Max(1, MaxGroupRows);
    }

    private static Size MeasureGroupDesiredSize(RibbonGroupBox group)
    {
        group.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return group.DesiredSize;
    }

    private void MeasureChildren(Size availableSize)
    {
        var childConstraint = Orientation == Orientation.Vertical
            ? new Size(availableSize.Width, double.PositiveInfinity)
            : new Size(double.PositiveInfinity, availableSize.Height);

        foreach (var child in Children)
            child.Measure(childConstraint);
    }

    private Size MeasureHorizontal(double availableWidth)
    {
        var hostWidth = GetFiniteExtent(availableWidth, Bounds.Width);
        var availableGroupsWidth = GetVisibleGroupsWidth(hostWidth);
        var rows = BuildHorizontalRows(availableGroupsWidth, GetHorizontalRowLimit());

        if (rows.Count == 0)
            return new Size(double.IsInfinity(hostWidth) ? 0 : hostWidth, 0);

        var desiredWidth = double.IsInfinity(hostWidth)
            ? rows.Max(row => row.Width)
            : hostWidth;

        var desiredHeight = rows.Sum(row => row.Height);
        return new Size(desiredWidth, desiredHeight);
    }

    private double GetVisibleGroupsWidth(double availableWidth)
    {
        if (double.IsInfinity(availableWidth) ||
            !Children.OfType<RibbonGroupBox>().Any(group => group.DisplayMode == GroupDisplayMode.Popup))
        {
            return availableWidth;
        }

        return GetWidthBeforeOverflowSlot(availableWidth);
    }

    private static double GetWidthBeforeOverflowSlot(double availableWidth)
    {
        return Math.Max(0, availableWidth - OverflowButtonWidth - OverflowContentBuffer);
    }

    private Size MeasureVertical()
    {
        var width = 0d;
        var height = 0d;

        foreach (var child in Children)
        {
            var childSize = child.DesiredSize;
            width = Math.Max(width, childSize.Width);
            height += childSize.Height;
        }

        return new Size(width, height);
    }

    private static bool SetDisplayMode(RibbonGroupBox group, GroupDisplayMode mode)
    {
        if (group.DisplayMode == mode)
            return false;

        group.DisplayMode = mode;
        group.InvalidateArrange();
        group.InvalidateMeasure();
        return true;
    }

    private bool SizeControls(Size availableSize)
    {
        if (_isSizingControls)
            return false;

        var groups = Children.OfType<RibbonGroupBox>().ToList();
        if (groups.Count == 0)
        {
            OverflowOwner?.SetGroupOverflow(this, Array.Empty<RibbonGroupBox>());
            return false;
        }

        var orientation = Orientation;
        var sizingExtent = orientation == Orientation.Vertical
            ? GetFiniteExtent(availableSize.Height, Bounds.Height)
            : GetFiniteExtent(availableSize.Width, Bounds.Width);

        var needsSizing = _isSizingDirty ||
                          _lastSizingOrientation != orientation ||
                          !ExtentsMatch(_lastSizingExtent, sizingExtent) ||
                          groups.Any(group => !group.IsMeasureValid);

        if (!needsSizing)
            return false;

        if (groups.Any(group => group.DisplayMode == GroupDisplayMode.Popup))
            OverflowOwner?.ReleaseGroupOverflowItems(this);

        _isSizingControls = true;

        try
        {
            if (orientation == Orientation.Vertical)
            {
                SizeControlsVertically(groups, sizingExtent);
            }
            else
            {
                SizeControlsHorizontally(
                    groups,
                    sizingExtent,
                    GetHorizontalRowLimit(),
                    GroupOverflowBehavior == RibbonGroupOverflowBehavior.WrapThenShrink,
                    new Size(
                        double.PositiveInfinity,
                        GetFiniteExtent(availableSize.Height, Bounds.Height)));
            }

            _lastSizingOrientation = orientation;
            _lastSizingExtent = sizingExtent;
            _isSizingDirty = false;

            var overflowGroups = groups
                .Where(group => group.DisplayMode == GroupDisplayMode.Popup)
                .ToArray();
            OverflowOwner?.SetGroupOverflow(this, overflowGroups);
            return true;
        }
        finally
        {
            _isSizingControls = false;
        }
    }

    private void MarkSizingDirty()
    {
        _isSizingDirty = true;
    }

    private static void SizeControlsHorizontally(
        IReadOnlyList<RibbonGroupBox> groups,
        double availableWidth,
        int maxRows,
        bool allowCollapsedPopupOverflow,
        Size groupConstraint)
    {
        if (double.IsInfinity(availableWidth))
        {
            for (var i = 0; i < groups.Count; i++)
                SetDisplayMode(groups[i], GroupDisplayMode.Large);

            return;
        }

        for (var i = 0; i < groups.Count; i++)
            SetDisplayMode(groups[i], GroupDisplayMode.Large);

        var usePopupOverflow = allowCollapsedPopupOverflow &&
                               groups.Any(group => group.AllowCollapsedPopup);
        var allowSmallMode = !usePopupOverflow;
        var sizingWidth = usePopupOverflow
            ? GetWidthBeforeOverflowSlot(availableWidth)
            : availableWidth;

        while (!CanFitHorizontal(groups, sizingWidth, maxRows, groupConstraint))
        {
            var candidate = FindWidestGroupThatCanDecrease(
                groups,
                groupConstraint,
                allowSmallMode);
            if (candidate is null)
                break;

            var candidateNextMode = CanDecreaseDisplayMode(candidate.DisplayMode, allowSmallMode);
            SetDisplayMode(candidate, candidateNextMode!.Value);
        }

        if (usePopupOverflow)
            CollapseOverflowGroupsToPopup(groups, availableWidth, maxRows, groupConstraint);
    }

    private static void SizeControlsVertically(IReadOnlyList<RibbonGroupBox> groups, double availableHeight)
    {
        if (double.IsInfinity(availableHeight))
        {
            for (var i = 0; i < groups.Count; i++)
                SetDisplayMode(groups[i], GroupDisplayMode.Large);

            return;
        }

        for (var i = 0; i < groups.Count; i++)
            SetDisplayMode(groups[i], GroupDisplayMode.Large);

        while (!CanFitVertical(groups, availableHeight))
        {
            var candidate = FindTallestGroupThatCanDecrease(groups);
            if (candidate is null)
                break;

            var candidateNextMode = CanDecreaseDisplayMode(candidate.DisplayMode, allowSmallMode: true);
            SetDisplayMode(candidate, candidateNextMode!.Value);
        }
    }

    private static GroupDisplayMode? CanDecreaseDisplayMode(
        GroupDisplayMode displayMode,
        bool allowSmallMode) => displayMode switch
    {
        GroupDisplayMode.Large => GroupDisplayMode.Medium,
        GroupDisplayMode.Medium => allowSmallMode ? GroupDisplayMode.Small : null,
        GroupDisplayMode.Small => null,
        GroupDisplayMode.Popup => null,
        _ => null
    };

    private static RibbonGroupBox? FindWidestGroupThatCanDecrease(
        IReadOnlyList<RibbonGroupBox> groups,
        Size groupConstraint,
        bool allowSmallMode)
    {
        RibbonGroupBox? candidate = null;
        var maxWidth = double.MinValue;

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            if (!CanDecreaseDisplayMode(group.DisplayMode, allowSmallMode).HasValue)
                continue;

            var width = MeasureGroupWidthForLayout(group, groupConstraint);
            if (width > maxWidth)
            {
                candidate = group;
                maxWidth = width;
            }
        }

        return candidate;
    }

    private static RibbonGroupBox? FindTallestGroupThatCanDecrease(IReadOnlyList<RibbonGroupBox> groups)
    {
        RibbonGroupBox? candidate = null;
        var maxHeight = double.MinValue;

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            if (!CanDecreaseDisplayMode(group.DisplayMode, allowSmallMode: true).HasValue)
                continue;

            var height = MeasureGroupDesiredSize(group).Height;
            if (height > maxHeight)
            {
                candidate = group;
                maxHeight = height;
            }
        }

        return candidate;
    }

    private static void CollapseOverflowGroupsToPopup(
        IReadOnlyList<RibbonGroupBox> groups,
        double availableWidth,
        int maxRows,
        Size groupConstraint)
    {
        var visibleGroupsWidth = GetWidthBeforeOverflowSlot(availableWidth);

        while (!CanFitHorizontal(groups, visibleGroupsWidth, maxRows, groupConstraint))
        {
            var candidate = groups.LastOrDefault(group =>
                group.DisplayMode != GroupDisplayMode.Popup);

            if (candidate == null || !candidate.AllowCollapsedPopup)
                break;

            SetDisplayMode(candidate, GroupDisplayMode.Popup);
        }
    }

    private static double MeasureGroupWidthForLayout(RibbonGroupBox group, Size groupConstraint)
    {
        if (group.DisplayMode == GroupDisplayMode.Popup)
            return 0;

        group.Measure(groupConstraint);
        return group.DesiredSize.Width;
    }

    private static double GetLayoutWidth(Control child)
    {
        if (child is RibbonGroupBox group && group.DisplayMode == GroupDisplayMode.Popup)
            return 0;

        return child.DesiredSize.Width;
    }

    private sealed class RowLayout
    {
        public List<Control> Children { get; } = new();

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
