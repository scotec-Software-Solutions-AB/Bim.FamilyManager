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
public class FamilySourcesOptionsSetup : IConfigureOptions<FamilySourcesOptions>
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourcesOptionsSetup" /> class.
    /// </summary>
    /// <param name="configuration">
    ///     The configuration instance used to retrieve the "FamilySources:Sources" section.
    /// </param>
    /// <param name="services">
    ///     The service provider used to resolve dependencies for configuring family source options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the necessary dependencies for configuring the <see cref="FamilySourcesOptions" />.
    /// </remarks>
    public FamilySourcesOptionsSetup(IConfiguration configuration, IServiceProvider services)
    {
        _configuration = configuration;
        _services = services;
    }

    /// <summary>
    ///     Configures the <see cref="FamilySourcesOptions" /> by populating its <see cref="FamilySourcesOptions.Sources" />
    ///     collection
    ///     based on the "FamilySources:Sources" section in the configuration.
    /// </summary>
    /// <param name="options">The <see cref="FamilySourcesOptions" /> instance to configure.</param>
    /// <remarks>
    ///     This method retrieves the configuration for family sources, resolves the appropriate options types,
    ///     and adds them to the <see cref="FamilySourcesOptions.Sources" /> collection.
    ///     If a type cannot be resolved or the options cannot be cast to <see cref="IFamilySourceOptions" />, the method skips
    ///     the entry.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="options" /> parameter is <c>null</c>.
    /// </exception>
    /// <example>
    ///     The following example demonstrates how this method is used:
    ///     <code>
    /// var optionsSetup = new FamilySourcesOptionsSetup(configuration, serviceProvider);
    /// var options = new FamilySourcesOptions();
    /// optionsSetup.Configure(options);
    /// </code>
    /// </example>
    public void Configure(FamilySourcesOptions options)
    {
        var familySourcesSection = _configuration.GetSection("FamilySources:Sources");
        foreach (var sourceSection in familySourcesSection.GetChildren())
        {
            var optionsType = _services.GetKeyedService<Type>(sourceSection.GetValue<string>("Type") + "Options");
            if (optionsType is null)
            {
                // TODO: Logging
                continue;
            }

            var soureOptions = sourceSection.Get(optionsType!) as IFamilySourceOptions;
            if (soureOptions is null)
            {
                // TODO: Logging
                continue;
            }

            options.Sources.Add(soureOptions);
        }
    }
}
