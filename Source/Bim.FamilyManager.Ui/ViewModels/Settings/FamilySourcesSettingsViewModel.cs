using System.Collections.ObjectModel;
using System.Windows.Input;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Base.Options;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     View model for managing the collection of family source settings in the application.
///     Provides commands and state for adding, editing, selecting, and removing family sources.
/// </summary>
/// <remarks>
///     This class extends <see cref="SettingsBaseViewModel" /> and implements logic for handling family sources,
///     including dialog management, command handling, and synchronization with options.
/// </remarks>
public class FamilySourcesSettingsViewModel : SettingsBaseViewModel
{
    private static readonly Uri DefaultImageUri =
        new("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/FamilySourcesPrimary_24x24.png");

    private static readonly Uri SelectionImageUri =
        new("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/FamilySourcesWhite_24x24.png");

    private readonly RelayCommand _addCommand;
    private readonly RelayCommand _editCommand;
    private readonly FamilySourceSettingsEditViewModel.Factory _editViewModelFactory;
    private readonly FamilySourceSelectionViewModel.Factory _familySourceSelectionViewModelFactory;
    private readonly IFamilySourceSettingsViewModel.Factory _familySourceviewModelFactory;
    private readonly FamilySourcesOptions _options;
    private readonly IFamilySourceOptions.Factory _optionsFactory;
    private readonly RelayCommand _removeCommand;
    private readonly ObservableCollection<IFamilySourceSettingsViewModel> _sourceSettings = new();
    private FamilySourceSettingsEditViewModel? _editViewModel;
    private bool _isAdding;
    private bool _isEditing;
    private bool _isSelecting;
    private IFamilySourceSettingsViewModel? _selectedFamilySourceSettings;
    private FamilySourceSelectionViewModel? _selectionViewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourcesSettingsViewModel" /> class.
    /// </summary>
    /// <param name="options">The options snapshot for <see cref="FamilySourcesOptions" />.</param>
    /// <param name="optionsFactory">Factory for creating family source options.</param>
    /// <param name="familySourceviewModelFactory">Factory for creating family source settings view models.</param>
    /// <param name="editViewModelFactory">Factory for creating edit view models.</param>
    /// <param name="familySourceSelectionViewModelFactory">Factory for creating selection view models.</param>
    public FamilySourcesSettingsViewModel(
        IOptionsSnapshot<FamilySourcesOptions> options,
        IFamilySourceOptions.Factory optionsFactory,
        IFamilySourceSettingsViewModel.Factory familySourceviewModelFactory,
        FamilySourceSettingsEditViewModel.Factory editViewModelFactory,
        FamilySourceSelectionViewModel.Factory familySourceSelectionViewModelFactory)
    {
        _optionsFactory = optionsFactory;
        _familySourceviewModelFactory = familySourceviewModelFactory;
        _editViewModelFactory = editViewModelFactory;
        _familySourceSelectionViewModelFactory = familySourceSelectionViewModelFactory;
        _options = options.Value;

        _addCommand = new RelayCommand(Add, () => true);
        _removeCommand = new RelayCommand(Remove, CanRemove);
        _editCommand = new RelayCommand(Edit, CanEdit);
    }

    /// <summary>
    ///     Gets or sets the view model used for editing family source settings.
    /// </summary>
    public FamilySourceSettingsEditViewModel? EditViewModel
    {
        get => _editViewModel;
        private set => SetProperty(ref _editViewModel, value);
    }

    /// <summary>
    ///     Gets or sets the view model used for selecting a family source.
    /// </summary>
    public FamilySourceSelectionViewModel? SelectionViewModel
    {
        get => _selectionViewModel;
        set => SetProperty(ref _selectionViewModel, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source settings are currently being edited.
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether a family source is currently being selected.
    /// </summary>
    public bool IsSelecting
    {
        get => _isSelecting;
        set => SetProperty(ref _isSelecting, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether a new family source is currently being added.
    /// </summary>
    public bool IsAdding
    {
        get => _isAdding;
        set => SetProperty(ref _isAdding, value);
    }

    /// <summary>
    ///     Gets the command that adds a new family source setting.
    /// </summary>
    public ICommand AddCommand => _addCommand;

    /// <summary>
    ///     Gets the command that removes a selected family source setting.
    /// </summary>
    public ICommand RemoveCommand => _removeCommand;

    /// <summary>
    ///     Gets the command that initiates the edit operation for a selected family source setting.
    /// </summary>
    public ICommand EditCommand => _editCommand;

    /// <summary>
    ///     Gets the unique identifier for the family source settings view model.
    /// </summary>
    public override int Id { get; } = 1;

    /// <summary>
    ///     Gets the name of the family source settings view model.
    /// </summary>
    public override string Name { get; } = "Family Sources";

    /// <summary>
    ///     Gets the collection of family source settings view models.
    /// </summary>
    public IList<IFamilySourceSettingsViewModel> FamilySourceSettings => _sourceSettings;

    /// <summary>
    ///     Gets or sets the currently selected family source settings.
    /// </summary>
    public IFamilySourceSettingsViewModel? SelectedFamilySourceSettings
    {
        get => _selectedFamilySourceSettings;
        set
        {
            SetProperty(ref _selectedFamilySourceSettings, value);
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    ///     Retrieves the options for the family source settings.
    /// </summary>
    /// <returns>An object representing the family source options, including the current state of each family source.</returns>
    /// <remarks>
    ///     This method ensures that any changes to the <c>IsActive</c> property, which might have been modified outside the
    ///     edit dialog,
    ///     are reflected in the returned options object.
    /// </remarks>
    public override object GetOptions()
    {
        var newOptions = new FamilySourcesOptions
        {
            Sources = FamilySourceSettings.Select(s =>
            {
                var options = s.FamilySourceOptions;
                options.IsActive = s.IsActive;
                return options;
            }).ToList()
        };
        return newOptions;
    }

    /// <summary>
    ///     Retrieves the default image URI for the family source settings view model.
    /// </summary>
    /// <returns>A <see cref="Uri" /> representing the default image associated with the family source settings.</returns>
    protected override Uri GetDefaultImage()
    {
        return DefaultImageUri;
    }

    /// <summary>
    ///     Retrieves the URI of the selection image for the family source settings view model.
    /// </summary>
    /// <returns>A <see cref="Uri" /> representing the selection image specific to the family source settings view model.</returns>
    protected override Uri GetSelectionImage()
    {
        return SelectionImageUri;
    }

    /// <summary>
    ///     Initializes the family source settings view model by populating the collection of family source settings.
    /// </summary>
    /// <remarks>
    ///     This method iterates through the family source options, orders them by name, and creates corresponding view models
    ///     for each source.
    ///     These view models are then added to the <c>FamilySourceSettings</c> collection.
    /// </remarks>
    protected override void OnInitialize()
    {
        foreach (var sourceOptions in _options.Sources.OrderBy(s => s.Name))
        {
            var viewModel = CreateFamilySourceSettingsViewModel(sourceOptions);
            FamilySourceSettings.Add(viewModel);
        }
    }

    /// <summary>
    ///     Handles the application of changes made in the edit dialog for family source settings.
    /// </summary>
    /// <remarks>
    ///     If a new family source is being added, it is appended to the <see cref="FamilySourceSettings" /> collection
    ///     and set as the <see cref="SelectedFamilySourceSettings" />. The editing and adding states are then reset,
    ///     and the <see cref="EditViewModel" /> is cleared.
    /// </remarks>
    private void OnEditDialogApply()
    {
        if (IsAdding && EditViewModel?.SelectedFamilySource is not null)
        {
            FamilySourceSettings.Add(EditViewModel.SelectedFamilySource);
            SelectedFamilySourceSettings = EditViewModel.SelectedFamilySource;
        }

        IsAdding = false;
        IsEditing = false;
        EditViewModel = null;
    }

    /// <summary>
    ///     Handles the application of changes made in the selection dialog for family source settings.
    /// </summary>
    private void OnSelectionDialogApply()
    {
        IsSelecting = false;
        if (SelectionViewModel?.SelectedFamilySource is not null)
        {
            EditViewModel = _editViewModelFactory(OnEditDialogApply, OnEditDialogClose, SelectionViewModel.SelectedFamilySource);
            IsEditing = true;
            IsAdding = true;
        }

        SelectionViewModel = null;
    }

    /// <summary>
    ///     Handles the closure of the edit dialog for family source settings.
    /// </summary>
    private void OnEditDialogClose()
    {
        IsAdding = false;
        IsEditing = false;
        EditViewModel = null;
    }

    /// <summary>
    ///     Handles the closure of the selection dialog for family source settings.
    /// </summary>
    private void OnSelectionDialogClose()
    {
        IsSelecting = false;
        SelectionViewModel = null;
    }

    /// <summary>
    ///     Triggers the <see cref="System.Windows.Input.ICommand.CanExecuteChanged" /> event for the associated commands.
    /// </summary>
    private void RaiseCanExecuteChanged()
    {
        _addCommand.NotifyCanExecuteChanged();
        _editCommand.NotifyCanExecuteChanged();
        _removeCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Determines whether the currently selected family source settings can be removed.
    /// </summary>
    /// <returns><c>true</c> if the selected family source settings are editable and can be removed; otherwise, <c>false</c>.</returns>
    private bool CanRemove()
    {
        return SelectedFamilySourceSettings?.IsEditable == true;
    }

    /// <summary>
    ///     Determines whether the currently selected family source setting can be edited.
    /// </summary>
    /// <returns><c>true</c> if the selected family source setting is editable; otherwise, <c>false</c>.</returns>
    private bool CanEdit()
    {
        return SelectedFamilySourceSettings?.IsEditable == true;
    }

    /// <summary>
    ///     Initiates the edit operation for the currently selected family source setting.
    /// </summary>
    private void Edit()
    {
        EditViewModel = _editViewModelFactory(OnEditDialogApply, OnEditDialogClose, SelectedFamilySourceSettings);
        IsEditing = true;
    }

    /// <summary>
    ///     Removes the currently selected family source settings from the collection.
    /// </summary>
    private void Remove()
    {
        if (SelectedFamilySourceSettings is not null)
        {
            FamilySourceSettings.Remove(SelectedFamilySourceSettings!);
        }
    }

    /// <summary>
    ///     Adds a new directory-based family source to the settings.
    /// </summary>
    private void Add()
    {
        SelectionViewModel = _familySourceSelectionViewModelFactory(OnSelectionDialogApply, OnSelectionDialogClose);
        IsSelecting = true;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="IFamilySourceSettingsViewModel" /> based on the provided family source
    ///     options.
    /// </summary>
    /// <param name="sourceOptions">The options that define the configuration of the family source.</param>
    /// <returns>A new instance of <see cref="IFamilySourceSettingsViewModel" /> initialized with the specified options.</returns>
    private IFamilySourceSettingsViewModel CreateFamilySourceSettingsViewModel(IFamilySourceOptions sourceOptions)
    {
        var viewModel = _familySourceviewModelFactory(sourceOptions);
        return viewModel;
    }
}
