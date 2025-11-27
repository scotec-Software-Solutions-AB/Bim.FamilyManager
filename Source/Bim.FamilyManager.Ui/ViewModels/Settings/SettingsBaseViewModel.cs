using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     Serves as the base class for settings view models in the application.
/// </summary>
/// <remarks>
///     This abstract class provides common functionality and properties for settings view models,
///     including support for managing selection state, image sources, and initialization logic.
///     Derived classes are expected to implement specific behavior for their respective settings.
/// </remarks>
public abstract class SettingsBaseViewModel : ViewModel, ISettingsViewModel
{
    private Uri? _image;
    private bool _isSelected;

    /// <summary>
    ///     Gets the unique identifier for the settings view model.
    /// </summary>
    /// <remarks>
    ///     This property is abstract and must be implemented by derived classes to provide
    ///     a specific identifier for each settings view model.
    /// </remarks>
    public abstract int Id { get; }

    /// <summary>
    ///     Gets the name of the settings view model.
    /// </summary>
    /// <remarks>
    ///     This property is abstract and must be implemented by derived classes to provide a specific name
    ///     representing the type of settings managed by the view model.
    /// </remarks>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets the URI of the image source associated with the settings view model.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri" /> representing the image source. If the settings are selected,
    ///     the selection image is returned; otherwise, the default image is returned.
    /// </value>
    /// <remarks>
    ///     The <see cref="ImageSource" /> property dynamically determines the appropriate image
    ///     based on the selection state of the settings. The image is cached for subsequent access.
    /// </remarks>
    public Uri ImageSource
    {
        get => _image ??= IsSelected ? GetSelectionImage() : GetDefaultImage();
        private set => SetProperty(ref _image, value);
    }

    /// <summary>
    ///     Gets the default image source associated with the settings view model.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri" /> representing the default image source for the settings.
    /// </value>
    /// <remarks>
    ///     This property retrieves the default image source by invoking the <see cref="GetDefaultImage" /> method.
    /// </remarks>
    public Uri DefaultImageSource => GetDefaultImage();

    /// <summary>
    ///     Gets or sets a value indicating whether the settings view model is selected.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings view model is selected; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Setting this property updates the <see cref="ImageSource" /> to reflect the selection state.
    /// </remarks>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            ImageSource = value ? GetSelectionImage() : GetDefaultImage();
            SetProperty(ref _isSelected, value);
        }
    }

    /// <summary>
    ///     Retrieves the options object associated with the settings view model.
    /// </summary>
    /// <returns>
    ///     An <see cref="object" /> representing the options for the settings view model.
    /// </returns>
    /// <remarks>
    ///     This method is abstract and must be implemented by derived classes to provide
    ///     the specific options relevant to their settings.
    /// </remarks>
    public abstract object GetOptions();

    /// <summary>
    ///     Initializes the settings view model.
    /// </summary>
    /// <remarks>
    ///     This method invokes <see cref="OnInitialize" /> to execute any custom initialization logic
    ///     defined in derived classes.
    /// </remarks>
    public void Initialize()
    {
        OnInitialize();
    }

    /// <summary>
    ///     Retrieves the default image URI for the settings view model.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri" /> representing the default image associated with the settings view model.
    /// </returns>
    /// <remarks>
    ///     This method is abstract and must be implemented by derived classes to provide a specific
    ///     default image for their respective settings view models.
    /// </remarks>
    protected abstract Uri GetDefaultImage();

    /// <summary>
    ///     Retrieves the URI of the selection image for the settings view model.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri" /> representing the selection image associated with the settings view model.
    /// </returns>
    /// <remarks>
    ///     This method is abstract and must be implemented by derived classes to provide a specific
    ///     selection image for their respective settings view models.
    /// </remarks>
    protected abstract Uri GetSelectionImage();

    /// <summary>
    ///     Executes initialization logic specific to the derived settings view model.
    /// </summary>
    /// <remarks>
    ///     This method is designed to be overridden by derived classes to implement custom initialization logic.
    ///     It is invoked during the initialization process to set up the state or properties of the view model.
    /// </remarks>
    protected abstract void OnInitialize();
}
