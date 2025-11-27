using Bim.FamilyManager.Abstractions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Provides the setup logic for configuring <see cref="FamilySourcesOptions" /> in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class is responsible for reading the "FamilySources:Sources" section from the configuration,
///     resolving the appropriate options types, and populating the <see cref="FamilySourcesOptions.Sources" /> collection.
/// </remarks>
public class DisplayOptionsSetup : IConfigureOptions<DisplayOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisplayOptionsSetup" /> class.
    /// </summary>
    /// <param name="configuration">
    ///     The configuration instance used to retrieve the "Display:Layouts" section and related settings.
    /// </param>
    /// <param name="services">
    ///     The service provider used to resolve dependencies, including keyed services for layout options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the dependencies required for configuring <see cref="DisplayOptions" />.
    ///     It ensures that layout options are properly resolved and populated based on the application's configuration.
    /// </remarks>
    public DisplayOptionsSetup(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
    }

    /// <summary>
    ///     Configures the <see cref="DisplayOptions" /> instance by populating its <see cref="DisplayOptions.Layouts" />
    ///     collection
    ///     with layout options retrieved from the "Display:Layouts" section of the configuration.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="DisplayOptions" /> instance to be configured.
    /// </param>
    /// <remarks>
    ///     This method iterates through the "Display:Layouts" configuration section, resolves the corresponding layout options
    ///     using the service provider, and adds them to the <see cref="DisplayOptions.Layouts" /> collection. If a layout
    ///     option
    ///     cannot be resolved or is invalid, it is skipped, and logging can be added for such cases.
    /// </remarks>
    public void Configure(DisplayOptions options)
    {
        var layoutSection = _configuration.GetSection("Display:Layouts");
        foreach (var sourceSection in layoutSection.GetChildren())
        {
            var optionsType = _services.GetKeyedService<Type>(sourceSection.Key);
            if (optionsType is null)
            {
                // TODO: Logging
                continue;
            }

            var sourceOptions = sourceSection.Get(optionsType!) as ILayoutOptions;
            if (sourceOptions is null)
            {
                // TODO: Logging
                continue;
            }

            options.Layouts.Add(sourceOptions);
        }
    }
}
