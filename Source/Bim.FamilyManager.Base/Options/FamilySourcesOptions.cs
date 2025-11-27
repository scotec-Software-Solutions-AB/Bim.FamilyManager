using Bim.FamilyManager.Abstractions.Options;
using Scotec.Extensions.Utilities.Configuration;

namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the configuration options for family sources in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class is used to define and manage the settings related to family sources.
///     It is associated with the "FamilySources" settings section through the <see cref="SettingsSectionAttribute" />.
/// </remarks>
[SettingsSection(SectionName = "FamilySources")]
public class FamilySourcesOptions
{
    /// <summary>
    ///     Gets or sets the collection of family source options.
    /// </summary>
    /// <remarks>
    ///     This property holds a list of <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     that represent the configuration for various family sources. These sources can be used to manage
    ///     and organize family data within the application.
    /// </remarks>
    public List<IFamilySourceOptions> Sources { get; set; } = [];
}
