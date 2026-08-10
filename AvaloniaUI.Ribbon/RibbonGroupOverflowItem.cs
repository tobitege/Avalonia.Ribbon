using System.Collections;

namespace AvaloniaUI.Ribbon;

public sealed class RibbonGroupOverflowItem
{
    internal RibbonGroupOverflowItem(RibbonGroupBox group)
    {
        Group = group;
    }

    public object? Header => Group.Header;

    public IEnumerable Items => Group.Items;

    internal RibbonGroupBox Group { get; }
}
