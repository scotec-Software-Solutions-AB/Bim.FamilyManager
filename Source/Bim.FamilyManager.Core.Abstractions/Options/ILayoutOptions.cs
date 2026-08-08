namespace Bim.FamilyManager.Core.Abstractions.Options;

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
    ///     Gets or sets the unique key that identifies the layout options instance.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Key" /> property serves as an identifier for distinguishing different layout options within the
    ///     Family Manager application.
    ///     It is typically initialized based on the type name of the implementing class or a specific configuration scenario.
    /// </remarks>
    string Key { get; set; }
}
