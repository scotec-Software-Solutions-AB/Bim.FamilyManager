namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Provides constant values used throughout the Bim.FamilyManager namespace.
/// </summary>
/// <remarks>
///     Contains static members representing fixed values, such as unique identifiers, that are utilized in various
///     components of the Family Manager application.
/// </remarks>
public static class Constants
{
    /// <summary>
    ///     Gets the globally unique identifier (GUID) for the Family Manager pane in the Revit application.
    /// </summary>
    /// <value>
    ///     A <see cref="Guid" /> used to register and reference the dockable pane associated with the Family Manager in Revit.
    /// </value>
    public static Guid PaneId => new("554CF631-9D8F-49A3-8754-30DAE5076355");

    /// <summary>
    ///     Gets the globally unique identifier (GUID) for the Family Manager application.
    /// </summary>
    /// <value>
    ///     A <see cref="Guid" /> used to identify the Family Manager application instance.
    /// </value>
    public static Guid ApplicationId => new("767167BD-79B6-4B62-A8DE-D0C32E217B75");
}
