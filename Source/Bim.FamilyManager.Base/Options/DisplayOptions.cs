using Bim.FamilyManager.Abstractions.Options;
using Scotec.Extensions.Utilities.Configuration;

namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the display-related configuration options for the Family Manager application.
/// </summary>
/// <remarks>
///     This class is marked with the <see cref="SettingsSectionAttribute" />
///     to associate it with the "Display" settings section. It provides properties to configure
///     various aspects of the display behavior within the application.
/// </remarks>
[SettingsSection(SectionName = "Display")]
public class DisplayOptions
{
    /// <summary>
    ///     Gets or sets the collection of layout options used to configure the appearance or behavior
    ///     of layouts in the Revit Family Manager.
    /// </summary>
    /// <value>
    ///     A list of <see cref="ILayoutOptions" /> instances representing the available layout configurations.
    /// </value>
    /// <remarks>
    ///     This property allows customization of the display layouts by specifying a collection of
    ///     layout options. Each layout option defines specific settings for a layout's appearance or behavior.
    /// </remarks>
    public List<ILayoutOptions> Layouts { get; set; } = [];

    /// <summary>
    ///     Gets or sets the key representing the layout configuration for the Family Manager.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> that specifies the key of the selected layout configuration.
    ///     The default value is "FamilyExplorerLayout".
    /// </value>
    /// <remarks>
    ///     This property is used to determine the layout configuration for the Family Manager.
    ///     It is typically set based on user preferences or application settings.
    /// </remarks>
    public string FamilyManagerLayout { get; set; } = "FamilyExplorerLayout";
}
