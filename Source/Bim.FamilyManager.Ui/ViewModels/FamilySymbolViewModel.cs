using System.Windows.Media;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Logic;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents the view model for a Revit family symbol, providing access to its properties and functionality.
/// </summary>
/// <remarks>
///     This class encapsulates the logic and data associated with a Revit family symbol.
///     It exposes the family symbol's name and other properties, and provides a factory delegate for creating instances.
/// </remarks>
public abstract class FamilySymbolViewModel : ViewModel, IFamilySymbolViewModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySymbolViewModel" /> class.
    /// </summary>
    /// <param name="familySymbol">
    ///     The <see cref="IRevitFamilySymbol" /> instance representing the Revit family symbol
    ///     to be encapsulated by this view model.
    /// </param>
    /// <remarks>
    ///     This constructor sets the <see cref="FamilySymbol" /> property, allowing access to
    ///     the underlying Revit family symbol's data and functionality.
    /// </remarks>
    protected FamilySymbolViewModel(IRevitFamilySymbol familySymbol)
    {
        FamilySymbol = familySymbol;
    }

    /// <summary>
    ///     Gets the Revit family symbol associated with this view model.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IRevitFamilySymbol" /> representing the Revit family symbol.
    /// </value>
    /// <remarks>
    ///     This property provides access to the underlying Revit family symbol, enabling interaction with its properties
    ///     and functionality, such as retrieving its name or associated family.
    /// </remarks>
    public IRevitFamilySymbol FamilySymbol { get; }

    /// <summary>
    ///     Gets the name of the Revit family symbol represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family symbol.
    /// </value>
    /// <remarks>
    ///     This property retrieves the <see cref="IRevitFamilySymbol.Name" /> value from the encapsulated
    ///     <see cref="IRevitFamilySymbol" /> instance, providing a user-friendly identifier for the family symbol.
    /// </remarks>
    public string Name => FamilySymbol.Name;

    /// <summary>
    ///     Gets the preview image for the Revit family symbol.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the preview image for this symbol, or <c>null</c> if unavailable.
    /// </value>
    /// <remarks>
    ///     The preview image is created from the symbol's preview stream, if available. The returned image is suitable for
    ///     display in WPF UI.
    /// </remarks>
    public ImageSource? Preview
    {
        get
        {
            var previewStream = FamilySymbol.Preview;

            return previewStream is null
                ? null
                : Helper.CreateBitmapFromStream(previewStream, Color.FromRgb(255, 255, 255));
        }
    }
}
