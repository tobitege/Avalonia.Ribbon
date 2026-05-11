using System.Windows.Input;

namespace AvaloniaUI.Ribbon.Tests;

public class DialogLauncherTests
{
    [Fact]
    public void LegacyCommandProperty_AliasesDialogLauncherCommand()
    {
        var group = new RibbonGroupBox();
        var command = new TestCommand();

        group.Command = command;

        Assert.Same(command, group.DialogLauncherCommand);
        Assert.Same(command, group.Command);
    }

    [Fact]
    public void LegacyCommandParameter_AliasesDialogLauncherCommandParameter()
    {
        var group = new RibbonGroupBox();
        var parameter = new object();

        group.CommandParameter = parameter;

        Assert.Same(parameter, group.DialogLauncherCommandParameter);
        Assert.Same(parameter, group.CommandParameter);
    }

    [Fact]
    public void DialogLauncherProperties_CanBeAssignedDirectly()
    {
        var group = new RibbonGroupBox();
        var command = new TestCommand();
        const string parameter = "launch";

        group.DialogLauncherCommand = command;
        group.DialogLauncherCommandParameter = parameter;

        Assert.Same(command, group.DialogLauncherCommand);
        Assert.Same(command, group.Command);
        Assert.Equal(parameter, group.DialogLauncherCommandParameter);
        Assert.Equal(parameter, group.CommandParameter);
    }

    private sealed class TestCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
