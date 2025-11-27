using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Defines the contract for a view model representing a panel associated with a family source in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Implementations of this interface provide the logic and data required to display and interact with a specific
///     family source panel in the UI.
///     The panel view model is typically used to present source-specific content, controls, or configuration.
/// </remarks>
public interface IFamilySourcePanelViewModel : IViewModel
{
    /// <summary>
    ///     Delegate for creating an instance of <see cref="IFamilySourcePanelViewModel" /> using a given family source.
    /// </summary>
    /// <param name="familySource">The <see cref="IFamilySource" /> to associate with the panel view model.</param>
    /// <returns>
    ///     An instance of <see cref="IFamilySourcePanelViewModel" /> for the specified family source, or <c>null</c> if
    ///     not applicable.
    /// </returns>
    public delegate IFamilySourcePanelViewModel? Factory(IFamilySource familySource);
}
