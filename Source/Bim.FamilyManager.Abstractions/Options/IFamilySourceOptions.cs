namespace Bim.FamilyManager.Abstractions.Options;

/// <summary>
///     Represents configuration options for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     Implement this interface to define options for different family source types, such as directory-based or
///     database-based sources.
///     Common properties include identification, type, name, activation, and editability.
/// </remarks>
public interface IFamilySourceOptions
{
    /// <summary>
    ///     Delegate for creating <see cref="IFamilySourceOptions" /> instances based on a key.
    /// </summary>
    /// <param name="key">A unique key identifying the family source configuration.</param>
    /// <returns>An instance of <see cref="IFamilySourceOptions" />.</returns>
    public delegate IFamilySourceOptions Factory(string key);

    /// <summary>
    ///     Gets the unique identifier for the family source configuration.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Gets or sets the type identifier of the family source.
    /// </summary>
    /// <remarks>
    ///     Used to distinguish the implementation or configuration of the family source (e.g., "Directory", "Database").
    ///     This value is typically used in factories or dependency injection to resolve the correct source.
    /// </remarks>
    string Type { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the family source.
    /// </summary>
    /// <remarks>
    ///     The name is shown in the UI and used to differentiate between sources.
    /// </remarks>
    string Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is currently active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the source is active and should be included in operations; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Inactive sources may be hidden or excluded from processing and UI lists.
    /// </remarks>
    bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source configuration is editable.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the source can be modified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Controls whether users can change the configuration of the source in the UI.
    /// </remarks>
    bool IsEditable { get; set; }
}
