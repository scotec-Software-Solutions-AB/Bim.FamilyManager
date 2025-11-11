using System.Windows.Input;
using Bim.FamilyManager.Abstractions.Options;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing settings of a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This interface provides properties and methods to handle the configuration, state, and commands
///     related to a specific family source. It is designed to be implemented by view models that manage
///     specific types of family sources.
/// </remarks>
public interface IFamilySourceSettingsViewModel : IViewModel
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="IFamilySourceSettingsViewModel" />.
    /// </summary>
    /// <param name="options">The options associated with the family source.</param>
    /// <returns>An instance of <see cref="IFamilySourceSettingsViewModel" />.</returns>
    public delegate IFamilySourceSettingsViewModel Factory(IFamilySourceOptions options);

    /// <summary>
    ///     Gets or sets the name of the family source.
    /// </summary>
    /// <remarks>
    ///     This property represents the display name of the family source, which is used to identify it
    ///     within the Revit Family Manager. Changes to this property may affect the state of the view model,
    ///     such as marking it as modified.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets the source identifier or description associated with the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the source of the family, which may include
    ///     a unique identifier, path, or descriptive information.
    /// </value>
    /// <remarks>
    ///     This property is typically used to display or reference the origin of the family source
    ///     in the Revit Family Manager. It is expected to be implemented by derived classes
    ///     to provide specific source details.
    /// </remarks>
    public string Source { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is currently active.
    /// </summary>
    /// <remarks>
    ///     This property determines the active state of the family source in the Revit Family Manager.
    ///     It can be used to enable or disable specific functionality or visibility of the family source.
    /// </remarks>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the current family source settings can be edited.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source settings are editable; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to determine if modifications to the family source settings are allowed.
    /// </remarks>
    public bool IsEditable { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the settings of the family source have been modified.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings have been modified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is typically used to track changes made to the settings of a family source
    ///     and can be utilized to determine whether actions such as saving or resetting are necessary.
    /// </remarks>
    public bool IsModified { get; }

    /// <summary>
    ///     Gets the configuration options for the family source.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the settings and attributes associated with the family source.
    ///     It is typically used to retrieve or modify the configuration of a specific family source type.
    /// </remarks>
    public IFamilySourceOptions FamilySourceOptions { get; }

    /// <summary>
    ///     Resets the settings of the family source to their default or initial state.
    /// </summary>
    /// <remarks>
    ///     Implementations of this method should define the specific logic to revert the settings
    ///     to their original or default configuration. This is typically used to discard changes
    ///     and restore the initial state of the family source settings.
    /// </remarks>
    public void Reset();

    public bool CanApply();

    public void Apply();
    
    public string TypeName { get; }
}
