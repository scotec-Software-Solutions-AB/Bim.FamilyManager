using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Ui.Views.Settings;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Scotec.Events.WeakEvents;
using Scotec.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bim.FamilyManager.Ui.FamilyNavigator.ViewModels;

/// <summary>
///     Represents the view model for managing Revit families within the application.
/// </summary>
/// <remarks>
///     This class serves as the primary view model for the Family Manager feature, providing
///     properties and commands to interact with Revit family data. It manages the loading,
///     filtering, and searching of family sources and families, as well as handling user interactions
///     through commands and properties.
/// </remarks>
public class FamilyManagerViewModel : ViewModel, IFamilyManagerViewModel
{
    private readonly FamilyViewModel.Factory _familyFactory;
    private readonly IFamilyManager _familyManager;
    private readonly RelayCommand _goBackCommand;
    private readonly RelayCommand _goHomeCommand;
    private readonly ObservableCollection<IFamilyManagerItemViewModel> _history = [];
    private readonly string? _logo;
    private readonly RelayCommand _reloadCommand;
    private readonly FamilySourceViewModel.Factory _sourceFactory;
    private IEnumerable<IFamilySourceViewModel>? _familySources;
    private IEnumerable<IFamilyManagerItemViewModel>? _items;
    private string _searchPattern = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManagerViewModel" /> class.
    /// </summary>
    /// <param name="familyManager">
    ///     The <see cref="IFamilyManager" /> instance responsible for managing families.
    /// </param>
    /// <param name="sourceFactory">
    ///     A factory delegate for creating instances of <see cref="FamilySourceViewModel" />.
    /// </param>
    /// <param name="familyFactory">
    ///     A factory delegate for creating instances of <see cref="FamilyViewModel" />.
    /// </param>
    /// <param name="options">
    ///     The <see cref="IOptions{TOptions}" /> instance containing configuration settings for
    ///     <see cref="FamilyManagerOptions" />.
    /// </param>
    /// <param name="settingsManagerWindowFactory">
    ///     A factory delegate for creating instances of <see cref="SettingsManagerWindow" />.
    /// </param>
    /// <remarks>
    ///     The constructor sets up the view model, initializes commands, and subscribes to the family manager's reload event.
    /// </remarks>
    public FamilyManagerViewModel(IFamilyManager familyManager,
                                  FamilySourceViewModel.Factory sourceFactory,
                                  FamilyViewModel.Factory familyFactory,
                                  IOptions<FamilyManagerOptions> options,
                                  SettingsManagerWindow.Factory settingsManagerWindowFactory)
    {
        SettingsManagerWindowFactory = settingsManagerWindowFactory;
        _familyManager = familyManager;
        _sourceFactory = sourceFactory;
        _familyFactory = familyFactory;

        _logo = options.Value.Logo;
        StaticWeakEventManager.AddWeakHandler(_familyManager, nameof(_familyManager.Reloaded), OnReloaded);

        _reloadCommand = new RelayCommand(() => { _familyManager.ReloadAsync(); });
        _goHomeCommand = new RelayCommand(GoHome, CanGoHome);
        _goBackCommand = new RelayCommand(GoBack, CanGoBack);
    }

    /// <summary>
    ///     Gets the navigation history of selected items in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     The history is used for breadcrumb navigation and back/home commands.
    /// </remarks>
    public IList<IFamilyManagerItemViewModel> History => _history;

    /// <summary>
    ///     Gets the first part of the breadcrumb navigation string.
    /// </summary>
    /// <remarks>
    ///     This property returns a string representing the navigation path, excluding the last item.
    /// </remarks>
    public string Breadcrumb1
    {
        get { return string.Join(" / ", _history.SkipLast(1).Select(item => item.Name)); }
    }

    /// <summary>
    ///     Gets the second part of the breadcrumb navigation string.
    /// </summary>
    /// <remarks>
    ///     This property returns the name of the last item in the navigation history, or a default label if empty.
    /// </remarks>
    public string Breadcrumb2
    {
        get
        {
            if (_history.Count == 0)
            {
                return "#Family sources";
            }

            var breadcrumb = _history.LastOrDefault()?.Name ?? string.Empty;

            if (_history.Count > 1)
            {
                breadcrumb = " / " + breadcrumb;
            }

            return breadcrumb;
        }
    }

    /// <summary>
    ///     Gets the command to navigate to the home/root view.
    /// </summary>
    public ICommand GoHomeCommand => _goHomeCommand;

    /// <summary>
    ///     Gets the command to navigate back in the navigation history.
    /// </summary>
    public ICommand GoBackCommand => _goBackCommand;

    /// <summary>
    ///     Gets the factory delegate used to create instances of <see cref="SettingsManagerWindow" />.
    /// </summary>
    /// <remarks>
    ///     This property provides a factory method to instantiate the <see cref="SettingsManagerWindow" />.
    ///     It is typically used to display the settings management interface within the Family Manager application.
    /// </remarks>
    public SettingsManagerWindow.Factory SettingsManagerWindowFactory { get; }

    /// <summary>
    ///     Gets or sets the currently selected family source in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property represents the active family source selected by the user.
    ///     It is updated automatically when a family source is marked as selected.
    ///     The selected family source determines the context for operations such as
    ///     filtering and searching families.
    /// </remarks>
    public IFamilySourceViewModel? SelectedFamilySource
    {
        get;
        set
        {
            if (field is not null)
            {
                field.PropertyChanged -= OnFoldersPropertyChanged;
            }

            SetProperty(ref field, value);

            if (field is not null)
            {
                field.PropertyChanged += OnFoldersPropertyChanged;
            }
        }
    }

    /// <summary>
    /// Handles the <see cref="INotifyPropertyChanged.PropertyChanged"/> event for the <see cref="IFamilySourceViewModel.Folders"/> property.
    /// </summary>
    /// <param name="sender">
    /// The source object that raised the event, typically an instance of <see cref="IFamilySourceViewModel"/>.
    /// </param>
    /// <param name="e">
    /// The <see cref="PropertyChangedEventArgs"/> containing the name of the property that changed.
    /// </param>
    /// <remarks>
    /// This method is triggered when the <see cref="IFamilySourceViewModel.Folders"/> property changes.
    /// If the folders collection becomes null or empty and the navigation history contains more than one item,
    /// all items except the first are removed from the history. The breadcrumb properties are updated accordingly.
    /// If only one item remains in the history, the <see cref="Items"/> property is updated to reflect the current folders.
    /// This ensures that the UI remains consistent when the folders collection of a family source changes, 
    /// such as when the source becomes connected or disconnected.
    /// </remarks>
    private void OnFoldersPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(sender is IFamilySourceViewModel source && e.PropertyName == nameof(IFamilySourceViewModel.Folders))
        {
            if ((source.Folders is null || source.Folders.Count == 0) && _history.Count > 1)
            {
                for (var i = _history.Count - 1; i > 0; i--)
                {
                    _history.RemoveAt(i);
                }
            }

            OnPropertyChanged(nameof(Breadcrumb1));
            OnPropertyChanged(nameof(Breadcrumb2));

            if (_history.Count == 1)
            {
                Items = source.Folders;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the collection of items currently displayed in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property is updated based on navigation and selection, and is used for data binding in the UI.
    /// </remarks>
    public IEnumerable<IFamilyManagerItemViewModel>? Items
    {
        get { return _items ??= FamilySources.Cast<IFamilyManagerItemViewModel>().ToList(); }
        set => SetProperty(ref _items, value);
    }

    /// <summary>
    ///     Gets or sets the currently selected item in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     Setting this property updates the selection state, navigation history, and the displayed items.
    /// </remarks>
    public IFamilyManagerItemViewModel? SelectedItem
    {
        get;
        set
        {
            if (field is not null)
            {
                field.IsSelected = false;
            }

            if (value != null)
            {
                value.IsSelected = true;
            }

            Items = value switch
            {
                IFamilySourceViewModel familySource => familySource.Folders?.Cast<IFamilyManagerItemViewModel>().ToList(),
                IFolderViewModel { Subfolders: not null } folder when folder.Subfolders.Any() => folder.Subfolders.Cast<IFamilyManagerItemViewModel>().ToList(),
                IFolderViewModel { Families: not null } folder => folder.Families.Cast<IFamilyManagerItemViewModel>().ToList(),
                _ => []
            };

            if (value is not null)
            {
                PushHistoryItem(value);
            }

            SetProperty(ref field, value);
        }
    }

    /// <summary>
    ///     Gets or sets the search pattern used to filter Revit families in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property is bound to the search input in the user interface and is updated
    ///     whenever the user modifies the search text. Changing this property triggers the
    ///     filtering of families based on the specified search pattern.
    /// </remarks>
    public string SearchPattern
    {
        get => _searchPattern;
        set
        {
            SetProperty(ref _searchPattern, value);
            FilterFamilies(_searchPattern);
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the search functionality is currently active.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the search functionality is enabled, and the application
    ///     displays search results based on the provided search pattern. When set to <c>false</c>,
    ///     the search functionality is disabled, and the application displays the default view.
    ///     This property is typically bound to the UI to toggle between search results and the
    ///     default view.
    /// </remarks>
    public bool IsActiveSearch
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Gets the search results for Revit families based on the current search pattern.
    /// </summary>
    /// <remarks>
    ///     This property holds the filtered list of <see cref="FamilyViewModel" /> objects that match the
    ///     search criteria specified by the <see cref="SearchPattern" /> property. The search is triggered
    ///     when the <see cref="SearchPattern" /> length is at least 3 characters. If no search is active,
    ///     this property will be <c>null</c>.
    /// </remarks>
    public IEnumerable<IFamilyViewModel>? SearchResult
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    ///     Gets the logo image for the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property retrieves the logo image specified in the application settings.
    ///     If the logo file path is invalid or the file does not exist, it returns <c>null</c>.
    /// </remarks>
    public ImageSource? Logo
    {
        get
        {
            if (string.IsNullOrEmpty(_logo))
            {
                return null;
            }

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            // Specify the file path for the logo image
            var logoFilePath = Path.Combine(path, _logo);

            // Check if the file exists
            if (!File.Exists(logoFilePath))
            {
                return null;
            }

            // Load the image from the file
            return new BitmapImage(new Uri(logoFilePath, UriKind.Absolute));
        }
    }

    /// <summary>
    ///     Gets the command that reloads the family data within the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This command is typically bound to a user interface element, such as a button, to allow
    ///     users to refresh the family data displayed in the application. It triggers the reloading
    ///     of family sources and updates the associated view models accordingly.
    /// </remarks>
    public ICommand ReloadCommand => _reloadCommand;

    /// <summary>
    ///     Gets the collection of family sources available in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property initializes and retrieves the list of family sources, represented by
    ///     <see cref="FamilySourceViewModel" /> instances. It also manages the selection state of
    ///     family sources, ensuring that one source is marked as selected at any given time.
    /// </remarks>
    public IEnumerable<IFamilySourceViewModel> FamilySources
    {
        get { return _familySources ??= _familyManager.FamilySources.Select(source => (IFamilySourceViewModel)_sourceFactory(source)).ToList(); }
        private set => SetProperty(ref _familySources, value);
    }

    /// <summary>
    ///     Refreshes the state of the view model by applying the active search filter, if enabled.
    /// </summary>
    /// <remarks>
    ///     This method checks whether the active search is enabled and, if so, filters the families
    ///     based on the current search pattern. It is typically invoked to update the displayed
    ///     family data in response to user interactions or changes in the search criteria.
    /// </remarks>
    public void Refresh()
    {
        if (IsActiveSearch)
        {
            FilterFamilies(SearchPattern);
        }
    }

    /// <summary>
    ///     Determines whether the GoHome command can be executed.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the navigation history is not empty; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Used by the GoHome command to enable or disable itself.
    /// </remarks>
    private bool CanGoHome()
    {
        return _history.Count > 0;
    }

    /// <summary>
    ///     Determines whether the GoBack command can be executed.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the navigation history is not empty; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Used by the GoBack command to enable or disable itself.
    /// </remarks>
    private bool CanGoBack()
    {
        return _history.Count > 0;
    }

    /// <summary>
    ///     Navigates back in the navigation history.
    /// </summary>
    /// <remarks>
    ///     Pops the last item from the history and updates the selected item and displayed items accordingly.
    /// </remarks>
    private void GoBack()
    {
        if (_history.Count > 0)
        {
            PopHistoryItem();
            SelectedItem = _history.LastOrDefault();
            if (SelectedItem is null)
            {
                Items = FamilySources;
            }
        }
    }

    /// <summary>
    ///     Removes the last item from the navigation history and updates related properties.
    /// </summary>
    /// <remarks>
    ///     This method is used internally to manage the navigation stack and update UI state.
    /// </remarks>
    private void PopHistoryItem()
    {
        _history.RemoveAt(_history.Count - 1);
        OnPropertyChanged(nameof(Breadcrumb1));
        OnPropertyChanged(nameof(Breadcrumb2));
        if (_history.Count == 0)
        {
            SelectedFamilySource = null;
        }

        NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Clears the navigation history and updates related properties.
    /// </summary>
    /// <remarks>
    ///     This method is used to reset the navigation state, typically when navigating home.
    /// </remarks>
    private void ClearHistory()
    {
        _history.Clear();
        SelectedFamilySource = null;
        OnPropertyChanged(nameof(Breadcrumb1));
        OnPropertyChanged(nameof(Breadcrumb2));

        NotifyCanExecuteChanged();
    }

    /// <summary>
    ///     Navigates to the home/root view in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     Clears the navigation history, resets the selected item, and displays the family sources.
    /// </remarks>
    private void GoHome()
    {
        ClearHistory();
        SelectedItem = null;

        // Assign the family sources directly to Items to prevent unnecessary reloading.
        Items = FamilySources;
    }

    /// <summary>
    ///     Adds an item to the navigation history if it is not already the last item.
    /// </summary>
    /// <param name="value">The item to add to the history.</param>
    /// <remarks>
    ///     This method is used to track navigation for breadcrumb and back/home functionality.
    /// </remarks>
    private void PushHistoryItem(IFamilyManagerItemViewModel? value)
    {
        if (value is not null && _history.LastOrDefault() != value)
        {
            _history.Add(value);
            OnPropertyChanged(nameof(Breadcrumb1));
            OnPropertyChanged(nameof(Breadcrumb2));
            if (_history.Count == 1)
            {
                SelectedFamilySource = (IFamilySourceViewModel)value;
            }

            NotifyCanExecuteChanged();
        }
    }

    private void NotifyCanExecuteChanged()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _goBackCommand.NotifyCanExecuteChanged();
            _goHomeCommand.NotifyCanExecuteChanged();
        });
    }

    /// <summary>
    ///     Handles the <see cref="IFamilyManager.Reloaded" /> event triggered when the family manager reloads its data.
    /// </summary>
    /// <param name="sender">The source of the event, typically the <see cref="IFamilyManager" /> instance.</param>
    /// <param name="e">The event data associated with the reload operation.</param>
    /// <remarks>
    ///     This method resets the <see cref="SelectedFamilySource" /> and <see cref="FamilySources" /> properties to null,
    ///     ensuring that the view model reflects the updated state of the family manager after a reload.
    /// </remarks>
    private void OnReloaded(IFamilyManager? sender, EventArgs e)
    {
        ClearHistory();
        SelectedItem = null;

        _familySources = null;
        _items = null;

        OnPropertyChanged(nameof(FamilySources));
        OnPropertyChanged(nameof(Items));
    }

    /// <summary>
    ///     Filters the Revit families based on the specified search pattern and the currently selected folder.
    /// </summary>
    /// <param name="searchPattern">
    ///     The search pattern to filter the families. A non-empty string with a minimum length of 3 characters is required
    ///     to perform the filtering.
    /// </param>
    /// <remarks>
    ///     This method updates the <see cref="SearchResult" /> property with the filtered families that match the search
    ///     pattern within the selected folder. If the search pattern is invalid or no folder is selected, the search result
    ///     is cleared, and the <see cref="IsActiveSearch" /> property is set to <c>false</c>.
    /// </remarks>
    private void FilterFamilies(string searchPattern)
    {
        var folder = SelectedFamilySource?.SelectedFolder;
        if (!string.IsNullOrWhiteSpace(searchPattern) && folder is not null)
        {
            IsActiveSearch = true;
            var searchResult = Task.Run(async () =>
            {
                if (_searchPattern.Length >= 3)
                {
                    var families = new List<IRevitFamily>();
                    await foreach (var family in _familyManager.SearchRevitFamiliesAsync(folder.Folder, searchPattern, CancellationToken.None))
                    {
                        families.Add(family);
                    }

                    return families.OrderBy(f => f.Name)
                                   .ToList();
                }

                return [];
            }).ConfigureAwait(true).GetAwaiter().GetResult();

            SearchResult = searchResult.Select(family => (IFamilyViewModel)_familyFactory(family))
                                       .ToList();
        }
        else
        {
            SearchResult = null;
            IsActiveSearch = false;
        }
    }
}
