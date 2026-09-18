namespace AvaloniaUI.Ribbon.Models;

/// <summary>
/// Order in which groups step down their <see cref="GroupDisplayMode"/> when the ribbon row does not fit.
/// </summary>
public enum RibbonGroupShrinkOrder
{
    /// <summary>The currently widest group steps down first, regardless of its position.</summary>
    WidestFirst = 0,

    /// <summary>
    /// The last group steps down until it reaches its smallest mode, then the group before it, and so on.
    /// Groups on the left keep their layout as long as possible (Office and classic ribbon behavior).
    /// </summary>
    RightToLeft = 1
}
