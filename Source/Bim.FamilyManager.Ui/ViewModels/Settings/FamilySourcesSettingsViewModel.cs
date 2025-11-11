using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Base.Options;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing family source settings in the application.
/// </summary>
/// <remarks>
///     This class provides functionality to manage, add, edit, and remove family source settings.
///     It extends <see cref="SettingsBaseViewModel" /> and implements specific logic for handling family sources.
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
    /// <param name="options">
    ///     An <see cref="IOptionsMonitor{TOptions}" /> instance providing the current configuration
    ///     for <see cref="FamilySourcesOptions" />.
    /// </param>
    /// <param name="familySourceviewModelFactory">
    ///     A factory delegate for creating instances of <see cref="IFamilySourceSettingsViewModel" />.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the commands and initializes the family source settings view model
    ///     using the provided options and factory.
    /// </remarks>
    public FamilySourcesSettingsViewModel(IOptionsSnapshot<FamilySourcesOptions> options,
                                          IFamilySourceOptions.Factory optionsFactory,
                                          IFamilySourceSettingsViewModel.Factory familySourceviewModelFactory,
                                          FamilySourceSettingsEditViewModel.Factory editViewModelFactory,
                                          FamilySourceSelectionViewModel.Factory familySourceSelectionViewModelFactory)
    {
        _optionsFactory = optionsFactory;
        _familySourceviewModelFactory = familySourceviewModelFactory;
        _editViewModelFactory = editViewModelFactory;
        _familySourceSelectionViewModelFactory = familySourceSelectionViewModelFactory;
        _options = options.Value; //.CurrentValue;

        _addCommand = new RelayCommand(Add, () => true);
        _removeCommand = new RelayCommand(Remove, CanRemove);
        _editCommand = new RelayCommand(Edit, CanEdit);
    }

    /// <summary>
    ///     Gets or sets the view model used for editing family source settings.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFamilySourceSettingsViewModel" /> representing the current edit view model.
    /// </value>
    /// <remarks>
    ///     This property is used to manage the editing state of family source settings.
    ///     It is updated when the user initiates or completes an edit operation.
    /// </remarks>
    public FamilySourceSettingsEditViewModel? EditViewModel
    {
        get => _editViewModel;
        private set => SetProperty(ref _editViewModel, value);
    }

    public FamilySourceSelectionViewModel? SelectionViewModel
    {
        get => _selectionViewModel;
        set => SetProperty(ref _selectionViewModel, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the family source settings are currently being edited.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family source settings are in edit mode; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to control the visibility and state of the edit dialog for family source settings.
    ///     When set to <c>true</c>, the edit dialog is displayed, allowing modifications to the selected family source
    ///     settings.
    /// </remarks>
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public bool IsSelecting
    {
        get => _isSelecting;
        set => SetProperty(ref _isSelecting, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether a new family source is currently being added.
    /// </summary>
    /// <value>
    ///     <c>true</c> if a new family source is being added; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to track the state of the view model when a new family source
    ///     is being created and added to the collection of family source settings.
    /// </remarks>
    public bool IsAdding
    {
        get => _isAdding;
        set => SetProperty(ref _isAdding, value);
    }

    /// <summary>
    ///     Gets the command that adds a new family source setting.
    /// </summary>
    /// <remarks>
    ///     This command is bound to the "Add" button in the UI and executes the logic
    ///     for adding a new family source setting. It uses the <see cref="RelayCommand" />
    ///     to encapsulate the add operation and its availability.
    /// </remarks>
    public ICommand AddCommand => _addCommand;

    /// <summary>
    ///     Gets the command that removes a selected family source setting.
    /// </summary>
    /// <remarks>
    ///     This command is bound to the "Remove" button in the UI and executes the logic
    ///     to remove the currently selected family source setting. It uses the <see cref="RelayCommand" />
    ///     to encapsulate the remove operation and its associated conditions.
    /// </remarks>
    public ICommand RemoveCommand => _removeCommand;

    /// <summary>
    ///     Gets the command that initiates the edit operation for a selected family source setting.
    /// </summary>
    /// <remarks>
    ///     This command is bound to the "Edit" button in the user interface and is used to
    ///     modify the currently selected family source setting. The command's execution logic
    ///     is defined in the <see cref="Edit" /> method, and its availability is determined by
    ///     the <see cref="CanEdit" /> method.
    /// </remarks>
    public ICommand EditCommand => _editCommand;

    /// <summary>
    ///     Gets the unique identifier for the family source settings view model.
    /// </summary>
    /// <remarks>
    ///     This property overrides the <see cref="SettingsBaseViewModel.Id" /> property to provide
    ///     a specific identifier for the <see cref="FamilySourcesSettingsViewModel" /> class.
    /// </remarks>
    public override int Id { get; } = 1;

    /// <summary>
    ///     Gets the name of the family source settings view model.
    /// </summary>
    /// <remarks>
    ///     This property provides a descriptive name, "Family Sources," for the
    ///     <see cref="FamilySourcesSettingsViewModel" /> class, which is used to identify
    ///     this specific settings view model in the application.
    /// </remarks>
    public override string Name { get; } = "Family Sources";

    /// <summary>
    ///     Gets the collection of family source settings view models.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the list of family source settings, allowing for management
    ///     of individual family source configurations. The collection is used as the data source for
    ///     the UI components, such as the <see cref="ListView" /> in the associated view.
    /// </remarks>
    public IList<IFamilySourceSettingsViewModel> FamilySourceSettings => _sourceSettings;

    /// <summary>
    ///     Gets or sets the currently selected family source settings.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="IFamilySourceSettingsViewModel" /> representing the selected family source settings.
    /// </value>
    /// <remarks>
    ///     This property allows the user to select a specific family source settings view model from the available list.
    ///     When the selection changes, related commands and UI elements are updated accordingly.
    /// </remarks>
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
    /// <returns>
    ///     An object representing the family source options, including the current state of each family source.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetOptions" /> method to provide
    ///     specific logic for constructing and returning the family source options. It ensures that
    ///     any changes to the <c>IsActive</c> property, which might have been modified outside the edit dialog,
    ///     are reflected in the returned options object.
    /// </remarks>
    public override object GetOptions()
    {
        var newOptions = new FamilySourcesOptions
        {
            Sources = FamilySourceSettings.Select(s =>
            {
                // The IsActive property might have been modified outside the edit dialog, for example, in the list view.
                // As a result, changes are not reflected in the options object. We need to apply them here.
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
    /// <returns>
    ///     A <see cref="Uri" /> representing the default image associated with the family source settings.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetDefaultImage" /> method to provide
    ///     a specific default image for the <see cref="FamilySourcesSettingsViewModel" /> class.
    /// </remarks>
    protected override Uri GetDefaultImage()
    {
        return DefaultImageUri;
    }

    /// <summary>
    ///     Retrieves the URI of the selection image for the family source settings view model.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri" /> representing the selection image specific to the
    ///     <see cref="FamilySourcesSettingsViewModel" />.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetSelectionImage" /> method
    ///     to provide a custom selection image for the family source settings.
    /// </remarks>
    protected override Uri GetSelectionImage()
    {
        return SelectionImageUri;
    }

    /// <summary>
    ///     Initializes the family source settings view model by populating the collection of family source settings.
    /// </summary>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.OnInitialize" /> method to provide specific
    ///     initialization logic for the <see cref="FamilySourcesSettingsViewModel" /> class. It iterates through the
    ///     family source options, orders them by name, and creates corresponding view models for each source.
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
    ///     This method finalizes the editing or addition of a family source setting.
    ///     If a new family source is being added, it is appended to the <see cref="FamilySourceSettings" /> collection
    ///     and set as the <see cref="SelectedFamilySourceSettings" />.
    ///     The editing and adding states are then reset, and the <see cref="EditViewModel" /> is cleared.
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

    private void OnSelectionDialogApply()
    {
        // Setting Is selected to false will close the selection dialog. This must be done before opening the edit dialog.
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
    /// <remarks>
    ///     This method resets the editing state by setting <see cref="IsAdding" /> and <see cref="IsEditing" /> to
    ///     <c>false</c>,
    ///     and clears the <see cref="EditViewModel" /> to indicate that no family source is currently being edited.
    /// </remarks>
    private void OnEditDialogClose()
    {
        IsAdding = false;
        IsEditing = false;
        EditViewModel = null;
    }

    private void OnSelectionDialogClose()
    {
        IsSelecting = false;
        SelectionViewModel = null;
    }

    /// <summary>
    ///     Triggers the <see cref="System.Windows.Input.ICommand.CanExecuteChanged" /> event for the associated commands.
    /// </summary>
    /// <remarks>
    ///     This method notifies the commands (<see cref="AddCommand" />, <see cref="EditCommand" />, and
    ///     <see cref="RemoveCommand" />)
    ///     that their execution state might have changed, prompting the UI to re-evaluate their availability.
    /// </remarks>
    private void RaiseCanExecuteChanged()
    {
        _addCommand.NotifyCanExecuteChanged();
        _editCommand.NotifyCanExecuteChanged();
        _removeCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Determines whether the currently selected family source settings can be removed.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the selected family source settings are editable and can be removed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks the <see cref="IFamilySourceSettingsViewModel.IsEditable" /> property of the
    ///     <see cref="SelectedFamilySourceSettings" /> to determine if the removal operation is allowed.
    /// </remarks>
    private bool CanRemove()
    {
        return SelectedFamilySourceSettings?.IsEditable == true;
    }

    /// <summary>
    ///     Determines whether the currently selected family source setting can be edited.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the selected family source setting is editable; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks the <see cref="IFamilySourceSettingsViewModel.IsEditable" /> property
    ///     of the currently selected family source setting to determine if editing is allowed.
    /// </remarks>
    private bool CanEdit()
    {
        return SelectedFamilySourceSettings?.IsEditable == true;
    }

    /// <summary>
    ///     Initiates the edit operation for the currently selected family source setting.
    /// </summary>
    /// <remarks>
    ///     This method sets the <see cref="EditViewModel" /> property to the currently selected
    ///     family source setting and updates the <see cref="IsEditing" /> property to indicate
    ///     that the edit mode is active. It is typically invoked by the <see cref="EditCommand" />.
    /// </remarks>
    private void Edit()
    {
        EditViewModel = _editViewModelFactory(OnEditDialogApply, OnEditDialogClose, SelectedFamilySourceSettings);
        IsEditing = true;
    }

    /// <summary>
    ///     Removes the currently selected family source settings from the collection.
    /// </summary>
    /// <remarks>
    ///     This method checks if a family source setting is selected and, if so, removes it from the
    ///     <see cref="FamilySourceSettings" /> collection.
    /// </remarks>
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
    /// <remarks>
    ///     This method initializes a new instance of <see cref="IFamilySourceSettingsViewModel" /> using the factory,
    ///     configures it as editable and active, and sets it as the current editing view model.
    ///     It also updates the editing state to indicate that a new family source is being added.
    /// </remarks>
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
    /// <remarks>
    ///     This method utilizes the factory delegate <see cref="IFamilySourceSettingsViewModel.Factory" /> to create
    ///     the view model. It also assigns actions for handling the apply and cancel events during editing.
    /// </remarks>
    private IFamilySourceSettingsViewModel CreateFamilySourceSettingsViewModel(IFamilySourceOptions sourceOptions)
    {
        var viewModel = _familySourceviewModelFactory(sourceOptions);

        return viewModel;
    }
}
