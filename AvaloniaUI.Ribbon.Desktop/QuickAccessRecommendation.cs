using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaUI.Ribbon;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon.Desktop;

public class QuickAccessRecommendation : AvaloniaObject
{
    public static readonly StyledProperty<ICanAddToQuickAccess> ItemProperty =
        QuickAccessItem.ItemProperty.AddOwner<QuickAccessRecommendation>();

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        ToggleButton.IsCheckedProperty.AddOwner<QuickAccessRecommendation>();

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<QuickAccessRecommendation, string?>(nameof(Label));

    public ICanAddToQuickAccess Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string DisplayLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Label))
                return Label;
            if (Item is not Control control)
                return string.Empty;
            if (ToolTip.GetTip(control) is string toolTip && !string.IsNullOrWhiteSpace(toolTip))
                return toolTip;
            if (control is ContentControl { Content: string content } && !string.IsNullOrWhiteSpace(content))
                return content;

            var automationName = AutomationProperties.GetName(control);
            if (!string.IsNullOrWhiteSpace(automationName))
                return automationName;

            return RibbonItem.GetId(control) ?? control.Name ?? string.Empty;
        }
    }
}
