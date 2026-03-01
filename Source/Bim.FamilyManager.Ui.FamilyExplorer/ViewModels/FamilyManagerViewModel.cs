using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Ui.Views.Settings;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Scotec.Events.WeakEvents;
using Scotec.Extensions.Linq;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.FamilyExplorer.ViewModels;

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
    private readonly string? _logo;
    private readonly AsyncRelayCommand _reloadCommand;
    private readonly FamilySourceViewModel.Factory _sourceFactory;
    private IEnumerable<IFamilySourceViewModel>? _familySources;
    private bool _isActiveSearch;
    private string _searchPattern = string.Empty;
    private IList<IFamilyViewModel>? _searchResult;
    private IFamilySourceViewModel? _selectedFamilySource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManagerViewModel" /> class.
    /// </summary>
    /// <param name="familyManager">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IFamilyManager" /> instance responsible for managing
    ///     families.
    /// </param>
    /// <param name="sourceFactory">
    ///     A factory delegate for creating instances of <see cref="FamilySourceViewModel" />.
    /// </param>
    /// <param name="familyFactory">
    ///     A factory delegate for creating instances of <see cref="FamilyViewModel" />.
    /// </param>
    /// <param name="options">
    ///     The <see cref="Microsoft.Extensions.Options.IOptions{TOptions}" /> instance containing configuration settings for
    ///     <see cref="FamilyManagerOptions" />.
    /// </param>
    /// <param name="settingsManagerWindowFactory">
    ///     A factory delegate for creating instances of <see cref="SettingsManagerWindow" />.
    /// </param>
    public FamilyManagerViewModel(IFamilyManager familyManager,
                                  FamilySourceViewModel.Factory sourceFactory,
                                  FamilyViewModel.Factory familyFactory,
                                  IOptions<FamilyManagerOptions> options,
                                  SettingsManagerWindow.Factory settingsManagerWindowFactory
    )
    {
        SettingsManagerWindowFactory = settingsManagerWindowFactory;
        _familyManager = familyManager;
        _sourceFactory = sourceFactory;
        _familyFactory = familyFactory;

        _logo = options.Value.Logo;
        StaticWeakEventManager.AddWeakHandler(_familyManager, nameof(_familyManager.Reloaded), OnReloaded);
        _reloadCommand = new AsyncRelayCommand(async () =>
        {
            await _familyManager.ReloadAsync();
        });
    }

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
        get => _selectedFamilySource;
        private set
        {
            if (_selectedFamilySource == value)
            {
                return;
            }

            if (_selectedFamilySource is not null)
            {
                _selectedFamilySource.IsSelected = false;
            }

            if (value is not null)
            {
                value.IsSelected = true;
            }

            SetProperty(ref _selectedFamilySource, value);
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
    /// <value>
    ///     A <see cref="string" /> representing the search pattern used for filtering families.
    /// </value>
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
        get => _isActiveSearch;
        set => SetProperty(ref _isActiveSearch, value);
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
    /// <value>
    ///     A list of <see cref="FamilyViewModel" /> objects representing the search results, or <c>null</c>
    ///     if no search is active.
    /// </value>
    public IList<IFamilyViewModel>? SearchResult
    {
        get => _searchResult;
        private set => SetProperty(ref _searchResult, value);
    }

    /// <summary>
    ///     Gets the logo image for the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This property retrieves the logo image specified in the application settings.
    ///     If the logo file path is invalid or the file does not exist, it returns <c>null</c>.
    /// </remarks>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the logo image, or <c>null</c> if the logo is not available.
    /// </value>
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
    /// <value>
    ///     A collection of <see cref="FamilySourceViewModel" /> objects representing the available
    ///     family sources. Returns <c>null</c> if the collection has not been initialized.
    /// </value>
    public IEnumerable<IFamilySourceViewModel> FamilySources
    {
        get
        {
            if (_familySources is null)
            {
                _familySources = _familyManager.FamilySources.Select(source => (IFamilySourceViewModel)_sourceFactory(source)).ToList();

                _familySources.ForAll(g => StaticWeakEventManager.AddWeakHandler<IFamilySourceViewModel, PropertyChangedEventArgs>(g, nameof(g.PropertyChanged),
                    (source, args) =>
                    {
                        if (args.PropertyName == nameof(IFamilySourceViewModel.IsSelected))
                        {
                            if (source.IsSelected)
                            {
                                SelectedFamilySource = source;
                            }
                        }
                    }));

                var selectedSource = _familySources.FirstOrDefault(g => g.IsSelected) ?? _familySources.FirstOrDefault();
                if (selectedSource is not null)
                {
                    selectedSource.IsSelected = true;
                }
            }

            return _familySources;
        }
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
        SelectedFamilySource = null;

        _familySources = null;
        OnPropertyChanged(nameof(FamilySources));
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
