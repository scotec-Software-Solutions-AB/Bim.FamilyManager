using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.FamilyExplorer.Options;
using Bim.FamilyManager.Ui.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scotec.Revit;

namespace Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

/// <summary>
///     Provides a view model for a Revit family, enabling interaction with family data, symbols, and related commands.
/// </summary>
/// <remarks>
///     This class extends <see cref="FamilyViewModel{FamilyExplorerLayoutOptions}" /> and customizes symbol view model
///     creation.
///     It manages dependencies for family operations, symbol instantiation, drag-and-drop handling, and logging.
/// </remarks>
public class FamilyViewModel : FamilyViewModel<FamilyExplorerLayoutOptions>
{
    /// <summary>
    ///     Delegate for creating instances of <see cref="FamilyViewModel" />.
    /// </summary>
    /// <param name="family">The <see cref="IRevitFamily" /> to be managed by the view model.</param>
    /// <returns>A new instance of <see cref="FamilyViewModel" />.</returns>
    /// <remarks>
    ///     This delegate is used for dependency injection and dynamic instantiation of family view models.
    /// </remarks>
    public delegate FamilyViewModel Factory(IRevitFamily family);

    private readonly FamilySymbolViewModel.Factory _symbolFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyViewModel" /> class.
    /// </summary>
    /// <param name="family">The <see cref="IRevitFamily" /> instance representing the Revit family to be managed.</param>
    /// <param name="familyManager">The <see cref="IFamilyManager" /> responsible for family operations.</param>
    /// <param name="symbolFactory">A factory delegate for creating <see cref="FamilySymbolViewModel" /> instances.</param>
    /// <param name="dropHandlerFactory">A factory function for creating <see cref="FamilyDropHandler" /> instances.</param>
    /// <param name="layoutOptions">
    ///     An <see cref="IOptionsMonitor{FamilyExplorerLayoutOptions}" /> for monitoring layout
    ///     options.
    /// </param>
    /// <param name="revitTask">The <see cref="RevitTask" /> instance for executing Revit-related tasks.</param>
    /// <param name="logger">The <see cref="ILogger{FamilyViewModel}" /> for logging messages.</param>
    /// <remarks>
    ///     This constructor sets up the view model with the provided dependencies, enabling management of family data,
    ///     symbol instantiation, drag-and-drop handling, and logging.
    /// </remarks>
    public FamilyViewModel(
        IRevitFamily family,
        IFamilyManager familyManager,
        FamilySymbolViewModel.Factory symbolFactory,
        Func<FamilyDropHandler> dropHandlerFactory,
        IOptionsMonitor<FamilyExplorerLayoutOptions> layoutOptions,
        RevitTask revitTask,
        ILogger<FamilyViewModel<FamilyExplorerLayoutOptions>> logger)
        : base(family, familyManager, dropHandlerFactory, layoutOptions, revitTask, logger)
    {
        _symbolFactory = symbolFactory;
    }

    /// <summary>
    ///     Creates a symbol view model for the specified family symbol.
    /// </summary>
    /// <param name="symbol">The <see cref="IRevitFamilySymbol" /> to create a view model for.</param>
    /// <returns>An <see cref="IFamilySymbolViewModel" /> representing the family symbol.</returns>
    /// <remarks>
    ///     This method uses the injected symbol factory to create a view model for the given family symbol.
    ///     It allows customization of symbol view model instantiation.
    /// </remarks>
    protected override IFamilySymbolViewModel CreateSymbolViewModel(IRevitFamilySymbol symbol)
    {
        return _symbolFactory(symbol);
    }
}
