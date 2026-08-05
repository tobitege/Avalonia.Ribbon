using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonIntegrationRegressionTests
{
    private static Application? _styledApplication;

    [Fact]
    public void DesktopStyleCollection_RendersBaseAndDesktopRibbon()
    {
        EnsureStyles();
        var baseRibbon = CreateRibbon(new Ribbon());
        var desktopRibbon = CreateRibbon(new DesktopRibbon());
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = new StackPanel
            {
                Children = { baseRibbon, desktopRibbon }
            }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(baseRibbon.GetVisualDescendants().OfType<Control>(),
                control => control.Name == "PART_ItemsPresenter");
            Assert.Contains(desktopRibbon.GetVisualDescendants().OfType<Control>(),
                control => control.Name == "PART_ItemsPresenter");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonWindow_OrientationStylesPositionPresenterWithoutBehaviors()
    {
        EnsureStyles();
        var toolbar = new QuickAccessToolbar();
        var ribbon = CreateRibbon(new DesktopRibbon { QuickAccessToolbar = toolbar });
        var window = new RibbonWindow
        {
            Width = 700,
            Height = 320,
            Orientation = Orientation.Horizontal,
            Ribbon = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var presenter = window.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_RibbonPresenter");

            Assert.Equal(Dock.Top, DockPanel.GetDock(presenter));
            Assert.Equal(VerticalAlignment.Top, presenter.VerticalAlignment);
            Assert.Equal(window.TitleBarForeground?.ToString(), toolbar.Foreground?.ToString());

            window.Orientation = Orientation.Vertical;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(Dock.Left, DockPanel.GetDock(presenter));
            Assert.Equal(VerticalAlignment.Stretch, presenter.VerticalAlignment);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonGroupBox_OrientationStylesSwitchSeparatorsWithoutBehaviors()
    {
        EnsureStyles();
        var group = new RibbonGroupBox { Header = "Group" };
        group.Items.Add(new RibbonButton { Content = "Action" });
        var tab = new RibbonTab { Header = "Tab" };
        tab.Groups.Add(group);
        var ribbon = new Ribbon
        {
            Orientation = Orientation.Horizontal,
            SelectedIndex = 0,
            Tabs = new ObservableCollection<Control> { tab }
        };
        var window = new Window { Width = 500, Height = 240, Content = ribbon };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var separators = group.GetVisualDescendants().OfType<Border>().ToArray();
            var vertical = separators.Single(control => control.Name == "VerticalSeparator");
            var horizontal = separators.Single(control => control.Name == "HorizontalSeparator");

            Assert.True(vertical.IsVisible);
            Assert.False(horizontal.IsVisible);

            ribbon.Orientation = Orientation.Vertical;
            Dispatcher.UIThread.RunJobs();

            Assert.False(vertical.IsVisible);
            Assert.True(horizontal.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void QuickAccessItems_RenderIconsAndForegroundOutsideRibbonWindow()
    {
        EnsureStyles();
        var buttonIcon = new Border { Width = 16, Height = 16 };
        var toggleIcon = new Border { Width = 16, Height = 16 };
        var button = new RibbonButton { Content = "Button", QuickAccessIcon = buttonIcon };
        var toggle = new RibbonToggleButton { Content = "Toggle", QuickAccessIcon = toggleIcon };
        var group = new RibbonGroupBox { Header = "Group" };
        group.Items.Add(button);
        group.Items.Add(toggle);
        var tab = new RibbonTab { Header = "Tab" };
        tab.Groups.Add(group);
        var ribbon = new Ribbon
        {
            SelectedIndex = 0,
            Tabs = new ObservableCollection<Control> { tab }
        };
        var toolbar = new QuickAccessToolbar();
        var window = new Window
        {
            Width = 600,
            Height = 260,
            Content = new StackPanel { Children = { ribbon, toolbar } }
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.True(toolbar.AddItem(button));
            Assert.True(toolbar.AddItem(toggle));
            Dispatcher.UIThread.RunJobs();

            var visuals = toolbar.GetVisualDescendants().ToArray();
            Assert.Contains(buttonIcon, visuals);
            Assert.Contains(toggleIcon, visuals);
            Assert.NotNull(toolbar.Foreground);
            Assert.Contains(toolbar.GetVisualDescendants().OfType<TemplatedControl>(),
                control => ReferenceEquals(control.DataContext, button) && Equals(control.Foreground, toolbar.Foreground));
            Assert.Contains(toolbar.GetVisualDescendants().OfType<Border>(),
                border => border.Width == 1 &&
                          border.BorderBrush?.ToString() == toolbar.Foreground?.ToString());

            var moreButton = toolbar.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "PART_MoreButton");
            var pointerPoint = moreButton.TranslatePoint(
                new Point(moreButton.Bounds.Width / 2, moreButton.Bounds.Height / 2),
                window);
            Assert.NotNull(pointerPoint);
            window.MouseMove(pointerPoint.Value);
            Dispatcher.UIThread.RunJobs();
            Assert.True(moreButton.IsPointerOver);
            Assert.Contains(moreButton.GetVisualDescendants().OfType<ContentPresenter>(),
                presenter => presenter.Name == "PART_ContentPresenter" && presenter.Background is not null);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void QuickAccessToolbar_HeightComesFromOverridableResource()
    {
        EnsureStyles();
        var toolbar = new QuickAccessToolbar();
        toolbar.Resources["QuickAccessToolbarHeight"] = 42d;
        var window = new Window { Width = 300, Height = 100, Content = toolbar };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(42d, toolbar.Height);
        }
        finally
        {
            window.Close();
        }
    }

    private static T CreateRibbon<T>(T ribbon)
        where T : Ribbon
    {
        var tab = new RibbonTab { Header = "Tab" };
        tab.Groups.Add(new RibbonGroupBox { Header = "Group" });
        ribbon.Tabs = new ObservableCollection<Control> { tab };
        ribbon.SelectedIndex = 0;
        return ribbon;
    }

    private static void EnsureStyles()
    {
        var application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia test application is not initialized.");
        if (ReferenceEquals(_styledApplication, application))
            return;

        application.Styles.Add(new FluentTheme());
        application.Styles.Add(new StyleInclude(new Uri("avares://AvaloniaUI.Ribbon.Desktop/"))
        {
            Source = new Uri("avares://AvaloniaUI.Ribbon.Desktop/Styles/Fluent/AvaloniaRibbon.axaml")
        });
        _styledApplication = application;
    }
}
