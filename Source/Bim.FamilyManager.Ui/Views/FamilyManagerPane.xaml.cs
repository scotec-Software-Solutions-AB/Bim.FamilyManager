using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scotec.Revit.Wpf;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.Views;

/// <summary>
///     Represents the pane for managing Revit families within the application.
/// </summary>
/// <remarks>
///     This class serves as a WPF page and implements the <see cref="Autodesk.Revit.UI.IDockablePaneProvider" /> interface
///     to integrate with Revit's dockable pane system. It provides functionality for displaying and interacting with
///     Revit family data through the associated view model.
/// </remarks>
public partial class FamilyManagerPane : RevitPage, IDockablePaneProvider
{
    private readonly IServiceProvider _serviceProvider;
    private string _displayType = string.Empty;
    private Document? _document;
    private IFamilyManagerViewModel? _familyManagerViewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyManagerPane" /> class.
    /// </summary>
    /// <param name="templateSelector">
    ///     The <see cref="GlobalViewModelTemplateSelector" /> used to select templates for the view.
    /// </param>
    /// <param name="serviceProvider">
    ///     The <see cref="IServiceProvider" /> used to resolve dependencies within the pane.
    /// </param>
    /// <remarks>
    ///     This constructor initializes the <see cref="FamilyManagerPane" /> by setting up its data context
    ///     and template selector. It enables interaction with Revit family data through the associated
    ///     view model, which is resolved using the provided <paramref name="serviceProvider" />.
    /// </remarks>
    public FamilyManagerPane(GlobalViewModelTemplateSelector templateSelector, IOptionsMonitor<DisplayOptions> displayOptions, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        displayOptions.OnChange(options => { BuildFamilyManagerViewModel(options.FamilyManagerLayout); });

        InitializeComponent();
        TemplateSelector = templateSelector;
        BuildFamilyManagerViewModel(displayOptions.CurrentValue.FamilyManagerLayout);
    }

    /// <summary>
    ///     Gets the <see cref="GlobalViewModelTemplateSelector" /> used to dynamically select
    ///     data templates for the views within the Family Manager pane.
    /// </summary>
    /// <remarks>
    ///     This property facilitates the customization of the visual representation of data
    ///     by mapping view models to their corresponding data templates.
    /// </remarks>
    public GlobalViewModelTemplateSelector TemplateSelector { get; }

    /// <summary>
    ///     Configures the dockable pane for the <see cref="FamilyManagerPane" /> within the Revit application.
    /// </summary>
    /// <param name="data">
    ///     The <see cref="DockablePaneProviderData" /> object used to define the pane's framework element and initial state.
    /// </param>
    /// <remarks>
    ///     This method sets up the <see cref="FamilyManagerPane" /> as a dockable pane in Revit, specifying its initial
    ///     docking position and tab placement relative to built-in dockable panes.
    /// </remarks>
    public void SetupDockablePane(DockablePaneProviderData data)
    {
        data.FrameworkElement = this;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Tabbed,
            TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
        };
    }

    /// <summary>
    ///     Initializes the <see cref="FamilyManagerPane" /> with the specified Revit application context.
    /// </summary>
    /// <param name="uiApplication">
    ///     The <see cref="UIApplication" /> instance representing the current Revit application context.
    /// </param>
    /// <remarks>
    ///     This method retrieves the active document and view from the Revit application context, allowing the pane
    ///     to interact with the current Revit project and display relevant information.
    /// </remarks>
    public void Initialize(UIApplication uiApplication)
    {
        _document = uiApplication.ActiveUIDocument.Document;

        // get the current document name
        var name = _document.PathName;
        // get the active view name
        var viewName = _document.ActiveView.Name;
    }

    /// <summary>
    ///     Builds and updates the view model for the Family Manager pane based on the specified layout.
    /// </summary>
    /// <param name="layout">
    ///     A <see cref="string" /> representing the layout type for the Family Manager.
    ///     This determines which implementation of <see cref="IFamilyManagerViewModel" /> is resolved and set as the data
    ///     context.
    /// </param>
    /// <remarks>
    ///     This method ensures that the view model is updated only when the layout changes. It uses the
    ///     <see cref="Dispatcher" /> to perform UI updates, such as setting the data context.
    ///     The view model is resolved from the <see cref="IServiceProvider" /> using the provided layout as a key.
    /// </remarks>
    private void BuildFamilyManagerViewModel(string layout)
    {
        if (_displayType == layout)
        {
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _displayType = layout;
            _familyManagerViewModel = _serviceProvider.GetRequiredKeyedService<IFamilyManagerViewModel>(layout);
            DataContext = _familyManagerViewModel;
        });
    }
}
