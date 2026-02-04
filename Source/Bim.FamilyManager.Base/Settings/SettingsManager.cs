using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scotec.Extensions.Utilities.Configuration;
using Scotec.Extensions.Utilities.Strings;
using SettingsManagerOptions = Bim.FamilyManager.Base.Options.SettingsManagerOptions;

namespace Bim.FamilyManager.Base.Settings;

/// <summary>
///     Provides functionality to manage application settings, including initialization, validation, and saving of
///     settings.
///     This class ensures that the settings file is properly configured and accessible before usage.
/// </summary>
public class SettingsManager
{
    private static Dictionary<string, Type>? s_sectionTypes;
    private static bool s_isInitialized;
    private static readonly JsonSerializerOptions JsonOptions;
    private readonly ILogger<SettingsManager> _logger;

    /// <summary>
    ///     Static constructor for the <see cref="SettingsManager" /> class.
    ///     Initializes static members, including the JSON serializer options used for handling settings serialization.
    /// </summary>
    /// <remarks>
    ///     The static constructor configures the <see cref="System.Text.Json.JsonSerializerOptions" /> instance
    ///     with custom converters for handling specific settings types, such as family source options and layout options.
    ///     It also enables indented JSON formatting for better readability.
    /// </remarks>
    static SettingsManager()
    {
        JsonOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonFamilySourceOptionsConverter(GetFamilySourceOptionsTypes()),
                new JsonLayoutOptionsConverter(GetLayoutOptionsTypes())
            },
            WriteIndented = true
        };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsManager" /> class.
    /// </summary>
    /// <param name="options">
    ///     An instance of <see cref="Microsoft.Extensions.Options.IOptions{TOptions}" /> containing the configuration options
    ///     for the <see cref="SettingsManager" />.
    /// </param>
    /// <param name="logger">
    ///     An instance of <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}" /> used for logging critical errors
    ///     and other information related to the settings manager.
    /// </param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the settings manager has not been initialized or when the settings file path is not specified in the
    ///     options.
    /// </exception>
    public SettingsManager(IOptions<SettingsManagerOptions> options, ILogger<SettingsManager> logger)
    {
        _logger = logger;

        if (!s_isInitialized || s_sectionTypes is null)
        {
            const string message = "Settings manager has not been initialized. Call SettingsManager.InitializeSettings() before using this class";
            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        Options = options.Value;

        if (string.IsNullOrWhiteSpace(Options.SettingsFile))
        {
            const string message = "Settings file path is not specified in the options.";
            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }
    }

    /// <summary>
    ///     Gets the configuration options for the <see cref="SettingsManager" />.
    /// </summary>
    /// <value>
    ///     An instance of <see cref="SettingsManagerOptions" /> containing the settings
    ///     file path and other configuration details required by the settings manager.
    /// </value>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the settings file path is not specified in the options during initialization.
    /// </exception>
    public SettingsManagerOptions Options { get; }

    /// <summary>
    ///     Initializes the settings manager using the provided configuration and returns the user settings file path.
    /// </summary>
    /// <param name="configuration">The application configuration containing settings manager options.</param>
    /// <param name="optionTypes">When this method returns, contains the dictionary of option types discovered.</param>
    /// <returns>The expanded path to the user settings file.</returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the settings manager is already initialized or if the settings file path is not specified.
    /// </exception>
    public static string InitializeSettings(IConfiguration configuration, out Dictionary<string, Type> optionTypes)
    {
        if (s_isInitialized)
        {
            const string message = "Settings manager has been already initialized.";
            throw new InvalidOperationException(message);
        }

        s_isInitialized = true;

        var sectionTypes = GetSectionTypes();

        var options = configuration.GetSection("SettingsManager").Get<SettingsManagerOptions>();
        if (string.IsNullOrWhiteSpace(options?.SettingsFile))
        {
            throw new InvalidOperationException("Settings file path is not specified in the application settings.");
        }

        var userSettingsFile = options.SettingsFile.ExpandVariables();

        InitializeSettings(userSettingsFile, sectionTypes);

        optionTypes = GetSectionTypes();

        return userSettingsFile;
    }

    /// <summary>
    ///     Saves the provided settings to the settings file specified in the options.
    /// </summary>
    /// <param name="settings">
    ///     A collection of settings objects to be saved. Each object should correspond to a type registered
    ///     in the settings manager's options types.
    /// </param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the settings file path is not specified in the options.
    /// </exception>
    /// <remarks>
    ///     This method serializes the provided settings into a JSON format and writes them to the settings file.
    ///     If the directory for the settings file does not exist, it will be created automatically.
    /// </remarks>
    public void SaveSettings(IEnumerable<object> settings)
    {
        if (string.IsNullOrWhiteSpace(Options.SettingsFile))
        {
            const string message = "Settings file path is not specified in the options.";
            _logger.LogCritical(message);
            throw new InvalidOperationException(message);
        }

        var result = settings.ToDictionary(
            setting => s_sectionTypes!.FirstOrDefault(pair => pair.Value == setting.GetType()).Key,
            setting => setting);

        SaveSettings(Options.SettingsFile, result, _logger);
    }

    /// <summary>
    ///     Retrieves a dictionary of layout option types defined in the loaded assemblies.
    /// </summary>
    /// <remarks>
    ///     This method scans all assemblies loaded in the current load context for types decorated with the
    ///     <see cref="LayoutOptionsAttribute" />. It then constructs a dictionary where the keys are the option names
    ///     specified in the <see cref="LayoutOptionsAttribute" /> and the values are the corresponding types.
    /// </remarks>
    /// <returns>
    ///     A dictionary where the keys are the option names defined in the <see cref="LayoutOptionsAttribute" /> and
    ///     the values are the types associated with those option names.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the current load context or any required assembly or type information is unavailable.
    /// </exception>
    public static Dictionary<string, Type> GetLayoutOptionTypes()
    {
        //// Get all loaded assemblies in the current load context
        var assemblies = AssemblyLoadContext.GetLoadContext(typeof(SettingsManager).Assembly)!.Assemblies.ToList();

        var layouts = assemblies.SelectMany(assembly => assembly.GetTypes())
                                .Where(type => type.GetCustomAttribute<LayoutOptionsAttribute>() != null)
                                .Select(type => (Type: type, Section: type.GetCustomAttribute<LayoutOptionsAttribute>()!.OptionsName));

        return layouts.ToDictionary(attributedType => attributedType.Section, attributedType => attributedType.Type);
    }

    /// <summary>
    ///     Retrieves a dictionary mapping section names to their corresponding types,
    ///     based on the <see cref="Extensions.Utilities.Configuration.SettingsSectionAttribute" /> applied to types in all
    ///     loaded assemblies.
    /// </summary>
    /// <returns>
    ///     A <see cref="Dictionary{TKey, TValue}" /> where the key is the section name (as defined in
    ///     <see cref="Extensions.Utilities.Configuration.SettingsSectionAttribute.SectionName" />) and the value is the
    ///     associated <see cref="Type" />.
    /// </returns>
    /// <remarks>
    ///     This method scans all assemblies loaded in the current load context for types decorated with
    ///     the <see cref="Extensions.Utilities.Configuration.SettingsSectionAttribute" />. It then constructs a dictionary
    ///     mapping the section names
    ///     specified in the attribute to their respective types.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the method is unable to retrieve the loaded assemblies or if the attribute is not properly configured.
    /// </exception>
    private static Dictionary<string, Type> GetSectionTypes()
    {
        //// Get all loaded assemblies in the current load context
        var assemblies = AssemblyLoadContext.GetLoadContext(typeof(SettingsManager).Assembly)!.Assemblies.ToList();

        // Search for types with the SettingsSectionAttribute in all assemblies
        s_sectionTypes = assemblies
                         .SelectMany(assembly => assembly.GetTypes())
                         .Where(type => type.GetCustomAttribute<SettingsSectionAttribute>() != null)
                         .Select(type => (Type: type, Section: type.GetCustomAttribute<SettingsSectionAttribute>()!.SectionName))
                         .ToDictionary(attributedType => attributedType.Section, attributedType => attributedType.Type);

        return s_sectionTypes;
    }

    /// <summary>
    ///     Retrieves a dictionary of family source option types, where the keys represent the option names
    ///     and the values represent the corresponding types. This method scans all loaded assemblies in the
    ///     current load context for types decorated with the <see cref="FamilySourceOptionsAttribute" />.
    /// </summary>
    /// <returns>
    ///     A dictionary where the keys are option names specified in the <see cref="FamilySourceOptionsAttribute" />
    ///     and the values are the corresponding types.
    /// </returns>
    /// <remarks>
    ///     This method is used to dynamically discover and register family source option types, enabling
    ///     extensibility and flexibility in handling various family source options.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if a type with a <see cref="FamilySourceOptionsAttribute" /> has a null or invalid <c>OptionsName</c>.
    /// </exception>
    private static Dictionary<string, Type> GetFamilySourceOptionsTypes()
    {
        //// Get all loaded assemblies in the current load context
        var assemblies = AssemblyLoadContext.GetLoadContext(typeof(SettingsManager).Assembly)!.Assemblies;

        // Search for types with the SettingsSectionAttribute in all assemblies
        return assemblies.SelectMany(assembly => assembly.GetTypes())
                         .Where(type => type.GetCustomAttribute<FamilySourceOptionsAttribute>() != null)
                         .Select(type => (Type: type, type.GetCustomAttribute<FamilySourceOptionsAttribute>()!.OptionsName))
                         .ToDictionary(attributedType => attributedType.OptionsName, attributedType => attributedType.Type);
    }

    /// <summary>
    ///     Retrieves a dictionary of layout option types available in the current application context.
    /// </summary>
    /// <remarks>
    ///     This method scans all loaded assemblies in the current load context for types decorated with the
    ///     <see cref="LayoutOptionsAttribute" />. It then creates a dictionary where the keys are the option names
    ///     specified in the attribute, and the values are the corresponding types.
    /// </remarks>
    /// <returns>
    ///     A dictionary where the keys are option names (as specified by the <see cref="LayoutOptionsAttribute.OptionsName" />
    ///     )
    ///     and the values are the types associated with those options.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if a type decorated with <see cref="LayoutOptionsAttribute" /> has a null or empty <c>OptionsName</c>.
    /// </exception>
    /// <example>
    ///     The following example demonstrates how the returned dictionary can be used:
    ///     <code>
    /// var layoutOptionsTypes = SettingsManager.GetLayoutOptionsTypes();
    /// foreach (var option in layoutOptionsTypes)
    /// {
    ///     Console.WriteLine($"Option Name: {option.Key}, Type: {option.Value.FullName}");
    /// }
    /// </code>
    /// </example>
    private static Dictionary<string, Type> GetLayoutOptionsTypes()
    {
        //// Get all loaded assemblies in the current load context
        var assemblies = AssemblyLoadContext.GetLoadContext(typeof(SettingsManager).Assembly)!.Assemblies;

        // Search for types with the SettingsSectionAttribute in all assemblies
        return assemblies.SelectMany(assembly => assembly.GetTypes())
                         .Where(type => type.GetCustomAttribute<LayoutOptionsAttribute>() != null)
                         .Select(type => (Type: type, type.GetCustomAttribute<LayoutOptionsAttribute>()!.OptionsName))
                         .ToDictionary(attributedType => attributedType.OptionsName, attributedType => attributedType.Type);
    }

    /// <summary>
    ///     Initializes the settings for the application using the specified settings file and option types.
    /// </summary>
    /// <param name="userSettingsFile">
    ///     The path to the user settings file. This file is expected to contain the configuration data for the application.
    /// </param>
    /// <param name="optionsTypes">
    ///     A dictionary mapping section names to their corresponding types. This is used to define the structure of the
    ///     settings.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the settings file path is invalid or if the settings cannot be initialized.
    /// </exception>
    /// <remarks>
    ///     This method loads the settings from the specified file, applies default values where necessary,
    ///     and saves the updated settings back to the file. It ensures that the settings are properly configured
    ///     and ready for use by the application.
    /// </remarks>
    private static void InitializeSettings(string userSettingsFile, Dictionary<string, Type> optionsTypes)
    {
        var settings = LoadSettings(userSettingsFile, optionsTypes, null);
        ApplyDefaultSettings(settings);
        SaveSettings(userSettingsFile, settings, null);
    }

    /// <summary>
    ///     Applies default settings to the provided settings dictionary.
    /// </summary>
    /// <param name="settings">
    ///     A dictionary containing the current application settings. The method modifies this dictionary
    ///     by merging default settings for family sources into it.
    /// </param>
    /// <remarks>
    ///     This method ensures that the default family sources are included in the settings. If a family source
    ///     from the default settings is already present in the current settings, it will be replaced while preserving
    ///     the "IsActive" property. Default family sources are marked as non-editable by setting their "IsEditable"
    ///     property to <c>false</c>.
    /// </remarks>
    private static void ApplyDefaultSettings(Dictionary<string, object> settings)
    {
        var familySources = (FamilySourcesOptions)settings["FamilySources"];
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var defaultFile = Path.Combine(path, "DefaultFamilySources.json");

        if (!File.Exists(defaultFile))
        {
            // No default settings.
            return;
        }

        // Load family sources from the default settings file. These family sources will then be merged into the current settings.
        // Family sources are identified by their names. If a family source is not present in the current settings, it will be added.
        // If a family source is already present, it will be replaced with the one from the default settings. // This ensures that
        // the family source always contains the latest data after updating the add-in.
        // Default family sources always have their "IsEditable" property set to false. As a result, users cannot modify the default
        // family sources in the settings dialog.
        // However, the value of the "IsActive" property will be preserved and the user will be able to activate or deactivate the sources
        // in the user settings dialog.

        var json = File.ReadAllText(defaultFile);
        var defaultFamilySources = JsonSerializer.Deserialize<FamilySourcesOptions>(json, JsonOptions);
        if (defaultFamilySources is null)
        {
            // No default settings available.
            return;
        }

        foreach (var defaultFamilySource in defaultFamilySources.Sources)
        {
            var source = familySources.Sources.FirstOrDefault(o => o.Name == defaultFamilySource.Name);
            if (source is null)
            {
                familySources.Sources.Add(defaultFamilySource);
            }
            else
            {
                familySources.Sources.Remove(source);

                var isActive = source.IsActive;
                defaultFamilySource.IsActive = isActive;
                familySources.Sources.Add(defaultFamilySource);
            }
        }
    }

    /// <summary>
    ///     Saves the specified settings to the provided file path in JSON format.
    /// </summary>
    /// <param name="settingsFilePath">
    ///     The file path where the settings will be saved. This path can include environment variables,
    ///     which will be expanded before saving.
    /// </param>
    /// <param name="settings">
    ///     A dictionary containing the settings to be saved, where the key represents the setting name
    ///     and the value represents the setting object.
    /// </param>
    /// <param name="logger">
    ///     An optional logger instance used to log information during the save operation. If <c>null</c>,
    ///     no logging will occur.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="settingsFilePath" /> is <c>null</c> or empty.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if an error occurs while writing to the specified file.
    /// </exception>
    /// <remarks>
    ///     This method ensures that the directory for the specified file path exists before attempting
    ///     to write the settings. If the directory does not exist, it will be created.
    /// </remarks>
    private static void SaveSettings(string settingsFilePath, Dictionary<string, object> settings, ILogger? logger)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);

        var file = settingsFilePath.ExpandVariables();
        var path = Path.GetDirectoryName(file);

        logger?.LogInformation($"Saving settings to '{file}'.");
        // Create the directory if it does not exist
        if (!Directory.Exists(path))
        {
            logger?.LogInformation($"Creating directory '{path}'.");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        }

        File.WriteAllText(file, json);
    }

    /// <summary>
    ///     Loads settings from a specified file and deserializes them into a dictionary of objects.
    /// </summary>
    /// <param name="settingsFile">
    ///     The path to the settings file. Environment variables within the path will be expanded automatically.
    /// </param>
    /// <param name="optionTypes">
    ///     A dictionary mapping section names to their corresponding types. These types are used to deserialize
    ///     the settings into strongly-typed objects.
    /// </param>
    /// <param name="logger">
    ///     An optional logger instance for logging information about the loading process.
    /// </param>
    /// <returns>
    ///     A dictionary containing the deserialized settings, where the keys are section names and the values
    ///     are the corresponding deserialized objects.
    /// </returns>
    /// <remarks>
    ///     If the specified settings file does not exist, a default empty JSON object is used. Each section
    ///     in the settings file is deserialized into an object of the type specified in the <paramref name="optionTypes" />.
    ///     If a section is missing in the file, a new instance of the corresponding type is created and added to the
    ///     dictionary.
    /// </remarks>
    /// <exception cref="JsonException">
    ///     Thrown if the settings file contains invalid JSON or if deserialization fails for any section.
    /// </exception>
    /// <exception cref="System.IO.IOException">
    ///     Thrown if there is an error reading the settings file.
    /// </exception>
    private static Dictionary<string, object> LoadSettings(string settingsFile, IDictionary<string, Type> optionTypes, ILogger? logger)
    {
        var file = settingsFile.ExpandVariables();

        // Creates an empty JSON object, which serves as a default if the settings file does not exist.
        var json = "{}";

        if (File.Exists(file))
        {
            logger?.LogInformation($"Reading settings from file '{file}'.");
            json = File.ReadAllText(file);
        }
        else
        {
            logger?.LogInformation($"Settings file '{file}' does not exist. Default settings will be used.");
        }

        var rawSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new Dictionary<string, JsonElement>();
        var settings = new Dictionary<string, object>();

        foreach (var (sectionName, optionType) in optionTypes)
        {
            object? options = null;
            if (rawSettings.TryGetValue(sectionName, out var jsonElement))
            {
                options = JsonSerializer.Deserialize(jsonElement.GetRawText(), optionType, JsonOptions);
            }

            // The JSON may not initially include all layout options, particularly during the first use of this add-in or when new options are introduced in a later version.
            // This process ensures that all layout options are created and included in the settings file when it is saved.
            // However, simply creating a new instance of the options object is insufficient.
            // Instead, the new object must be serialized and then deserialized.
            // This approach ensures that additional logic in type-specific JSON converters is executed during deserialization.

            if (options is null)
            {
                var newOptions = Activator.CreateInstance(optionType);
                if (newOptions is not null)
                {
                    var serializedOptions = JsonSerializer.Serialize(newOptions, optionType, JsonOptions);
                    options = JsonSerializer.Deserialize(serializedOptions, optionType, JsonOptions);
                }
            }

            if (options is not null)
            {
                settings[sectionName] = options;
            }
        }

        return settings;
    }
}
