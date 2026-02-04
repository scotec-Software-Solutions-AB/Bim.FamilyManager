using System.IO;
using System.Reflection;
using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Base.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager;

/// <summary>
///     Provides extension methods for configuring an <see cref="IHostBuilder" /> with logging and settings functionality.
/// </summary>
/// <remarks>
///     This static class includes methods to enhance the <see cref="IHostBuilder" /> by adding custom logging
///     and settings initialization. It simplifies the setup process for applications by integrating
///     configuration and logging providers, as well as initializing application-specific settings.
/// </remarks>
public static class HostBuilderExtensions
{
    /// <summary>
    ///     Configures logging for the specified <see cref="IHostBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure logging for.</param>
    /// <returns>The same <see cref="IHostBuilder" /> instance, allowing for method chaining.</returns>
    /// <remarks>
    ///     This method sets up logging for the application by:
    ///     - Adding logging configuration from the application's configuration.
    ///     - Clearing default logging providers.
    ///     - Adding a Log4Net provider with a configuration file located in the same directory as the assembly.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var hostBuilder = Host.CreateDefaultBuilder();
    /// hostBuilder.ConfigureLogging();
    /// </code>
    /// </example>
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        builder.ConfigureLogging((context, loggingBuilder) =>
        {
            var path = Path.GetDirectoryName(typeof(HostBuilderExtensions).Assembly.Location);

            loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
            loggingBuilder.ClearProviders(); // Clear default providers
            loggingBuilder.AddLog4Net(new Log4NetProviderOptions(Path.Combine(path!, "log4net.config"), false));
        });

        return builder;
    }

    /// <summary>
    ///     Configures the settings for the specified <see cref="IHostBuilder" /> instance.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> instance to configure.</param>
    /// <returns>The configured <see cref="IHostBuilder" /> instance.</returns>
    /// <remarks>
    ///     This method initializes application-specific settings by loading configuration files and
    ///     registering necessary services. It simplifies the setup process by integrating settings
    ///     management into the host builder.
    /// </remarks>
    public static IHostBuilder ConfigureSettings(this IHostBuilder builder)
    {
        builder.InitializeSettings();

        return builder;
    }

    /// <summary>
    ///     Initializes the application settings for the specified <see cref="IHostBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure with the application settings.</param>
    /// <remarks>
    ///     This method sets up the application configuration by loading the base configuration from
    ///     the "appsettings.json" file and additional user-specific settings. It also configures services
    ///     based on the settings sections and their corresponding types.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the settings file path is not specified in the application configuration.
    /// </exception>
    private static void InitializeSettings(this IHostBuilder builder)
    {
        var appSettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        // Determine environment (default to "Production" if not set)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        // Build the configuration
        var configuration = new ConfigurationBuilder()
                            .AddJsonFile(Path.Combine(appSettingsPath, "appsettings.json"), false, false)
                            .AddJsonFile(Path.Combine(appSettingsPath, $"appsettings.{environment}.json"), true, false)
                            .Build();

        var options = configuration.GetSection("SettingsManager").Get<SettingsManagerOptions>();
        if (string.IsNullOrWhiteSpace(options?.SettingsFile))
        {
            throw new InvalidOperationException("Settings file path is not specified in the application settings.");
        }

        var userSettingsFile = SettingsManager.InitializeSettings(configuration, out var optionsTypes);

        builder.ConfigureAppConfiguration(configBuilder => { configBuilder.AddJsonFile(userSettingsFile, false, true); });

        builder.ConfigureServices((hostContext, services) =>
        {
            foreach (var (sectionName, type) in optionsTypes)
            {
                //TODO: Probably use naming conventions to avoid the switch and the need to add new cases for new sections.
                switch (sectionName)
                {
                    case "FamilySources":
                    {
                        services.AddSingleton<IConfigureOptions<FamilySourcesOptions>, FamilySourcesOptionsSetup>();
                        break;
                    }
                    case "Display":
                    {
                        services.AddSingleton<IConfigureOptions<DisplayOptions>, DisplayOptionsSetup>();
                        break;
                    }
                }

                services.Configure(type, hostContext.Configuration.GetSection(sectionName));
            }

            // Iterates through all layout option types provided by SettingsManager,
            // constructs the corresponding configuration section name for each layout,
            // and configures the service collection to bind each layout option type
            // to its respective configuration section. This enables dynamic registration
            // of layout-specific settings for the application.
            foreach (var type in SettingsManager.GetLayoutOptionTypes())
            {
                var sectionName = "Display:Layouts:" + type.Key;
                services.Configure(type.Value, hostContext.Configuration.GetSection(sectionName));
            }
        });
    }

    /// <summary>
    ///     Configures the specified <see cref="IServiceCollection" /> with options of the given type
    ///     using the provided configuration section.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to configure.</param>
    /// <param name="optionsType">The type of the options to configure.</param>
    /// <param name="section">The configuration section containing the options data.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the generic <c>Configure&lt;T&gt;</c> method cannot be found.
    /// </exception>
    /// <remarks>
    ///     This method dynamically invokes the generic <c>Configure&lt;T&gt;</c> method from
    ///     <see cref="OptionsConfigurationServiceCollectionExtensions" /> to bind the configuration
    ///     section to the specified options type.
    /// </remarks>
    private static void Configure(this IServiceCollection services, Type optionsType, IConfigurationSection section)
    {
        // Get the generic Configure<T> method
        var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod("Configure", [typeof(IServiceCollection), typeof(IConfiguration)]);

        if (configureMethod == null)
        {
            throw new InvalidOperationException("Unable to find Configure<T> method.");
        }

        // Make the method generic with the runtime type
        var genericMethod = configureMethod.MakeGenericMethod(optionsType);
        // Invoke the method with the provided services and configuration section
        genericMethod.Invoke(null, [services, section]);
    }
}
