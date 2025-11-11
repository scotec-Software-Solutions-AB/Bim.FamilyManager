using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Scotec.Extensions.Linq;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

public class FamilySourceSettingsEditViewModel : ViewModel
{
    public delegate FamilySourceSettingsEditViewModel Factory(Action applyAction, Action cancelAction,
                                                              IFamilySourceSettingsViewModel? familySource = null);

    private readonly Action _applyAction;

    private readonly RelayCommand _applyCommand;
    private readonly Action _cancelAction;
    private readonly RelayCommand _cancelCommand;
    private IFamilySourceSettingsViewModel? _selectedFamilySource;

    public FamilySourceSettingsEditViewModel(Action applyAction, Action cancelAction,
                                             IFamilySourceSettingsViewModel? familySource,
                                             IEnumerable<IFamilySourceOptions> options,
                                             IFamilySourceSettingsViewModel.Factory familySourceViewModelFactory,
                                             ILogger<FamilySourceSettingsEditViewModel> logger)
    {
        _applyAction = applyAction;
        _cancelAction = cancelAction;

        if (familySource is null)
        {
            FamilySources = options.Select(o => familySourceViewModelFactory(o))
                                   .OrderBy(fs => fs.TypeName)
                                   .ToList();
        }
        else
        {
            // The user cannot select other type of family source in edit mode.
            FamilySources = [familySource];
        }

        SelectedFamilySource = FamilySources.First();

        FamilySources.ForAll(fs => fs.PropertyChanged += (sender, args) => { _applyCommand?.NotifyCanExecuteChanged(); });

        _applyCommand = new RelayCommand(Apply, CanApply);
        _cancelCommand = new RelayCommand(Cancel, () => true);
    }

    public IList<IFamilySourceSettingsViewModel> FamilySources { get; }

    public IFamilySourceSettingsViewModel? SelectedFamilySource
    {
        get => _selectedFamilySource;
        set => SetProperty(ref _selectedFamilySource, value);
    }

    public ICommand ApplyCommand => _applyCommand;

    public ICommand CancelCommand => _cancelCommand;

    private void Cancel()
    {
        SelectedFamilySource?.Reset();
        _cancelAction();
    }

    private bool CanApply()
    {
        return SelectedFamilySource?.CanApply() ?? false;
    }

    private void Apply()
    {
        SelectedFamilySource?.Apply();
        _applyAction();
    }
}
