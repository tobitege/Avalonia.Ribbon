# Group Containers Developer Guide

This document is the developer reference for:

- `RibbonGroupTriple`
- `RibbonGroupLines`
- `RibbonGroupCluster`

Use it for API defaults, layout behavior, and styling/customization points.

## Source locations

- Base container: `AvaloniaUI.Ribbon/RibbonGroupContainer.cs`
- Triple: `AvaloniaUI.Ribbon/RibbonGroupTriple.cs`
- Lines: `AvaloniaUI.Ribbon/RibbonGroupLines.cs`
- Cluster: `AvaloniaUI.Ribbon/RibbonGroupCluster.cs`
- Fluent default styles: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml`

## Common container API

All three controls inherit from `RibbonGroupContainer`.

| Member | Type | Default | Notes |
| - | - | - | - |
| `DisplayMode` | `GroupDisplayMode` | inherited | Usually driven by parent `RibbonGroupBox`. |
| `MinimumSize` | `RibbonControlSize` | `Small` | Lower clamp for child ribbon control size. |
| `MaximumSize` | `RibbonControlSize` | `Large` | Upper clamp for child ribbon control size. |
| `ItemSpacing` | `double` | `2` | Spacing used by the layout algorithm. |
| `CurrentSize` | `RibbonControlSize` (read-only) | computed | Effective size currently applied to children. |
| `ApplyDisplayMode(...)` | method | n/a | Forces display-mode update and child size refresh. |

## Per-control specifics

### RibbonGroupTriple

File: `AvaloniaUI.Ribbon/RibbonGroupTriple.cs`

| Property | Type | Default | Meaning |
| - | - | - | - |
| `MaxItemsPerColumn` | `int` | `3` | Number of vertical slots before starting a new column. |
| `ItemAlignment` | `RibbonItemAlignment` | `Near` | Horizontal alignment inside each slot (`Near`, `Center`, `Far`). |

Layout model:

- Column-major (fills rows in a column first, then starts the next column).
- Uses normal size resolution (`Large` is allowed, then clamped by min/max).

### RibbonGroupLines

File: `AvaloniaUI.Ribbon/RibbonGroupLines.cs`

| Property | Type | Default | Meaning |
| - | - | - | - |
| `LargeLineCount` | `int` | `2` | Number of rows in large display mode. |
| `SmallLineCount` | `int` | `3` | Number of rows in small display mode. |

Layout model:

- Column-major with line count determined by display mode.
- In large display mode, requested `Large` is intentionally coerced to `Medium` before clamp.

### RibbonGroupCluster

File: `AvaloniaUI.Ribbon/RibbonGroupCluster.cs`

Defaults:

- `MinimumSize = Small`
- `MaximumSize = Medium`
- `ItemSpacing = 0`

Layout model:

- Single horizontal row.
- No internal wrapping.
- In large display mode, requested `Large` is coerced to `Medium`.

Automatically assigned child classes (styling hooks):

- `cluster-first`
- `cluster-middle`
- `cluster-last`
- `cluster-single`

## Styling: where to edit

Global cluster styling is defined here:

- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupContainers.axaml`

This file currently does two important things for clusters:

1. Removes per-button full outlines inside a bank.
2. Draws the bank outline using cluster position classes (`first/middle/last/single`).

If cluster bank borders are too faint, adjust `BorderBrush` there (for example `ThemeBorderLowBrush` -> `ThemeBorderMidBrush`).

## App-level overrides

Add overrides after the ribbon style include in `App.axaml`, or in a specific view (`Ribbon.Styles`) for local scope.

Example: stronger cluster bank border in app scope.

```xaml
<Style Selector="RibbonGroupCluster RibbonButton /template/ Border.RibbonButtonBackgroundBorder, RibbonGroupCluster RibbonDropDownButton /template/ Border.RibbonButtonBackgroundBorder, RibbonGroupCluster RibbonSplitButton /template/ Border.RibbonButtonBackgroundBorder, RibbonGroupCluster RibbonToggleButton /template/ Border.RibbonToggleButtonBackgroundBorder, RibbonGroupCluster SplitButtonControl /template/ Border.RibbonToggleButtonBackgroundBorder">
    <Setter Property="BorderBrush" Value="{DynamicResource ThemeBorderMidBrush}" />
</Style>
```

## Tips and tricks

- Treat each `RibbonGroupCluster` as one bank unit.
- Use `RibbonGroupLines` around clusters to control how many banks appear per row.
- `LargeLineCount` controls rows in large mode.
- `SmallLineCount` controls rows in small mode.
- Keep cluster children `Small`/`Medium` for canonical ribbon density.
- Keep cluster `ItemSpacing=0` for contiguous bank visuals.

## Troubleshooting

Border not visible:

- Ensure selector targets template borders (`/template/ Border...`), not only control `BorderBrush`.
- Ensure style load order places your override after `AvaloniaRibbon.axaml`.
- Ensure controls are actually inside `RibbonGroupCluster`.

Build fails with DLL copy-lock while demos run:

- Stop the running demo process, then rebuild.
