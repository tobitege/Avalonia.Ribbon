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

    private bool _isSizingControls;

    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<RibbonGroupsStackPanel>();

    public static readonly StyledProperty<RibbonGroupOverflowBehavior> GroupOverflowBehaviorProperty =
        Ribbon.GroupOverflowBehaviorProperty.AddOwner<RibbonGroupsStackPanel>();

    public static readonly StyledProperty<int> MaxGroupRowsProperty =
        Ribbon.MaxGroupRowsProperty.AddOwner<RibbonGroupsStackPanel>();

    static RibbonGroupsStackPanel()
    {
        AffectsMeasure<RibbonGroupsStackPanel>(OrientationProperty, GroupOverflowBehaviorProperty, MaxGroupRowsProperty);
        AffectsArrange<RibbonGroupsStackPanel>(OrientationProperty, GroupOverflowBehaviorProperty, MaxGroupRowsProperty);

        ParentProperty.Changed.AddClassHandler<RibbonGroupsStackPanel>((sender, _) =>
        {
            Dispatcher.UIThread.Post(sender.InvalidateMeasure);
        });

        BoundsProperty.Changed.AddClassHandler<RibbonGroupsStackPanel>((sender, _) =>
        {
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

    protected override Size MeasureOverride(Size availableSize)
    {
        SizeControls(availableSize);

        var childConstraint = Orientation == Orientation.Vertical
            ? new Size(availableSize.Width, double.PositiveInfinity)
            : new Size(double.PositiveInfinity, availableSize.Height);

        foreach (var child in Children)
            child.Measure(childConstraint);

        return Orientation == Orientation.Vertical
            ? MeasureVertical()
            : MeasureHorizontal(availableSize.Width);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return Orientation == Orientation.Vertical
            ? ArrangeVertical(finalSize)
            : ArrangeHorizontal(finalSize);
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        InvalidateMeasure();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        InvalidateMeasure();
    }

    private Size ArrangeHorizontal(Size finalSize)
    {
        var widthConstraint = GetFiniteExtent(finalSize.Width, Bounds.Width);
        var rows = BuildHorizontalRows(widthConstraint, GetHorizontalRowLimit());

        var y = 0d;
        var maxWidth = 0d;

        foreach (var row in rows)
        {
            var x = 0d;
            foreach (var child in row.Children)
            {
                var childWidth = child.DesiredSize.Width;
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
            var childSize = child.DesiredSize;
            var canWrap = !double.IsInfinity(availableWidth) && rowLimit > rows.Count;
            var shouldWrap = canWrap && current.Width > Epsilon && current.Width + childSize.Width > availableWidth + Epsilon;

            if (shouldWrap)
            {
                current = new RowLayout();
                rows.Add(current);
            }

            current.Children.Add(child);
            current.Width += childSize.Width;
            current.Height = Math.Max(current.Height, childSize.Height);
        }

        return rows.Where(row => row.Children.Count > 0).ToList();
    }

    private static bool CanFitHorizontal(IReadOnlyList<RibbonGroupBox> groups, double availableWidth, int maxRows)
    {
        if (groups.Count == 0 || double.IsInfinity(availableWidth))
            return true;

        if (availableWidth <= Epsilon)
            return false;

        var row = 1;
        var rowWidth = 0d;

        for (var i = 0; i < groups.Count; i++)
        {
            var width = MeasureGroupDesiredSize(groups[i]).Width;
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
        if (!double.IsNaN(candidate) && candidate > 0)
            return candidate;

        if (!double.IsNaN(fallback) && fallback > 0)
            return fallback;

        return double.PositiveInfinity;
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

    private Size MeasureHorizontal(double availableWidth)
    {
        var widthConstraint = GetFiniteExtent(availableWidth, Bounds.Width);
        var rows = BuildHorizontalRows(widthConstraint, GetHorizontalRowLimit());

        if (rows.Count == 0)
            return default;

        var desiredWidth = rows.Max(row => row.Width);
        if (!double.IsInfinity(widthConstraint))
            desiredWidth = Math.Min(desiredWidth, widthConstraint);

        var desiredHeight = rows.Sum(row => row.Height);
        return new Size(desiredWidth, desiredHeight);
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

    private void SizeControls(Size availableSize)
    {
        if (_isSizingControls)
            return;

        var groups = Children.OfType<RibbonGroupBox>().ToList();
        if (groups.Count == 0)
            return;

        _isSizingControls = true;

        try
        {
            if (Orientation == Orientation.Vertical)
            {
                SizeControlsVertically(groups, GetFiniteExtent(availableSize.Height, Bounds.Height));
                return;
            }

            SizeControlsHorizontally(groups, GetFiniteExtent(availableSize.Width, Bounds.Width), GetHorizontalRowLimit());
        }
        finally
        {
            _isSizingControls = false;
        }
    }

    private static void SizeControlsHorizontally(IReadOnlyList<RibbonGroupBox> groups, double availableWidth, int maxRows)
    {
        if (double.IsInfinity(availableWidth))
        {
            for (var i = 0; i < groups.Count; i++)
                SetDisplayMode(groups[i], GroupDisplayMode.Large);

            return;
        }

        for (var i = 0; i < groups.Count; i++)
        {
            if (CanFitHorizontal(groups, availableWidth, maxRows))
                break;

            var candidate = groups.LastOrDefault(group => group.DisplayMode == GroupDisplayMode.Large);
            if (candidate is null)
                break;

            SetDisplayMode(candidate, GroupDisplayMode.Small);
        }

        for (var i = 0; i < groups.Count; i++)
        {
            var candidate = groups.FirstOrDefault(group => group.DisplayMode != GroupDisplayMode.Large);
            if (candidate is null)
                break;

            SetDisplayMode(candidate, GroupDisplayMode.Large);

            if (!CanFitHorizontal(groups, availableWidth, maxRows))
                SetDisplayMode(candidate, GroupDisplayMode.Small);
        }
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
        {
            if (CanFitVertical(groups, availableHeight))
                break;

            var candidate = groups.LastOrDefault(group => group.DisplayMode == GroupDisplayMode.Large);
            if (candidate is null)
                break;

            SetDisplayMode(candidate, GroupDisplayMode.Small);
        }

        for (var i = 0; i < groups.Count; i++)
        {
            var candidate = groups.FirstOrDefault(group => group.DisplayMode != GroupDisplayMode.Large);
            if (candidate is null)
                break;

            SetDisplayMode(candidate, GroupDisplayMode.Large);

            if (!CanFitVertical(groups, availableHeight))
                SetDisplayMode(candidate, GroupDisplayMode.Small);
        }
    }

    private sealed class RowLayout
    {
        public List<Control> Children { get; } = new();

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
