using System;
using System.Collections.Generic;

namespace AvaloniaUI.Ribbon;

public readonly record struct RibbonSettingsAddress(
    string Tenant,
    string Area,
    string User,
    string Section,
    string Key);

public interface IRibbonSettingsStore
{
    string? Read(RibbonSettingsAddress address);

    void Write(RibbonSettingsAddress address, string value);

    void Delete(RibbonSettingsAddress address);
}

public sealed class InMemoryRibbonSettingsStore : IRibbonSettingsStore
{
    private readonly Dictionary<RibbonSettingsAddress, string> _values = new();
    private readonly object _sync = new();

    public string? Read(RibbonSettingsAddress address)
    {
        lock (_sync)
            return _values.TryGetValue(address, out var value) ? value : null;
    }

    public void Write(RibbonSettingsAddress address, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        lock (_sync)
            _values[address] = value;
    }

    public void Delete(RibbonSettingsAddress address)
    {
        lock (_sync)
            _values.Remove(address);
    }
}
