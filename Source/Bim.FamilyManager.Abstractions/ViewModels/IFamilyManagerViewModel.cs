using System.Windows.Input;
using System.Windows.Media;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the view model contract for managing families in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides properties for accessing the application logo, reload command, and the collection of available family
///     sources.
///     Used to coordinate the main Family Manager UI and its data.
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
    ///     Used in the UI to visually represent the Family Manager or add-in.
    /// </remarks>
    ImageSource? Logo { get; }

    /// <summary>
    ///     Gets the command that triggers a reload operation for the Family Manager.
    /// </summary>
    /// <remarks>
    ///     Used to refresh or reload the data displayed in the Family Manager, ensuring the latest information is shown.
    /// </remarks>
    ICommand ReloadCommand { get; }

    /// <summary>
    ///     Gets the collection of family sources available in the Family Manager.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IFamilySourceViewModel}" /> representing the available family sources.
    /// </value>
    /// <remarks>
    ///     Each family source may include folders and a selected folder. This property is typically bound to the UI for
    ///     display and interaction.
    /// </remarks>
    IEnumerable<IFamilySourceViewModel> FamilySources { get; }
}
