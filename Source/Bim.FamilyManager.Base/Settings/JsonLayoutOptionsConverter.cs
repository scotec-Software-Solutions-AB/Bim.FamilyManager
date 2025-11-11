using System.Text.Json;
using System.Text.Json.Serialization;
using Bim.FamilyManager.Abstractions.Options;

namespace Bim.FamilyManager.Base.Settings;

/// <summary>
///     Provides functionality to convert a JSON representation of a list of <see cref="ILayoutOptions" />
///     to and from its object representation. This converter is used to handle custom serialization and
///     deserialization logic for layout options in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class ensures that layout options are properly loaded and saved, including handling missing
///     or unused options. It uses a dictionary of option types to map JSON properties to their corresponding
///     layout option types.
/// </remarks>
public class JsonLayoutOptionsConverter : JsonConverter<List<ILayoutOptions>>
{
    private readonly Dictionary<string, Type> _optionsTypes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonLayoutOptionsConverter" /> class with a dictionary
    ///     of layout option types.
    /// </summary>
    /// <param name="optionsTypes">
    ///     A dictionary where the key represents the name of the layout option, and the value represents the
    ///     corresponding <see cref="Type" /> of the layout option. This dictionary is used to map JSON properties
    ///     to their respective layout option types during serialization and deserialization.
    /// </param>
    /// <remarks>
    ///     This constructor is essential for enabling the converter to handle custom serialization and deserialization
    ///     of layout options, ensuring that all defined layout option types are properly recognized and processed.
    /// </remarks>
    public JsonLayoutOptionsConverter(Dictionary<string, Type> optionsTypes)
    {
        _optionsTypes = optionsTypes;
    }

    /// <summary>
    ///     Reads and converts a JSON representation of layout options into a list of <see cref="ILayoutOptions" /> objects.
    /// </summary>
    /// <param name="reader">
    ///     The <see cref="Utf8JsonReader" /> to read the JSON data from.
    /// </param>
    /// <param name="typeToConvert">
    ///     The type of the object to convert, which is expected to be a list of <see cref="ILayoutOptions" />.
    /// </param>
    /// <param name="options">
    ///     The <see cref="JsonSerializerOptions" /> that provide custom serialization or deserialization behavior.
    /// </param>
    /// <returns>
    ///     A list of <see cref="ILayoutOptions" /> objects deserialized from the JSON data.
    /// </returns>
    /// <exception cref="JsonException">
    ///     Thrown when the JSON data does not represent an object or when deserialization of a layout option fails.
    /// </exception>
    /// <remarks>
    ///     This method handles missing or unused layout options by excluding them from the deserialized list.
    ///     Additionally, it ensures that any new layout options not present in the JSON data are instantiated
    ///     and added to the list, so they can be included in the settings file during the next save operation.
    /// </remarks>
    public override List<ILayoutOptions> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var layouts = new List<ILayoutOptions>();

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Expected an object for Layouts.");
        }

        foreach (var property in root.EnumerateObject())
        {
            // If the type is not in the list, the layout may have been removed from the Family Manager add-in and is therefore no longer in use.
            // Since these options will not be loaded here, they will be removed from the settings file the next time the options are saved.
            if (_optionsTypes.TryGetValue(property.Name, out var type))
            {
                var layout = (ILayoutOptions)property.Value.Deserialize(type, options)!;
                layouts.Add(layout);
            }
        }

        // There may be new options added to the Family Manager add-in that are not yet included in the settings file.
        // These options will be added here so that they appear in the settings file the next time the options are saved.
        var loadedTypes = layouts.Select(o => o.GetType());
        var missingTypes = _optionsTypes.Values.Except(loadedTypes);
        foreach (var missingType in missingTypes)
        {
            if (Activator.CreateInstance(missingType) is ILayoutOptions newOptions)
            {
                layouts.Add(newOptions);
            }
        }

        return layouts;
    }

    /// <summary>
    ///     Writes a JSON representation of a list of <see cref="ILayoutOptions" /> to the specified
    ///     <see cref="Utf8JsonWriter" />.
    /// </summary>
    /// <param name="writer">
    ///     The <see cref="Utf8JsonWriter" /> to which the JSON representation of the layout options will be written.
    /// </param>
    /// <param name="value">
    ///     The list of <see cref="ILayoutOptions" /> to serialize into JSON format.
    /// </param>
    /// <param name="options">
    ///     The <see cref="JsonSerializerOptions" /> to use for customizing the serialization process.
    /// </param>
    /// <remarks>
    ///     This method serializes each layout option in the list as a JSON property, where the property name is derived
    ///     from the type name of the layout option (with the "Options" suffix removed). The method ensures that each
    ///     layout option is serialized using its specific type.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="writer" /> or <paramref name="value" /> is <c>null</c>.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, List<ILayoutOptions> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var layout in value)
        {
            var typeName = layout.GetType().Name.Replace("Options", "");
            writer.WritePropertyName(typeName);
            JsonSerializer.Serialize(writer, layout, layout.GetType(), options);
        }

        writer.WriteEndObject();
    }
}
