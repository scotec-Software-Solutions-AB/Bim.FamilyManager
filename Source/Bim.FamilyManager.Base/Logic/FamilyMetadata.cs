namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Represents metadata information for a family, including description, version, modification details, and author.
/// </summary>
public class FamilyMetadata
{
    /// <summary>
    ///     Gets or sets the description of the family.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the version of the family.
    /// </summary>
    public required Version Version { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the family was last modified.
    /// </summary>
    public required DateTime LastModified { get; set; }

    /// <summary>
    ///     Gets or sets the name of the user who last modified the family.
    /// </summary>
    public required string ModifiedBy { get; set; }
}
