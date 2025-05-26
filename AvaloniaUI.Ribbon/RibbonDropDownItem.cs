using Avalonia.Controls;

using AvaloniaUI.Ribbon.Contracts;

using System;

namespace AvaloniaUI.Ribbon;

public class RibbonDropDownItem : MenuItem, IRibbonCommand
{
    #region Properties

    protected override Type StyleKeyOverride => typeof(RibbonDropDownItem);

    #endregion Properties
}