namespace Bim.FamilyManager.Abstractions.Options;

/// <summary>
///     Represents a set of layout options for configuring the appearance or behavior of a layout in the Revit Family
///     Manager.
/// </summary>
public interface ILayoutOptions
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="ILayoutOptions" />.
    /// </summary>
    /// <param name="key">The unique key associated with the layout options to be created.</param>
    /// <returns>An instance of <see cref="ILayoutOptions" /> corresponding to the specified key.</returns>
    public delegate ILayoutOptions Factory(string key);

    /// <summary>
    ///     Gets or sets the unique key associated with the layout options.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Key" /> property serves as an identifier for distinguishing
    ///     different layout options within the Family Manager application. It is
    ///     typically initialized based on the type name of the implementing class.
    /// </remarks>
    string Key { get; set; }
}
