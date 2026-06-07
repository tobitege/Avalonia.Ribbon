using Avalonia.Automation.Peers;
using Avalonia.Controls.Primitives;

namespace AvaloniaUI.Ribbon.Automation;

internal sealed class RibbonToggleButtonAutomationPeer : ToggleButtonAutomationPeer
{
    public RibbonToggleButtonAutomationPeer(ToggleButton owner) : base(owner)
    {
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
}
