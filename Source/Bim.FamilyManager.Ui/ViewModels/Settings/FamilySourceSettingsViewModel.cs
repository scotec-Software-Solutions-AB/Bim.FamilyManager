using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     Represents the base view model for managing settings of a family source in the Revit Family Manager.
/// </summary>
/// <typeparam name="TOptions">
///     The type of the family source options, which must implement
///     <see cref="Abstractions.Options.IFamilySourceOptions" />.
/// </typeparam>
/// <remarks>
///     This abstract class provides common functionality for managing and applying settings related to a family source.
///     It includes properties for tracking the state of the settings, commands for applying or canceling changes,
///     and methods for resetting or validating the settings.
/// </remarks>
public abstract class FamilySourceSettingsViewModel<TOptions> : ViewModel, IFamilySourceSettingsViewModel
    where TOptions : class, IFamilySourceOptions
{
    private bool _isActive;
    private bool _isEditable;
    private bool _isModified;
    private string _name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSettingsViewModel{TOptions}" /> class.
    /// </summary>
    /// <param name="options">
    ///     The family source options of type <typeparamref name="TOptions" />.
    ///     These options must implement <see cref="Abstractions.Options.IFamilySourceOptions" />.
    /// </param>
    /// <remarks>
    ///     This constructor initializes the view model with the provided options and sets up the commands for applying and
    ///     canceling changes.
    ///     It also initializes the state of the view model based on the provided options.
    /// </remarks>
    protected FamilySourceSettingsViewModel(TOptions options)
    {
        Options = options;

        _name = options.Name;
        _isActive = options.IsActive;
        _isEditable = options.IsEditable;
    }

    public Guid Id => Options.Id;

    /// <summary>
    ///     Gets the options associated with the family source settings.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the specific options of the family source,
    ///     which implement the <see cref="Abstractions.Options.IFamilySourceOptions" /> interface.
    ///     These options include properties such as
    ///     <see cref="IFamilySourceOptions.Name" />,
    ///     <see cref="IFamilySourceOptions.IsActive" />,
    ///     and <see cref="IFamilySourceOptions.IsEditable" />.
    /// </remarks>
    protected TOptions Options { get; }

    /// <summary>
    ///     Gets or sets the name of the family source.
    /// </summary>
    /// <remarks>
    ///     This property represents the name associated with the family source settings.
    ///     It is bound to the UI and updates dynamically when modified. Changes to this property
    ///     mark the settings as modified and trigger the necessary command updates.
    /// </remarks>
    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets the source identifier or path associated with the family source settings.
    /// </summary>
    /// <value>
    ///     A string representing the source of the family settings, such as a file path or identifier.
    /// </value>
    /// <remarks>
    ///     The specific implementation of this property depends on the derived class.
    ///     For example, in <see cref="DirectorySourceSettingsViewModel" />,
    ///     this property returns the directory path.
    /// </remarks>
    public abstract string Source { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source is active.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source is active; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property reflects the active state of the family source and is used to determine
    ///     whether the source should be included in operations. Modifying this property will
    ///     also mark the settings as modified.
    /// </remarks>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            SetProperty(ref _isActive, value);
            IsModified = true;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source settings can be edited.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings are editable; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property reflects the editable state of the family source settings and is typically used
    ///     to enable or disable editing functionality in the user interface.
    /// </remarks>
    public bool IsEditable
    {
        get => _isEditable;
        set => SetProperty(ref _isEditable, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the settings have been modified.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the settings have been modified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to track changes made to the settings. When a property
    ///     affecting the settings is updated, this property is typically set to <c>true</c>.
    /// </remarks>
    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    /// <summary>
    ///     Gets the family source options associated with this view model.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     that represents the configuration options for the family source.
    /// </value>
    /// <remarks>
    ///     This property provides access to the underlying options used to configure the family source.
    ///     It is typically used to retrieve or manipulate the settings specific to the family source type.
    /// </remarks>
    public IFamilySourceOptions FamilySourceOptions => Options;

    /// <summary>
    ///     Resets the settings of the family source to their default or initial state.
    /// </summary>
    /// <remarks>
    ///     This method is intended to be overridden in derived classes to provide specific reset logic
    ///     for the corresponding family source settings.
    /// </remarks>
    public void Reset()
    {
        OnReset();
    }

    public void Apply()
    {
        OnApply();
    }

    public abstract string TypeName { get; }

    /// <summary>
    ///     Determines whether the current settings of the family source can be applied.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the settings can be applied; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method evaluates the validity of the current settings, such as ensuring that
    ///     the <see cref="Name" /> property is not null, empty, or whitespace. Derived classes
    ///     can override this method to include additional validation logic specific to their
    ///     implementation.
    /// </remarks>
    public virtual bool CanApply()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    /// <summary>
    ///     Applies the current settings to the associated family source options.
    /// </summary>
    /// <remarks>
    ///     This method updates the <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     with the current values of the settings. It is intended to be overridden in derived classes
    ///     to include additional logic specific to the type of family source being managed.
    /// </remarks>
    protected virtual void OnApply()
    {
        Options.Name = Name;
        Options.IsActive = IsActive;
        Options.IsEditable = IsEditable;
    }

    /// <summary>
    ///     Handles the cancellation of changes made to the family source settings.
    /// </summary>
    /// <remarks>
    ///     This method reverts the settings to their original state by restoring the values of
    ///     <see cref="Name" />, <see cref="IsActive" />, and <see cref="IsEditable" /> properties
    ///     from the associated <see cref="Options" />. It also resets the <see cref="IsModified" />
    ///     property to <c>false</c>.
    /// </remarks>
    protected virtual void OnReset()
    {
        Name = Options.Name;
        IsActive = Options.IsActive;
        IsEditable = Options.IsEditable;
        IsModified = false;
    }
}
