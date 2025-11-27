using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bim.FamilyManager.Abstractions.ViewModels.Settings;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Base.Settings;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Scotec.Extensions.Linq;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.ViewModels.Settings;

/// <summary>
///     Represents the view model for managing application settings in the Family Manager.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and interact with application settings, including
///     initialization, selection, and commands for saving and closing settings. It also handles the
///     display of a logo image associated with the settings.
/// </remarks>
public class SettingsManagerViewModel : ViewModel
{
    private readonly RelayCommand _closeCommand;
    private readonly string? _logo;
    private readonly RelayCommand _saveCommand;
    private readonly SettingsManager _settingsManager;
    private ISettingsViewModel _selectedSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsManagerViewModel" /> class.
    /// </summary>
    /// <param name="settingsManager">
    ///     An instance of <see cref="SettingsManager" /> responsible for managing application settings.
    /// </param>
    /// <param name="settings">
    ///     A collection of <see cref="Abstractions.ViewModels.Settings.ISettingsViewModel" /> representing the
    ///     available settings to be managed.
    /// </param>
    /// <param name="options">
    ///     An instance of <see cref="Microsoft.Extensions.Options.IOptions{TOptions}" /> containing the
    ///     <see cref="FamilyManagerOptions" />
    ///     used to configure the settings manager, including the logo.
    /// </param>
    /// <remarks>
    ///     This constructor initializes the settings manager view model by setting up commands, initializing settings, and
    ///     selecting the first available setting.
    /// </remarks>
    public SettingsManagerViewModel(SettingsManager settingsManager, IEnumerable<ISettingsViewModel> settings,
                                    IOptionsSnapshot<FamilyManagerOptions> options)
    {
        _settingsManager = settingsManager;
        _closeCommand = new RelayCommand(OnClose);
        _saveCommand = new RelayCommand(OnSave);
        _logo = options.Value.Logo;

        Settings = settings.OrderBy(ts => ts.Id).ToList();
        Settings.ForAll(settings => settings.Initialize());

        var first = Settings.First();
        first.IsSelected = true;
        _selectedSettings = first;
    }

    /// <summary>
    ///     Gets the logo image associated with the settings.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the logo image, or <c>null</c> if the logo is not specified
    ///     or the corresponding file does not exist.
    /// </value>
    /// <remarks>
    ///     The logo image is loaded from a file specified in the application settings. If the file path is invalid
    ///     or the file does not exist, this property returns <c>null</c>.
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
    ///     Gets or sets the action to be invoked when the settings view is closed.
    /// </summary>
    /// <remarks>
    ///     This property allows assigning a custom action that will be executed when the settings view is closed.
    ///     It is typically used to handle cleanup or navigation logic upon closing the settings window.
    /// </remarks>
    public Action? CloseAction { get; set; }

    /// <summary>
    ///     Gets or sets the currently selected settings view model.
    /// </summary>
    /// <remarks>
    ///     This property represents the settings view model that is currently selected by the user.
    ///     It is bound to the UI components, such as a <see cref="System.Windows.Controls.ListBox" />
    ///     for displaying the list of available settings and a <see cref="System.Windows.Controls.ContentControl" />
    ///     for displaying the details of the selected settings.
    /// </remarks>
    public ISettingsViewModel SelectedSettings
    {
        get => _selectedSettings;
        set => SetProperty(ref _selectedSettings, value);
    }

    /// <summary>
    ///     Gets the collection of settings view models managed by this view model.
    /// </summary>
    /// <remarks>
    ///     The collection is initialized and sorted by the identifier of each settings view model.
    ///     Each settings view model in the collection is also initialized during the construction of
    ///     the <see cref="SettingsManagerViewModel" />. This property is used to bind the settings
    ///     to the UI and manage their interactions.
    /// </remarks>
    public IList<ISettingsViewModel> Settings { get; }

    /// <summary>
    ///     Gets the command that saves the current settings in the Family Manager.
    /// </summary>
    /// <remarks>
    ///     This command is bound to the "Save" button in the settings view and executes the logic
    ///     for persisting the current settings. It ensures that any changes made to the settings
    ///     are properly saved and applied.
    /// </remarks>
    public ICommand SaveCommand => _saveCommand;

    /// <summary>
    ///     Gets the command that is executed to close the settings manager window.
    /// </summary>
    /// <remarks>
    ///     This command is typically bound to a UI element, such as a button, to trigger the closing
    ///     of the settings manager. The associated action is defined in the <see cref="OnClose" /> method.
    /// </remarks>
    public ICommand CloseCommand => _closeCommand;

    /// <summary>
    ///     Handles the logic for closing the settings manager window.
    /// </summary>
    /// <remarks>
    ///     This method invokes the <see cref="CloseAction" /> delegate if it is set, allowing the
    ///     settings manager to perform any necessary cleanup or finalization before closing.
    /// </remarks>
    private void OnClose()
    {
        CloseAction?.Invoke();
    }

    /// <summary>
    ///     Saves the current settings and triggers the associated close action.
    /// </summary>
    /// <remarks>
    ///     This method collects the options from all settings view models, saves them using the
    ///     <see cref="SettingsManager.SaveSettings" /> method,
    ///     and then invokes the <see cref="CloseAction" /> to close the view.
    /// </remarks>
    private void OnSave()
    {
        var settingsToSave = Settings.Select(ts => ts.GetOptions()).ToList();
        _settingsManager.SaveSettings(settingsToSave);

        // Close the view after saving
        CloseAction?.Invoke();
    }
}
