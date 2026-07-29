using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonButtonLayoutTests
{
    private static Application? _styledApplication;

    [Fact]
    public void LargeButtons_WithAutoHeight_AlignIconsAtTop()
    {
        var context = CreateRibbon();

        try
        {
            context.Window.Show();
            Dispatcher.UIThread.RunJobs();

            var firstTop = context.FirstIcon.TranslatePoint(new Point(0, 0), context.Ribbon);
            var secondTop = context.SecondIcon.TranslatePoint(new Point(0, 0), context.Ribbon);
            var firstButtonTop = context.FirstButton.TranslatePoint(new Point(0, 0), context.Ribbon);
            var secondButtonTop = context.SecondButton.TranslatePoint(new Point(0, 0), context.Ribbon);
            var firstIconTopInButton = context.FirstIcon.TranslatePoint(new Point(0, 0), context.FirstButton);
            var secondIconTopInButton = context.SecondIcon.TranslatePoint(new Point(0, 0), context.SecondButton);

            Assert.NotNull(firstTop);
            Assert.NotNull(secondTop);
            Assert.NotNull(firstButtonTop);
            Assert.NotNull(secondButtonTop);
            Assert.NotNull(firstIconTopInButton);
            Assert.NotNull(secondIconTopInButton);

            var difference = Math.Abs(firstTop.Value.Y - secondTop.Value.Y);
            Assert.True(
                difference <= 1,
                $"Icon difference: {difference}; " +
                $"button tops: {firstButtonTop.Value.Y}/{secondButtonTop.Value.Y}; " +
                $"button heights: {context.FirstButton.Bounds.Height}/{context.SecondButton.Bounds.Height}; " +
                $"icon tops in buttons: {firstIconTopInButton.Value.Y}/{secondIconTopInButton.Value.Y}");
        }
        finally
        {
            context.Window.Close();
        }
    }

    [Fact]
    public void LargeButton_WithAutoHeight_RespondsAcrossLowerFreeArea()
    {
        var context = CreateRibbon();
        var clickCount = 0;
        context.FirstButton.Click += (_, _) => clickCount++;

        try
        {
            context.Window.Show();
            Dispatcher.UIThread.RunJobs();

            var point = context.FirstButton.TranslatePoint(
                new Point(2, context.FirstButton.Bounds.Height - 2),
                context.Window);

            Assert.NotNull(point);

            context.Window.MouseMove(point.Value);
            Assert.True(context.FirstButton.IsPointerOver);

            context.Window.MouseDown(point.Value, MouseButton.Left);
            context.Window.MouseUp(point.Value, MouseButton.Left);
            Assert.Equal(1, clickCount);
        }
        finally
        {
            context.Window.Close();
        }
    }

    private static TestContext CreateRibbon()
    {
        EnsureStyles();

        var firstIcon = CreateIcon();
        var secondIcon = CreateIcon();
        var firstButton = CreateButton("Neu", firstIcon);
        var secondButton = CreateButton("Offene\nPosten", secondIcon);

        var firstGroup = new RibbonGroupBox { Header = "A" };
        firstGroup.Items.Add(firstButton);

        var secondGroup = new RibbonGroupBox { Header = "B" };
        secondGroup.Items.Add(secondButton);

        var tab = new RibbonTab { Header = "T" };
        tab.Groups.Add(firstGroup);
        tab.Groups.Add(secondGroup);

        var ribbon = new Ribbon
        {
            Tabs = new ObservableCollection<Control> { tab },
            SelectedIndex = 0
        };

        var window = new Window
        {
            Width = 600,
            Height = 220,
            Content = ribbon
        };

        return new TestContext(window, ribbon, firstButton, secondButton, firstIcon, secondIcon);
    }

    private static RibbonButton CreateButton(string text, PathIcon icon)
    {
        return new RibbonButton
        {
            Size = RibbonControlSize.Large,
            MinSize = RibbonControlSize.Large,
            MaxSize = RibbonControlSize.Large,
            Height = double.NaN,
            MaxWidth = double.PositiveInfinity,
            Content = new TextBlock
            {
                Text = text,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            },
            LargeIcon = icon
        };
    }

    private static PathIcon CreateIcon()
    {
        return new PathIcon
        {
            Data = Geometry.Parse("M0,0 L16,0 L16,16 Z"),
            Width = 18,
            Height = 18
        };
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

    private sealed record TestContext(
        Window Window,
        Ribbon Ribbon,
        RibbonButton FirstButton,
        RibbonButton SecondButton,
        PathIcon FirstIcon,
        PathIcon SecondIcon);
}
