using Bim.FamilyManager.Source.Directory.Options;
using Bim.FamilyManager.Ui.ViewModels.Settings;

namespace Bim.FamilyManager.Source.Directory.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing settings related to a directory-based family source.
/// </summary>
/// <remarks>
///     This class extends
///     <see cref="FamilySourceSettingsViewModel{TOptions}" />
///     with specific functionality for handling directory-based family source options.
/// </remarks>
public class DirectorySourceSettingsViewModel : FamilySourceSettingsViewModel<DirectorySourceOptions>
{
    private string _path;

    public DirectorySourceSettingsViewModel(DirectorySourceOptions options)
        : base(options)
    {
        _path = options.Path;
    }

    /// <summary>
    ///     Gets the source identifier for the directory-based family source.
    /// </summary>
    /// <remarks>
    ///     This property overrides the
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.Source" />
    ///     property to return the value of the <see cref="Path" /> property, which represents the directory path.
    /// </remarks>
    public override string Source => Path;

    /// <summary>
    ///     Gets or sets the directory path associated with the directory-based family source.
    /// </summary>
    /// <remarks>
    ///     This property represents the path to the directory used as the source for family management.
    ///     Modifying this property will mark the settings as modified and trigger necessary updates.
    /// </remarks>
    public string Path
    {
        get => _path;
        set
        {
            SetProperty(ref _path, value);
            IsModified = true;
            OnPropertyChanged(nameof(Source));
        }
    }

    public override string TypeName => "Directory";

    /// <summary>
    ///     Determines whether the settings for the directory-based family source can be applied.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the settings can be applied; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.CanApply" />
    ///     to include additional validation specific to directory-based family sources.
    ///     It ensures that the <see cref="Path" /> property is not null, empty, or whitespace,
    ///     and that the specified directory exists.
    /// </remarks>
    public override bool CanApply()
    {
        return base.CanApply() && !string.IsNullOrWhiteSpace(Path) && System.IO.Directory.Exists(Path);
    }

    /// <summary>
    ///     Applies the current settings of the directory-based family source.
    /// </summary>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.OnApply" />
    ///     to update the <see cref="Options.Path" /> property with the value of the <see cref="Path" /> property.
    /// </remarks>
    /// <exception cref="NotImplementedException">
    ///     Thrown if the base implementation is not properly overridden.
    /// </exception>
    protected override void OnApply()
    {
        base.OnApply();
        Options.Path = Path;
    }

    /// <summary>
    ///     Handles the cancellation of changes made to the directory-based family source settings.
    /// </summary>
    /// <remarks>
    ///     This method overrides
    ///     <see cref="FamilySourceSettingsViewModel{TOptions}.OnReset" />
    ///     to reset the <see cref="Path" /> property to its value in <see cref="Options" />.
    /// </remarks>
    protected override void OnReset()
    {
        base.OnReset();
        Path = Options.Path;
    }
}
