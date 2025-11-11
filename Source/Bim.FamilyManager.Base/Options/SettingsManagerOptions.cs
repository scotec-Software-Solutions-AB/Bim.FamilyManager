namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the configuration options for managing settings within the application.
/// </summary>
/// <remarks>
///     This class is used to define and store the settings required for the settings manager.
///     It includes properties such as the path to the settings file, which is essential for
///     initializing and managing application-specific configurations.
/// </remarks>
public class SettingsManagerOptions
{
    /// <summary>
    ///     Gets or sets the path to the settings file used by the application.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the file path to the settings file.
    /// </value>
    /// <remarks>
    ///     This property is essential for specifying the location of the settings file
    ///     that contains application-specific configurations. It must be set to a valid
    ///     file path before using the settings manager.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the settings file path is not specified or is invalid during initialization or usage.
    /// </exception>
    public string SettingsFile { get; set; } = string.Empty;
}
