using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AvaloniaUI.Ribbon.Desktop;

public class RibbonShell : RibbonWindow
{
    private const string VisualStyleSection = "VisualStyle";

    private readonly RibbonShellContext _context;
    private readonly bool _iconsEnabled;
    private readonly RibbonComboBox _visualStyleCombo;
    private bool _applyingVisualStyle;
    private bool _loadScheduled;
    private bool _loaded;

    public RibbonShell(RibbonShellContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        ValidateVisualStyles(context);
        _iconsEnabled = context.IconsEnabled;

        RibbonControl = new DesktopRibbon();
        QuickAccessToolbar = new QuickAccessToolbar();
        Ribbon = RibbonControl;

        Lookup = new RibbonItemLookup(RibbonControl);
        Ownership = new RibbonQatOwnership(ResolveUserFunctionTab);
        AppQat = new AppQatController(
            RibbonControl,
            context.Settings,
            context.TenantProvider,
            context.UserProvider,
            context.ApplicationId,
            Ownership);
        ViewQat = new ViewQatController(
            RibbonControl,
            context.Settings,
            context.TenantProvider,
            context.UserProvider,
            context.FormId,
            Ownership);
        EventRouter = new RibbonEventRouter(RibbonControl);

        _visualStyleCombo = BuildDefaultRibbon();
        RegisterHandlers();

        Opened += OnOpened;
        Closing += OnClosing;
        Closed += OnClosed;
    }

    public DesktopRibbon RibbonControl { get; }

    public RibbonMenu ApplicationMenu { get; private set; } = null!;

    public RibbonItemLookup Lookup { get; }

    public RibbonQatOwnership Ownership { get; }

    public AppQatController AppQat { get; }

    public ViewQatController ViewQat { get; }

    public RibbonEventRouter EventRouter { get; }

    public void LoadRibbonState()
    {
        if (_loaded || string.IsNullOrWhiteSpace(_context.UserProvider()))
            return;

        AppQat.Load();
        LoadVisualStyle();
        UpdateRibbonState();
        _loaded = true;
    }

    public void SaveRibbonState()
    {
        if (!_loaded)
            return;

        AppQat.Save();
        SaveVisualStyle(RibbonControl.VisualStyle);
    }

    public void RebuildView(string? previousView, string? newView, Action rebuild)
    {
        ArgumentNullException.ThrowIfNull(rebuild);

        ViewQat.Stash();
        ViewQat.Save(previousView);
        rebuild();
        ViewQat.Load(newView);
        ViewQat.Restore();
    }

    public void DeleteView(string? viewName)
    {
        ViewQat.Delete(viewName);
    }

    protected object? GetIcon(object? image)
    {
        return _iconsEnabled ? image : null;
    }

    protected virtual void UpdateRibbonState()
    {
        var supportAvailable = _context.SupportGate();
        SetGate(RibbonShellItemNames.TenantSwitch, _context.TenantSwitchGate());
        SetGate(RibbonShellItemNames.License, _context.LicenseGate());
        SetGate(RibbonShellItemNames.Support, supportAvailable);
        SetGate(RibbonShellItemNames.ApplicationMenuSupport, supportAvailable);
    }

    protected virtual void ReapplyDependentAccentColors()
    {
        if (_context.ApplicationMenuAccent is not IBrush accent)
            return;

        ApplicationMenu.SetCurrentValue(TemplatedControl.BackgroundProperty, accent);
        ApplicationMenu.SetCurrentValue(RibbonMenu.AccentBrushProperty, accent);
    }

    protected virtual void OnNewClicked(RibbonEventArgs args) { }

    protected virtual void OnRefreshClicked(RibbonEventArgs args) { }

    protected virtual void OnSaveClicked(RibbonEventArgs args) { }

    protected virtual void OnPrintClicked(RibbonEventArgs args) { }

    protected virtual void OnExportClicked(RibbonEventArgs args) { }

    protected virtual void OnInformationClicked(RibbonEventArgs args) { }

    protected virtual void OnSettingsClicked(RibbonEventArgs args) { }

    protected virtual void OnTenantSwitchClicked(RibbonEventArgs args) { }

    protected virtual void OnLicenseClicked(RibbonEventArgs args) { }

    protected virtual void OnSupportClicked(RibbonEventArgs args) { }

    protected virtual void OnAboutClicked(RibbonEventArgs args) { }

    protected virtual void OnDirectHelpClicked(RibbonEventArgs args) { }

    protected virtual void OnExitClicked(RibbonEventArgs args)
    {
        Close();
    }

    private RibbonComboBox BuildDefaultRibbon()
    {
        RibbonControl.BeginUpdate();
        try
        {
            BuildApplicationMenu();
            BuildPrimaryTab();
            var visualStyle = BuildWindowTab();
            BuildHelpTab();
            BuildConfigToolBar();
            RibbonControl.HelpPaneContent = RibbonControl.ConfigToolBar;
            return visualStyle;
        }
        finally
        {
            RibbonControl.EndUpdate();
        }
    }

    private void BuildApplicationMenu()
    {
        ApplicationMenu = new RibbonMenu
        {
            Name = RibbonShellItemNames.ApplicationMenu,
            LargeImage = GetIcon(_context.Icons.LargeLogo),
            SmallImage = GetIcon(_context.Icons.SmallLogo),
            AccentBrush = _context.ApplicationMenuAccent,
            Content = "File"
        };

        AddMenuItem(
            RibbonShellItemNames.ApplicationMenuSettings,
            "Settings",
            _context.Icons.Settings,
            group: "Configuration");
        AddMenuItem(
            RibbonShellItemNames.TenantSwitch,
            "Switch tenant",
            _context.Icons.TenantSwitch,
            group: "Configuration");
        AddSeparator("ApplicationMenuSeparator1");
        AddMenuItem(RibbonShellItemNames.License, "License", _context.Icons.License, group: "Assistance");
        AddMenuItem(
            RibbonShellItemNames.ApplicationMenuSupport,
            "Support",
            _context.Icons.Support,
            group: "Assistance");
        AddSeparator("ApplicationMenuSeparator2");
        AddMenuItem(RibbonShellItemNames.Exit, "Exit", _context.Icons.Exit, "Exit", bottomDocked: true);

        RibbonControl.ApplicationMenu = ApplicationMenu;
        ReapplyDependentAccentColors();
    }

    private void BuildPrimaryTab()
    {
        var tab = RibbonBuilder.InsertOrAddTab(
            RibbonControl,
            RibbonShellItemNames.PrimaryTab,
            "Home");
        var edit = RibbonBuilder.InsertOrAddGroup(tab, RibbonShellItemNames.EditGroup, "Edit");
        AddButton(edit, RibbonShellItemNames.New, "New", _context.Icons.New);
        AddButton(edit, RibbonShellItemNames.Refresh, "Refresh", _context.Icons.Refresh, RibbonShellItemNames.New);
        AddButton(edit, RibbonShellItemNames.Save, "Save", _context.Icons.Save, RibbonShellItemNames.Refresh);

        var output = RibbonBuilder.InsertOrAddGroup(tab, RibbonShellItemNames.OutputGroup, "Output", RibbonShellItemNames.EditGroup);
        AddButton(output, RibbonShellItemNames.Print, "Print", _context.Icons.Print);
        AddButton(output, RibbonShellItemNames.Export, "Export", _context.Icons.Export, RibbonShellItemNames.Print);

        var quickInfo = RibbonBuilder.InsertOrAddGroup(
            tab,
            RibbonShellItemNames.QuickInfoGroup,
            "Quick info",
            RibbonShellItemNames.OutputGroup);
        AddButton(quickInfo, RibbonShellItemNames.Information, "Information", _context.Icons.Information);

        var organisation = RibbonBuilder.InsertOrAddGroup(
            tab,
            RibbonShellItemNames.OrganisationGroup,
            "Organisation",
            RibbonShellItemNames.QuickInfoGroup);
        AddButton(
            organisation,
            RibbonShellItemNames.Settings,
            "Settings",
            _context.Icons.Settings,
            canBeAddedToQat: false);
    }

    private RibbonComboBox BuildWindowTab()
    {
        var tab = RibbonBuilder.InsertOrAddTab(
            RibbonControl,
            RibbonShellItemNames.WindowTab,
            "Window",
            RibbonShellItemNames.PrimaryTab);
        var group = RibbonBuilder.InsertOrAddGroup(tab, RibbonShellItemNames.ViewGroup, "View");
        var combo = new RibbonComboBox
        {
            Name = RibbonShellItemNames.VisualStyle,
            Content = "Theme",
            ItemsSource = _context.AvailableVisualStyles
        };
        combo.SelectionChanged += OnVisualStyleChanged;
        group.Items.Add(combo);
        return combo;
    }

    private void BuildHelpTab()
    {
        var tab = RibbonBuilder.InsertOrAddTab(
            RibbonControl,
            RibbonShellItemNames.HelpTab,
            "Help",
            RibbonShellItemNames.WindowTab);
        var group = RibbonBuilder.InsertOrAddGroup(tab, RibbonShellItemNames.HelpGroup, "Help");
        AddButton(group, RibbonShellItemNames.Support, "Support", _context.Icons.Support);
        AddButton(group, RibbonShellItemNames.About, "About", _context.Icons.Information, RibbonShellItemNames.Support);
    }

    private void BuildConfigToolBar()
    {
        var directHelp = new RibbonButton
        {
            Name = RibbonShellItemNames.DirectHelp,
            Content = "Direct help",
            Icon = GetIcon(_context.Icons.Help),
            QuickAccessIcon = GetIcon(_context.Icons.Help),
            CanAddToQuickAccess = false
        };
        RibbonControl.ConfigToolBar.Items.Add(directHelp);
    }

    private RibbonButton AddButton(
        RibbonGroupBox group,
        string name,
        string text,
        object? icon,
        string? afterName = null,
        bool canBeAddedToQat = true)
    {
        var resolvedIcon = GetIcon(icon);
        return RibbonBuilder.InsertOrAddButton(
            group,
            name,
            text,
            resolvedIcon,
            resolvedIcon,
            text,
            afterName,
            canBeAddedToQat);
    }

    private void AddMenuItem(
        string name,
        string text,
        object? icon,
        string group = "Default",
        bool bottomDocked = false)
    {
        var item = new RibbonMenuItem
        {
            Name = name,
            Header = text,
            Group = group,
            IsBottomDocked = bottomDocked,
            IsTopDocked = !bottomDocked
        };
        var resolvedIcon = GetIcon(icon);
        if (resolvedIcon is not null)
            item.Icon = resolvedIcon;
        ApplicationMenu.LeftPaneItems.Add(item);
    }

    private void AddSeparator(string name)
    {
        ApplicationMenu.LeftPaneItems.Add(new Separator { Name = name });
    }

    private void RegisterHandlers()
    {
        EventRouter.Register(RibbonShellItemNames.New, OnNewClicked);
        EventRouter.Register(RibbonShellItemNames.Refresh, OnRefreshClicked);
        EventRouter.Register(RibbonShellItemNames.Save, OnSaveClicked);
        EventRouter.Register(RibbonShellItemNames.Print, OnPrintClicked);
        EventRouter.Register(RibbonShellItemNames.Export, OnExportClicked);
        EventRouter.Register(RibbonShellItemNames.Information, OnInformationClicked);
        EventRouter.Register(RibbonShellItemNames.Settings, OnSettingsClicked);
        EventRouter.Register(RibbonShellItemNames.ApplicationMenuSettings, OnSettingsClicked);
        EventRouter.Register(RibbonShellItemNames.TenantSwitch, OnTenantSwitchClicked);
        EventRouter.Register(RibbonShellItemNames.License, OnLicenseClicked);
        EventRouter.Register(RibbonShellItemNames.Support, OnSupportClicked);
        EventRouter.Register(RibbonShellItemNames.ApplicationMenuSupport, OnSupportClicked);
        EventRouter.Register(RibbonShellItemNames.About, OnAboutClicked);
        EventRouter.Register(RibbonShellItemNames.DirectHelp, OnDirectHelpClicked);
        EventRouter.Register(RibbonShellItemNames.Exit, OnExitClicked);
    }

    private void SetGate(string itemName, bool allowed)
    {
        Lookup.SetItemState(itemName, allowed, allowed);
    }

    private RibbonTab? ResolveUserFunctionTab()
    {
        return string.IsNullOrWhiteSpace(_context.UserFunctionTabName)
            ? null
            : Lookup.FindTab(_context.UserFunctionTabName);
    }

    private void OnOpened(object? sender, EventArgs args)
    {
        if (_loadScheduled)
            return;

        _loadScheduled = true;
        Dispatcher.UIThread.Post(LoadRibbonState, DispatcherPriority.ApplicationIdle);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs args)
    {
        SaveRibbonState();
    }

    private void OnClosed(object? sender, EventArgs args)
    {
        EventRouter.Dispose();
    }

    private void OnVisualStyleChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (_applyingVisualStyle || _visualStyleCombo.SelectedItem is not RibbonVisualStyle style)
            return;

        ApplyVisualStyle(style);
        SaveVisualStyle(style);
    }

    private void LoadVisualStyle()
    {
        var style = _context.FallbackVisualStyle;
        if (TryCreateVisualStyleAddress(out var address))
        {
            var stored = _context.Settings.Read(address);
            if (Enum.TryParse<RibbonVisualStyle>(stored, ignoreCase: false, out var parsed) &&
                _context.AvailableVisualStyles.Contains(parsed))
            {
                style = parsed;
            }
        }

        ApplyVisualStyle(style);
    }

    private void ApplyVisualStyle(RibbonVisualStyle style)
    {
        _applyingVisualStyle = true;
        try
        {
            RibbonControl.SetCurrentValue(AvaloniaUI.Ribbon.Ribbon.VisualStyleProperty, style);
            _visualStyleCombo.SelectedItem = style;
            if (Application.Current is { } application)
                application.RequestedThemeVariant = ToThemeVariant(style);
            ReapplyDependentAccentColors();
        }
        finally
        {
            _applyingVisualStyle = false;
        }
    }

    private void SaveVisualStyle(RibbonVisualStyle style)
    {
        if (TryCreateVisualStyleAddress(out var address))
            _context.Settings.Write(address, style.ToString());
    }

    private bool TryCreateVisualStyleAddress(out RibbonSettingsAddress address)
    {
        var user = _context.UserProvider();
        if (string.IsNullOrWhiteSpace(user))
        {
            address = default;
            return false;
        }

        address = new RibbonSettingsAddress(
            _context.TenantProvider() ?? string.Empty,
            AppQatController.Area,
            user,
            VisualStyleSection,
            _context.ApplicationId);
        return true;
    }

    private static ThemeVariant ToThemeVariant(RibbonVisualStyle style)
    {
        return style switch
        {
            RibbonVisualStyle.Light => ThemeVariant.Light,
            RibbonVisualStyle.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private static void ValidateVisualStyles(RibbonShellContext context)
    {
        if (context.AvailableVisualStyles.Count == 0 ||
            !context.AvailableVisualStyles.Contains(context.FallbackVisualStyle))
        {
            throw new ArgumentException(
                "Available visual styles must contain the documented fallback style.",
                nameof(context));
        }
    }
}
