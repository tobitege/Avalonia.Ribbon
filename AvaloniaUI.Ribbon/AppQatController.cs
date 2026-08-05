using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon;

public sealed class AppQatController
{
    public const string Area = "RibbonBar";
    public const string QatSection = "Qat";
    public const string StateSection = "State";

    private static readonly string[] DefaultSeedNames = { "New", "Refresh", "Save" };

    private readonly string _applicationId;
    private readonly IReadOnlyList<string> _defaultNames;
    private readonly Ribbon _ribbon;
    private readonly RibbonQatOwnership _ownership;
    private readonly IRibbonSettingsStore _settings;
    private readonly Func<string?> _tenantProvider;
    private readonly Func<string?> _userProvider;

    public AppQatController(
        Ribbon ribbon,
        IRibbonSettingsStore settings,
        Func<string?> tenantProvider,
        Func<string?> userProvider,
        string applicationId,
        RibbonQatOwnership ownership,
        IEnumerable<string>? defaultNames = null)
    {
        _ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
        _applicationId = string.IsNullOrWhiteSpace(applicationId)
            ? throw new ArgumentException("An application identifier is required.", nameof(applicationId))
            : applicationId;
        _defaultNames = (defaultNames ?? DefaultSeedNames).ToArray();
    }

    public void Load()
    {
        if (!TryCreateAddress(QatSection, out var qatAddress))
            return;

        RemoveApplicationItems();

        var storedQat = _settings.Read(qatAddress);
        if (storedQat is null)
            SeedDefaults();
        else
            InsertResolvedNames(storedQat.Split(';', StringSplitOptions.None));

        var stateAddress = CreateAddress(StateSection, qatAddress.Tenant, qatAddress.User);
        var storedState = _settings.Read(stateAddress);
        if (storedState is null)
            return;

        var fields = storedState.Split(';', StringSplitOptions.None);
        if (fields.Length != 2)
            return;

        _ribbon.Qat.BelowRibbon = fields[0] == "1";
        _ribbon.Minimized = fields[1] == "1";
    }

    public void Save()
    {
        if (!TryCreateAddress(QatSection, out var qatAddress))
            return;

        var names = _ribbon.Qat.Items
            .Where(item => !_ownership.IsViewOwned(item))
            .Select(GetItemIdentity)
            .Where(name => !string.IsNullOrWhiteSpace(name));
        var qatValue = string.Join(';', names);
        _settings.Write(qatAddress, qatValue.Length == 0 ? " " : qatValue);

        var stateAddress = CreateAddress(StateSection, qatAddress.Tenant, qatAddress.User);
        var stateValue = $"{(_ribbon.Qat.BelowRibbon ? 1 : 0)};{(_ribbon.Minimized ? 1 : 0)}";
        _settings.Write(stateAddress, stateValue);
    }

    private void RemoveApplicationItems()
    {
        for (var index = _ribbon.Qat.Items.Count - 1; index >= 0; index--)
        {
            if (!_ownership.IsViewOwned(_ribbon.Qat.Items[index]))
                _ribbon.Qat.Items.RemoveAt(index);
        }
    }

    private void SeedDefaults()
    {
        InsertResolvedNames(_defaultNames);
    }

    private void InsertResolvedNames(IEnumerable<string> names)
    {
        var insertionIndex = 0;
        foreach (var name in names)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                _ribbon.GetItemByName(name) is not ICanAddToQuickAccess item ||
                !item.CanAddToQuickAccess)
            {
                continue;
            }

            _ribbon.Qat.Items.Insert(Math.Min(insertionIndex, _ribbon.Qat.Items.Count), item);
            insertionIndex++;
        }
    }

    private bool TryCreateAddress(string section, out RibbonSettingsAddress address)
    {
        var user = _userProvider();
        if (string.IsNullOrWhiteSpace(user))
        {
            address = default;
            return false;
        }

        address = CreateAddress(section, _tenantProvider() ?? string.Empty, user);
        return true;
    }

    private RibbonSettingsAddress CreateAddress(string section, string tenant, string user)
    {
        return new RibbonSettingsAddress(tenant, Area, user, section, _applicationId);
    }

    private static string? GetItemIdentity(ICanAddToQuickAccess item)
    {
        return item is Control control ? RibbonItem.GetIdentity(control) : null;
    }
}
