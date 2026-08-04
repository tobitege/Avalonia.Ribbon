using System.Linq;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;
using AvaloniaUI.Ribbon.Desktop;
using AvaloniaUI.Ribbon;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class DesktopRibbonQuickAccessTests
{
    [Fact]
    public void Ribbon_QuickAccessProperties_DefaultToExpectedValues()
    {
        var ribbon = new Ribbon();

        Assert.Equal(RibbonQatLocation.Above, ribbon.QuickAccessLocation);
        Assert.True(ribbon.ShowQatOverflowButton);
        Assert.Empty(ribbon.QuickAccessItems);
    }

    [Fact]
    public void DesktopRibbon_QuickAccessItems_CollectionSyncsToToolbar()
    {
        var toolbar = new QuickAccessToolbar();
        var ribbon = new DesktopRibbon { QuickAccessToolbar = toolbar };
        var first = CreateQuickAccessButton("First");
        var second = CreateQuickAccessButton("Second");
        ribbon.QuickAccessItems.Add(first);
        ribbon.QuickAccessItems.Add(second);

        var toolbarItems = toolbar.Items.OfType<QuickAccessItem>();
        Assert.Equal(2, toolbarItems.Count());
        Assert.Equal(new[] { first, second }, toolbarItems.Select(x => x.Item));

        ribbon.QuickAccessItems.Add(first);
        Assert.Single(ribbon.QuickAccessItems, item => item == first);

        ribbon.QuickAccessItems.Remove(first);
        Assert.Equal(new[] { second }, toolbarItems.Select(x => x.Item));
    }

    [Fact]
    public void DesktopRibbon_QuickAccessToolbar_AddAndRemoveSyncWithRibbonCollection()
    {
        var toolbar = new QuickAccessToolbar();
        var ribbon = new DesktopRibbon { QuickAccessToolbar = toolbar };
        var item = CreateQuickAccessButton("Item");

        Assert.True(toolbar.AddItem(item));
        Assert.Single(ribbon.QuickAccessItems);
        Assert.Contains(item, ribbon.QuickAccessItems);

        Assert.False(toolbar.AddItem(item));
        Assert.Single(ribbon.QuickAccessItems);
        Assert.Equal(new[] { item }, toolbar.Items.OfType<QuickAccessItem>().Select(x => x.Item));

        Assert.True(toolbar.RemoveItem(item));
        Assert.Empty(ribbon.QuickAccessItems);
        Assert.Empty(toolbar.Items.OfType<QuickAccessItem>());
    }

    [Fact]
    public void QuickAccessToolbar_RestoresAndPersistsItemsById()
    {
        var first = CreateQuickAccessButton("First");
        var second = CreateQuickAccessButton("Second");
        QuickAccessToolbar.SetPersistenceId((Avalonia.AvaloniaObject)first, "first");
        QuickAccessToolbar.SetPersistenceId((Avalonia.AvaloniaObject)second, "second");

        var provider = new TestPersistenceProvider(new[] { "second", "first" });
        var toolbar = new QuickAccessToolbar
        {
            PersistenceKey = "main",
            PersistenceProvider = provider
        };
        toolbar.AvailableItems.Add(first);
        toolbar.AvailableItems.Add(second);

        Assert.True(toolbar.RestoreState());
        Assert.Equal(new[] { second, first },
            toolbar.Items.OfType<QuickAccessItem>().Select(item => item.Item));

        toolbar.RemoveItem(second);

        Assert.Equal("main", provider.SavedKey);
        Assert.Equal(new[] { "first" }, provider.SavedItemIds);
    }

    private static ICanAddToQuickAccess CreateQuickAccessButton(string content)
    {
        return new RibbonButton { Content = content };
    }

    private sealed class TestPersistenceProvider : IQuickAccessToolbarPersistenceProvider
    {
        private readonly IReadOnlyList<string>? _loadedItemIds;

        public TestPersistenceProvider(IReadOnlyList<string>? loadedItemIds)
        {
            _loadedItemIds = loadedItemIds;
        }

        public string? SavedKey { get; private set; }

        public IReadOnlyList<string>? SavedItemIds { get; private set; }

        public IReadOnlyList<string>? Load(string key)
        {
            return _loadedItemIds;
        }

        public void Save(string key, IReadOnlyList<string> itemIds)
        {
            SavedKey = key;
            SavedItemIds = itemIds;
        }
    }
}
