using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace AvaloniaUI.Ribbon.Desktop;

public sealed class RibbonShellContext
{
    public RibbonShellContext(
        IRibbonSettingsStore settings,
        string applicationId,
        string formId,
        Func<string?> tenantProvider,
        Func<string?> userProvider)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ApplicationId = string.IsNullOrWhiteSpace(applicationId)
            ? throw new ArgumentException("An application identifier is required.", nameof(applicationId))
            : applicationId;
        FormId = string.IsNullOrWhiteSpace(formId)
            ? throw new ArgumentException("A form identifier is required.", nameof(formId))
            : formId;
        TenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        UserProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
    }

    public IRibbonSettingsStore Settings { get; }

    public string ApplicationId { get; }

    public string FormId { get; }

    public Func<string?> TenantProvider { get; }

    public Func<string?> UserProvider { get; }

    public string? UserFunctionTabName { get; init; }

    public RibbonShellIcons Icons { get; init; } = new();

    public bool IconsEnabled { get; init; } = true;

    public IBrush? ApplicationMenuAccent { get; init; }

    public RibbonVisualStyle FallbackVisualStyle { get; init; } = RibbonVisualStyle.Default;

    public IReadOnlyList<RibbonVisualStyle> AvailableVisualStyles { get; init; } =
        new[] { RibbonVisualStyle.Default, RibbonVisualStyle.Light, RibbonVisualStyle.Dark };

    public Func<bool> TenantSwitchGate { get; init; } = static () => true;

    public Func<bool> LicenseGate { get; init; } = static () => true;

    public Func<bool> SupportGate { get; init; } = static () => true;
}

public sealed class RibbonShellIcons
{
    public object? LargeLogo { get; init; }

    public object? SmallLogo { get; init; }

    public object? New { get; init; }

    public object? Refresh { get; init; }

    public object? Save { get; init; }

    public object? Print { get; init; }

    public object? Export { get; init; }

    public object? Information { get; init; }

    public object? Settings { get; init; }

    public object? TenantSwitch { get; init; }

    public object? License { get; init; }

    public object? Support { get; init; }

    public object? Exit { get; init; }

    public object? Help { get; init; }
}

public static class RibbonShellItemNames
{
    public const string ApplicationMenu = "ApplicationMenu";
    public const string ApplicationMenuSettings = "ApplicationMenu.Settings";
    public const string ApplicationMenuSupport = "ApplicationMenu.Support";
    public const string Settings = "Settings";
    public const string TenantSwitch = "TenantSwitch";
    public const string License = "License";
    public const string Support = "Support";
    public const string Exit = "Exit";
    public const string PrimaryTab = "Primary";
    public const string EditGroup = "Edit";
    public const string OutputGroup = "Output";
    public const string QuickInfoGroup = "QuickInfo";
    public const string OrganisationGroup = "Organisation";
    public const string New = "New";
    public const string Refresh = "Refresh";
    public const string Save = "Save";
    public const string Print = "Print";
    public const string Export = "Export";
    public const string Information = "Information";
    public const string WindowTab = "Window";
    public const string ViewGroup = "View";
    public const string VisualStyle = "VisualStyle";
    public const string HelpTab = "Help";
    public const string HelpGroup = "HelpActions";
    public const string About = "About";
    public const string DirectHelp = "DirectHelp";
}
