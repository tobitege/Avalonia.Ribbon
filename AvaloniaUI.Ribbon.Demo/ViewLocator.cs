using System;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using AvaloniaUI.Ribbon.Demo.ViewModels;

namespace AvaloniaUI.Ribbon.Demo;

public class ViewLocator : IDataTemplate
{
    public bool SupportsRecycling => false;

    public Control? Build(object? data)
    {
        if (data is null) return new TextBlock { Text = "No Data" };
        var name = data?.GetType()?.FullName?.Replace("ViewModel", "View");
        if (string.IsNullOrEmpty(name))
        {
            return new TextBlock { Text = "No View Found" };
        }
        var type = Type.GetType(name);

        if (type != null) return (Control)Activator.CreateInstance(type)!;

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}