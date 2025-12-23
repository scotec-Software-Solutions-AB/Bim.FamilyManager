using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing display settings in the Family Manager UI.
/// </summary>
/// <remarks>
///     This class is responsible for handling the display-related settings, including layout options and their selection.
///     It inherits from <see cref="SettingsBaseViewModel" /> and provides specific implementations for display settings.
/// </remarks>
public class DisplaySettingsViewModel : SettingsBaseViewModel
{
    private static readonly Uri DefaultImageUri =
        new("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/DisplayPrimary_24x24.png");

    private static readonly Uri SelectionImageUri =
        new("pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/DisplayWhite_24x24.png");

    private readonly ILayoutOptionsViewModel.Factory _layoutOptionsFactory;
    private readonly DisplayOptions _options;
    private ILayoutOptionsViewModel? _selectedLayoutOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisplaySettingsViewModel" /> class.
    /// </summary>
    /// <param name="layoutOptionsFactory">
    ///     A factory delegate for creating instances of <see cref="ILayoutOptionsViewModel" />.
    /// </param>
    /// <param name="options">
    ///     An <see cref="IOptionsMonitor{TOptions}" /> instance for monitoring changes to <see cref="DisplayOptions" />.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the dependencies required for managing display settings,
    ///     including the layout options factory and the display options configuration.
    /// </remarks>
    public DisplaySettingsViewModel(ILayoutOptionsViewModel.Factory layoutOptionsFactory, IOptionsSnapshot<DisplayOptions> options)
    {
        _layoutOptionsFactory = layoutOptionsFactory;
        _options = options.Value;
    }

    /// <summary>
    ///     Gets the collection of layout options available for the display settings.
    /// </summary>
    /// <remarks>
    ///     This property provides a list of layout options that can be selected for configuring the Family Manager UI.
    ///     Each layout option is represented by an instance of <see cref="ILayoutOptionsViewModel" />.
    ///     The collection is initialized during the view model's initialization process.
    /// </remarks>
    public IList<ILayoutOptionsViewModel> LayoutOptions { get; private set; } = [];

    /// <summary>
    ///     Gets or sets the currently selected layout option in the display settings.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="ILayoutOptionsViewModel" /> representing the selected layout option.
    /// </value>
    /// <remarks>
    ///     This property is bound to the UI to reflect and update the user's selection of layout options.
    ///     It plays a crucial role in determining the active layout configuration in the Family Manager.
    /// </remarks>
    public ILayoutOptionsViewModel? SelectedLayoutOptions
    {
        get => _selectedLayoutOptions;
        set => SetProperty(ref _selectedLayoutOptions, value);
    }

    /// <summary>
    ///     Gets the unique identifier for the display settings view model.
    /// </summary>
    /// <remarks>
    ///     This property overrides the <see cref="SettingsBaseViewModel.Id" /> property to provide
    ///     a specific identifier for the <see cref="DisplaySettingsViewModel" /> class.
    /// </remarks>
    public override int Id { get; } = 1;

    /// <summary>
    ///     Gets the name of the display settings view model.
    /// </summary>
    /// <remarks>
    ///     This property provides a user-friendly name for the <see cref="DisplaySettingsViewModel" /> class,
    ///     which is used to identify the display settings in the Family Manager UI.
    /// </remarks>
    public override string Name { get; } = "Display";

    /// <summary>
    ///     Retrieves the display options for the Family Manager UI.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="DisplayOptions" /> containing the selected layout and available layout options.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetOptions" /> method to provide
    ///     display-specific options, including the selected layout and a list of available layouts.
    /// </remarks>
    public override object GetOptions()
    {
        return new DisplayOptions
        {
            FamilyManagerLayout = SelectedLayoutOptions?.Key ?? "FamilyExplorerLayout",
            Layouts = LayoutOptions.Select(o => o.GetOptions()).ToList()
        };
    }

    /// <summary>
    ///     Retrieves the default image URI for the display settings view model.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri" /> representing the default image associated with the display settings.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetDefaultImage" /> method to provide
    ///     a specific default image for the <see cref="DisplaySettingsViewModel" /> class.
    /// </remarks>
    protected override Uri GetDefaultImage()
    {
        return DefaultImageUri;
    }

    /// <summary>
    ///     Retrieves the URI of the selection image for the display settings view model.
    /// </summary>
    /// <returns>
    ///     A <see cref="Uri" /> representing the selection image associated with the display settings.
    /// </returns>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.GetSelectionImage" /> method to provide
    ///     a specific selection image for the <see cref="DisplaySettingsViewModel" /> class.
    /// </remarks>
    protected override Uri GetSelectionImage()
    {
        return SelectionImageUri;
    }

    /// <summary>
    ///     Initializes the display settings view model by setting up layout options and selecting the default layout.
    /// </summary>
    /// <remarks>
    ///     This method overrides the <see cref="SettingsBaseViewModel.OnInitialize" /> method to provide
    ///     initialization logic specific to the <see cref="DisplaySettingsViewModel" /> class. It populates the
    ///     <see cref="LayoutOptions" /> collection using the available layouts from <see cref="DisplayOptions" /> and
    ///     sets the <see cref="SelectedLayoutOptions" /> to the default layout based on the family manager configuration.
    /// </remarks>
    protected override void OnInitialize()
    {
        LayoutOptions = _options.Layouts.Select(o => _layoutOptionsFactory(o.Key, o)).ToList();
        SelectedLayoutOptions = LayoutOptions.FirstOrDefault(o => o.Key == _options.FamilyManagerLayout);
    }
}
