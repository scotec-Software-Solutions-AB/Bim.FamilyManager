using System.Windows.Input;
using System.Windows.Media;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Represents the view model for managing families in the Revit Family Manager application.
/// </summary>
/// <remarks>
///     This interface defines the contract for the Family Manager view model, which includes properties
///     for accessing the application logo, reload command, and a collection of family sources.
/// </remarks>
public interface IFamilyManagerViewModel : IViewModel
{
    /// <summary>
    ///     Gets the logo image representing the application or feature.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the logo, or <c>null</c> if no logo is available.
    /// </value>
    /// <remarks>
    ///     The logo is typically used in the user interface to visually represent the add-in or feature.
    /// </remarks>
    public ImageSource? Logo { get; }

    /// <summary>
    ///     Gets the command that triggers the reload operation for the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This command is used to refresh or reload the data displayed in the Family Manager,
    ///     ensuring that the latest information is retrieved and displayed to the user.
    /// </remarks>
    public ICommand ReloadCommand { get; }

    /// <summary>
    ///     Gets a collection of family sources available in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     Each family source represents a group of families, which may include folders and a selected folder.
    ///     This property is typically used to bind the collection of family sources to the UI for display and interaction.
    /// </remarks>
    public IEnumerable<IFamilySourceViewModel> FamilySources { get; }
}
