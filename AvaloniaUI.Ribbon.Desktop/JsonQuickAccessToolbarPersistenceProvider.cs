using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AvaloniaUI.Ribbon.Desktop;

public sealed class JsonQuickAccessToolbarPersistenceProvider : IQuickAccessToolbarPersistenceProvider
{
    private readonly object _syncRoot = new();
    private readonly string _filePath;

    public JsonQuickAccessToolbarPersistenceProvider(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<string>? Load(string key)
    {
        lock (_syncRoot)
        {
            var state = ReadState();
            return state.TryGetValue(key, out var itemIds) ? itemIds : null;
        }
    }

    public void Save(string key, IReadOnlyList<string> itemIds)
    {
        lock (_syncRoot)
        {
            var state = ReadState();
            state[key] = new List<string>(itemIds);

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(state);
            File.WriteAllText(_filePath, json);
        }
    }

    private Dictionary<string, List<string>> ReadState()
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, List<string>>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ??
               new Dictionary<string, List<string>>();
    }
}
