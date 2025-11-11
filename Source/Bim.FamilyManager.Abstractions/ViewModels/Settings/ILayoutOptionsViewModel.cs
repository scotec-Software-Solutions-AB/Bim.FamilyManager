using Bim.FamilyManager.Abstractions.Options;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Represents the view model for layout options in the Family Manager application.
/// </summary>
/// <remarks>
///     This interface provides functionality to manage and retrieve layout options,
///     and includes a factory delegate for creating instances of <see cref="ILayoutOptionsViewModel" />.
/// </remarks>
public interface ILayoutOptionsViewModel : IViewModel
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="ILayoutOptionsViewModel" />.
    /// </summary>
    /// <param name="key">The unique key associated with the layout options view model.</param>
    /// <param name="options">The layout options to be used for configuring the view model.</param>
    /// <returns>An instance of <see cref="ILayoutOptionsViewModel" />.</returns>
    public delegate ILayoutOptionsViewModel Factory(string key, ILayoutOptions options);

    /// <summary>
    ///     Gets the unique identifier for the layout options associated with this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the key that uniquely identifies the layout options.
    /// </value>
    /// <remarks>
    ///     The <c>Key</c> property is used to distinguish between different layout configurations
    ///     in the Family Manager application. It serves as a reference for retrieving or managing
    ///     specific layout options.
    /// </remarks>
    string Key { get; }

    string LayoutName { get; }

    /// <summary>
    ///     Retrieves the layout options associated with the current view model.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="ILayoutOptions" /> representing the configuration
    ///     for the layout managed by this view model.
    /// </returns>
    /// <remarks>
    ///     This method is used to obtain the layout options that define the appearance
    ///     or behavior of a layout in the Revit Family Manager. The returned options
    ///     can be utilized for further customization or persistence.
    /// </remarks>
    ILayoutOptions GetOptions();
}
