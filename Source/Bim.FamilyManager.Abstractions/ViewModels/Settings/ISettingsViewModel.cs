using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Represents the interface for a settings view model in the application.
/// </summary>
/// <remarks>
///     This interface defines the contract for settings view models, including properties for
///     identification, display name, selection state, and image sources. It also provides methods
///     for retrieving options and initializing the view model.
/// </remarks>
public interface ISettingsViewModel : IViewModel
{
    /// <summary>
    ///     Gets the unique identifier for the settings view model.
    /// </summary>
    /// <remarks>
    ///     This property is used to uniquely identify a settings view model instance.
    ///     It is commonly utilized for sorting, comparison, or retrieval operations.
    /// </remarks>
    int Id { get; }

    /// <summary>
    ///     Gets the display name of the settings view model.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Name" /> property represents a human-readable name that is used
    ///     to identify and display the settings view model in the user interface.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the settings view model is selected.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings view model is selected; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to manage the selection state of the settings view model.
    ///     When set, it updates the <see cref="ImageSource" /> property to reflect the selection state.
    /// </remarks>
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Gets the URI of the image source associated with the settings view model.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri" /> representing the image source. The value depends on the
    ///     <see cref="IsSelected" /> property, returning a selection-specific image if selected,
    ///     or the default image otherwise.
    /// </value>
    /// <remarks>
    ///     This property is used to provide a visual representation of the settings view model,
    ///     such as an icon displayed in the user interface. The image source is dynamically
    ///     determined based on the selection state of the view model.
    /// </remarks>
    public Uri ImageSource { get; }

    /// <summary>
    ///     Gets the default image source associated with the settings view model.
    /// </summary>
    /// <remarks>
    ///     This property provides the URI of the default image that represents the settings view model
    ///     when no specific selection state is applied. It is typically used to display a fallback image
    ///     in the user interface.
    /// </remarks>
    public Uri DefaultImageSource { get; }

    /// <summary>
    ///     Retrieves the options associated with the settings view model.
    /// </summary>
    /// <returns>
    ///     An object representing the options of the settings view model. The exact type and structure
    ///     of the returned object depend on the specific implementation of the view model.
    /// </returns>
    /// <remarks>
    ///     This method is intended to provide a way to extract the configuration or state of the
    ///     settings view model in a format that can be used for saving or processing purposes.
    /// </remarks>
    object GetOptions();

    /// <summary>
    ///     Initializes the settings view model.
    /// </summary>
    /// <remarks>
    ///     This method is responsible for preparing the settings view model for use. It may include
    ///     tasks such as setting default values, loading necessary resources, or performing other
    ///     initialization logic specific to the implementation.
    /// </remarks>
    public void Initialize();
}
