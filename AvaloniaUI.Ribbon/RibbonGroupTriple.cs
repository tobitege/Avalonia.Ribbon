using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupTriple : RibbonGroupContainer
{
    private const int DefaultRows = 3;

    static RibbonGroupTriple()
    {
        AffectsArrange<RibbonGroupTriple>(ItemAlignmentProperty, MaxItemsPerColumnProperty);
        AffectsMeasure<RibbonGroupTriple>(ItemAlignmentProperty, MaxItemsPerColumnProperty);
    }

    public static readonly StyledProperty<RibbonItemAlignment> ItemAlignmentProperty =
        AvaloniaProperty.Register<RibbonGroupTriple, RibbonItemAlignment>(nameof(ItemAlignment), RibbonItemAlignment.Near);

    public static readonly StyledProperty<int> MaxItemsPerColumnProperty =
        AvaloniaProperty.Register<RibbonGroupTriple, int>(nameof(MaxItemsPerColumn), DefaultRows);

    public RibbonItemAlignment ItemAlignment
    {
        get => GetValue(ItemAlignmentProperty);
        set => SetValue(ItemAlignmentProperty, value);
    }

    public int MaxItemsPerColumn
    {
        get => GetValue(MaxItemsPerColumnProperty);
        set => SetValue(MaxItemsPerColumnProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureChildControlSizes();

        var visibleChildren = GetVisibleChildren();
        if (visibleChildren.Count == 0)
            return default;

        var metrics = BuildLayoutMetrics(visibleChildren, ResolveRows());
        return new Size(metrics.DesiredWidth, metrics.DesiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var visibleChildren = GetVisibleChildren();
        if (visibleChildren.Count == 0)
            return default;

        var metrics = BuildLayoutMetrics(visibleChildren, ResolveRows());
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
                var slotWidth = metrics.ColumnWidths[column];
                var desiredSize = child.DesiredSize;
                var childWidth = Math.Min(slotWidth, desiredSize.Width);

                var alignedX = x + ResolveHorizontalAlignmentOffset(slotWidth, childWidth, ItemAlignment);
                child.Arrange(new Rect(alignedX, y, childWidth, rowHeight));

                x += slotWidth + spacing;
            }

            y += rowHeight + spacing;
        }

        return new Size(metrics.DesiredWidth, metrics.DesiredHeight);
    }

    private int ResolveRows()
    {
        return Math.Max(1, MaxItemsPerColumn);
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

    private static double ResolveHorizontalAlignmentOffset(double slotWidth, double childWidth, RibbonItemAlignment alignment)
    {
        return alignment switch
        {
            RibbonItemAlignment.Center => (slotWidth - childWidth) / 2,
            RibbonItemAlignment.Far => slotWidth - childWidth,
            _ => 0
        };
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
