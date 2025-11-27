using Bim.FamilyManager.Abstractions.Options;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Defines the contract for a view model that manages settings for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     Implement this interface to provide properties, events, and methods for configuring, tracking, and applying
///     settings
///     related to a specific family source. It supports state management, editability, and change notification.
/// </remarks>
public interface IFamilySourceSettingsViewModel : IViewModel
{
    /// <summary>
    ///     Delegate for creating <see cref="IFamilySourceSettingsViewModel" /> instances using the specified options.
    /// </summary>
    /// <param name="options">The <see cref="IFamilySourceOptions" /> used to configure the family source settings view model.</param>
    /// <returns>An instance of <see cref="IFamilySourceSettingsViewModel" /> configured with the provided options.</returns>
    public delegate IFamilySourceSettingsViewModel Factory(IFamilySourceOptions options);

    /// <summary>
    ///     Gets the display name of the family source.
    /// </summary>
    /// <remarks>
    ///     Used to identify the family source within the Revit Family Manager UI. Changing this property may mark the view
    ///     model as modified.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets a string that identifies or describes the source of the family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> containing a unique identifier, path, or descriptive information about the family source.
    /// </value>
    /// <remarks>
    ///     Used for display or reference purposes to indicate the origin of the family source.
    /// </remarks>
    public string Source { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is currently active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source is active; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Controls the enabled state and visibility of the family source in the UI and related operations.
    /// </remarks>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source settings can be edited.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings are editable; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Determines if modifications to the family source settings are permitted.
    /// </remarks>
    public bool IsEditable { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the settings of the family source have been modified.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings have been changed since initialization or last reset; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     Used to track changes and determine if actions such as saving or resetting are required.
    /// </remarks>
    public bool IsModified { get; }

    /// <summary>
    ///     Gets the configuration options associated with the family source.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFamilySourceOptions" /> containing the settings and attributes for the family source.
    /// </value>
    /// <remarks>
    ///     Provides access to the underlying options for retrieving or updating the configuration of the family source.
    /// </remarks>
    public IFamilySourceOptions FamilySourceOptions { get; }

    /// <summary>
    ///     Gets the type name of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the type or category of the family source.
    /// </value>
    public string TypeName { get; }

    /// <summary>
    ///     Occurs when the settings of the family source have been modified.
    /// </summary>
    event EventHandler Modified;

    /// <summary>
    ///     Resets the settings of the family source to their default or initial state.
    /// </summary>
    /// <remarks>
    ///     Implementations should revert all changes and restore the original configuration.
    /// </remarks>
    public void Reset();

    /// <summary>
    ///     Determines whether the current settings can be applied.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the settings can be applied; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Validates the state before applying changes.
    /// </remarks>
    public bool CanApply();

    /// <summary>
    ///     Applies the current settings to the family source.
    /// </summary>
    /// <remarks>
    ///     Implementations should persist changes and update the configuration as needed.
    /// </remarks>
    public void Apply();
}
