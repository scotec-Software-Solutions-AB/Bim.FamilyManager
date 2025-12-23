using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     Represents the view model for a Revit family symbol in the UI layer.
/// </summary>
/// <remarks>
///     This class extends <see cref="Ui.ViewModels.FamilySymbolViewModel" /> and implements
///     <see cref="IFamilySymbolViewModel" />.
///     It provides a factory delegate for instantiation and encapsulates the logic for managing a Revit family symbol
///     within the UI.
/// </remarks>
public class FamilySymbolViewModel : Ui.ViewModels.FamilySymbolViewModel, IFamilySymbolViewModel
{
    /// <summary>
    ///     Factory delegate for creating instances of <see cref="FamilySymbolViewModel" />.
    /// </summary>
    /// <param name="familySymbol">The <see cref="IRevitFamilySymbol" /> to be managed by the view model.</param>
    /// <returns>A new instance of <see cref="FamilySymbolViewModel" />.</returns>
    /// <remarks>
    ///     This delegate is used for dependency injection and dynamic creation of family symbol view models.
    /// </remarks>
    public delegate FamilySymbolViewModel Factory(IRevitFamilySymbol familySymbol);

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySymbolViewModel" /> class.
    /// </summary>
    /// <param name="familySymbol">
    ///     The <see cref="IRevitFamilySymbol" /> instance representing the Revit family symbol to be
    ///     encapsulated by this view model.
    /// </param>
    /// <remarks>
    ///     This constructor passes the provided <paramref name="familySymbol" /> to the base class,
    ///     enabling access to its properties and functionality within the UI context.
    /// </remarks>
    public FamilySymbolViewModel(IRevitFamilySymbol familySymbol) : base(familySymbol)
    {
    }
}
