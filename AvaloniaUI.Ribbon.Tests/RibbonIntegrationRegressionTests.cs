using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaUI.Ribbon.Desktop;
using AvaloniaUI.Ribbon.Models;
using ShapePath = Avalonia.Controls.Shapes.Path;

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
    public void DesktopRibbonOverflow_RendersAndOpensSharedPopupInRibbonWindow()
    {
        EnsureStyles();

        var groups = Enumerable.Range(1, 4)
            .Select(index =>
            {
                var group = new RibbonGroupBox
                {
                    Header = $"Group {index}",
                    Width = 180,
                    AllowCollapsedPopup = true
                };
                group.Items.Add(new RibbonButton { Content = $"Action {index}" });
                return group;
            })
            .ToArray();
        var tab = new RibbonTab { Header = "Home" };
        foreach (var group in groups)
            tab.Groups.Add(group);

        var ribbon = new OverflowTestDesktopRibbon
        {
            GroupOverflowBehavior = RibbonGroupOverflowBehavior.WrapThenShrink,
            MaxGroupRows = 1,
            SelectedIndex = 0,
            Tabs = new ObservableCollection<Control> { tab }
        };
        var overflowAccent = new SolidColorBrush(Color.FromRgb(17, 31, 47));
        ribbon.Resources["ThemeAccentBrush2"] = overflowAccent;
        var outsideButton = new Button
        {
            Width = 60,
            Height = 30,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        var outsideClicks = 0;
        outsideButton.Click += (_, _) => outsideClicks++;
        var window = new RibbonWindow
        {
            Width = 320,
            Height = 300,
            Ribbon = ribbon,
            Content = outsideButton
        };

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
            var overflowButton = ribbon.GroupOverflowButton;
            var overflowPopup = ribbon.GroupOverflowPopup;
            var overflowHost = ribbon.GroupOverflowHost;
            Assert.NotNull(overflowButton);
            Assert.NotNull(overflowPopup);
            Assert.NotNull(overflowHost);
            Assert.True(overflowButton.Bounds.Width > 0);
            Assert.True(overflowPopup.IsLightDismissEnabled);
            Assert.True(overflowPopup.OverlayDismissEventPassThrough);
            var overflowGrid = Assert.IsType<Grid>(overflowHost.Parent);
            Assert.Equal(overflowGrid.Bounds.Height, overflowHost.Bounds.Height);
            Assert.Equal(overflowGrid.Bounds.Width, panel.Bounds.Width);

            var buttonPresenter = overflowButton.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "PART_ContentPresenter");
            overflowButton.IsChecked = true;
            Dispatcher.UIThread.RunJobs();

            Assert.True(overflowPopup.IsOpen);
            Assert.True(overflowButton.IsChecked);
            Assert.Equal(overflowAccent.Color, Assert.IsAssignableFrom<ISolidColorBrush>(buttonPresenter.Background).Color);
            Assert.NotNull(overflowPopup.Child);
            var overflowItemsControl = ribbon.GroupOverflowGroups;
            Assert.NotNull(overflowItemsControl);
            Assert.Equal(overflowGroups.Length, overflowItemsControl.ItemCount);

            var outsidePoint = outsideButton.TranslatePoint(
                new Point(outsideButton.Bounds.Width / 2, outsideButton.Bounds.Height / 2),
                window);
            Assert.NotNull(outsidePoint);
            window.MouseMove(outsidePoint.Value);
            window.MouseDown(outsidePoint.Value, MouseButton.Left);
            window.MouseUp(outsidePoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, outsideClicks);
            Assert.False(overflowPopup.IsOpen);
            Assert.False(ribbon.IsGroupOverflowOpen);
            Assert.False(overflowButton.IsChecked);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_ReopensAgainstCurrentWindowAfterWindowMoves()
    {
        EnsureStyles();
        var menu = new RibbonMenu();
        var ribbon = CreateRibbon(new Ribbon { Menu = menu });
        var window = new Window
        {
            Position = new PixelPoint(80, 90),
            Width = 700,
            Height = 320,
            Content = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();
            var contentButton = menu.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "ContentButton");

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            Assert.False(popup.ShouldUseOverlayLayer);
            Assert.Same(contentButton, popup.PlacementTarget);
            Assert.Same(window, TopLevel.GetTopLevel(popup.Child));

            menu.IsMenuOpen = false;
            Dispatcher.UIThread.RunJobs();
            var movedPosition = new PixelPoint(420, 310);
            window.Position = movedPosition;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(movedPosition, window.Position);

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            Assert.False(popup.ShouldUseOverlayLayer);
            Assert.Same(contentButton, popup.PlacementTarget);
            Assert.Same(window, TopLevel.GetTopLevel(popup.Child));
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_BackButtonUsesOpenMenuForegroundAndInsetGlyph()
    {
        EnsureStyles();
        var expectedForeground = Brushes.Magenta;
        var menu = new RibbonMenu { Foreground = expectedForeground };
        var ribbon = CreateRibbon(new Ribbon { Menu = menu });
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            var menuRoot = Assert.IsType<Border>(popup.Child);
            var backButton = menuRoot.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "BackButton");
            var circle = backButton.GetVisualDescendants().OfType<Ellipse>().Single();
            var line = backButton.GetVisualDescendants().OfType<Rectangle>().Single();
            var arrow = backButton.GetVisualDescendants().OfType<ShapePath>().Single();
            var glyph = Assert.IsType<Panel>(arrow.Parent);

            Assert.Same(expectedForeground, circle.Stroke);
            Assert.Same(expectedForeground, line.Fill);
            Assert.Same(expectedForeground, arrow.Stroke);
            Assert.Equal(new Thickness(2, 0, 0, 0), glyph.Margin);
            Assert.Equal(Brushes.Transparent, backButton.Background);
            Assert.Equal(Brushes.Transparent, backButton.BorderBrush);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_HeaderPresentersOnlyReserveNonNullContent()
    {
        EnsureStyles();
        var menu = new RibbonMenu
        {
            Content = null,
            SmallImage = new Border { Width = 16, Height = 16 }
        };
        var ribbon = CreateRibbon(new Ribbon { Menu = menu });
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var smallImagePresenter = menu.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "SmallImagePresenter");
            var menuContentPresenter = menu.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "MenuContentPresenter");

            Assert.True(smallImagePresenter.IsVisible);
            Assert.False(menuContentPresenter.IsVisible);

            menu.Content = "File";
            Dispatcher.UIThread.RunJobs();

            Assert.True(smallImagePresenter.IsVisible);
            Assert.True(menuContentPresenter.IsVisible);

            menu.SmallImage = null;
            Dispatcher.UIThread.RunJobs();

            Assert.False(smallImagePresenter.IsVisible);
            Assert.True(menuContentPresenter.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_FontSizeFlowsToAllMenuTextSources()
    {
        EnsureStyles();
        const double expectedFontSize = 19;
        var menu = new RibbonMenu { FontSize = expectedFontSize };
        menu.Items.Add(new RibbonMenuItem { Header = "Open" });
        menu.RecentDocuments.Add(new RibbonRecentDocument { Title = "Document" });
        var ribbon = CreateRibbon(new Ribbon { Menu = menu });
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();
            var contentButton = menu.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "ContentButton");

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            var menuRoot = Assert.IsType<Border>(popup.Child);
            var menuItemButton = menuRoot.GetVisualDescendants()
                .OfType<Button>()
                .Single(control => control.Name == "PART_ContentButton");
            var recentSection = menuRoot.GetVisualDescendants()
                .OfType<ItemsControl>()
                .Single(control => control.Name == "RecentDocumentsSection");
            var recentButton = recentSection.GetVisualDescendants().OfType<Button>().Single();
            var recentHeading = recentSection.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(control => control.Text == "Recent");

            Assert.Equal(expectedFontSize, contentButton.FontSize);
            Assert.Equal(expectedFontSize, menuItemButton.FontSize);
            Assert.Equal(expectedFontSize, recentButton.FontSize);
            Assert.Equal(expectedFontSize, recentHeading.FontSize);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenuItem_TemplateButtonExecutesCommand()
    {
        EnsureStyles();
        var executionCount = 0;
        var command = new CallbackCommand(() => executionCount++);
        var menuItem = new RibbonMenuItem
        {
            Header = "Exit",
            Command = command
        };
        var window = new Window
        {
            Width = 300,
            Height = 120,
            Content = menuItem
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var contentButton = menuItem.GetVisualDescendants()
                .OfType<Button>()
                .Single(control => control.Name == "PART_ContentButton");
            var peer = ControlAutomationPeer.CreatePeerForElement(contentButton);
            var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetProvider<IInvokeProvider>());

            Assert.Same(command, contentButton.Command);
            invokeProvider.Invoke();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, executionCount);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_RecentDocumentsSectionTracksCollectionState()
    {
        EnsureStyles();
        var menu = new RibbonMenu();
        var ribbon = CreateRibbon(new Ribbon { Menu = menu });
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = ribbon
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            var menuRoot = Assert.IsType<Border>(popup.Child);
            var section = menuRoot.GetVisualDescendants()
                .OfType<ItemsControl>()
                .Single(control => control.Name == "RecentDocumentsSection");

            Assert.Empty(menu.RecentDocuments);
            Assert.False(section.IsVisible);

            menu.RecentDocuments.Add(new RibbonRecentDocument { Title = "Document" });
            Dispatcher.UIThread.RunJobs();

            Assert.True(section.IsVisible);
            var heading = section.GetVisualDescendants()
                .OfType<TextBlock>()
                .Single(control => control.Text == "Recent");
            var separator = section.GetVisualDescendants().OfType<Separator>().Single();
            var headingPosition = heading.TranslatePoint(default, section);
            var separatorPosition = separator.TranslatePoint(default, section);

            Assert.Equal(FontWeight.Normal, heading.FontWeight);
            Assert.NotNull(headingPosition);
            Assert.NotNull(separatorPosition);
            Assert.True(separatorPosition.Value.Y < headingPosition.Value.Y);

            menu.RecentDocuments.Clear();
            Dispatcher.UIThread.RunJobs();

            Assert.False(section.IsVisible);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_ToggleOpensWithBottomDockedItemAddedAfterInitialEmptyTemplate()
    {
        EnsureStyles();
        var menu = new RibbonMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        Assert.Equal(RibbonMenuDisplayMode.FullClient, menu.DisplayMode);
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Empty(menu.BottomDockedGroupedItems);
            var exitItem = new RibbonMenuItem
            {
                Header = "Exit",
                IsBottomDocked = true
            };
            menu.Items.Add(exitItem);
            Dispatcher.UIThread.RunJobs();

            Assert.Contains(exitItem, menu.BottomDockedGroupedItems.SelectMany(group => group));
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();
            var contentButton = menu.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "ContentButton");
            var clickCount = 0;
            contentButton.Click += (_, _) => clickCount++;
            var peer = ControlAutomationPeer.CreatePeerForElement(contentButton);
            var toggleProvider = Assert.IsAssignableFrom<IToggleProvider>(peer.GetProvider<IToggleProvider>());

            toggleProvider.Toggle();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, clickCount);
            Assert.True(menu.IsMenuOpen);
            Assert.True(popup.IsOpen);
            Assert.False(popup.ShouldUseOverlayLayer);
            var menuRoot = Assert.IsType<Border>(popup.Child);
            var contentButtonTop = contentButton.TranslatePoint(default, window)?.Y;
            Assert.NotNull(contentButtonTop);
            Assert.Equal(window.ClientSize.Width, popup.Width);
            Assert.Equal(window.ClientSize.Height - contentButtonTop.Value, popup.Height);
            Assert.Equal(popup.Width, menuRoot.Width);
            Assert.Equal(popup.Height, menuRoot.Height);
            Assert.Contains(exitItem, menuRoot.GetVisualDescendants().OfType<RibbonMenuItem>());
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void RibbonMenu_SwitchToCompactDisplayModeUsesContentSizedPopup()
    {
        EnsureStyles();
        const double hiddenContentWidth = 320;
        var openItem = new RibbonMenuItem { Header = "Open" };
        var exitItem = new RibbonMenuItem
        {
            Header = "Exit",
            IsBottomDocked = true
        };
        var menu = new RibbonMenu
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            LargeImage = new Border { Width = hiddenContentWidth, Height = 80 },
            SelectedItemContent = new Border { Width = hiddenContentWidth, Height = 160 }
        };
        menu.Items.Add(openItem);
        menu.Items.Add(exitItem);
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();
            var contentButton = menu.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "ContentButton");

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(window.ClientSize.Width, popup.Width);

            menu.DisplayMode = RibbonMenuDisplayMode.Compact;
            Dispatcher.UIThread.RunJobs();

            var menuRoot = Assert.IsType<Border>(popup.Child);
            var backButton = menuRoot.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "BackButton");
            var largeImagePresenter = menuRoot.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "LargeImagePresenter");
            var selectedContentPresenter = menuRoot.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .Single(control => control.Name == "SelectedContentPresenter");

            Assert.True(popup.IsOpen);
            Assert.Equal(RibbonMenuDisplayMode.Compact, menu.DisplayMode);
            Assert.Same(contentButton, popup.PlacementTarget);
            Assert.Equal(PlacementMode.BottomEdgeAlignedLeft, popup.Placement);
            Assert.Equal(0, popup.HorizontalOffset);
            Assert.Equal(0, popup.VerticalOffset);
            Assert.True(double.IsNaN(popup.Width));
            Assert.True(double.IsNaN(popup.Height));
            Assert.True(double.IsNaN(menuRoot.Width));
            Assert.True(double.IsNaN(menuRoot.Height));
            Assert.False(backButton.IsVisible);
            Assert.False(largeImagePresenter.IsVisible);
            Assert.False(selectedContentPresenter.IsVisible);
            Assert.True(menuRoot.Bounds.Width >= 200);
            Assert.True(menuRoot.Bounds.Width < hiddenContentWidth);
            Assert.True(menuRoot.Bounds.Height < window.ClientSize.Height);
            Assert.Contains(openItem, menuRoot.GetVisualDescendants().OfType<RibbonMenuItem>());
            Assert.Contains(exitItem, menuRoot.GetVisualDescendants().OfType<RibbonMenuItem>());
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(RibbonMenuDisplayMode.FullClient)]
    [InlineData(RibbonMenuDisplayMode.Compact)]
    public void RibbonMenu_EscapeClosesFromButtonAndPopupContent(RibbonMenuDisplayMode displayMode)
    {
        EnsureStyles();
        var menu = new RibbonMenu
        {
            DisplayMode = displayMode,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        menu.Items.Add(new RibbonMenuItem { Header = "Open" });
        var window = new Window
        {
            Width = 700,
            Height = 320,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var popup = menu.GetVisualDescendants().OfType<Popup>().Single();
            var contentButton = menu.GetVisualDescendants()
                .OfType<ToggleButton>()
                .Single(control => control.Name == "ContentButton");

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            contentButton.Focus();
            Dispatcher.UIThread.RunJobs();

            Assert.True(contentButton.IsFocused);
            Assert.True(popup.IsOpen);
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(menu.IsMenuOpen);
            Assert.False(popup.IsOpen);
            Assert.False(contentButton.IsChecked);

            menu.IsMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            var menuItemButton = Assert.IsType<Border>(popup.Child)
                .GetVisualDescendants()
                .OfType<Button>()
                .Single(control => control.Name == "PART_ContentButton");
            menuItemButton.Focus();
            Dispatcher.UIThread.RunJobs();

            Assert.True(menuItemButton.IsFocused);
            Assert.True(popup.IsOpen);
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            window.KeyRelease(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, null);
            Dispatcher.UIThread.RunJobs();

            Assert.False(menu.IsMenuOpen);
            Assert.False(popup.IsOpen);
            Assert.False(contentButton.IsChecked);
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
        toolbar.Resources["QuickAccessButtonMinWidth"] = 32d;
        toolbar.Resources["QuickAccessButtonHeight"] = 28d;
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
            var quickAccessButtons = toolbar.GetVisualDescendants()
                .OfType<Button>()
                .Where(control => control.Classes.Contains("quickAccessButton"))
                .ToArray();
            Assert.Equal(2, quickAccessButtons.Length);
            Assert.All(quickAccessButtons, control =>
            {
                Assert.Equal(32d, control.MinWidth);
                Assert.Equal(28d, control.Height);
                Assert.Equal(new Thickness(0), control.Padding);
                Assert.Equal(new Thickness(0), control.BorderThickness);
                Assert.Equal(Brushes.Transparent, control.Background);
                Assert.Equal(Brushes.Transparent, control.BorderBrush);
            });

            var quickAccessItems = toolbar.GetVisualDescendants().OfType<QuickAccessItem>().ToArray();
            var buttonSeparator = quickAccessItems.Single(item => ReferenceEquals(item.Item, button))
                .GetVisualDescendants()
                .OfType<Border>()
                .Single(control => control.Name == "PART_Separator");
            var toggleSeparator = quickAccessItems.Single(item => ReferenceEquals(item.Item, toggle))
                .GetVisualDescendants()
                .OfType<Border>()
                .Single(control => control.Name == "PART_Separator");
            Assert.True(buttonSeparator.IsVisible);
            Assert.False(toggleSeparator.IsVisible);

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

    private sealed class OverflowTestDesktopRibbon : DesktopRibbon
    {
        public ToggleButton? GroupOverflowButton { get; private set; }

        public Popup? GroupOverflowPopup { get; private set; }

        public Border? GroupOverflowHost { get; private set; }

        public ItemsControl? GroupOverflowGroups { get; private set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            GroupOverflowButton = e.NameScope.Find<ToggleButton>("PART_GroupOverflowButton");
            GroupOverflowPopup = e.NameScope.Find<Popup>("PART_GroupOverflowPopup");
            GroupOverflowHost = e.NameScope.Find<Border>("PART_GroupOverflowHost");
            GroupOverflowGroups = e.NameScope.Find<ItemsControl>("PART_GroupOverflowGroups");
        }
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

    private sealed class CallbackCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
