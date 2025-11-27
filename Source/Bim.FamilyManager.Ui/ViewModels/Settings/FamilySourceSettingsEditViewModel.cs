using System.Windows.Input;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Scotec.Events.WeakEvents;
using Scotec.Extensions.Linq;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     View model for editing a family source in the settings UI.
///     Provides commands and state for managing the selected source and applying or cancelling changes.
/// </summary>
public class FamilySourceSettingsEditViewModel : ViewModel
{
    /// <summary>
    ///     Factory delegate for creating <see cref="FamilySourceSettingsEditViewModel" /> instances.
    /// </summary>
    /// <param name="applyAction">The action to execute when applying changes.</param>
    /// <param name="cancelAction">The action to execute when cancelling changes.</param>
    /// <param name="familySource">The family source to edit, or <c>null</c> to allow selection.</param>
    /// <returns>A new <see cref="FamilySourceSettingsEditViewModel" /> instance.</returns>
    public delegate FamilySourceSettingsEditViewModel Factory(Action applyAction, Action cancelAction,
                                                              IFamilySourceSettingsViewModel? familySource = null);

    private readonly Action _applyAction;
    private readonly RelayCommand _applyCommand;
    private readonly Action _cancelAction;
    private readonly RelayCommand _cancelCommand;
    private IFamilySourceSettingsViewModel? _selectedFamilySource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceSettingsEditViewModel" /> class.
    /// </summary>
    /// <param name="applyAction">The action to execute when applying changes.</param>
    /// <param name="cancelAction">The action to execute when cancelling changes.</param>
    /// <param name="familySource">The family source to edit, or <c>null</c> to allow selection.</param>
    /// <param name="options">The available family source options.</param>
    /// <param name="familySourceViewModelFactory">Factory for creating family source view models.</param>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
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

        FamilySources.ForAll(fs =>
            StaticWeakEventManager.AddWeakHandler(fs, nameof(fs.PropertyChanged), (sender, args) => { _applyCommand?.NotifyCanExecuteChanged(); }));

        _applyCommand = new RelayCommand(Apply, CanApply);
        _cancelCommand = new RelayCommand(Cancel, () => true);
    }

    /// <summary>
    ///     Gets the list of available family source view models.
    /// </summary>
    public IList<IFamilySourceSettingsViewModel> FamilySources { get; }

    /// <summary>
    ///     Gets or sets the currently selected family source view model.
    ///     Changing the selection updates event subscriptions and command state.
    /// </summary>
    public IFamilySourceSettingsViewModel? SelectedFamilySource
    {
        get => _selectedFamilySource;
        set
        {
            if (_selectedFamilySource is not null)
            {
                _selectedFamilySource.Modified -= SelectedFamilySourceOnModified;
            }

            SetProperty(ref _selectedFamilySource, value);
            if (_selectedFamilySource is not null)
            {
                _selectedFamilySource.Modified += SelectedFamilySourceOnModified;
            }

            void SelectedFamilySourceOnModified(object? sender, EventArgs e)
            {
                NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    ///     Gets the command to apply changes to the selected family source.
    /// </summary>
    public ICommand ApplyCommand => _applyCommand;

    /// <summary>
    ///     Gets the command to cancel changes and reset the selected family source.
    /// </summary>
    public ICommand CancelCommand => _cancelCommand;

    /// <summary>
    ///     Notifies the apply and cancel commands that their ability to execute may have changed.
    /// </summary>
    private void NotifyCanExecuteChanged()
    {
        _applyCommand.NotifyCanExecuteChanged();
        _cancelCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Executes the cancel action and resets the selected family source.
    /// </summary>
    private void Cancel()
    {
        SelectedFamilySource?.Reset();
        _cancelAction();
    }

    /// <summary>
    ///     Determines whether the apply command can execute.
    /// </summary>
    /// <returns><c>true</c> if the selected family source can be applied; otherwise, <c>false</c>.</returns>
    private bool CanApply()
    {
        return SelectedFamilySource?.CanApply() ?? false;
    }

    /// <summary>
    ///     Executes the apply action and applies changes to the selected family source.
    /// </summary>
    private void Apply()
    {
        SelectedFamilySource?.Apply();
        _applyAction();
    }
}
