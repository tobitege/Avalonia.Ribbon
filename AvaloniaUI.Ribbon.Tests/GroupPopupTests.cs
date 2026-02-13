using System.Linq;
using Avalonia;
using Avalonia.Layout;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class GroupPopupTests
{
    [Fact]
    public void Overflow_CollapsesGroupsToPopupWhenAllowed()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 3, allowCollapsedPopup: true);

        RunLayout(panel, 120);

        Assert.Contains(groups, group => group.IsCollapsedToPopup);
    }

    [Fact]
    public void Overflow_DoesNotCollapseGroupsToPopupWhenDisallowed()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 3, allowCollapsedPopup: false);

        RunLayout(panel, 120);

        Assert.DoesNotContain(groups, group => group.IsCollapsedToPopup);
    }

    [Fact]
    public void WiderLayout_ResetsCollapsedPopupState()
    {
        var panel = new RibbonGroupsStackPanel
        {
            Orientation = Orientation.Horizontal,
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1
        };

        var groups = CreateGroups(panel, 3, allowCollapsedPopup: true);

        RunLayout(panel, 120);
        Assert.Contains(groups, group => group.IsCollapsedToPopup);

        RunLayout(panel, 600);
        Assert.DoesNotContain(groups, group => group.IsCollapsedToPopup);
    }

    private static TestRibbonGroupBox[] CreateGroups(
        RibbonGroupsStackPanel panel,
        int count,
        bool allowCollapsedPopup)
    {
        var groups = new TestRibbonGroupBox[count];
        for (var i = 0; i < count; i++)
        {
            groups[i] = new TestRibbonGroupBox
            {
                AllowCollapsedPopup = allowCollapsedPopup,
                DisplayMode = GroupDisplayMode.Large,
                LargeDesiredSize = new Size(180, 96),
                MediumDesiredSize = new Size(130, 96),
                SmallDesiredSize = new Size(95, 96)
            };

            panel.Children.Add(groups[i]);
        }

        return groups;
    }

    private static void RunLayout(RibbonGroupsStackPanel panel, double width)
    {
        panel.Measure(new Size(width, double.PositiveInfinity));
        var height = Math.Max(panel.DesiredSize.Height, 1);
        panel.Arrange(new Rect(0, 0, width, height));
    }

    private sealed class TestRibbonGroupBox : RibbonGroupBox
    {
        public Size LargeDesiredSize { get; set; } = new(180, 96);

        public Size MediumDesiredSize { get; set; } = new(130, 96);

        public Size SmallDesiredSize { get; set; } = new(95, 96);

        protected override Size MeasureOverride(Size availableSize)
        {
            return DisplayMode switch
            {
                GroupDisplayMode.Small => SmallDesiredSize,
                GroupDisplayMode.Medium => MediumDesiredSize,
                _ => LargeDesiredSize
            };
        }
    }
}
