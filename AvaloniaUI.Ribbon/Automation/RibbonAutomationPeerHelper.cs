using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaUI.Ribbon.Automation;

internal static class RibbonAutomationPeerHelper
{
    public static string GetName(Control owner)
    {
        return FirstNonEmpty(
            AutomationProperties.GetName(owner),
            ContentToString(GetOwnerContent(owner)),
            ContentToString(ToolTip.GetTip(owner)),
            KeyTip.GetKeyTipKeys(owner),
            owner.Name,
            owner.GetType().Name);
    }

    public static string GetAutomationId(Control owner)
    {
        return FirstNonEmpty(
            AutomationProperties.GetAutomationId(owner),
            owner.Name,
            KeyTip.GetKeyTipKeys(owner),
            owner.GetType().Name);
    }

    public static string GetHelpText(Control owner)
    {
        return FirstNonEmpty(
            AutomationProperties.GetHelpText(owner),
            ContentToString(ToolTip.GetTip(owner)));
    }

    private static object? GetOwnerContent(Control owner)
    {
        if (owner is HeaderedItemsControl headeredItemsControl)
        {
            return headeredItemsControl.Header;
        }

        if (owner is HeaderedContentControl headeredContentControl)
        {
            return headeredContentControl.Header;
        }

        if (owner is ContentControl contentControl)
        {
            return contentControl.Content;
        }

        if (owner is RibbonDropDownButton dropDownButton)
        {
            return dropDownButton.Content;
        }

        return null;
    }

    private static string ContentToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string text)
        {
            return text;
        }

        if (value is TextBlock textBlock)
        {
            return textBlock.Text ?? string.Empty;
        }

        if (value is AccessText accessText)
        {
            return accessText.Text ?? string.Empty;
        }

        return value.ToString() ?? string.Empty;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }
}
