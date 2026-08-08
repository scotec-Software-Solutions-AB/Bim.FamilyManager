namespace Bim.FamilyManager.Core.Logic;

/// <summary>
///     Represents metadata information for a family, including description, version, modification details, and author.
/// </summary>
public sealed record FamilyMetadata
{
    /// <summary>
    ///     Gets the description of the family.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Gets the version of the family.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    ///     Gets the date and time when the family was last modified.
    /// </summary>
    public required DateTime LastModified { get; init; }

    /// <summary>
    ///     Gets the name of the user who last modified the family.
    /// </summary>
    public required string ModifiedBy { get; init; }
}
