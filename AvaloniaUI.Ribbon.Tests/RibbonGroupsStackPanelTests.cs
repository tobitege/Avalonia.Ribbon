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
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Medium);
        Assert.All(groups, group => Assert.NotEqual(GroupDisplayMode.Small, group.DisplayMode));
    }

    [Fact]
    public void WrapThenShrink_UsesMediumBeforeSmall()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 5, mediumWidth: 150);

        RunLayout(panel, 550);

        Assert.Equal(2, GetRowCount(groups));
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Medium);
        Assert.All(groups, group => Assert.NotEqual(GroupDisplayMode.Small, group.DisplayMode));
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
    public void ReExpandsThroughMediumBeforeReturningToLarge()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 3, smallWidth: 100, mediumWidth: 150, largeWidth: 200);

        RunLayout(panel, 350);
        var smallCountAtNarrowWidth = groups.Count(group => group.DisplayMode == GroupDisplayMode.Small);
        Assert.True(smallCountAtNarrowWidth > 0);

        RunLayout(panel, 500);
        Assert.All(groups, group => Assert.NotEqual(GroupDisplayMode.Small, group.DisplayMode));
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Medium);
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Large);
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

    [Fact]
    public void VerticalOrientation_RemainsSingleColumnWithWrapThenShrinkSettings()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Vertical,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 4);

        RunVerticalLayout(panel, 1000);

        var columnCount = groups
            .Select(group => Math.Round(group.Bounds.X, 3))
            .Distinct()
            .Count();

        Assert.Equal(1, columnCount);
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));
    }

    private static List<TestRibbonGroupBox> CreateGroups(
        RibbonGroupsStackPanel panel,
        int count,
        double smallWidth = 100,
        double mediumWidth = 125,
        double largeWidth = 200)
    {
        var groups = new List<TestRibbonGroupBox>();

        for (var i = 0; i < count; i++)
        {
            var group = new TestRibbonGroupBox
            {
                DisplayMode = GroupDisplayMode.Large,
                LargeDesiredSize = new Size(largeWidth, 96),
                MediumDesiredSize = new Size(mediumWidth, 96),
                SmallDesiredSize = new Size(smallWidth, 96)
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

    private static void RunVerticalLayout(RibbonGroupsStackPanel panel, double height)
    {
        panel.Measure(new Size(double.PositiveInfinity, height));

        var arrangedWidth = Math.Max(1, panel.DesiredSize.Width);
        panel.Arrange(new Rect(0, 0, arrangedWidth, height));
    }

    private sealed class TestRibbonGroupBox : RibbonGroupBox
    {
        public Size LargeDesiredSize { get; set; } = new(200, 96);

        public Size MediumDesiredSize { get; set; } = new(125, 96);

        public Size SmallDesiredSize { get; set; } = new(100, 96);

        protected override Size MeasureOverride(Size availableSize)
        {
            return DisplayMode switch
            {
                GroupDisplayMode.Medium => MediumDesiredSize,
                GroupDisplayMode.Small => SmallDesiredSize,
                _ => LargeDesiredSize
            };
        }
    }
}
