using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

public class FamilySourceSelectionViewModel : ViewModel
{
    public delegate FamilySourceSelectionViewModel Factory(Action applyAction, Action cancelAction);

    private readonly Action _applyAction;

    private readonly RelayCommand _applyCommand;
    private readonly Action _cancelAction;
    private readonly RelayCommand _cancelCommand;

    private readonly IFamilySourceSettingsViewModel.Factory _familySourceViewModelFactory;
    private readonly ILogger<FamilySourceSelectionViewModel> _logger;
    private IFamilySourceSettingsViewModel? _selectedFamilySource;

    public FamilySourceSelectionViewModel(Action applyAction, Action cancelAction,
                                          IFamilySourceSettingsViewModel.Factory familySourceViewModelFactory,
                                          IEnumerable<IFamilySourceOptions> options,
                                          ILogger<FamilySourceSelectionViewModel> logger)
    {
        _applyAction = applyAction;
        _cancelAction = cancelAction;
        _familySourceViewModelFactory = familySourceViewModelFactory;
        _logger = logger;

        _applyCommand = new RelayCommand(Apply, CanApply);
        _cancelCommand = new RelayCommand(Cancel, () => true);

        FamilySources = options.Select(o => familySourceViewModelFactory(o))
                               .OrderBy(fs => fs.TypeName)
                               .ToList();

        SelectedFamilySource = FamilySources.FirstOrDefault();
    }

    public ICommand ApplyCommand => _applyCommand;

    public ICommand CancelCommand => _cancelCommand;

    public IList<IFamilySourceSettingsViewModel> FamilySources { get; }

    public IFamilySourceSettingsViewModel? SelectedFamilySource
    {
        get => _selectedFamilySource;
        set
        {
            SetProperty(ref _selectedFamilySource, value);
            _applyCommand.NotifyCanExecuteChanged();
        }
    }

    private void Cancel()
    {
        _cancelAction();
    }

    private bool CanApply()
    {
        return SelectedFamilySource is not null;
    }

    private void Apply()
    {
        _applyAction();
    }
}
