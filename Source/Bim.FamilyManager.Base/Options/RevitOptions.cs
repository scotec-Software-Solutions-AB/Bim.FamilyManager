namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the Revit-specific configuration options for the Scotec Revit Family Manager application.
/// </summary>
/// <remarks>
///     This class provides properties to configure Revit integration settings, such as the names of tabs and panels
///     within the Revit user interface. It is used to customize how the Family Manager interacts with Revit's UI
///     components.
/// </remarks>
public class RevitOptions
{
    /// <summary>
    ///     Gets or sets the name of the tab in the Revit user interface that is associated with the Scotec Revit Family
    ///     Manager.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the tab. This value can be <c>null</c> if no tab name is
    ///     specified.
    /// </value>
    /// <remarks>
    ///     This property allows customization of the tab name where the Family Manager's tools and features are displayed
    ///     within the Revit UI.
    /// </remarks>
    public string? TabName { get; set; }

    /// <summary>
    ///     Gets or sets the name of the panel within the Revit user interface associated with the Scotec Revit Family Manager.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the panel. This value can be <c>null</c> if no panel name is
    ///     specified.
    /// </value>
    /// <remarks>
    ///     This property is used to define or retrieve the name of the panel where the Family Manager's tools and features
    ///     will be displayed within the Revit UI. It allows for customization of the panel's label to align with user
    ///     preferences or project requirements.
    /// </remarks>
    public string? PanelName { get; set; }
}
