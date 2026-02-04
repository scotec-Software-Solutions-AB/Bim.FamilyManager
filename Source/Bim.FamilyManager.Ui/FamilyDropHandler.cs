using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Ui.ViewModels;
using Bim.FamilyManager.Ui.Views;
using Microsoft.Extensions.Logging;
using ScaleHQ.DotScreen;
using Point = System.Drawing.Point;

namespace Bim.FamilyManager.Ui;

/// <summary>
///     Represents a handler for managing drag-and-drop operations involving Revit family elements.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IControllableDropHandler" /> interface to handle drop operations
///     within the Revit environment. It utilizes factories for creating drop windows and view models,
///     and integrates logging for debugging purposes.
/// </remarks>
public class FamilyDropHandler : IControllableDropHandler
{
    private readonly FamilyDropViewModel.Factory _dropViewModelFactory;
    private readonly FamilyDropWindow.Factory _dropWindowFactory;
    private readonly IFamilyManager _familyManager;
    private readonly ILogger<FamilyDropHandler> _logger;
    private FamilyDropWindow? _window;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyDropHandler" /> class.
    /// </summary>
    /// <param name="familyManager">
    ///     An instance of <see cref="IFamilyManager" /> responsible for managing Revit family elements.
    /// </param>
    /// <param name="dropWindowFactory">
    ///     A factory for creating instances of <see cref="FamilyDropWindow" /> used to display the drop interface.
    /// </param>
    /// <param name="dropViewModelFactory">
    ///     A factory for creating instances of <see cref="FamilyDropViewModel" /> used to manage the drop operation's data and
    ///     behavior.
    /// </param>
    /// <param name="logger">
    ///     An instance of <see cref="ILogger{FamilyDropHandler}" /> used for logging diagnostic and debugging information.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the dependencies required for handling drag-and-drop operations
    ///     involving Revit family elements. It ensures that the necessary factories and logging mechanisms
    ///     are available for the handler's functionality.
    /// </remarks>
    public FamilyDropHandler(IFamilyManager familyManager, FamilyDropWindow.Factory dropWindowFactory, FamilyDropViewModel.Factory dropViewModelFactory,
                             ILogger<FamilyDropHandler> logger)
    {
        _familyManager = familyManager;
        _dropWindowFactory = dropWindowFactory;
        _dropViewModelFactory = dropViewModelFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Executes the drag-and-drop operation for a Revit family element.
    /// </summary>
    /// <param name="uiDocument">The <see cref="UIDocument" /> representing the current Revit document context.</param>
    /// <param name="data">
    ///     The data associated with the drop operation. Expected to be of type <see cref="IFamilyViewModel" />.
    ///     If the data is not of the expected type, the method will return without performing any action.
    /// </param>
    /// <remarks>
    ///     This method determines the mouse position, initializes a drop window with the appropriate view model,
    ///     and displays the window at the calculated position. It also logs debug information such as the mouse position
    ///     and screen details to assist in troubleshooting.
    /// </remarks>
    public void Execute(UIDocument uiDocument, object data)
    {
        if (data is not IFamilyViewModel familyViewModel)
        {
            return;
        }

        Interop.GetMousePosition(out var position);
        _logger.LogDebug($"Mouse position: X={position.X}, Y={position.Y}");

        _window = _dropWindowFactory(_dropViewModelFactory(familyViewModel, symbolViewModel => DropAction(symbolViewModel, uiDocument)));
        _window.Loaded += OnWindowOnLoaded;

        var screen = Screen.FromPoint(new Point(position.X, position.Y));
        _logger.LogDebug($"Screen at mouse position: {screen.DeviceName}, Resolution: {screen.Bounds.Width}x{screen.Bounds.Height}");

        var dpi = VisualTreeHelper.GetDpi(_window.Owner);
        _window.WindowStartupLocation = WindowStartupLocation.Manual;
        _window.Left = position.X / dpi.DpiScaleX;
        _window.Top = position.Y / dpi.DpiScaleY;
        _window.ShowDialog();
    }

    /// <summary>
    ///     Determines whether the specified drop operation can be executed.
    /// </summary>
    /// <param name="document">The <see cref="UIDocument" /> representing the current Revit document context.</param>
    /// <param name="data">The data object associated with the drop operation.</param>
    /// <param name="dropViewId">
    ///     The <see cref="Autodesk.Revit.DB.ElementId" /> of the view where the drop operation is
    ///     occurring.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the drop operation can be executed; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method checks if the provided <paramref name="data" /> is of type <see cref="FamilyViewModel" />,
    ///     which is required for the drop operation to proceed.
    /// </remarks>
    public bool CanExecute(UIDocument document, object data, ElementId dropViewId)
    {
        return data is IFamilyViewModel;
    }

    /// <summary>
    ///     Handles the <see cref="FrameworkElement.Loaded" /> event for the drop window.
    /// </summary>
    /// <param name="sender">The source of the event, typically the <see cref="FamilyDropWindow" /> instance.</param>
    /// <param name="args">The event data associated with the <see cref="FrameworkElement.Loaded" /> event.</param>
    /// <remarks>
    ///     This method adjusts the position of the drop window to ensure it is properly centered at the mouse position
    ///     and remains within the bounds of the Revit application window. It also logs the new position of the drop window.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the drop window is not initialized before the event is handled.
    /// </exception>
    private void OnWindowOnLoaded(object sender, RoutedEventArgs args)
    {
        if (_window is null)
        {
            const string message = "Drop-Window is not initialized.";
            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        const int borderOffset = 12;
        if (sender is FamilyDropWindow window)
        {
            //LogWindowInfo(window);

            Interop.GetWindowRect(window.Owner, out var rectRevitWindow);
            Interop.GetWindowRect(window, out var rectDropWindow);
            var dpiInfoRevit = VisualTreeHelper.GetDpi(_window.Owner);

            var offsetX = rectDropWindow.Width / -2;
            var offsetY = rectDropWindow.Height / -2;

            // Center the window at mouse position.
            rectDropWindow.Offset(offsetX, offsetY);

            var positionX = rectDropWindow.Left;
            var positionY = rectDropWindow.Top;

            // Parts of the window can be outside the screen (outside Revit), so we need to adjust the position.
            if (rectDropWindow.Bottom > rectRevitWindow.Bottom)
            {
                positionY = rectRevitWindow.Bottom - rectDropWindow.Height - borderOffset;
            }

            if (rectDropWindow.Top < rectRevitWindow.Top)
            {
                positionY = rectRevitWindow.Top + borderOffset;
            }

            if (rectDropWindow.Left < rectRevitWindow.Left)
            {
                positionX = rectRevitWindow.Left + borderOffset;
            }

            if (rectDropWindow.Right > rectRevitWindow.Right)
            {
                positionX = rectRevitWindow.Right - rectDropWindow.Width - borderOffset;
            }

            _logger.LogDebug($"New Drop-Window position: X={positionX}, Y={positionY}");

            window.Top = positionY / dpiInfoRevit.DpiScaleX;
            window.Left = positionX / dpiInfoRevit.DpiScaleY;
        }
    }

    /// <summary>
    ///     Handles the drop action for a Revit family symbol within the Revit environment.
    /// </summary>
    /// <param name="symbolViewModel">
    ///     The view model representing the Revit family symbol to be processed during the drop action.
    /// </param>
    /// <param name="uiDocument">
    ///     The <see cref="Autodesk.Revit.UI.UIDocument" /> representing the current Revit document where the drop action is
    ///     performed.
    /// </param>
    /// <remarks>
    ///     This method is responsible for closing the drop window, initiating a transaction to load the family,
    ///     locating the appropriate family symbol, and placing the symbol in the Revit document.
    ///     If the symbol cannot be placed in the current view, an error message is displayed.
    /// </remarks>
    private void DropAction(IFamilySymbolViewModel symbolViewModel, UIDocument uiDocument)
    {
        if (_window is null)
        {
            return;
        }

        _window!.Close();

        using var transaction = new Transaction(uiDocument.Document, "Load Family");
        transaction.Start();

        //var family = LoadFamily(symbolViewModel.FamilySymbol.Family, uiDocument.Document);
        //var symbol = FindFamilySymbol(family, symbolViewModel.FamilySymbol.Name);

        //TODO: Family may not have symbols. In that case, we need to add the family.

        if (!_familyManager.TryLoadFamilySymbol(symbolViewModel.FamilySymbol, uiDocument.Document, out var familySymbol))
        {
            transaction.RollBack();
            return;
        }

        transaction.Commit();

        if (!uiDocument.CanPlaceElementType(familySymbol))
        {
            TaskDialog.Show("Error", "The selected symbol cannot be placed in the current view.");
        }
        //var placementTypes = uiDocument.GetPlacementTypes(symbol, uiDocument.Document.ActiveView);

        if (uiDocument.CanPlaceElementType(familySymbol))
        {
            //var placementTypes = uiDocument.GetPlacementTypes(symbol, uiDocument.Document.ActiveView);
            PlaceSymbol(familySymbol, uiDocument);
        }
        else
        {
            TaskDialog.Show("Error", "The selected symbol cannot be placed in the current view.");
        }
    }

    /// <summary>
    ///     Places a specified Revit family symbol in the active view of the given Revit document.
    /// </summary>
    /// <param name="symbol">
    ///     The <see cref="Autodesk.Revit.DB.FamilySymbol" /> to be placed in the active view. This represents the family type
    ///     to be instantiated.
    /// </param>
    /// <param name="uiDocument">
    ///     The <see cref="Autodesk.Revit.UI.UIDocument" /> representing the current Revit document context in which the symbol
    ///     will be placed.
    /// </param>
    /// <remarks>
    ///     This method posts a request to Revit to place the specified family symbol in the active view.
    ///     It assumes that the symbol is valid for placement in the current view.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="symbol" /> or <paramref name="uiDocument" /> is <c>null</c>.
    /// </exception>
    private static void PlaceSymbol(FamilySymbol symbol, UIDocument uiDocument)
    {
        // TODO: Consider checking if the user has pressed a key.
        // This could help determine whether to only add the family or symbol, or to both add and place them.
        uiDocument.PostRequestForElementTypePlacement(symbol);
    }

    /// <summary>
    ///     Attempts to load a Revit family into the specified document.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="IRevitFamily" /> instance representing the Revit family to be loaded.
    /// </param>
    /// <param name="document">
    ///     The <see cref="Document" /> instance representing the Revit document where the family will be loaded.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the loaded <see cref="Autodesk.Revit.DB.Family" /> object if the operation
    ///     succeeded; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the family was successfully loaded into the document; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method delegates the loading operation to the <see cref="IFamilyManager" />.
    ///     It ensures that the family is properly integrated into the Revit environment, if possible.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="revitFamily" /> or <paramref name="document" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="Autodesk.Revit.Exceptions.InvalidOperationException">
    ///     Thrown if the family cannot be loaded into the specified document.
    /// </exception>
    private bool TryLoadFamily(IRevitFamily revitFamily, Document document, [NotNullWhen(true)] out Family? family)
    {
        return _familyManager.TryLoadFamily(revitFamily, document, out family);
    }

    /// <summary>
    ///     Finds a specific <see cref="Autodesk.Revit.DB.FamilySymbol" /> within a given
    ///     <see cref="Autodesk.Revit.DB.Family" /> by its name.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="Autodesk.Revit.DB.Family" /> object in which to search for the symbol.
    /// </param>
    /// <param name="symbolName">
    ///     The name of the <see cref="Autodesk.Revit.DB.FamilySymbol" /> to locate.
    /// </param>
    /// <returns>
    ///     The <see cref="Autodesk.Revit.DB.FamilySymbol" /> matching the specified name.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when no symbol with the specified name is found in the given family.
    /// </exception>
    /// <remarks>
    ///     If the found symbol is not active, it will be activated before being returned.
    /// </remarks>
    private FamilySymbol FindFamilySymbol(Family family, string symbolName)
    {
        var symbolIds = family.GetFamilySymbolIds();
        foreach (var symbolId in symbolIds)
        {
            var symbol = family.Document.GetElement(symbolId) as FamilySymbol;
            if (symbol!.Name.Equals(symbolName, StringComparison.OrdinalIgnoreCase))
            {
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                }

                return symbol;
            }
        }

        var message = $"Cannot find symbol '{symbolName}' in family '{family.Name}'.";
        _logger.LogError(message);
        throw new InvalidOperationException(message);
    }
}
