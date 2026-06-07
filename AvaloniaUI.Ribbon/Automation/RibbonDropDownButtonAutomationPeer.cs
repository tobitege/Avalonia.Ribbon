using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace AvaloniaUI.Ribbon.Automation;

internal class RibbonDropDownButtonAutomationPeer : ControlAutomationPeer, IExpandCollapseProvider, IInvokeProvider
{
    public RibbonDropDownButtonAutomationPeer(RibbonDropDownButton owner) : base(owner)
    {
    }

    public ExpandCollapseState ExpandCollapseState
    {
        get
        {
            var owner = GetOwner();
            return owner != null && owner.IsDropDownOpen
                ? ExpandCollapseState.Expanded
                : ExpandCollapseState.Collapsed;
        }
    }

    public bool ShowsMenu => true;

    public void Expand()
    {
        var owner = GetOwner();
        if (owner != null)
        {
            owner.IsDropDownOpen = true;
        }
    }

    public void Collapse()
    {
        var owner = GetOwner();
        if (owner != null)
        {
            owner.IsDropDownOpen = false;
        }
    }

    public void Invoke()
    {
        InvokeCore();
    }

    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.SplitButton;
    }

    protected override string GetNameCore()
    {
        var name = RibbonAutomationPeerHelper.GetName(Owner);
        return !string.IsNullOrWhiteSpace(name) ? name : base.GetNameCore() ?? string.Empty;
    }

    protected override string GetAutomationIdCore()
    {
        var automationId = RibbonAutomationPeerHelper.GetAutomationId(Owner);
        return !string.IsNullOrWhiteSpace(automationId) ? automationId : base.GetAutomationIdCore() ?? string.Empty;
    }

    protected override string GetHelpTextCore()
    {
        var helpText = RibbonAutomationPeerHelper.GetHelpText(Owner);
        return !string.IsNullOrWhiteSpace(helpText) ? helpText : base.GetHelpTextCore() ?? string.Empty;
    }

    protected virtual void InvokeCore()
    {
        var owner = GetOwner();
        if (owner != null)
        {
            owner.IsDropDownOpen = !owner.IsDropDownOpen;
        }
    }

    private RibbonDropDownButton? GetOwner()
    {
        return Owner as RibbonDropDownButton;
    }
}
