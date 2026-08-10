using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupLines : RibbonGroupContainer
{
    private const int DefaultLargeLineCount = 2;
    private const int DefaultSmallLineCount = 3;

    static RibbonGroupLines()
    {
        AffectsArrange<RibbonGroupLines>(LargeLineCountProperty, SmallLineCountProperty);
        AffectsMeasure<RibbonGroupLines>(LargeLineCountProperty, SmallLineCountProperty);
    }

    public static readonly StyledProperty<int> LargeLineCountProperty =
        AvaloniaProperty.Register<RibbonGroupLines, int>(nameof(LargeLineCount), DefaultLargeLineCount);

    public static readonly StyledProperty<int> SmallLineCountProperty =
        AvaloniaProperty.Register<RibbonGroupLines, int>(nameof(SmallLineCount), DefaultSmallLineCount);

    public int LargeLineCount
    {
        get => GetValue(LargeLineCountProperty);
        set => SetValue(LargeLineCountProperty, value);
    }

    public int SmallLineCount
    {
        get => GetValue(SmallLineCountProperty);
        set => SetValue(SmallLineCountProperty, value);
    }

    protected override RibbonControlSize ResolveTargetSize(GroupDisplayMode displayMode)
    {
        var requestedSize = base.ResolveTargetSize(displayMode);
        return requestedSize == RibbonControlSize.Large ? RibbonControlSize.Medium : requestedSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureChildControlSizes();

        var visibleChildren = GetVisibleChildren();
        if (visibleChildren.Count == 0)
            return default;

        var metrics = BuildLayoutMetrics(visibleChildren, ResolveLineCount());
        return new Size(metrics.DesiredWidth, metrics.DesiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = GetVisibleChildren();
        if (visibleChildren.Count == 0)
            return default;

        var metrics = BuildLayoutMetrics(visibleChildren, ResolveLineCount());
        var spacing = Math.Max(0, ItemSpacing);

        var y = 0d;
        for (var row = 0; row < metrics.RowHeights.Length; row++)
        {
            var rowHeight = metrics.RowHeights[row];
            var x = 0d;

            for (var column = 0; column < metrics.ColumnWidths.Length; column++)
            {
                var childIndex = (column * metrics.RowHeights.Length) + row;
                if (childIndex >= visibleChildren.Count)
                    continue;

                var child = visibleChildren[childIndex];
                var desiredSize = child.DesiredSize;
                var arrangedWidth = Math.Min(metrics.ColumnWidths[column], desiredSize.Width);

                child.Arrange(new Rect(x, y, arrangedWidth, rowHeight));
                x += metrics.ColumnWidths[column] + spacing;
            }

            y += rowHeight + spacing;
        }

        return new Size(metrics.DesiredWidth, metrics.DesiredHeight);
    }

    private int ResolveLineCount()
    {
        var lineCount = DisplayMode is GroupDisplayMode.Small or GroupDisplayMode.Popup
            ? SmallLineCount
            : LargeLineCount;
        return Math.Max(1, lineCount);
    }

    private IReadOnlyList<Control> GetVisibleChildren()
    {
        var visibleChildren = new List<Control>();

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].IsVisible)
                visibleChildren.Add(Children[i]);
        }

        return visibleChildren;
    }

    private LayoutMetrics BuildLayoutMetrics(IReadOnlyList<Control> visibleChildren, int rows)
    {
        var rowCount = Math.Min(rows, visibleChildren.Count);
        var columnCount = (int)Math.Ceiling((double)visibleChildren.Count / rows);
        var columnWidths = new double[columnCount];
        var rowHeights = new double[rowCount];

        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var row = i % rows;
            var column = i / rows;
            var desired = child.DesiredSize;

            if (columnWidths[column] < desired.Width)
                columnWidths[column] = desired.Width;

            if (row < rowHeights.Length && rowHeights[row] < desired.Height)
                rowHeights[row] = desired.Height;
        }

        var spacing = Math.Max(0, ItemSpacing);
        var desiredWidth = 0d;
        var desiredHeight = 0d;

        for (var i = 0; i < columnWidths.Length; i++)
            desiredWidth += columnWidths[i];

        for (var i = 0; i < rowHeights.Length; i++)
            desiredHeight += rowHeights[i];

        if (columnWidths.Length > 1)
            desiredWidth += spacing * (columnWidths.Length - 1);

        if (rowHeights.Length > 1)
            desiredHeight += spacing * (rowHeights.Length - 1);

        return new LayoutMetrics(columnWidths, rowHeights, desiredWidth, desiredHeight);
    }

    private sealed class LayoutMetrics
    {
        public LayoutMetrics(double[] columnWidths, double[] rowHeights, double desiredWidth, double desiredHeight)
        {
            ColumnWidths = columnWidths;
            RowHeights = rowHeights;
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
        }

        public double[] ColumnWidths { get; }

        public double[] RowHeights { get; }

        public double DesiredWidth { get; }

        public double DesiredHeight { get; }
    }
}
