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

    /// <summary>
    ///     Gets or sets the unique identifier for the family source options.
    /// </summary>
    /// <value>
    ///     A <see cref="Guid" /> representing the unique identifier.
    /// </value>
    public Guid Id { get; init; }

    /// <summary>
    ///     Gets or sets the type of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the type of the family source.
    /// </value>
    public string Type { get; set; }

    /// <summary>
    ///     Gets or sets the name of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family source.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source is active; otherwise, <c>false</c>.
    /// </value>
    public bool IsActive { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is editable.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source is editable; otherwise, <c>false</c>.
    /// </value>
    public bool IsEditable { get; set; } = true;
}
