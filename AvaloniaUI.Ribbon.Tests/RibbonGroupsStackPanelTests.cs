using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonGroupsStackPanelTests
{
    [Fact]
    public void DefaultBehavior_RemainsSingleRowShrinkOnly()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 3);

        RunLayout(panel, 450);

        Assert.Equal(1, GetRowCount(groups));
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Small);
    }

    [Fact]
    public void WrapThenShrink_UsesSecondRowBeforeShrinking()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 3);

        RunLayout(panel, 400);

        Assert.Equal(2, GetRowCount(groups));
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));
    }

    [Fact]
    public void WrapThenShrink_ShrinksWhenRowLimitCannotFit()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 5);

        RunLayout(panel, 350);

        Assert.True(GetRowCount(groups) <= 2);
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Small);
    }

    [Fact]
    public void WrapThenShrink_ReExpandsWhenWidthIncreases()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 5);

        RunLayout(panel, 350);
        var largeAtNarrowWidth = groups.Count(group => group.DisplayMode == GroupDisplayMode.Large);

        RunLayout(panel, 600);
        var largeAtWideWidth = groups.Count(group => group.DisplayMode == GroupDisplayMode.Large);

        Assert.True(largeAtWideWidth > largeAtNarrowWidth);
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));
    }

    [Fact]
    public void WrapThenShrink_AllowsMoreThanTwoRowsWhenConfigured()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 3
        };

        var groups = CreateGroups(panel, 6);

        RunLayout(panel, 450);

        Assert.Equal(3, GetRowCount(groups));
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));
    }

    private static List<TestRibbonGroupBox> CreateGroups(RibbonGroupsStackPanel panel, int count)
    {
        var groups = new List<TestRibbonGroupBox>();

        for (var i = 0; i < count; i++)
        {
            var group = new TestRibbonGroupBox
            {
                DisplayMode = GroupDisplayMode.Large,
                LargeDesiredSize = new Size(200, 96),
                SmallDesiredSize = new Size(100, 96)
            };

            panel.Children.Add(group);
            groups.Add(group);
        }

        return groups;
    }

    private static int GetRowCount(IEnumerable<TestRibbonGroupBox> groups)
    {
        return groups
            .Select(group => Math.Round(group.Bounds.Y, 3))
            .Distinct()
            .Count();
    }

    private static void RunLayout(RibbonGroupsStackPanel panel, double width)
    {
        panel.Measure(new Size(width, double.PositiveInfinity));

        var height = Math.Max(1, panel.DesiredSize.Height);
        panel.Arrange(new Rect(0, 0, width, height));
    }

    private sealed class TestRibbonGroupBox : RibbonGroupBox
    {
        public Size LargeDesiredSize { get; set; } = new(200, 96);

        public Size SmallDesiredSize { get; set; } = new(100, 96);

        protected override Size MeasureOverride(Size availableSize)
        {
            return DisplayMode == GroupDisplayMode.Large ? LargeDesiredSize : SmallDesiredSize;
        }
    }
}
