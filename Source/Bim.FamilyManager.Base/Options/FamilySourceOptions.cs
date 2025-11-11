using Bim.FamilyManager.Abstractions.Options;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Represents the base configuration options for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This abstract class provides common properties and functionality for configuring different types of family sources.
///     Derived classes can extend this base class to include additional properties specific to their respective family
///     source types.
/// </remarks>
public abstract class FamilySourceOptions : IFamilySourceOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceOptions" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets the <see cref="Type" /> property to the name of the derived class,
    ///     excluding the "Options" suffix. It ensures that the <see cref="Type" /> property is initialized
    ///     with a meaningful value based on the specific implementation.
    /// </remarks>
    protected FamilySourceOptions()
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }
        Type = GetType().Name.Replace("Options", "");
    }

    public Guid Id { get; init; }

    /// <inheritdoc />
    public string Type { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public bool IsEditable { get; set; } = true;
}
