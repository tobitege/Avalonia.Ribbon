using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon;

public class RibbonGroupWrapPanel : WrapPanel
{
    private const double Epsilon = 0.01;

    public static readonly StyledProperty<GroupDisplayMode> DisplayModeProperty =
        RibbonGroupBox.DisplayModeProperty
            .AddOwner<
                RibbonGroupWrapPanel>(); //AvaloniaProperty.Register<RibbonGroupWrapPanel, GroupDisplayMode>(nameof(DisplayMode), defaultValue: GroupDisplayMode.Large);

    public static readonly StyledProperty<int> LargeLineCountProperty =
        AvaloniaProperty.Register<RibbonGroupWrapPanel, int>(nameof(LargeLineCount), 3,
            coerce: (_, value) => Math.Max(1, value));

    public static readonly StyledProperty<int> SmallLineCountProperty =
        AvaloniaProperty.Register<RibbonGroupWrapPanel, int>(nameof(SmallLineCount), 3,
            coerce: (_, value) => Math.Max(1, value));

    static RibbonGroupWrapPanel()
    {
        AffectsArrange<RibbonGroupWrapPanel>(DisplayModeProperty, LargeLineCountProperty, SmallLineCountProperty);
        AffectsMeasure<RibbonGroupWrapPanel>(DisplayModeProperty, LargeLineCountProperty, SmallLineCountProperty);
        AffectsRender<RibbonGroupWrapPanel>(DisplayModeProperty);

        DisplayModeProperty.Changed.AddClassHandler<RibbonGroupWrapPanel>((sender, args) =>
        {
            if (args.NewValue is GroupDisplayMode displayMode)
                sender.ApplyDisplayMode(displayMode);
        });
    }

    public RibbonGroupWrapPanel()
    {
        Children.CollectionChanged += (_, _) => ApplyDisplayMode(DisplayMode);

        if (TemplatedParent is RibbonGroupBox parentBox)
        {
            parentBox.Rearranged += (_, _) => ArrangeOverride(Bounds.Size);
            parentBox.Remeasured += (_, _) => MeasureOverride(Bounds.Size);
        }
    }

    public GroupDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

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

    protected override Size MeasureOverride(Size availableSize)
    {
        var lines = BuildLines(availableSize);
        if (lines.Count == 0)
            return default;

        var primary = 0d;
        var cross = 0d;
        for (var i = 0; i < lines.Count; i++)
        {
            primary = Math.Max(primary, lines[i].PrimarySize);
            cross += lines[i].CrossSize;
        }

        return Orientation == Orientation.Horizontal
            ? new Size(primary, cross)
            : new Size(cross, primary);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var lines = BuildLines(finalSize);
        if (lines.Count == 0)
            return finalSize;

        var crossOffset = 0d;
        var maxPrimary = 0d;
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var primaryOffset = 0d;

            for (var itemIndex = 0; itemIndex < line.Children.Count; itemIndex++)
            {
                var child = line.Children[itemIndex];
                var childPrimary = GetPrimary(child.DesiredSize);

                var arrangedRect = Orientation == Orientation.Horizontal
                    ? new Rect(primaryOffset, crossOffset, childPrimary, line.CrossSize)
                    : new Rect(crossOffset, primaryOffset, line.CrossSize, childPrimary);

                child.Arrange(arrangedRect);
                primaryOffset += childPrimary;
            }

            maxPrimary = Math.Max(maxPrimary, primaryOffset);
            crossOffset += line.CrossSize;
        }

        return Orientation == Orientation.Horizontal
            ? new Size(maxPrimary, crossOffset)
            : new Size(crossOffset, maxPrimary);
    }

    private void ApplyDisplayMode(GroupDisplayMode displayMode)
    {
        Orientation = displayMode == GroupDisplayMode.Small ? Orientation.Vertical : Orientation.Horizontal;

        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i] is IRibbonGroupContainer groupContainer)
                groupContainer.ApplyDisplayMode(displayMode);

            if (Children[i] is IRibbonControl control)
                control.Size = displayMode == GroupDisplayMode.Small ? control.MinSize : control.MaxSize;
        }
    }

    private int ResolveLineCount()
    {
        return DisplayMode == GroupDisplayMode.Small
            ? Math.Max(1, SmallLineCount)
            : Math.Max(1, LargeLineCount);
    }

    private double GetPrimary(Size size)
    {
        return Orientation == Orientation.Horizontal ? size.Width : size.Height;
    }

    private double GetCross(Size size)
    {
        return Orientation == Orientation.Horizontal ? size.Height : size.Width;
    }

    private List<LineInfo> BuildLines(Size availableSize)
    {
        var visibleChildren = new List<Control>();
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].IsVisible)
                visibleChildren.Add(Children[i]);
        }

        if (visibleChildren.Count == 0)
            return new List<LineInfo>();

        var maxItemsPerLine = ResolveLineCount();
        var availablePrimary = GetPrimary(availableSize);
        var canWrapBySize = !double.IsInfinity(availablePrimary) && availablePrimary > Epsilon;

        var lines = new List<LineInfo>();
        var currentLine = new LineInfo();
        lines.Add(currentLine);

        for (var i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var childPrimary = GetPrimary(child.DesiredSize);
            var childCross = GetCross(child.DesiredSize);

            var wrapByCount = currentLine.Children.Count >= maxItemsPerLine;
            var wrapBySize = canWrapBySize &&
                currentLine.Children.Count > 0 &&
                currentLine.PrimarySize + childPrimary > availablePrimary + Epsilon;

            if (currentLine.Children.Count > 0 && (wrapByCount || wrapBySize))
            {
                currentLine = new LineInfo();
                lines.Add(currentLine);
            }

            currentLine.Children.Add(child);
            currentLine.PrimarySize += childPrimary;
            currentLine.CrossSize = Math.Max(currentLine.CrossSize, childCross);
        }

        return lines;
    }

    private sealed class LineInfo
    {
        public List<Control> Children { get; } = new();

        public double PrimarySize { get; set; }

        public double CrossSize { get; set; }
    }
}
