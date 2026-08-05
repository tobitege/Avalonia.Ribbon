using System.Linq;
using Avalonia.Headless.XUnit;
using AvaloniaUI.Ribbon.Desktop;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonShellTests
{
    [AvaloniaFact]
    public void Shell_BuildsFixedStructureAndSeedsQatAtExplicitLoadBoundary()
    {
        var store = new InMemoryRibbonSettingsStore();
        var context = new RibbonShellContext(
            store,
            "app",
            "form",
            static () => "tenant",
            static () => "user");
        var shell = new RibbonShell(context);

        Assert.Equal(
            new[] { "Primary", "Window", "Help" },
            shell.RibbonControl.Tabs.Select(tab => tab.Name));
        Assert.Single(shell.RibbonControl.ConfigToolBar.Items);
        Assert.Equal(
            "DirectHelp",
            Assert.IsType<RibbonButton>(shell.RibbonControl.ConfigToolBar.Items[0]).Name);
        Assert.Equal(7, shell.ApplicationMenu.LeftPaneItems.Count);
        var exit = shell.ApplicationMenu.LeftPaneItems
            .OfType<RibbonMenuItem>()
            .Single(item => item.Name == RibbonShellItemNames.Exit);
        Assert.True(exit.IsBottomDocked);
        var stableNames = shell.Lookup.EnumerateItems()
            .Select(item => item.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToArray();
        Assert.Equal(stableNames.Length, stableNames.Distinct(System.StringComparer.Ordinal).Count());

        shell.LoadRibbonState();

        Assert.Equal(
            new[] { "New", "Refresh", "Save" },
            shell.RibbonControl.Qat.Items.Select(item => ((Avalonia.Controls.Control)item).Name));
    }

    [AvaloniaFact]
    public void ThemeSelection_PersistsAndUsesDocumentedFallback()
    {
        var store = new InMemoryRibbonSettingsStore();
        var context = new RibbonShellContext(
            store,
            "app",
            "form",
            static () => "tenant",
            static () => "user")
        {
            FallbackVisualStyle = RibbonVisualStyle.Light
        };
        var first = new RibbonShell(context);
        first.LoadRibbonState();
        Assert.Equal(RibbonVisualStyle.Light, first.RibbonControl.VisualStyle);

        var combo = Assert.IsType<RibbonComboBox>(first.RibbonControl.GetItemByName("VisualStyle"));
        combo.SelectedItem = RibbonVisualStyle.Dark;

        var restarted = new RibbonShell(context);
        restarted.LoadRibbonState();
        Assert.Equal(RibbonVisualStyle.Dark, restarted.RibbonControl.VisualStyle);
    }
}
