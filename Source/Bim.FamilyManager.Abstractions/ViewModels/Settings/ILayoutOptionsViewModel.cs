using Bim.FamilyManager.Abstractions.Options;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Defines the view model contract for managing layout options in the Family Manager application.
/// </summary>
/// <remarks>
///     This interface is used by Bim.FamilyManager to provide access to layout configuration, identification, and
///     retrieval functionality.
/// </remarks>
public interface ILayoutOptionsViewModel : IViewModel
{
    /// <summary>
    ///     Delegate for creating <see cref="ILayoutOptionsViewModel" /> instances using a key and layout options.
    /// </summary>
    /// <param name="key">A unique string key identifying the layout options view model.</param>
    /// <param name="options">The <see cref="ILayoutOptions" /> used to configure the view model.</param>
    /// <returns>An instance of <see cref="ILayoutOptionsViewModel" /> configured with the provided key and options.</returns>
    public delegate ILayoutOptionsViewModel Factory(string key, ILayoutOptions options);

    /// <summary>
    ///     Gets the unique key that identifies the layout options associated with this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the key for the layout options.
    /// </value>
    /// <remarks>
    ///     Used to distinguish between different layout configurations in the Family Manager application.
    /// </remarks>
    string Key { get; }

    /// <summary>
    ///     Gets the display name of the layout.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the layout.
    /// </value>
    string LayoutName { get; }

    /// <summary>
    ///     Retrieves the layout options associated with this view model.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="ILayoutOptions" /> representing the configuration for the managed layout.
    /// </returns>
    /// <remarks>
    ///     Returns the options that define the appearance or behavior of the layout in the Family Manager.
    /// </remarks>
    ILayoutOptions GetOptions();
}
