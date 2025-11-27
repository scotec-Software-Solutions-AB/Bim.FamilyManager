namespace Bim.FamilyManager.Abstractions.Options;

/// <summary>
///     Defines configuration options for customizing the appearance or behavior of a layout in the Revit Family Manager.
/// </summary>
/// <remarks>
///     Implement this interface to provide specific layout settings, such as identifiers or parameters, that control how
///     layouts are presented or managed within the application.
/// </remarks>
public interface ILayoutOptions
{
    /// <summary>
    ///     Delegate for creating <see cref="ILayoutOptions" /> instances based on a unique key.
    /// </summary>
    /// <param name="key">A unique string key identifying the layout options to create.</param>
    /// <returns>An instance of <see cref="ILayoutOptions" /> corresponding to the specified key.</returns>
    public delegate ILayoutOptions Factory(string key);

    /// <summary>
    ///     Gets or sets the unique key that identifies the layout options instance.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Key" /> property serves as an identifier for distinguishing different layout options within the
    ///     Family Manager application.
    ///     It is typically initialized based on the type name of the implementing class or a specific configuration scenario.
    /// </remarks>
    string Key { get; set; }
}
