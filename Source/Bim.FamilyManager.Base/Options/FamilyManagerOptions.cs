namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the configuration options for the Family Manager in the Scotec Revit Family Manager application.
/// </summary>
/// <remarks>
///     This class provides properties to configure various aspects of the Family Manager, such as the logo,
///     working directory, and Revit-specific options. It is primarily used to supply configuration data
///     to components that depend on these settings.
/// </remarks>
public class FamilyManagerOptions
{
    /// <summary>
    ///     Gets or sets the logo associated with the Family Manager configuration.
    /// </summary>
    /// <value>
    ///     A string representing the file path or identifier of the logo.
    ///     This value can be <c>null</c> if no logo is specified.
    /// </value>
    /// <remarks>
    ///     The logo is used to visually represent the Family Manager in the application.
    ///     It can be utilized in various UI components or documentation to enhance branding.
    /// </remarks>
    public string? Logo { get; set; }

    /// <summary>
    ///     Gets or sets the working directory used for managing Revit families.
    /// </summary>
    /// <remarks>
    ///     This property specifies the directory where Revit family files are stored and managed.
    ///     It supports environment variable expansion, allowing dynamic configuration of the directory path.
    ///     If the specified directory does not exist, it will be created automatically when accessed.
    /// </remarks>
    /// <value>
    ///     A <see cref="string" /> representing the path to the working directory. Defaults to an empty string.
    /// </value>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Revit-specific configuration options for the Family Manager.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="Base.Options.RevitOptions" /> that contains
    ///     Revit-specific settings, such as the tab and panel names.
    /// </value>
    /// <remarks>
    ///     This property allows customization of Revit integration settings, enabling the Family Manager
    ///     to interact with Revit's user interface components.
    /// </remarks>
    public RevitOptions? RevitOptions { get; set; }
}
