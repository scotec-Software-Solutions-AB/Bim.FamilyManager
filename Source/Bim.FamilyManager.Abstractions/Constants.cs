namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Provides constant values used throughout the Bim.FamilyManager namespace.
/// </summary>
/// <remarks>
///     This class contains static members that represent fixed values, such as identifiers,
///     which are utilized in various components of the Revit Family Manager application.
/// </remarks>
public static class Constants
{
    /// <summary>
    ///     Gets the unique identifier for the Family Manager pane in the Revit application.
    /// </summary>
    /// <remarks>
    ///     This property provides a globally unique identifier (GUID) that is used to register
    ///     and reference the dockable pane associated with the Family Manager in Revit.
    /// </remarks>
    public static Guid PaneId => new("554CF631-9D8F-49A3-8754-30DAE5076355");

    public static Guid ApplicationId => new("767167BD-79B6-4B62-A8DE-D0C32E217B75");
}
