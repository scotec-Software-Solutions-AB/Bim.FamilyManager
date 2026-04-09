using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.UI;
using Autofac;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Commands;
using Bim.FamilyManager.Modules;
using Bim.FamilyManager.Ui.Resources;
using Bim.FamilyManager.Ui.Views;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scotec.Extensions.Linq;
using Scotec.Revit;
using Scotec.Revit.Isolation;
using Scotec.Revit.Ui;
using Scotec.Wpf.ViewModels;

[assembly: RevitAddinIsolationContext(ContextName = "Bim.FamilyManager")]

namespace Bim.FamilyManager;

/// <summary>
///     Represents the main application class for the Revit Family Manager.
///     This class is responsible for initializing and configuring the application,
///     including setting up tabs, panels, and commands within the Revit UI.
/// </summary>
/// <remarks>
///     The <see cref="RevitFamilyManagerApp" /> class extends the functionality of the Revit application
///     by leveraging dependency injection, service registration, and UI customization.
///     It uses Autofac for dependency injection and integrates with Revit's API to provide
///     a seamless user experience.
///     The class runs in its own <see cref="System.Runtime.Loader.AssemblyLoadContext" /> to ensure isolation,
///     preventing potential conflicts with other assemblies.
/// </remarks>
[RevitApplicationIsolation(ContextName = "Bim.FamilyManager")]
public class RevitFamilyManagerApp : RevitApp
{
    private ILogger? _logger;
    private FamilyManagerPane? _pane;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilyManagerApp" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor is responsible for setting up the logging environment for the application.
    ///     It creates a dedicated log directory within the assembly's location and configures the log file path.
    ///     The logging configuration is stored in the global context to ensure consistent logging behavior
    ///     throughout the application.
    /// </remarks>
    public RevitFamilyManagerApp()
    {
        var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
        var logDirectory = Path.Combine(assemblyPath!, "Log");
        Directory.CreateDirectory(logDirectory);
        GlobalContext.Properties["FamilyManagerLogFile"] = Path.Combine(logDirectory, "Scotec.FamilyManager.log");
    }

    /// <summary>
    ///     Handles the shutdown process of the Revit Family Manager application.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the shutdown process completed successfully.
    /// </returns>
    /// <remarks>
    ///     This method is invoked when the application is shutting down. It provides an opportunity
    ///     to release resources, unregister services, or perform any necessary cleanup tasks.
    /// </remarks>
    protected override bool OnShutdown()
    {
        return true;
    }

    /// <summary>
    ///     Initializes the Revit Family Manager application by setting up the user interface components,
    ///     including tabs, panels, and buttons, and registering the dockable pane for the Family Manager Panel.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the startup process was successful.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for creating the necessary UI elements such as tabs and panels
    ///     using <see cref="TabManager" /> and registering the <see cref="FamilyManagerPane" /> as a dockable pane.
    ///     It also initializes the required services from the dependency injection container.
    /// </remarks>
    protected override bool OnStartup()
    {
        _logger = Services.GetRequiredService<ILogger<RevitFamilyManagerApp>>();
        _logger.LogInformation("Running startup for family manager add-in.");

        var options = Services.GetRequiredService<IOptions<FamilyManagerOptions>>().Value;
        var tabName = options.RevitOptions?.TabName;
        var panelName = options.RevitOptions?.PanelName ?? "Bim.FamilyManager";

        // Create tabs, panels ond buttons
        //RevitTabManager.CreateTab(Application ?? throw new InvalidOperationException("Application is null."), StringResources.Tab_Name);

        RibbonPanel panel;
        if (string.IsNullOrEmpty(tabName))
        {
            panel = RevitTabManager.GetPanel(Application!, panelName, Tab.AddIns);
        }
        else
        {
            RevitTabManager.CreateTab(Application ?? throw new InvalidOperationException("Application is null."), tabName);
            panel = RevitTabManager.GetPanel(Application, panelName, tabName);
        }

        panel.AddItem(CreateFamilyManagerButtonData());
        panel.AddItem(CreateSettingsButtonData());
        panel.AddItem(CreatePreviewImageButtonData());

        _pane = Services.GetRequiredService<FamilyManagerPane>();

        Application!.RegisterDockablePane(new DockablePaneId(Constants.PaneId), StringResources.Pane_Name, _pane);

        return true;
    }

    /// <summary>
    ///     Configures the application by setting up dependency injection and service registration.
    /// </summary>
    /// <param name="builder">
    ///     An <see cref="IHostBuilder" /> instance used to configure the application's services and dependency container.
    /// </param>
    /// <remarks>
    ///     This method is invoked during the startup process of the application. It utilizes Autofac modules
    ///     and service registration to configure the application's dependencies. The method allows for the
    ///     registration of services, customization of the dependency injection container, and configuration
    ///     of application-specific settings.
    /// </remarks>
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        // Load dependend plugin assemblies.
        LoadDependentAssemblies();

        // Register services by using Autofac modules.
        builder.ConfigureContainer<ContainerBuilder>(containerBuilder => containerBuilder.RegisterModule<RegistrationModule>());
        builder.ConfigureLogging();
        builder.ConfigureSettings();
        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddViewModelTemplateSelector();
            // You can also register services here.
            // However, using Autofac modules gives you more flexibility.

            // Register FamilyManagerOptions with custom setup
            //services.AddOptions<FamilyManagerOptions>();
            // Register the custom IConfigureOptions implementation
            //services.AddSingleton<IConfigureOptions<FamilyManagerOptions>, FamilyManagerOptionsSetup>();

            services.Configure<FamilyManagerOptions>(hostContext.Configuration.GetSection("FamilyManager"));
            services.Configure<SettingsManagerOptions>(hostContext.Configuration.GetSection("SettingsManager"));

            // Register a RevitTask.
            // This service is used to handle external events within the Revit context,
            // enabling asynchronous operations to be executed in the Revit API environment.
            // It ensures that tasks requiring interaction with Revit's UI or model
            // are executed safely and efficiently within the appropriate context.
            services.AddSingleton(new RevitTask());
        });
    }

    /// <summary>
    ///     Loads dependent plugin assemblies required for the application to function.
    /// </summary>
    /// <remarks>
    ///     This method is responsible for dynamically loading assemblies specified in the application's configuration file.
    ///     It ensures that all required modules are loaded into the current
    ///     <see cref="System.Runtime.Loader.AssemblyLoadContext" />.
    ///     The configuration is expected to include a "Modules" section listing the assemblies to be loaded.
    /// </remarks>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Thrown if the configuration file "appsettings.json" is not found in the application's directory.
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the assembly names specified in the "Modules" section of the configuration are null.
    /// </exception>
    /// <example>
    ///     Example of a "Modules" section in the configuration file:
    ///     <code>
    /// {
    ///   "Modules": [
    ///     "AssemblyName1",
    ///     "AssemblyName2"
    ///   ]
    /// }
    /// </code>
    /// </example>
    private static void LoadDependentAssemblies()
    {
        // Load configuration
        var appSettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        // Determine environment (default to "Production" if not set)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
                            .AddJsonFile(Path.Combine(appSettingsPath, "appsettings.json"), false, false)
                            .AddJsonFile(Path.Combine(appSettingsPath, $"appsettings.{environment}.json"), true, false)
                            .Build();

        var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        if (context is null)
        {
            return;
        }

        // Get module types from configuration
        //TODO: Support wildcards or regex.
        configuration.GetSection("Modules").Get<string[]>()?.ForAll(a => context.LoadFromAssemblyName(new AssemblyName(a)));
    }

    /// <summary>
    ///     Creates the <see cref="PushButtonData" /> instance for the Family Manager button in the Revit UI.
    /// </summary>
    /// <remarks>
    ///     This method configures the button's properties, including its name, text, description, and associated images.
    ///     It also specifies the command and availability factories required for the button's functionality.
    /// </remarks>
    /// <returns>
    ///     A <see cref="PushButtonData" /> object representing the Family Manager button configuration.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if required resources or configurations are missing during the creation of the button data.
    /// </exception>
    private static PushButtonData CreateFamilyManagerButtonData()
    {
        var smallImageSource = BuildImageResourcePath("FamilyManagerPrimary_16x16.png");
        var largeImageSource = BuildImageResourcePath("FamilyManagerPrimary_32x32.png");

        var pushButtonData = RevitControlFactory.CreateButtonData("FamilyManager.Open"
            , Resources.StringResources.Command_OpenFamilyManager_Text
            , Resources.StringResources.Command_OpenFamilyManager_Description
            , smallImageSource
            , largeImageSource
            , typeof(OpenFamilyManagerCommandFactory));

        return pushButtonData;
    }

    /// <summary>
    ///     Creates the <see cref="PushButtonData" /> instance for the settings button in the Revit Family Manager UI.
    /// </summary>
    /// <returns>
    ///     A <see cref="PushButtonData" /> object configured with the necessary properties, such as name, text, description,
    ///     image resources, and the associated command factory type.
    /// </returns>
    /// <remarks>
    ///     This method is responsible for defining the settings button's appearance and behavior within the Revit UI.
    ///     It specifies the button's small and large images, text, description, and the command factory type that handles
    ///     the button's functionality.
    /// </remarks>
    private static PushButtonData CreateSettingsButtonData()
    {
        var smallImageSource = BuildImageResourcePath("SettingsPrimary_16x16.png");
        var largeImageSource = BuildImageResourcePath("SettingsPrimary_32x32.png");

        var pushButtonData = RevitControlFactory.CreateButtonData("FamilyManager.Settings"
            , Resources.StringResources.Command_OpenFamilyManagerSettings_Text
            , Resources.StringResources.Command_OpenFamilyManagerSettings_Description
            , smallImageSource
            , largeImageSource
            , typeof(OpenFamilyManagerSettingsCommandFactory)
            , typeof(OpenFamilyManagerSettingsCommandAvailabilityFactory));

        return pushButtonData;
    }

    private static PushButtonData CreatePreviewImageButtonData()
    {
        var smallImageSource = BuildImageResourcePath("CreatePreviewPrimary_16x16.png");
        var largeImageSource = BuildImageResourcePath("CreatePreviewPrimary_32x32.png");

        var pushButtonData = RevitControlFactory.CreateButtonData("FamilyManager.CreatePreviewImage"
            , Resources.StringResources.Command_CreatePreviewImage_Text
            , Resources.StringResources.Command_CreatePreviewImage_Description
            , smallImageSource
            , largeImageSource
            , typeof(CreatePreviewImageCommandFactory)
            , typeof(CreatePreviewImageCommandAvailabilityFactory));

        return pushButtonData;
    }

    /// <summary>
    ///     Builds the URI path for an image resource within the application.
    /// </summary>
    /// <param name="imageFileName">
    ///     The name of the image file, including its extension (e.g., "example.png").
    /// </param>
    /// <returns>
    ///     A <see cref="Uri" /> representing the resource path to the specified image file.
    /// </returns>
    /// <remarks>
    ///     The method constructs a URI using the "pack" URI scheme to locate the image
    ///     resource within the application's assembly. This is typically used for embedding
    ///     resources such as icons or images in WPF applications.
    /// </remarks>
    /// <exception cref="System.UriFormatException">
    ///     Thrown if the constructed URI is not valid.
    /// </exception>
    private static Uri BuildImageResourcePath(string imageFileName)
    {
        return new Uri($"pack://application:,,,/Bim.FamilyManager.Ui;component/Resources/Images/{imageFileName}");
    }
}
