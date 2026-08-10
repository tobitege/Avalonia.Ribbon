using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonGroupContainerTests
{
    [Fact]
    public void Triple_AppliesDisplayModeAndClampsChildSize()
    {
        var triple = new RibbonGroupTriple();
        var child = new TestRibbonControl
        {
            MinSize = RibbonControlSize.Medium,
            MaxSize = RibbonControlSize.Large
        };

        triple.Children.Add(child);

        triple.ApplyDisplayMode(GroupDisplayMode.Small);

        Assert.Equal(GroupDisplayMode.Small, triple.DisplayMode);
        Assert.Equal(RibbonControlSize.Medium, child.Size);

        triple.ApplyDisplayMode(GroupDisplayMode.Medium);

        Assert.Equal(GroupDisplayMode.Medium, triple.DisplayMode);
        Assert.Equal(RibbonControlSize.Medium, child.Size);

        triple.ApplyDisplayMode(GroupDisplayMode.Large);

        Assert.Equal(RibbonControlSize.Large, child.Size);
    }

    [Fact]
    public void Lines_MapsLargeDisplayToMediumChildSize()
    {
        var lines = new RibbonGroupLines();
        var child = new TestRibbonControl
        {
            MinSize = RibbonControlSize.Small,
            MaxSize = RibbonControlSize.Large
        };

        lines.Children.Add(child);
        lines.ApplyDisplayMode(GroupDisplayMode.Medium);

        Assert.Equal(RibbonControlSize.Medium, lines.CurrentSize);
        Assert.Equal(RibbonControlSize.Medium, child.Size);

        lines.ApplyDisplayMode(GroupDisplayMode.Large);

        Assert.Equal(RibbonControlSize.Medium, lines.CurrentSize);
        Assert.Equal(RibbonControlSize.Medium, child.Size);

        lines.ApplyDisplayMode(GroupDisplayMode.Small);

        Assert.Equal(RibbonControlSize.Small, lines.CurrentSize);
        Assert.Equal(RibbonControlSize.Small, child.Size);
    }

    [Fact]
    public void Cluster_UsesMediumAsMaximumSize()
    {
        var cluster = new RibbonGroupCluster();
        var child = new TestRibbonControl
        {
            MinSize = RibbonControlSize.Small,
            MaxSize = RibbonControlSize.Large
        };

        cluster.Children.Add(child);
        cluster.ApplyDisplayMode(GroupDisplayMode.Large);

        Assert.Equal(RibbonControlSize.Medium, cluster.CurrentSize);
        Assert.Equal(RibbonControlSize.Medium, child.Size);
    }

    [Fact]
    public void GroupWrapPanel_PropagatesDisplayModeToNestedContainers()
    {
        var wrapPanel = new RibbonGroupWrapPanel
        {
            DisplayMode = GroupDisplayMode.Large
        };

        var lines = new RibbonGroupLines();
        var child = new TestRibbonControl
        {
            MinSize = RibbonControlSize.Small,
            MaxSize = RibbonControlSize.Large
        };

        lines.Children.Add(child);
        wrapPanel.Children.Add(lines);

        wrapPanel.DisplayMode = GroupDisplayMode.Small;

        Assert.Equal(Orientation.Vertical, wrapPanel.Orientation);
        Assert.Equal(GroupDisplayMode.Small, lines.DisplayMode);
        Assert.Equal(RibbonControlSize.Small, child.Size);
    }

    [Fact]
    public void GroupWrapPanel_PopupDisplayModeUsesCompactItemSizing()
    {
        var wrapPanel = new RibbonGroupWrapPanel();
        var lines = new RibbonGroupLines();
        var child = new TestRibbonControl
        {
            MinSize = RibbonControlSize.Small,
            MaxSize = RibbonControlSize.Large
        };

        lines.Children.Add(child);
        wrapPanel.Children.Add(lines);

        wrapPanel.DisplayMode = GroupDisplayMode.Popup;

        Assert.Equal(Orientation.Vertical, wrapPanel.Orientation);
        Assert.Equal(GroupDisplayMode.Popup, lines.DisplayMode);
        Assert.Equal(RibbonControlSize.Small, child.Size);
    }

    [Fact]
    public void GroupWrapPanel_SmallDisplayModeRespectsSmallLineCount()
    {
        var wrapPanel = new RibbonGroupWrapPanel
        {
            SmallLineCount = 2
        };

        for (var i = 0; i < 5; i++)
            wrapPanel.Children.Add(new TestRibbonControl());

        wrapPanel.DisplayMode = GroupDisplayMode.Small;
        wrapPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        wrapPanel.Arrange(new Rect(0, 0, wrapPanel.DesiredSize.Width, wrapPanel.DesiredSize.Height));

        var rowCount = wrapPanel.Children
            .Select(child => Math.Round(child.Bounds.Y, 3))
            .Distinct()
            .Count();

        Assert.Equal(2, rowCount);
    }

    private sealed class TestRibbonControl : Control, IRibbonControl
    {
        public RibbonControlSize Size { get; set; } = RibbonControlSize.Large;

        public RibbonControlSize MinSize { get; set; } = RibbonControlSize.Small;

        public RibbonControlSize MaxSize { get; set; } = RibbonControlSize.Large;

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(36, 22);
        }
    }
}
