using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;

public abstract class FamilySourcePanelViewModel<TFamilySource> : ViewModel, IFamilySourcePanelViewModel where TFamilySource : IFamilySource
{
    protected FamilySourcePanelViewModel(TFamilySource familySource)
    {
        FamilySource = familySource;
    }

    protected TFamilySource FamilySource { get; }
}
