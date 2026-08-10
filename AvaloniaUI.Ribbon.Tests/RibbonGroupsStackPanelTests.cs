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
    public void PopupOverflow_ReservesTheSharedButtonWidthInEveryRow()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateGroups(panel, 5, smallWidth: 70, mediumWidth: 70, largeWidth: 70);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 150);

        var visibleGroups = groups
            .Where(group => group.DisplayMode != GroupDisplayMode.Popup)
            .ToArray();
        Assert.Equal(2, visibleGroups.Length);
        Assert.Equal(2, GetRowCount(visibleGroups));
        Assert.All(visibleGroups, group => Assert.True(group.Bounds.Right <= 120.01));
    }

    [Fact]
    public void PopupOverflow_DoesNotUseSmallModeForVisibleGroups()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 5);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 300);

        var visibleGroups = groups
            .Where(group => group.DisplayMode != GroupDisplayMode.Popup)
            .ToArray();

        Assert.NotEmpty(visibleGroups);
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Popup);
        Assert.All(
            visibleGroups,
            group => Assert.NotEqual(GroupDisplayMode.Small, group.DisplayMode));
    }

    [Fact]
    public void PopupOverflow_UsesMediumBeforeRemovingLastGroup()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(
            panel,
            count: 2,
            smallWidth: 80,
            mediumWidth: 120,
            largeWidth: 140);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 300);

        Assert.DoesNotContain(groups, group => group.DisplayMode == GroupDisplayMode.Popup);
        Assert.Contains(groups, group => group.DisplayMode == GroupDisplayMode.Medium);
        Assert.All(groups, group => Assert.NotEqual(GroupDisplayMode.Small, group.DisplayMode));
    }

    [Fact]
    public void PopupOverflow_KeepsLastGroupVisibleWhileItsActualRightEdgeHasBuffer()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = Enumerable.Range(0, 4)
            .Select(_ => new ConstraintSensitiveRibbonGroupBox
            {
                AllowCollapsedPopup = true
            })
            .ToArray();

        foreach (var group in groups)
            panel.Children.Add(group);

        const double panelWidth = 450;
        const double panelHeight = 96;
        panel.Measure(new Size(panelWidth, panelHeight));
        panel.Arrange(new Rect(0, 0, panelWidth, panelHeight));

        Assert.DoesNotContain(groups, group => group.DisplayMode == GroupDisplayMode.Popup);

        var lastGroupRight = groups.Max(group => group.Bounds.Right);
        var freeSpaceBeforeOverflowSlot = panelWidth - 30 - lastGroupRight;
        Assert.True(freeSpaceBeforeOverflowSlot > 10);

        const double narrowerPanelWidth = 439;
        panel.Measure(new Size(narrowerPanelWidth, panelHeight));
        panel.Arrange(new Rect(0, 0, narrowerPanelWidth, panelHeight));

        var overflowGroup = Assert.Single(
            groups,
            group => group.DisplayMode == GroupDisplayMode.Popup);
        Assert.Same(groups[^1], overflowGroup);

        var lastVisibleGroup = groups.Last(group => group.DisplayMode != GroupDisplayMode.Popup);
        var remainingFreeSpace = narrowerPanelWidth - 30 - lastVisibleGroup.Bounds.Right;
        Assert.True(remainingFreeSpace > 10);
    }

    [Fact]
    public void InfiniteMeasureWidth_UsesFiniteBoundsForPopupCollapse()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 5);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 350);

        Assert.Equal(350, panel.Bounds.Width);
        Assert.Contains(groups, group => group.IsCollapsedToPopup);

        panel.InvalidateMeasure();
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(panel.Bounds.Width, panel.DesiredSize.Width);
        Assert.Contains(groups, group => group.IsCollapsedToPopup);
    }

    [Fact]
    public void InfiniteMeasureFollowedByResize_AppliesCurrentArrangeWidthImmediately()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 5);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 1200);
        Assert.DoesNotContain(groups, group => group.IsCollapsedToPopup);
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));

        RunInfiniteMeasureAndArrange(panel, 350);
        Assert.Contains(groups, group => group.IsCollapsedToPopup);
        Assert.True(groups.Max(group => group.Bounds.Right) <= 350 + 0.01);

        RunInfiniteMeasureAndArrange(panel, 1200);
        Assert.DoesNotContain(groups, group => group.IsCollapsedToPopup);
        Assert.All(groups, group => Assert.Equal(GroupDisplayMode.Large, group.DisplayMode));
    }

    [Fact]
    public void RepeatedInfiniteMeasureAtSameBounds_DoesNotToggleLayoutState()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 5);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 350);
        Assert.Contains(groups, group => group.IsCollapsedToPopup);

        foreach (var group in groups)
            group.ResetLayoutStateChangeCounts();

        RunInfiniteMeasureAndArrange(panel, 350);

        Assert.All(groups, group => Assert.Equal(0, group.DisplayModeChangeCount));
        Assert.All(groups, group => Assert.Equal(0, group.CollapsedStateChangeCount));
    }

    [Fact]
    public void OnePixelResizeCycles_ReturnToTheSameLayoutState()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 5);
        foreach (var group in groups)
            group.AllowCollapsedPopup = true;

        RunLayout(panel, 350);
        var stateAt350 = GetLayoutState(groups);

        RunInfiniteMeasureAndArrange(panel, 351);
        var stateAt351 = GetLayoutState(groups);

        RunInfiniteMeasureAndArrange(panel, 350);
        Assert.Equal(stateAt350, GetLayoutState(groups));

        RunInfiniteMeasureAndArrange(panel, 351);
        Assert.Equal(stateAt351, GetLayoutState(groups));

        RunInfiniteMeasureAndArrange(panel, 350);
        Assert.Equal(stateAt350, GetLayoutState(groups));
    }

    [Fact]
    public void ResizeThreshold_IsIndependentOfPreviousWidth()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateThresholdGroups();

        foreach (var group in groups)
            panel.Children.Add(group);

        RunLayout(panel, 375);
        var directStateAt375 = GetLayoutState(groups);

        RunInfiniteMeasureAndArrange(panel, 376);
        RunInfiniteMeasureAndArrange(panel, 375);

        Assert.Equal(directStateAt375, GetLayoutState(groups));
    }

    [Fact]
    public void IncreasingWidth_NeverCompactsAGroupAgain()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 2
        };

        var groups = CreateThresholdGroups();

        foreach (var group in groups)
            panel.Children.Add(group);

        RunLayout(panel, 350);
        var previousModes = groups.Select(group => GetDisplayModeRank(group.DisplayMode)).ToList();

        for (var width = 351; width <= 650; width++)
        {
            RunInfiniteMeasureAndArrange(panel, width);

            var currentModes = groups.Select(group => GetDisplayModeRank(group.DisplayMode)).ToList();
            for (var i = 0; i < groups.Count; i++)
                Assert.True(currentModes[i] >= previousModes[i]);

            previousModes = currentModes;
        }
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

    private static TestRibbonGroupBox CreateGroup(double smallWidth, double mediumWidth, double largeWidth)
    {
        return new TestRibbonGroupBox
        {
            DisplayMode = GroupDisplayMode.Large,
            SmallDesiredSize = new Size(smallWidth, 96),
            MediumDesiredSize = new Size(mediumWidth, 96),
            LargeDesiredSize = new Size(largeWidth, 96)
        };
    }

    private static List<TestRibbonGroupBox> CreateThresholdGroups()
    {
        return
        [
            CreateGroup(82, 130, 159),
            CreateGroup(67, 107, 151),
            CreateGroup(71, 92, 147),
            CreateGroup(72, 94, 127),
            CreateGroup(71, 114, 157)
        ];
    }

    private static int GetDisplayModeRank(GroupDisplayMode mode) => mode switch
    {
        GroupDisplayMode.Small => 0,
        GroupDisplayMode.Medium => 1,
        GroupDisplayMode.Large => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    private static int GetRowCount(IEnumerable<TestRibbonGroupBox> groups)
    {
        return groups
            .Select(group => Math.Round(group.Bounds.Y, 3))
            .Distinct()
            .Count();
    }

    private static IReadOnlyList<(GroupDisplayMode DisplayMode, bool IsCollapsedToPopup)> GetLayoutState(
        IEnumerable<TestRibbonGroupBox> groups)
    {
        return groups
            .Select(group => (group.DisplayMode, group.IsCollapsedToPopup))
            .ToList();
    }

    private static void RunLayout(RibbonGroupsStackPanel panel, double width)
    {
        panel.Measure(new Size(width, double.PositiveInfinity));

        var height = Math.Max(1, panel.DesiredSize.Height);
        panel.Arrange(new Rect(0, 0, width, height));
    }

    private static void RunInfiniteMeasureAndArrange(RibbonGroupsStackPanel panel, double width)
    {
        panel.InvalidateMeasure();
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

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
        public int DisplayModeChangeCount { get; private set; }

        public int CollapsedStateChangeCount { get; private set; }

        public Size LargeDesiredSize { get; set; } = new(200, 96);

        public Size MediumDesiredSize { get; set; } = new(125, 96);

        public Size SmallDesiredSize { get; set; } = new(100, 96);

        public void ResetLayoutStateChangeCounts()
        {
            DisplayModeChangeCount = 0;
            CollapsedStateChangeCount = 0;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == DisplayModeProperty)
                DisplayModeChangeCount++;
            else if (change.Property == IsCollapsedToPopupProperty)
                CollapsedStateChangeCount++;
        }

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

    private sealed class ConstraintSensitiveRibbonGroupBox : RibbonGroupBox
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var width = double.IsInfinity(availableSize.Height) ? 200 : 100;
            return new Size(width, 96);
        }
    }
}
