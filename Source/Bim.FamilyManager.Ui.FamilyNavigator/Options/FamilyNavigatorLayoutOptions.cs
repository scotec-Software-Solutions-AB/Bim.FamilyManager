using Bim.FamilyManager.Core.Options;

namespace Bim.FamilyManager.Ui.FamilyNavigator.Options;

/// <summary>
///     Represents the layout options specific to the family navigator UI of the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class extends the <see cref="Bim.FamilyManager.Core.Options.LayoutOptions" /> base class
///     and implements the <see cref="Bim.FamilyManager.Core.Abstractions.Options.ILayoutOptions" /> interface.
///     It is decorated with the <see cref="Bim.FamilyManager.Core.Options.LayoutOptionsAttribute" />
///     to specify the associated options name as "FamilyNavigatorLayout".
///     This class provides configuration for family navigator layout settings used in the UI.
/// </remarks>
[LayoutOptions(OptionsName = "FamilyNavigatorLayout")]
public class FamilyNavigatorLayoutOptions : LayoutOptions
{
}
