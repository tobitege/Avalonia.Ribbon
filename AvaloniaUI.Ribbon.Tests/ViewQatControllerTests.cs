using System;
using System.Linq;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon.Tests;

public class ViewQatControllerTests
{
    [Fact]
    public void SwitchingViews_SavesOldLoadsNewAndPreservesApplicationItems()
    {
        var store = new InMemoryRibbonSettingsStore();
        var fixture = CreateFixture(store, new RibbonButton { Name = "FunctionA" }, new RibbonButton { Name = "FunctionB" });
        fixture.Ribbon.Qat.Items.Add(fixture.ApplicationItem);
        fixture.Ribbon.Qat.Items.Add(fixture.ViewItems[1]);
        fixture.Controller.Save("ViewB");

        fixture.Ribbon.Qat.Items.Clear();
        fixture.Ribbon.Qat.Items.Add(fixture.ApplicationItem);
        fixture.Ribbon.Qat.Items.Add(fixture.ViewItems[0]);
        fixture.Controller.Save("ViewA");
        fixture.Ribbon.Qat.Items.Remove(fixture.ViewItems[0]);
        fixture.Controller.Load("ViewB");

        Assert.Equal(
            new ICanAddToQuickAccess[] { fixture.ApplicationItem, fixture.ViewItems[1] },
            fixture.Ribbon.Qat.Items);
        Assert.Equal("FunctionA", store.Read(Address("ViewA")));
    }

    [Fact]
    public void RebuildRoundTrip_RestoresOrderWithCurrentInstancesOnly()
    {
        var store = new InMemoryRibbonSettingsStore();
        var oldItem = new RibbonButton { Name = "Function" };
        var fixture = CreateFixture(store, oldItem);
        fixture.Ribbon.Qat.Items.Add(fixture.ApplicationItem);
        fixture.Ribbon.Qat.Items.Add(oldItem);

        fixture.Controller.Stash();
        fixture.Controller.Save("View");
        fixture.Ribbon.Qat.Items.Clear();
        fixture.ViewGroup.Items.Clear();
        var rebuiltItem = new RibbonButton { Name = "Function" };
        fixture.ViewGroup.Items.Add(rebuiltItem);
        fixture.Controller.Load("View");
        fixture.Controller.Restore();

        Assert.Equal(
            new ICanAddToQuickAccess[] { fixture.ApplicationItem, rebuiltItem },
            fixture.Ribbon.Qat.Items);
        Assert.DoesNotContain(oldItem, fixture.Ribbon.Qat.Items);
    }

    [Fact]
    public void Load_RemovesAndDisposesStaleInstanceWithoutTouchingSharedImage()
    {
        var store = new InMemoryRibbonSettingsStore();
        var sharedImage = new object();
        var stale = new DisposableRibbonButton { Name = "Function", QuickAccessIcon = sharedImage };
        var fixture = CreateFixture(store, stale);
        fixture.Ribbon.Qat.Items.Add(fixture.ApplicationItem);
        fixture.Ribbon.Qat.Items.Add(stale);
        fixture.ViewGroup.Items.Clear();
        var current = new RibbonButton { Name = "Function", QuickAccessIcon = sharedImage };
        fixture.ViewGroup.Items.Add(current);

        fixture.Controller.Load("View");

        Assert.True(stale.IsDisposed);
        Assert.DoesNotContain(stale, fixture.Ribbon.Qat.Items);
        Assert.Contains(fixture.ApplicationItem, fixture.Ribbon.Qat.Items);
        Assert.Same(sharedImage, current.QuickAccessIcon);
    }

    [Fact]
    public void DeleteView_DeletesItsQuickStartRecord()
    {
        var store = new InMemoryRibbonSettingsStore();
        var fixture = CreateFixture(store, new RibbonButton { Name = "Function" });
        fixture.Ribbon.Qat.Items.Add(fixture.ViewItems[0]);
        fixture.Controller.Save("View");

        fixture.Controller.Delete("View");

        Assert.Null(store.Read(Address("View")));
    }

    [Fact]
    public void QuickStartDefinition_AppendsGeneratedItemOnce()
    {
        var store = new InMemoryRibbonSettingsStore();
        var item = new RibbonButton();
        var fixture = CreateFixture(store, item);
        var definition = new RibbonItemDefinition("Function", "Function", QuickStart: true);

        fixture.Controller.AddGeneratedItem(item, definition);
        fixture.Controller.AddGeneratedItem(item, definition);

        Assert.Equal("Function", item.Name);
        Assert.Single(fixture.Ribbon.Qat.Items, candidate => ReferenceEquals(candidate, item));
    }

    private static Fixture CreateFixture(InMemoryRibbonSettingsStore store, params RibbonButton[] viewItems)
    {
        var ribbon = new Ribbon();
        var applicationTab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var applicationGroup = RibbonBuilder.InsertOrAddGroup(applicationTab, "Edit", "Edit");
        var applicationItem = RibbonBuilder.InsertOrAddButton(
            applicationGroup,
            "New",
            "New",
            null,
            null,
            null);
        var viewTab = RibbonBuilder.InsertOrAddTab(ribbon, "UserFunctions", "Functions", "Home");
        var viewGroup = RibbonBuilder.InsertOrAddGroup(viewTab, "Generated", "Generated");
        foreach (var item in viewItems)
            viewGroup.Items.Add(item);

        var ownership = new RibbonQatOwnership(() => viewTab);
        var controller = new ViewQatController(
            ribbon,
            store,
            static () => "tenant",
            static () => "user",
            "form",
            ownership);
        return new Fixture(ribbon, viewGroup, applicationItem, viewItems, controller);
    }

    private static RibbonSettingsAddress Address(string viewName)
    {
        return new RibbonSettingsAddress("tenant", "RibbonBar", "user", "QatFunctions", $"form:{viewName}");
    }

    private sealed record Fixture(
        Ribbon Ribbon,
        RibbonGroupBox ViewGroup,
        RibbonButton ApplicationItem,
        RibbonButton[] ViewItems,
        ViewQatController Controller);

    private sealed class DisposableRibbonButton : RibbonButton, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
