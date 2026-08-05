using System.Linq;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonFeatureFoundationTests
{
    [Fact]
    public void SettingsStore_DistinguishesMissingFromPresentEmptyValue()
    {
        var store = new InMemoryRibbonSettingsStore();
        var address = new RibbonSettingsAddress("tenant", "RibbonBar", "user", "section", "key");

        Assert.Null(store.Read(address));

        store.Write(address, string.Empty);

        Assert.Equal(string.Empty, store.Read(address));
    }

    [Fact]
    public void Builder_IsIdempotent_AndAppendsForMissingOrLastAnchor()
    {
        var ribbon = new Ribbon();
        ribbon.BeginUpdate();
        var home = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var missingAnchor = RibbonBuilder.InsertOrAddTab(ribbon, "Window", "Window", "Missing");
        var lastAnchor = RibbonBuilder.InsertOrAddTab(ribbon, "Help", "Help", "Window");
        var duplicate = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Updated home");
        ribbon.EndUpdate();

        Assert.Same(home, duplicate);
        Assert.Equal("Updated home", home.Header);
        Assert.Equal(new Control[] { home, missingAnchor, lastAnchor }, ribbon.Tabs);
        Assert.True(ribbon.Tabs.Contains("Home"));
    }

    [Fact]
    public void Lookup_FindsTreeItems_ChangesOnlyRequestedState_AndMovesWithinOwner()
    {
        var ribbon = new Ribbon();
        var tab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var group = RibbonBuilder.InsertOrAddGroup(tab, "Edit", "Edit");
        var first = RibbonBuilder.InsertOrAddButton(group, "First", "First", null, null, null);
        var second = RibbonBuilder.InsertOrAddButton(group, "Second", "Second", null, null, null);
        var third = RibbonBuilder.InsertOrAddButton(group, "Third", "Third", null, null, null);
        var lookup = new RibbonItemLookup(ribbon);

        Assert.Same(tab, lookup.FindTab("Home"));
        Assert.Same(group, RibbonItemLookup.FindGroup(tab, "Edit"));
        Assert.Same(second, RibbonItemLookup.FindItem<RibbonButton>(group, "Second"));
        Assert.True(lookup.SetItemState("Second", visible: false, enabled: null));
        Assert.False(second.IsVisible);
        Assert.True(second.IsEnabled);
        Assert.True(lookup.SetItemPosition("Third", "First", RibbonItemPosition.Before));
        Assert.Equal(new[] { third, first, second }, group.Items.OfType<RibbonButton>());
        Assert.False(lookup.SetItemPosition("Missing", "First", RibbonItemPosition.After));
    }

    [Fact]
    public void Lookup_FindsIdOnlyItemWithNonNameIdentifier()
    {
        const string itemId = "462C5AF2-165A-450E-BB06-23FA02923D3C";
        var ribbon = new Ribbon();
        var tab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var group = RibbonBuilder.InsertOrAddGroup(tab, "Edit", "Edit");
        var item = new RibbonButton();
        RibbonItem.SetId(item, itemId);
        group.Items.Add(item);

        Assert.Null(item.Name);
        Assert.Same(item, ribbon.GetItemByName(itemId));
    }

    [Fact]
    public void Ribbon_ExposesRequiredStateThroughSingleCanonicalValues()
    {
        var ribbon = new Ribbon();

        ribbon.Qat.BelowRibbon = true;
        ribbon.Minimized = true;
        ribbon.SelectedTabIndex = 0;

        Assert.True(ribbon.Qat.BelowRibbon);
        Assert.True(ribbon.IsCollapsed);
        Assert.Equal(0, ribbon.SelectedIndex);
        Assert.Same(ribbon.QuickAccessItems, ribbon.Qat.Items);
    }
}
