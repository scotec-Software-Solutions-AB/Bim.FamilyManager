namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Specifies metadata for a family source options class in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This attribute is used to associate a unique options name with a class that represents
///     configuration options for a specific type of family source. It enables the identification
///     and management of family source options dynamically at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FamilySourceOptionsAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the unique name associated with the family source options.
    /// </summary>
    /// <remarks>
    ///     This property is used to uniquely identify a specific set of family source options.
    ///     It is essential for dynamically managing and retrieving configuration options
    ///     for different family sources in the Revit Family Manager.
    /// </remarks>
    public required string OptionsName { get; set; }
}
