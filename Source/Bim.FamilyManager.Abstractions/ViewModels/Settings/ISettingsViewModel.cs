using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Defines the interface for a settings view model used by Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties for identification, display name, selection state, and image sources, as well as methods for
///     retrieving options and initializing the view model.
/// </remarks>
public interface ISettingsViewModel : IViewModel
{
    /// <summary>
    ///     Gets the unique identifier for the settings view model.
    /// </summary>
    /// <remarks>
    ///     Used to uniquely identify a settings view model instance for sorting, comparison, or retrieval.
    /// </remarks>
    int Id { get; }

    /// <summary>
    ///     Gets the display name of the settings view model.
    /// </summary>
    /// <remarks>
    ///     Represents a human-readable name for identifying and displaying the settings view model in the UI.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the settings view model is selected.
    /// </summary>
    /// <value>
    ///     <c>true</c> if selected; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Manages the selection state and updates the image source to reflect selection.
    /// </remarks>
    bool IsSelected { get; set; }

    /// <summary>
    ///     Gets the URI of the image source associated with the settings view model.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri" /> representing the image source, which depends on the selection state.
    /// </value>
    /// <remarks>
    ///     Provides a visual representation, such as an icon, for the settings view model in the UI.
    /// </remarks>
    Uri ImageSource { get; }

    /// <summary>
    ///     Gets the default image source URI for the settings view model.
    /// </summary>
    /// <remarks>
    ///     Provides the URI of the default image when no selection state is applied.
    /// </remarks>
    Uri DefaultImageSource { get; }

    /// <summary>
    ///     Retrieves the options associated with the settings view model.
    /// </summary>
    /// <returns>
    ///     An object representing the options of the settings view model.
    /// </returns>
    /// <remarks>
    ///     Returns the configuration or state of the settings view model for saving or processing.
    /// </remarks>
    object GetOptions();

    /// <summary>
    ///     Initializes the settings view model.
    /// </summary>
    /// <remarks>
    ///     Prepares the settings view model for use, such as setting default values or loading resources.
    /// </remarks>
    void Initialize();
}
