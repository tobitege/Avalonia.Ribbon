namespace AvaloniaUI.Ribbon.Tests;

public class RibbonEventRouterTests
{
    [Fact]
    public void SplitButtonClick_OpensDropDownOnlyWhenRibbonIsExpanded()
    {
        var ribbon = new Ribbon();
        var tab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var group = RibbonBuilder.InsertOrAddGroup(tab, "Edit", "Edit");
        var split = new RibbonSplitButton { Name = "Split" };
        group.Items.Add(split);
        using var router = new RibbonEventRouter(ribbon);

        router.Route(new RibbonEventArgs(split, RibbonEventType.Click));
        Assert.True(split.DroppedDown);

        split.DroppedDown = false;
        ribbon.Minimized = true;
        router.Route(new RibbonEventArgs(split, RibbonEventType.Click));
        Assert.False(split.DroppedDown);
    }

    [Fact]
    public void Router_DispatchesByStableNameAndIgnoresRemovedItems()
    {
        var ribbon = new Ribbon();
        var tab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var group = RibbonBuilder.InsertOrAddGroup(tab, "Edit", "Edit");
        var button = RibbonBuilder.InsertOrAddButton(group, "Run", "Run", null, null, null);
        using var router = new RibbonEventRouter(ribbon);
        var invocationCount = 0;
        router.Register("Run", _ => invocationCount++);

        router.Route(new RibbonEventArgs(button, RibbonEventType.Click));
        group.Items.Remove(button);
        router.Route(new RibbonEventArgs(button, RibbonEventType.Click));

        Assert.Equal(1, invocationCount);
    }
}
