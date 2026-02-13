using Avalonia.Media;

namespace AvaloniaUI.Ribbon.Tests;

public class ContextualTabGroupTests
{
    [Fact]
    public void ContextColor_SyncsToBackground()
    {
        var group = new RibbonContextualTabGroup();
        var brush = new SolidColorBrush(Colors.Red);

        group.ContextColor = brush;

        Assert.Same(brush, group.Background);
    }

    [Fact]
    public void Background_SyncsToContextColor()
    {
        var group = new RibbonContextualTabGroup();
        var brush = new SolidColorBrush(Colors.Green);

        group.Background = brush;

        Assert.Same(brush, group.ContextColor);
    }
}
