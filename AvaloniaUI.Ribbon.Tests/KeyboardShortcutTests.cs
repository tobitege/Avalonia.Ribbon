using System.Windows.Input;
using Avalonia.Input;

namespace AvaloniaUI.Ribbon.Tests;

public class KeyboardShortcutTests
{
    [Fact]
    public void Shortcut_TriggersCommand_WhenEnabledAndVisible()
    {
        var ribbon = new TestRibbon();
        var command = new CountingCommand();
        var button = new RibbonButton
        {
            Command = command,
            ShortcutKeys = new KeyGesture(Key.S, KeyModifiers.Control)
        };

        var group = new RibbonGroupBox();
        group.Items.Add(button);
        ribbon.SelectedGroups.Add(group);

        var handled = ribbon.InvokeShortcut(Key.S, KeyModifiers.Control);

        Assert.True(handled);
        Assert.Equal(1, command.ExecuteCount);
    }

    [Fact]
    public void Shortcut_DoesNotTrigger_WhenDisabledOrHidden()
    {
        var ribbon = new TestRibbon();
        var command = new CountingCommand();
        var button = new RibbonButton
        {
            Command = command,
            ShortcutKeys = new KeyGesture(Key.D, KeyModifiers.Control)
        };

        var group = new RibbonGroupBox();
        group.Items.Add(button);
        ribbon.SelectedGroups.Add(group);

        button.IsEnabled = false;
        Assert.False(ribbon.InvokeShortcut(Key.D, KeyModifiers.Control));
        Assert.Equal(0, command.ExecuteCount);

        button.IsEnabled = true;
        button.IsVisible = false;
        Assert.False(ribbon.InvokeShortcut(Key.D, KeyModifiers.Control));
        Assert.Equal(0, command.ExecuteCount);
    }

    [Fact]
    public void Escape_NavigatesBackToRibbonKeyTipLevel()
    {
        var ribbon = new TestRibbon();
        var tab = new RibbonTab { Header = "Home" };
        ribbon.Items.Add(tab);
        ribbon.SelectedItem = tab;

        KeyTip.SetShowChildKeyTipKeys(tab, true);
        var navigated = ribbon.InvokeNavigateBack();

        Assert.True(navigated);
        Assert.False(KeyTip.GetShowChildKeyTipKeys(tab));
        Assert.True(KeyTip.GetShowChildKeyTipKeys(ribbon));
    }

    private sealed class TestRibbon : Ribbon
    {
        public bool InvokeShortcut(Key key, KeyModifiers modifiers)
        {
            return TryHandleShortcut(key, modifiers);
        }

        public bool InvokeNavigateBack()
        {
            return TryNavigateBackFromTabKeyTips();
        }
    }

    private sealed class CountingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
        }

        public event EventHandler? CanExecuteChanged;
    }
}
