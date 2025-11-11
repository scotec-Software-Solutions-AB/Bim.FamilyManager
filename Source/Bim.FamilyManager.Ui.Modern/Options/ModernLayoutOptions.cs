using Bim.FamilyManager.Base.Options;

namespace Bim.FamilyManager.Ui.Modern.Options;

/// <summary>
///     Represents the layout options specific to the modern UI of the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class extends the <see cref="Bim.FamilyManager.Base.Options.LayoutOptions" /> base class
///     and implements the <see cref="Abstractions.Options.ILayoutOptions" /> interface.
///     It is decorated with the <see cref="Bim.FamilyManager.Base.Options.LayoutOptionsAttribute" />
///     to specify the associated options name as "ModernLayout".
///     This class provides configuration for modern layout settings used in the UI.
/// </remarks>
[LayoutOptions(OptionsName = "ModernLayout")]
public class ModernLayoutOptions : LayoutOptions
{
}
