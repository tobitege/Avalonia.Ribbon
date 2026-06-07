namespace AvaloniaUI.Ribbon.Automation;

internal sealed class RibbonSplitButtonAutomationPeer : RibbonDropDownButtonAutomationPeer
{
    public RibbonSplitButtonAutomationPeer(RibbonSplitButton owner) : base(owner)
    {
    }

    protected override void InvokeCore()
    {
        var owner = Owner as RibbonSplitButton;
        if (owner?.Command != null)
        {
            if (owner.Command.CanExecute(owner.CommandParameter))
            {
                owner.Command.Execute(owner.CommandParameter);
            }

            return;
        }

        base.InvokeCore();
    }
}
