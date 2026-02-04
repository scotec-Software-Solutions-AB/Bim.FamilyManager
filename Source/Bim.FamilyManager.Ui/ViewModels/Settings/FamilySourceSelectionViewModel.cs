using System.Windows.Input;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     View model for selecting and applying a family source in the settings UI.
///     Provides commands and state for managing available sources and user selection.
/// </summary>
public class FamilySourceSelectionViewModel : ViewModel
{
    /// <summary>
    ///     Factory delegate for creating <see cref="FamilySourceSelectionViewModel" /> instances.
    /// </summary>
    /// <param name="applyAction">The action to execute when applying the selection.</param>
    /// <param name="cancelAction">The action to execute when cancelling the selection.</param>
    /// <returns>A new <see cref="FamilySourceSelectionViewModel" /> instance.</returns>
    public delegate FamilySourceSelectionViewModel Factory(Action applyAction, Action cancelAction);

    private readonly Action _applyAction;
    private readonly RelayCommand _applyCommand;
    private readonly Action _cancelAction;
    private readonly RelayCommand _cancelCommand;
    private readonly IFamilySourceSettingsViewModel.Factory _familySourceViewModelFactory;
    private readonly ILogger<FamilySourceSelectionViewModel> _logger;
    private IFamilySourceSettingsViewModel? _selectedFamilySource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSelectionViewModel" /> class.
    /// </summary>
    /// <param name="applyAction">The action to execute when applying the selection.</param>
    /// <param name="cancelAction">The action to execute when cancelling the selection.</param>
    /// <param name="familySourceViewModelFactory">Factory for creating family source view models.</param>
    /// <param name="options">The available family source options.</param>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public FamilySourceSelectionViewModel(
        Action applyAction,
        Action cancelAction,
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
    }

    /// <summary>
    ///     Gets the command to apply the selected family source.
    /// </summary>
    public ICommand ApplyCommand => _applyCommand;

    /// <summary>
    ///     Gets the command to cancel the selection.
    /// </summary>
    public ICommand CancelCommand => _cancelCommand;

    /// <summary>
    ///     Gets the list of available family source view models.
    /// </summary>
    public IList<IFamilySourceSettingsViewModel> FamilySources { get; }

    /// <summary>
    ///     Gets or sets the currently selected family source view model.
    ///     Changing the selection updates the apply command's state.
    /// </summary>
    public IFamilySourceSettingsViewModel? SelectedFamilySource
    {
        get => _selectedFamilySource;
        set
        {
            SetProperty(ref _selectedFamilySource, value);
            _applyCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    ///     Executes the cancel action.
    /// </summary>
    private void Cancel()
    {
        _cancelAction();
    }

    /// <summary>
    ///     Determines whether the apply command can execute.
    /// </summary>
    /// <returns><c>true</c> if a family source is selected; otherwise, <c>false</c>.</returns>
    private bool CanApply()
    {
        return SelectedFamilySource is not null;
    }

    /// <summary>
    ///     Executes the apply action.
    /// </summary>
    private void Apply()
    {
        _applyAction();
    }
}
