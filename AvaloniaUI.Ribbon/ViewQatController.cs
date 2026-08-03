using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using AvaloniaUI.Ribbon.Contracts;

namespace AvaloniaUI.Ribbon;

public sealed class ViewQatController
{
    public const string Section = "QatFunctions";

    private readonly string _formId;
    private readonly Ribbon _ribbon;
    private readonly RibbonQatOwnership _ownership;
    private readonly IRibbonSettingsStore _settings;
    private readonly Func<string?> _tenantProvider;
    private readonly Func<string?> _userProvider;
    private IReadOnlyList<StashedItem>? _stash;

    public ViewQatController(
        Ribbon ribbon,
        IRibbonSettingsStore settings,
        Func<string?> tenantProvider,
        Func<string?> userProvider,
        string formId,
        RibbonQatOwnership ownership)
    {
        _ribbon = ribbon ?? throw new ArgumentNullException(nameof(ribbon));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        _userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
        _ownership = ownership ?? throw new ArgumentNullException(nameof(ownership));
        _formId = string.IsNullOrWhiteSpace(formId)
            ? throw new ArgumentException("A form identifier is required.", nameof(formId))
            : formId;
    }

    public void Load(string? viewName)
    {
        CleanupStaleEntries();
        if (!TryCreateAddress(viewName, out var address))
            return;

        var value = _settings.Read(address);
        if (value is null)
            return;

        foreach (var name in value.Split(';', StringSplitOptions.None))
        {
            if (string.IsNullOrWhiteSpace(name) ||
                _ribbon.GetItemByName(name) is not ICanAddToQuickAccess item ||
                !item.CanAddToQuickAccess ||
                _ribbon.Qat.Items.Contains(item))
            {
                continue;
            }

            _ribbon.Qat.Items.Add(item);
        }
    }

    public void Save(string? viewName)
    {
        if (!TryCreateAddress(viewName, out var address))
            return;

        var names = _ownership.EnumerateViewItems()
            .Where(item => _ribbon.Qat.Items.Contains(item))
            .Select(GetItemName)
            .Where(name => !string.IsNullOrWhiteSpace(name));
        _settings.Write(address, string.Join(';', names));
    }

    public void Delete(string? viewName)
    {
        if (TryCreateAddress(viewName, out var address))
            _settings.Delete(address);
    }

    public void Stash()
    {
        _stash = _ribbon.Qat.Items
            .Select(item => new StashedItem(GetItemName(item), item))
            .ToArray();
    }

    public void Restore()
    {
        if (_stash is null)
            return;

        var restored = new List<ICanAddToQuickAccess>();
        var seen = new HashSet<ICanAddToQuickAccess>(ReferenceEqualityComparer.Instance);

        foreach (var entry in _stash)
        {
            var item = ResolveStashedItem(entry);
            if (item is not null && item.CanAddToQuickAccess && seen.Add(item))
                restored.Add(item);
        }

        foreach (var item in _ribbon.Qat.Items)
        {
            if (seen.Add(item))
                restored.Add(item);
        }

        _ribbon.Qat.Items.Clear();
        foreach (var item in restored)
            _ribbon.Qat.Items.Add(item);

        _stash = null;
    }

    public void AddGeneratedItem(ICanAddToQuickAccess item, RibbonItemDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ArgumentException("Generated ribbon items require a stable name.", nameof(definition));
        if (item is not Control control)
            throw new ArgumentException("Generated QAT items must be controls.", nameof(item));

        if (string.IsNullOrWhiteSpace(control.Name))
            control.Name = definition.Name;
        else if (!string.Equals(control.Name, definition.Name, StringComparison.Ordinal))
            throw new ArgumentException("The item definition name must match the generated control name.", nameof(definition));

        if (definition.QuickStart && item.CanAddToQuickAccess && !_ribbon.Qat.Items.Contains(item))
            _ribbon.Qat.Items.Add(item);
    }

    private void CleanupStaleEntries()
    {
        foreach (var item in _ribbon.Qat.Items.ToArray())
        {
            var name = GetItemName(item);
            var resolved = string.IsNullOrWhiteSpace(name) ? null : _ribbon.GetItemByName(name);
            if (ReferenceEquals(resolved, item))
                continue;

            _ribbon.Qat.Items.Remove(item);
            if (item is IDisposable disposable)
                disposable.Dispose();
        }
    }

    private ICanAddToQuickAccess? ResolveStashedItem(StashedItem entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Name))
            return entry.Item;

        return _ribbon.GetItemByName(entry.Name) as ICanAddToQuickAccess;
    }

    private bool TryCreateAddress(string? viewName, out RibbonSettingsAddress address)
    {
        var user = _userProvider();
        if (string.IsNullOrWhiteSpace(viewName) || string.IsNullOrWhiteSpace(user))
        {
            address = default;
            return false;
        }

        var key = $"{_formId}:{viewName}";
        address = new RibbonSettingsAddress(
            _tenantProvider() ?? string.Empty,
            AppQatController.Area,
            user,
            Section,
            key);
        return true;
    }

    private static string? GetItemName(ICanAddToQuickAccess item)
    {
        return (item as Control)?.Name;
    }

    private sealed record StashedItem(string? Name, ICanAddToQuickAccess Item);
}
