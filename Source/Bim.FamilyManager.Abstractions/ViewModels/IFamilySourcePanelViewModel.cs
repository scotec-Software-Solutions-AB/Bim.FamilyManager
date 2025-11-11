using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Abstractions.ViewModels;


public interface IFamilySourcePanelViewModel : IViewModel
{
    public delegate IFamilySourcePanelViewModel? Factory(IFamilySource familySource);
}
