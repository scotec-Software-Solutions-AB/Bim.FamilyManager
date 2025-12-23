using Bim.FamilyManager.Base.Options;

namespace Bim.FamilyManager.Ui.FamilyExplorer.Options;

/// <summary>
///     Represents the standard layout options for configuring the appearance or behavior of a layout
///     in the Revit Family Manager UI.
/// </summary>
/// <remarks>
///     This class derives from <see cref="LayoutOptions" /> and is used to provide a specific set of
///     layout configuration options identified by the "StandardLayout" key. It can be extended to
///     include additional properties or behaviors specific to the standard layout.
/// </remarks>
[LayoutOptions(OptionsName = "StandardLayout")]
public class StandardLayoutOptions : LayoutOptions
{
}
