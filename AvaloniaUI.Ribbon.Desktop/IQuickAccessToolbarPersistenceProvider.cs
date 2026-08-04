using System.Collections.Generic;

namespace AvaloniaUI.Ribbon.Desktop;

public interface IQuickAccessToolbarPersistenceProvider
{
    IReadOnlyList<string>? Load(string key);

    void Save(string key, IReadOnlyList<string> itemIds);
}
