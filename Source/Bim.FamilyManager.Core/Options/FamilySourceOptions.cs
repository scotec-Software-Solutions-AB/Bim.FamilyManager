using Bim.FamilyManager.Core.Abstractions.Options;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Bim.FamilyManager.Core.Options;

/// <summary>
///     Represents the base configuration options for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This abstract class provides common properties and functionality for configuring different types of family sources.
///     Derived classes must be annotated with <see cref="FamilySourceOptionsAttribute" /> to declare their
///     <see cref="Type" /> key. This ensures the type identifier is explicit and rename-safe.
/// </remarks>
public abstract class FamilySourceOptions : IFamilySourceOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceOptions" /> class.
    /// </summary>
    /// <remarks>
    ///     Reads the <see cref="Type" /> identifier from the <see cref="FamilySourceOptionsAttribute" /> applied to the
    ///     concrete derived class. Throws <see cref="InvalidOperationException" /> if the attribute is missing, ensuring
    ///     that every family source options class is explicitly registered with a stable type key.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the concrete class is not annotated with <see cref="FamilySourceOptionsAttribute" />.
    /// </exception>
    protected FamilySourceOptions()
    {
        if (Id == Guid.Empty)
        {
            Id = Guid.NewGuid();
        }

        var attribute = GetType().GetCustomAttributes(typeof(FamilySourceOptionsAttribute), inherit: false)
                                 .OfType<FamilySourceOptionsAttribute>()
                                 .FirstOrDefault()
                        ?? throw new InvalidOperationException(
                            $"'{GetType().Name}' must be annotated with [{nameof(FamilySourceOptionsAttribute)}] to declare its source type key.");

        Type = attribute.OptionsName;
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
    ///     A <see cref="string" /> representing the type of the family source. Derived from the
    ///     <see cref="FamilySourceOptionsAttribute.OptionsName" /> declared on the concrete class.
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
