using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class GroupPopupTests
{
    private static Application? _styledApplication;

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

    [Fact]
    public void RibbonOverflow_RendersOneButtonAndOpensSharedPopup()
    {
        EnsureStyles();

        var groups = Enumerable.Range(1, 5)
            .Select(index =>
            {
                var group = new RibbonGroupBox
                {
                    Header = $"Group {index}",
                    AllowCollapsedPopup = true
                };

                for (var action = 1; action <= 3; action++)
                {
                    group.Items.Add(new RibbonButton
                    {
                        Content = $"Action {index}.{action}",
                        MinSize = RibbonControlSize.Small,
                        MaxSize = RibbonControlSize.Large
                    });
                }

                return group;
            })
            .ToArray();

        var tab = new RibbonTab { Header = "Home" };
        foreach (var group in groups)
            tab.Groups.Add(group);

        var ribbon = new OverflowTestRibbon
        {
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1,
            SelectedIndex = 0,
            Tabs = new ObservableCollection<Control> { tab }
        };
        var window = new Window { Width = 320, Height = 240, Content = ribbon };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var panel = ribbon.GetVisualDescendants()
                .OfType<RibbonGroupsStackPanel>()
                .Single(control => control.Children.Count > 0);
            var overflowGroups = groups
                .Where(group => group.DisplayMode == GroupDisplayMode.Popup)
                .ToArray();
            Assert.NotEmpty(overflowGroups);
            Assert.Same(ribbon, panel.OverflowOwner);
            Assert.True(ribbon.HasGroupOverflow);

            var toggleButton = ribbon.GroupOverflowButton;
            var popup = ribbon.GroupOverflowPopup;
            var overflowHost = ribbon.GroupOverflowHost;
            Assert.NotNull(toggleButton);
            Assert.NotNull(popup);
            Assert.NotNull(overflowHost);

            Assert.Equal(
                overflowGroups.Select(group => group.Header),
                ribbon.OverflowGroups.Select(group => group.Header));
            Assert.Equal(RibbonGroupOverflowBehavior.WrapThenShrink, panel.GroupOverflowBehavior);
            Assert.Equal(1, panel.MaxGroupRows);
            Assert.True(double.IsFinite(panel.Bounds.Width));
            Assert.True(ribbon.HasGroupOverflow);
            Assert.True(toggleButton.IsVisible);
            Assert.Equal(1, overflowHost.Opacity);
            Assert.True(toggleButton.IsEnabled);
            Assert.True(toggleButton.Bounds.Width > 0);
            Assert.False(popup.IsOpen);

            toggleButton.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            Assert.True(popup.IsOpen);
            Assert.NotNull(popup.Child);
            Assert.Same(window, TopLevel.GetTopLevel(popup.Child));
            Assert.Equal(
                overflowGroups.Sum(group => group.Items.Count),
                popup.Child.GetVisualDescendants().OfType<RibbonButton>().Count());

            window.Width = 2000;
            Dispatcher.UIThread.RunJobs();

            Assert.Empty(ribbon.OverflowGroups);
            Assert.False(ribbon.HasGroupOverflow);
            Assert.False(ribbon.IsGroupOverflowOpen);
            Assert.Equal(0, overflowHost.Opacity);
            Assert.False(toggleButton.IsEnabled);
            Assert.DoesNotContain(groups, group => group.DisplayMode == GroupDisplayMode.Popup);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class OverflowTestRibbon : Ribbon
    {
        public ToggleButton? GroupOverflowButton { get; private set; }

        public Popup? GroupOverflowPopup { get; private set; }

        public Border? GroupOverflowHost { get; private set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            GroupOverflowButton = e.NameScope.Find<ToggleButton>("PART_GroupOverflowButton");
            GroupOverflowPopup = e.NameScope.Find<Popup>("PART_GroupOverflowPopup");
            GroupOverflowHost = e.NameScope.Find<Border>("PART_GroupOverflowHost");
        }
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

    private static void EnsureStyles()
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia test application is not initialized.");

        if (ReferenceEquals(_styledApplication, application))
            return;

        application.Styles.Add(new FluentTheme());
        application.Styles.Add(new StyleInclude(new Uri("avares://AvaloniaUI.Ribbon/"))
        {
            Source = new Uri("avares://AvaloniaUI.Ribbon/Styles/Fluent/AvaloniaRibbon.axaml")
        });

        _styledApplication = application;
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
