namespace Bim.FamilyManager.Abstractions.Options;

/// <summary>
///     Defines the contract for configuration options of a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This interface is implemented by various classes to represent different types of family sources,
///     such as directory-based or database-based sources. It provides common properties that are shared
///     across all family source types.
/// </remarks>
public interface IFamilySourceOptions
{
    public delegate IFamilySourceOptions Factory(string key);

    Guid Id { get; }
    
    /// <summary>
    ///     Gets or sets the type of the family source.
    /// </summary>
    /// <remarks>
    ///     This property identifies the specific type of the family source, which can be used to determine
    ///     the appropriate configuration or behavior for the source. The value is typically used in factory
    ///     methods or dependency injection to resolve the corresponding family source implementation.
    /// </remarks>
    string Type { get; set; }

    /// <summary>
    ///     Gets or sets the name of the family source.
    /// </summary>
    /// <remarks>
    ///     This property represents the display name or identifier for the family source.
    ///     It is used to distinguish between different family sources in the Revit Family Manager.
    /// </remarks>
    string Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source is active; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to determine whether the family source should be included
    ///     in operations such as filtering or processing. Active sources are typically
    ///     prioritized or displayed in the user interface.
    /// </remarks>
    bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is editable.
    /// </summary>
    /// <remarks>
    ///     This property determines if the configuration of the family source can be modified.
    ///     It is used to control the editability of the family source in the Revit Family Manager.
    /// </remarks>
    bool IsEditable { get; set; }
}
