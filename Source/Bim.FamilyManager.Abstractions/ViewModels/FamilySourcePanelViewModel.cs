using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

/// <summary>
///     Provides a base implementation for a view model that represents a panel associated with a family source in
///     Bim.FamilyManager.
/// </summary>
/// <typeparam name="TFamilySource">
///     The type of the family source, which must implement <see cref="IFamilySource" />.
/// </typeparam>
/// <remarks>
///     This abstract class is intended to be used as a base for view models that display or interact with a specific
///     family source.
///     It exposes the underlying family source instance for derived classes.
/// </remarks>
public abstract class FamilySourcePanelViewModel<TFamilySource> : ViewModel, IFamilySourcePanelViewModel where TFamilySource : IFamilySource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourcePanelViewModel{TFamilySource}" /> class.
    /// </summary>
    /// <param name="familySource">The family source instance associated with this panel view model.</param>
    protected FamilySourcePanelViewModel(TFamilySource familySource)
    {
        FamilySource = familySource;
    }

    /// <summary>
    ///     Gets the family source instance associated with this panel view model.
    /// </summary>
    protected TFamilySource FamilySource { get; }
}
