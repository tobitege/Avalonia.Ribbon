using System.Windows.Input;
using AvaloniaUI.Ribbon.Models;

namespace AvaloniaUI.Ribbon.Tests;

public class RecentDocsTests
{
    [Fact]
    public void RibbonMenu_InitializesRecentDocumentsCollection()
    {
        var menu = new RibbonMenu();

        Assert.NotNull(menu.RecentDocuments);
        Assert.Empty(menu.RecentDocuments);
    }

    [Fact]
    public void RecentDocumentCommand_ExecutesAndRaisesEvent()
    {
        var menu = new RibbonMenu();
        var command = new CountingCommand();
        var recent = new RibbonRecentDocument
        {
            Title = "Draft.docx",
            Path = @"C:\Docs\Draft.docx",
            Command = command,
            CommandParameter = "open-draft"
        };

        RibbonRecentDocument? raisedDocument = null;
        menu.RecentDocumentInvoked += (_, document) => raisedDocument = document;
        menu.RecentDocuments.Add(recent);

        menu.RecentDocumentClickCommand.Execute(recent);

        Assert.Equal(1, command.ExecuteCount);
        Assert.Equal("open-draft", command.LastParameter);
        Assert.Same(recent, raisedDocument);
    }

    private sealed class CountingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
