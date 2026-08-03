using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Tests;

public class AppQatControllerTests
{
    [Fact]
    public void FirstLoadWithoutRecord_SeedsNewRefreshSaveInOrder()
    {
        var store = new InMemoryRibbonSettingsStore();
        var fixture = CreateFixture(store);

        fixture.Controller.Load();

        Assert.Equal(new[] { "New", "Refresh", "Save" }, GetNames(fixture.Ribbon));
    }

    [Fact]
    public void UserOrder_RoundTripsAcrossControllerAndRibbonInstances()
    {
        var store = new InMemoryRibbonSettingsStore();
        var first = CreateFixture(store, "Export");
        first.Controller.Load();
        var export = (ICanAddToQuickAccess)first.Ribbon.GetItemByName("Export")!;
        first.Ribbon.Qat.Items.Insert(1, export);
        first.Controller.Save();

        var restarted = CreateFixture(store, "Export");
        restarted.Controller.Load();

        Assert.Equal(new[] { "New", "Export", "Refresh", "Save" }, GetNames(restarted.Ribbon));
    }

    [Fact]
    public void ClearedQat_RemainsClearedAndDoesNotReseed()
    {
        var store = new InMemoryRibbonSettingsStore();
        var first = CreateFixture(store);
        first.Controller.Load();
        first.Ribbon.Qat.Items.Clear();
        first.Controller.Save();

        var restarted = CreateFixture(store);
        restarted.Controller.Load();

        Assert.Empty(restarted.Ribbon.Qat.Items);
        Assert.Equal(" ", store.Read(Address("Qat")));
    }

    [Fact]
    public void BelowRibbonAndMinimized_RoundTripTogether()
    {
        var store = new InMemoryRibbonSettingsStore();
        var first = CreateFixture(store);
        first.Ribbon.Qat.BelowRibbon = true;
        first.Ribbon.Minimized = true;
        first.Controller.Save();

        var restarted = CreateFixture(store);
        restarted.Controller.Load();

        Assert.True(restarted.Ribbon.Qat.BelowRibbon);
        Assert.True(restarted.Ribbon.Minimized);
    }

    [Fact]
    public void MalformedStateRecord_IsIgnoredAsAWhole()
    {
        var store = new InMemoryRibbonSettingsStore();
        store.Write(Address("State"), "1;1;unexpected");
        var fixture = CreateFixture(store);

        fixture.Controller.Load();

        Assert.False(fixture.Ribbon.Qat.BelowRibbon);
        Assert.False(fixture.Ribbon.Minimized);
    }

    [Fact]
    public void MissingSignedInUser_LeavesRibbonAndStoreUntouched()
    {
        var store = new InMemoryRibbonSettingsStore();
        var fixture = CreateFixture(store, user: string.Empty);

        fixture.Controller.Load();
        fixture.Controller.Save();

        Assert.Empty(fixture.Ribbon.Qat.Items);
        Assert.Null(store.Read(new RibbonSettingsAddress("tenant", "RibbonBar", string.Empty, "Qat", "app")));
    }

    [Fact]
    public void MissingPersistedItem_IsDroppedWithoutFailure()
    {
        var store = new InMemoryRibbonSettingsStore();
        var first = CreateFixture(store, "RemovedLater");
        first.Ribbon.Qat.Items.Add((ICanAddToQuickAccess)first.Ribbon.GetItemByName("RemovedLater")!);
        first.Controller.Save();

        var restarted = CreateFixture(store);
        restarted.Controller.Load();

        Assert.Empty(restarted.Ribbon.Qat.Items);
    }

    [Fact]
    public void UnnamedRuntimeItem_IsExplicitlyNotPersisted()
    {
        var store = new InMemoryRibbonSettingsStore();
        var fixture = CreateFixture(store);
        fixture.Ribbon.Qat.Items.Add(new RibbonButton());

        fixture.Controller.Save();

        Assert.Equal(" ", store.Read(Address("Qat")));
    }

    [Fact]
    public void OptedOutItem_CannotBeAddedByQatCustomization()
    {
        var toolbar = new QuickAccessToolbar();
        var item = new RibbonButton { Name = "Blocked", CanBeAddedToQat = false };

        Assert.False(toolbar.AddItem(item));
        Assert.Empty(toolbar.Items);
    }

    [Fact]
    public void ApplicationUserTenantScopes_DoNotOverwriteOneAnother()
    {
        var store = new InMemoryRibbonSettingsStore();
        var first = CreateFixture(store, tenant: "TenantA", user: "UserA", applicationId: "AppA");
        first.Ribbon.Qat.Items.Add((ICanAddToQuickAccess)first.Ribbon.GetItemByName("New")!);
        first.Controller.Save();

        var second = CreateFixture(store, tenant: "TenantB", user: "UserB", applicationId: "AppB");
        second.Ribbon.Qat.Items.Add((ICanAddToQuickAccess)second.Ribbon.GetItemByName("Refresh")!);
        second.Controller.Save();

        Assert.Equal("New", store.Read(new RibbonSettingsAddress("TenantA", "RibbonBar", "UserA", "Qat", "AppA")));
        Assert.Equal("Refresh", store.Read(new RibbonSettingsAddress("TenantB", "RibbonBar", "UserB", "Qat", "AppB")));
    }

    private static Fixture CreateFixture(
        IRibbonSettingsStore store,
        string? extraName = null,
        string tenant = "tenant",
        string user = "user",
        string applicationId = "app")
    {
        var ribbon = new Ribbon();
        var tab = RibbonBuilder.InsertOrAddTab(ribbon, "Home", "Home");
        var group = RibbonBuilder.InsertOrAddGroup(tab, "Edit", "Edit");
        foreach (var name in new[] { "New", "Refresh", "Save" })
            RibbonBuilder.InsertOrAddButton(group, name, name, null, null, null);
        if (!string.IsNullOrWhiteSpace(extraName))
            RibbonBuilder.InsertOrAddButton(group, extraName, extraName, null, null, null);

        var ownership = new RibbonQatOwnership(static () => null);
        var controller = new AppQatController(
            ribbon,
            store,
            () => tenant,
            () => user,
            applicationId,
            ownership);
        return new Fixture(ribbon, controller);
    }

    private static string[] GetNames(Ribbon ribbon)
    {
        return ribbon.Qat.Items.Select(item => ((Control)item).Name!).ToArray();
    }

    private static RibbonSettingsAddress Address(string section)
    {
        return new RibbonSettingsAddress("tenant", "RibbonBar", "user", section, "app");
    }

    private sealed record Fixture(Ribbon Ribbon, AppQatController Controller);
}
