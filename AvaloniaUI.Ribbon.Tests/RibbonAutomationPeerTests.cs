using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaUI.Ribbon;
using System.Windows.Input;

namespace AvaloniaUI.Ribbon.Tests;

public class RibbonAutomationPeerTests
{
    [Fact]
    public void RibbonButtonAutomationPeer_UsesAutomationProperties()
    {
        var button = new RibbonButton
        {
            Name = "SaveButton",
            Content = "Save"
        };
        AutomationProperties.SetName(button, "Save document");
        AutomationProperties.SetAutomationId(button, "Ribbon_Save");
        AutomationProperties.SetHelpText(button, "Saves the current document");

        var peer = ControlAutomationPeer.CreatePeerForElement(button);

        Assert.Equal("Save document", peer.GetName());
        Assert.Equal("Ribbon_Save", peer.GetAutomationId());
        Assert.Equal("Saves the current document", peer.GetHelpText());
        Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
        Assert.NotNull(peer.GetProvider<IInvokeProvider>());
    }

    [Fact]
    public void RibbonButtonAutomationPeer_FallsBackToContentNameAndControlNameAutomationId()
    {
        var button = new RibbonButton
        {
            Name = "OpenButton",
            Content = "Open"
        };

        var peer = ControlAutomationPeer.CreatePeerForElement(button);

        Assert.Equal("Open", peer.GetName());
        Assert.Equal("OpenButton", peer.GetAutomationId());
    }

    [Fact]
    public void RibbonButtonAutomationPeer_UsesToolTipForIconOnlyButtonName()
    {
        var button = new RibbonButton
        {
            Name = "CopyButton"
        };
        ToolTip.SetTip(button, "Copy");

        var peer = ControlAutomationPeer.CreatePeerForElement(button);

        Assert.Equal("Copy", peer.GetName());
        Assert.Equal("Copy", peer.GetHelpText());
        Assert.Equal("CopyButton", peer.GetAutomationId());
    }

    [Fact]
    public void RibbonButtonAutomationPeer_FallsBackToKeyTipForIconOnlyButtonWithoutToolTip()
    {
        var button = new RibbonButton();
        KeyTip.SetKeyTipKeys(button, "S");

        var peer = ControlAutomationPeer.CreatePeerForElement(button);

        Assert.Equal("S", peer.GetName());
        Assert.Equal("S", peer.GetAutomationId());
    }

    [Fact]
    public void RibbonToggleButtonAutomationPeer_ExposesToggleProviderAndName()
    {
        var button = new RibbonToggleButton
        {
            Name = "BoldButton",
            Content = "Bold"
        };

        var peer = ControlAutomationPeer.CreatePeerForElement(button);

        Assert.Equal("Bold", peer.GetName());
        Assert.Equal("BoldButton", peer.GetAutomationId());
        Assert.NotNull(peer.GetProvider<IToggleProvider>());
    }

    [Fact]
    public void RibbonDropDownButtonAutomationPeer_ExposesExpandableSplitButton()
    {
        var button = new RibbonDropDownButton
        {
            Name = "StylesButton",
            Content = "Styles"
        };

        var peer = ControlAutomationPeer.CreatePeerForElement(button);
        var expandCollapseProvider = peer.GetProvider<IExpandCollapseProvider>();
        var invokeProvider = peer.GetProvider<IInvokeProvider>();

        Assert.Equal("Styles", peer.GetName());
        Assert.Equal("StylesButton", peer.GetAutomationId());
        Assert.Equal(AutomationControlType.SplitButton, peer.GetAutomationControlType());
        Assert.NotNull(expandCollapseProvider);
        Assert.NotNull(invokeProvider);

        Assert.Equal(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);
        expandCollapseProvider.Expand();
        Assert.True(button.IsDropDownOpen);
        Assert.Equal(ExpandCollapseState.Expanded, expandCollapseProvider.ExpandCollapseState);
        invokeProvider.Invoke();
        Assert.False(button.IsDropDownOpen);
    }

    [Fact]
    public void RibbonSplitButtonAutomationPeer_InvokeExecutesCommandWhenAvailable()
    {
        var executed = false;
        var button = new RibbonSplitButton
        {
            Name = "RunButton",
            Content = "Run",
            Command = new TestCommand(() => executed = true)
        };

        var peer = ControlAutomationPeer.CreatePeerForElement(button);
        var invokeProvider = peer.GetProvider<IInvokeProvider>();

        Assert.NotNull(invokeProvider);
        invokeProvider.Invoke();

        Assert.True(executed);
        Assert.False(button.IsDropDownOpen);
    }

    private sealed class TestCommand : ICommand
    {
        private readonly Action _execute;

        public TestCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
